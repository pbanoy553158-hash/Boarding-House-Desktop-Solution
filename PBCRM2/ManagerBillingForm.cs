using PBCRM2;
using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PBCRM2
{
    public class ManagerBillingForm : Form
    {
        private readonly ApiService _apiService;

        private DataGridView dgvBilling = null!;

        private Button btnAddBilling = null!;
        private Button btnRecordPayment = null!;
        private Button btnViewDetails = null!;
        private Button btnRefresh = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbStatusFilter = null!;
        private Button btnClearSearch = null!;

        private Label lblSelectedInfo = null!;

        private List<BillingDto> _billings = new();

        // =========================================================
        // COLORS
        // =========================================================

        private static readonly Color BrandBg =
            Color.FromArgb(32, 24, 18);

        private static readonly Color BrandAccent =
            Color.FromArgb(224, 194, 140);

        private static readonly Color CtaColor =
            Color.FromArgb(170, 130, 80);

        private static readonly Color PanelBg =
            Color.FromArgb(250, 247, 242);

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ManagerBillingForm(ApiService apiService)
        {
            _apiService = apiService;

            Text = "Billing & Payments";

            StartPosition =
                FormStartPosition.CenterScreen;

            Size =
                new Size(1250, 780);

            MinimumSize =
                new Size(1050, 680);

            BackColor = PanelBg;

            BuildInterface();

            Shown += async (_, _) =>
                await LoadBillingAsync();
        }

        // =========================================================
        // BUILD INTERFACE
        // =========================================================

        private void BuildInterface()
        {
            var main = new Panel { Dock = DockStyle.Fill, BackColor = PanelBg };

            // =====================================================
            // HEADER
            // =====================================================

            var header = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = BrandBg };

            var title = new Label { Text = "BILLING & PAYMENTS", ForeColor = Color.White, Font = new Font("Segoe UI", 22F, FontStyle.Bold), AutoSize = true, Location = new Point(25, 22) };

            var subtitle = new Label { Text = "Manage tenant billing records and payment transactions.", ForeColor = BrandAccent, Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(28, 67) };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            // =====================================================
            // HEADER GAP
            // =====================================================

            var headerGap = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = PanelBg };

            // =====================================================
            // ACTION TOOLBAR
            // =====================================================

            var actionPanel = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = PanelBg };

            btnAddBilling = CreateButton("Add Billing", 110);
            btnAddBilling.Location = new Point(20, 10);
            btnAddBilling.Click += async (_, _) => await AddBillingAsync();

            btnRecordPayment = CreateButton("Record Payment", 125);
            btnRecordPayment.Location = new Point(140, 10);
            btnRecordPayment.Click += async (_, _) => await RecordPaymentAsync();

            btnViewDetails = CreateButton("View Details", 110);
            btnViewDetails.Location = new Point(275, 10);
            btnViewDetails.Click += async (_, _) => await ViewPaymentDetailsAsync();

            btnRefresh = CreateButton("Refresh", 95);
            btnRefresh.Location = new Point(395, 10);
            btnRefresh.Click += async (_, _) => await LoadBillingAsync();

            btnRecordPayment.Enabled = false;
            btnViewDetails.Enabled = false;

            actionPanel.Controls.Add(btnAddBilling);
            actionPanel.Controls.Add(btnRecordPayment);
            actionPanel.Controls.Add(btnViewDetails);
            actionPanel.Controls.Add(btnRefresh);

            // =====================================================
            // TOOLBAR GAP
            // =====================================================

            var toolbarGap = new Panel { Dock = DockStyle.Top, Height = 15, BackColor = PanelBg };

            // =====================================================
            // SEARCH / FILTER PANEL
            // =====================================================

            var filterPanel = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = PanelBg };

            // SEARCH LABEL

            var searchLabel = new Label { Text = "Search:", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = BrandBg, Location = new Point(20, 18) };

            // SEARCH BOX

            txtSearch = new TextBox { Location = new Point(78, 12), Width = 290, Height = 30, Font = new Font("Segoe UI", 9F), BorderStyle = BorderStyle.FixedSingle };

            txtSearch.TextChanged += (_, _) => ApplyFilters();

            // FILTER LABEL

            var filterLabel = new Label { Text = "Status:", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = BrandBg, Location = new Point(390, 18) };

            // STATUS FILTER

            cmbStatusFilter = new ComboBox { Location = new Point(445, 12), Width = 180, Height = 30, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F) };

            cmbStatusFilter.Items.Add("All Status");
            cmbStatusFilter.Items.Add("Pending");
            cmbStatusFilter.Items.Add("Partially Paid");
            cmbStatusFilter.Items.Add("Paid");
            cmbStatusFilter.Items.Add("Overdue");

            cmbStatusFilter.SelectedIndex = 0;

            cmbStatusFilter.SelectedIndexChanged += (_, _) => ApplyFilters();

            // CLEAR BUTTON

            btnClearSearch = CreateButton("Clear", 80);
            btnClearSearch.Location = new Point(640, 12);

            btnClearSearch.Click += (_, _) =>
            {
                txtSearch.Clear();
                cmbStatusFilter.SelectedIndex = 0;
            };

            filterPanel.Controls.Add(searchLabel);
            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(filterLabel);
            filterPanel.Controls.Add(cmbStatusFilter);
            filterPanel.Controls.Add(btnClearSearch);

            // =====================================================
            // FILTER GAP
            // =====================================================

            var filterGap = new Panel { Dock = DockStyle.Top, Height = 15, BackColor = PanelBg };

            // =====================================================
            // CONTENT
            // =====================================================

            var content = new Panel { Dock = DockStyle.Fill, BackColor = PanelBg, Padding = new Padding(20, 0, 20, 20) };

            // =====================================================
            // BILLING LABEL
            // =====================================================

            lblSelectedInfo = new Label { Text = "BILLING RECORDS", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = BrandBg, TextAlign = ContentAlignment.MiddleLeft };

            // =====================================================
            // BILLING GRID
            // =====================================================

            dgvBilling = CreateGrid();
            dgvBilling.Dock = DockStyle.Fill;

            dgvBilling.SelectionChanged += (_, _) =>
            {
                bool hasSelection = dgvBilling.SelectedRows.Count > 0;
                btnRecordPayment.Enabled = hasSelection;
                btnViewDetails.Enabled = hasSelection;
            };

            dgvBilling.CellDoubleClick += async (_, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    await ViewPaymentDetailsAsync();
                }
            };

            // =====================================================
            // ADD CONTENT CONTROLS
            // =====================================================

            content.Controls.Add(dgvBilling);
            content.Controls.Add(lblSelectedInfo);

            // =====================================================
            // ADD MAIN CONTROLS
            // =====================================================

            main.Controls.Add(content);
            main.Controls.Add(filterGap);
            main.Controls.Add(filterPanel);
            main.Controls.Add(toolbarGap);
            main.Controls.Add(actionPanel);
            main.Controls.Add(headerGap);
            main.Controls.Add(header);

            Controls.Add(main);
        }

        // =========================================================
        // CREATE GRID
        // =========================================================

        private DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 34,
                ShowCellToolTips = false
            };

            grid.RowTemplate.Height = 30;

            // =====================================================
            // DOUBLE BUFFER
            // =====================================================

            typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(grid, true, null);

            // =====================================================
            // HEADER STYLE
            // =====================================================

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = BrandBg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.False
            };

            // =====================================================
            // ROW STYLE
            // =====================================================

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 8.5F),
                BackColor = Color.White,
                ForeColor = BrandBg,
                SelectionBackColor = Color.FromArgb(232, 220, 199),
                SelectionForeColor = BrandBg,
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(247, 244, 239)
            };

            // =====================================================
            // TENANT
            // =====================================================

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenantName", HeaderText = "Tenant", DataPropertyName = "TenantName", FillWeight = 22, MinimumWidth = 140 });

            // =====================================================
            // ROOM
            // =====================================================

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RoomNumber", HeaderText = "Room", DataPropertyName = "RoomNumber", FillWeight = 8, MinimumWidth = 60 });

            // =====================================================
            // BED
            // =====================================================

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "BedNumber", HeaderText = "Bed", DataPropertyName = "BedNumber", FillWeight = 8, MinimumWidth = 60 });

            // =====================================================
            // AMOUNT
            // =====================================================

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MonthlyRentalAmount",
                HeaderText = "Amount",
                DataPropertyName = "MonthlyRentalAmount",
                FillWeight = 12,
                MinimumWidth = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "₱#,##0.00",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            // =====================================================
            // BILLING PERIOD
            // =====================================================

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "BillingPeriod", HeaderText = "Billing Period", DataPropertyName = "BillingPeriod", FillWeight = 20, MinimumWidth = 150 });

            // =====================================================
            // DUE DATE
            // =====================================================

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DueDateDisplay", HeaderText = "Due Date", DataPropertyName = "DueDateDisplay", FillWeight = 12, MinimumWidth = 100 });

            // =====================================================
            // PAID
            // =====================================================

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalPaid",
                HeaderText = "Paid",
                DataPropertyName = "TotalPaid",
                FillWeight = 11,
                MinimumWidth = 90,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "₱#,##0.00",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            // =====================================================
            // BALANCE
            // =====================================================

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OutstandingBalance",
                HeaderText = "Balance",
                DataPropertyName = "OutstandingBalance",
                FillWeight = 12,
                MinimumWidth = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "₱#,##0.00",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            // =====================================================
            // STATUS
            // =====================================================

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 11, MinimumWidth = 90 });

            return grid;
        }

        // =========================================================
        // CREATE BUTTON
        // =========================================================

        private Button CreateButton(string text, int width)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = CtaColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        // =========================================================
        // LOAD BILLING
        // =========================================================

        private async Task LoadBillingAsync()
        {
            try
            {
                btnRefresh.Enabled = false;

                var result = await _apiService.GetAsync<List<BillingDto>>("api/Billing");
                _billings = result ?? new List<BillingDto>();

                // =================================================
                // PREPARE DISPLAY VALUES
                // =================================================

                foreach (BillingDto billing in _billings)
                {
                    billing.BillingPeriod = $"{billing.BillingPeriodStart:MMM dd, yyyy} - {billing.BillingPeriodEnd:MMM dd, yyyy}";
                    billing.DueDateDisplay = billing.DueDate.ToString("MMM dd, yyyy");

                    if (string.IsNullOrWhiteSpace(billing.Status))
                    {
                        if (billing.OutstandingBalance <= 0)
                        {
                            billing.Status = "Paid";
                        }
                        else if (billing.TotalPaid > 0)
                        {
                            billing.Status = billing.DueDate.Date < DateTime.Today ? "Overdue" : "Partially Paid";
                        }
                        else if (billing.DueDate.Date < DateTime.Today)
                        {
                            billing.Status = "Overdue";
                        }
                        else
                        {
                            billing.Status = "Pending";
                        }
                    }
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load billing records.\n\n{ex.Message}", "PBCRM2", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        // =========================================================
        // APPLY SEARCH AND FILTER
        // =========================================================

        private void ApplyFilters()
        {
            if (dgvBilling == null) return;

            string search = txtSearch?.Text.Trim().ToLowerInvariant() ?? string.Empty;
            string selectedStatus = cmbStatusFilter?.SelectedItem?.ToString() ?? "All Status";

            IEnumerable<BillingDto> filtered = _billings;

            // =====================================================
            // SEARCH
            // =====================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(billing =>
                    (billing.TenantName ?? string.Empty).ToLowerInvariant().Contains(search) ||
                    (billing.RoomNumber ?? string.Empty).ToLowerInvariant().Contains(search) ||
                    (billing.BedNumber ?? string.Empty).ToLowerInvariant().Contains(search) ||
                    (billing.BillingPeriod ?? string.Empty).ToLowerInvariant().Contains(search) ||
                    (billing.Status ?? string.Empty).ToLowerInvariant().Contains(search) ||
                    (billing.DueDateDisplay ?? string.Empty).ToLowerInvariant().Contains(search));
            }

            // =====================================================
            // STATUS FILTER
            // =====================================================

            if (!string.Equals(selectedStatus, "All Status", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(billing => string.Equals(billing.Status, selectedStatus, StringComparison.OrdinalIgnoreCase));
            }

            // =====================================================
            // BIND DATA
            // =====================================================

            dgvBilling.DataSource = null;
            dgvBilling.DataSource = filtered.ToList();

            // =====================================================
            // BUTTON STATES
            // =====================================================

            btnRecordPayment.Enabled = dgvBilling.SelectedRows.Count > 0;
            btnViewDetails.Enabled = dgvBilling.SelectedRows.Count > 0;

            // =====================================================
            // UPDATE RECORD COUNT
            // =====================================================

            int count = filtered.Count();
            lblSelectedInfo.Text = count == 1 ? "BILLING RECORDS  •  1 RECORD" : $"BILLING RECORDS  •  {count} RECORDS";
        }

        // =========================================================
        // ADD BILLING
        // =========================================================

        private async Task AddBillingAsync()
        {
            try
            {
                List<TenantDto>? tenants = await _apiService.GetAsync<List<TenantDto>>("api/Tenants");

                if (tenants == null || tenants.Count == 0)
                {
                    MessageBox.Show("No tenants were found.", "Add Billing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                List<TenantDto> activeTenants = tenants.Where(t => string.Equals(t.Status, "Active", StringComparison.OrdinalIgnoreCase)).ToList();

                if (activeTenants.Count == 0)
                {
                    MessageBox.Show("There are no active tenants available for billing.", "Add Billing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using Form form = new Form
                {
                    Text = "Add Billing",
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ClientSize = new Size(500, 490),
                    BackColor = PanelBg
                };

                Label lblTenant = CreateInputLabel("Tenant", 30, 25);
                ComboBox cmbTenant = new ComboBox { Location = new Point(30, 50), Width = 440, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

                foreach (TenantDto tenant in activeTenants)
                {
                    cmbTenant.Items.Add(tenant);
                }

                cmbTenant.DisplayMember = nameof(TenantDto.FullName);

                if (cmbTenant.Items.Count > 0)
                {
                    cmbTenant.SelectedIndex = 0;
                }

                Label lblAmount = CreateInputLabel("Monthly Rental Amount", 30, 95);
                NumericUpDown nudAmount = new NumericUpDown { Location = new Point(30, 120), Width = 440, Minimum = 1, Maximum = 1000000, DecimalPlaces = 2, ThousandsSeparator = true, Font = new Font("Segoe UI", 10) };

                Label lblStart = CreateInputLabel("Billing Period Start", 30, 165);
                DateTimePicker dtpStart = new DateTimePicker { Location = new Point(30, 190), Width = 440, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };

                Label lblEnd = CreateInputLabel("Billing Period End", 30, 235);
                DateTimePicker dtpEnd = new DateTimePicker { Location = new Point(30, 260), Width = 440, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };

                dtpEnd.Value = dtpStart.Value.AddMonths(1).AddDays(-1);

                dtpStart.ValueChanged += (_, _) =>
                {
                    dtpEnd.Value = dtpStart.Value.AddMonths(1).AddDays(-1);
                };

                Label lblDue = CreateInputLabel("Due Date", 30, 305);
                DateTimePicker dtpDue = new DateTimePicker { Location = new Point(30, 330), Width = 440, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };

                Label lblNotes = CreateInputLabel("Notes", 30, 375);
                TextBox txtNotes = new TextBox { Location = new Point(30, 400), Width = 440, Height = 25, Font = new Font("Segoe UI", 10) };

                Button btnCancel = CreateButton("Cancel", 130);
                btnCancel.Location = new Point(170, 445);
                btnCancel.DialogResult = DialogResult.Cancel;

                Button btnSave = CreateButton("Save Billing", 145);
                btnSave.Location = new Point(315, 445);

                form.Controls.Add(lblTenant);
                form.Controls.Add(cmbTenant);
                form.Controls.Add(lblAmount);
                form.Controls.Add(nudAmount);
                form.Controls.Add(lblStart);
                form.Controls.Add(dtpStart);
                form.Controls.Add(lblEnd);
                form.Controls.Add(dtpEnd);
                form.Controls.Add(lblDue);
                form.Controls.Add(dtpDue);
                form.Controls.Add(lblNotes);
                form.Controls.Add(txtNotes);
                form.Controls.Add(btnCancel);
                form.Controls.Add(btnSave);

                form.AcceptButton = btnSave;
                form.CancelButton = btnCancel;

                BillingRequestDto? request = null;

                btnSave.Click += (_, _) =>
                {
                    if (cmbTenant.SelectedItem is not TenantDto selectedTenant)
                    {
                        MessageBox.Show("Please select a tenant.", "Add Billing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (nudAmount.Value <= 0)
                    {
                        MessageBox.Show("Monthly rental amount must be greater than zero.", "Add Billing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dtpEnd.Value.Date < dtpStart.Value.Date)
                    {
                        MessageBox.Show("Billing period end cannot be earlier than the start date.", "Add Billing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dtpDue.Value.Date < dtpStart.Value.Date)
                    {
                        MessageBox.Show("Due date cannot be earlier than the billing period start.", "Add Billing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    request = new BillingRequestDto
                    {
                        TenantId = selectedTenant.Id,
                        MonthlyRentalAmount = nudAmount.Value,
                        BillingPeriodStart = dtpStart.Value.Date,
                        BillingPeriodEnd = dtpEnd.Value.Date,
                        DueDate = dtpDue.Value.Date,
                        Notes = txtNotes.Text.Trim()
                    };

                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };

                if (form.ShowDialog(this) != DialogResult.OK || request == null)
                {
                    return;
                }

                var response = await _apiService.PostAsync("api/Billing", request);

                if (response != null)
                {
                    MessageBox.Show("Billing record created successfully.", "Add Billing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadBillingAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to create billing record.\n\n{ex.Message}", "Add Billing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // RECORD PAYMENT
        // =========================================================

        private async Task RecordPaymentAsync()
        {
            BillingDto? selectedBilling = GetSelectedBilling();

            if (selectedBilling == null)
            {
                MessageBox.Show("Please select a billing record first.", "Record Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selectedBilling.OutstandingBalance <= 0)
            {
                MessageBox.Show("This billing record has no outstanding balance.", "Record Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using Form form = new Form
            {
                Text = "Record Payment",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(500, 420),
                BackColor = PanelBg
            };

            Label lblTenant = new Label { AutoSize = true, Text = $"Tenant: {selectedBilling.TenantName}", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = BrandBg, Location = new Point(30, 25) };

            Label lblBalance = new Label { AutoSize = true, Text = $"Outstanding Balance: ₱{selectedBilling.OutstandingBalance:N2}", Font = new Font("Segoe UI", 10), ForeColor = BrandBg, Location = new Point(30, 55) };

            Label lblAmount = CreateInputLabel("Payment Amount", 30, 95);
            NumericUpDown nudAmount = new NumericUpDown { Location = new Point(30, 120), Width = 440, Minimum = 0.01M, Maximum = selectedBilling.OutstandingBalance, DecimalPlaces = 2, ThousandsSeparator = true, Font = new Font("Segoe UI", 10) };

            Label lblDate = CreateInputLabel("Payment Date", 30, 165);
            DateTimePicker dtpDate = new DateTimePicker { Location = new Point(30, 190), Width = 440, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = new Font("Segoe UI", 10) };

            Label lblMethod = CreateInputLabel("Payment Method", 30, 235);
            ComboBox cmbMethod = new ComboBox { Location = new Point(30, 260), Width = 440, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

            cmbMethod.Items.Add("Cash");
            cmbMethod.Items.Add("GCash");
            cmbMethod.Items.Add("Bank Transfer");
            cmbMethod.Items.Add("Other");
            cmbMethod.SelectedIndex = 0;

            Label lblReference = CreateInputLabel("Reference Number", 30, 305);
            TextBox txtReference = new TextBox { Location = new Point(30, 330), Width = 440, Font = new Font("Segoe UI", 10) };

            Button btnCancel = CreateButton("Cancel", 130);
            btnCancel.Location = new Point(170, 365);
            btnCancel.DialogResult = DialogResult.Cancel;

            Button btnSave = CreateButton("Save Payment", 145);
            btnSave.Location = new Point(315, 365);

            form.Controls.Add(lblTenant);
            form.Controls.Add(lblBalance);
            form.Controls.Add(lblAmount);
            form.Controls.Add(nudAmount);
            form.Controls.Add(lblDate);
            form.Controls.Add(dtpDate);
            form.Controls.Add(lblMethod);
            form.Controls.Add(cmbMethod);
            form.Controls.Add(lblReference);
            form.Controls.Add(txtReference);
            form.Controls.Add(btnCancel);
            form.Controls.Add(btnSave);

            form.AcceptButton = btnSave;
            form.CancelButton = btnCancel;

            PaymentRequestDto? request = null;

            btnSave.Click += (_, _) =>
            {
                if (nudAmount.Value <= 0)
                {
                    MessageBox.Show("Payment amount must be greater than zero.", "Record Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (nudAmount.Value > selectedBilling.OutstandingBalance)
                {
                    MessageBox.Show("Payment cannot exceed the outstanding balance.", "Record Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                request = new PaymentRequestDto
                {
                    Amount = nudAmount.Value,
                    PaymentDate = dtpDate.Value.Date,
                    PaymentMethod = cmbMethod.SelectedItem?.ToString() ?? "Cash",
                    ReferenceNumber = string.IsNullOrWhiteSpace(txtReference.Text) ? null : txtReference.Text.Trim()
                };

                form.DialogResult = DialogResult.OK;
                form.Close();
            };

            if (form.ShowDialog(this) != DialogResult.OK || request == null)
            {
                return;
            }

            try
            {
                var response = await _apiService.PostAsync($"api/Billing/{selectedBilling.Id}/payments", request);

                if (response != null)
                {
                    MessageBox.Show("Payment recorded successfully.", "Record Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadBillingAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to record payment.\n\n{ex.Message}", "Record Payment", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // VIEW PAYMENT DETAILS
        // =========================================================

        private async Task ViewPaymentDetailsAsync()
        {
            BillingDto? selectedBilling = GetSelectedBilling();

            if (selectedBilling == null)
            {
                MessageBox.Show("Please select a billing record first.", "Payment Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                List<PaymentDto>? payments = await _apiService.GetAsync<List<PaymentDto>>($"api/Billing/{selectedBilling.Id}/payments");
                payments ??= new List<PaymentDto>();

                using Form form = new Form
                {
                    Text = $"Payment Details - {selectedBilling.TenantName}",
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.Sizable,
                    MinimumSize = new Size(850, 500),
                    Size = new Size(950, 550),
                    BackColor = PanelBg
                };

                Label title = new Label { AutoSize = true, Text = "PAYMENT DETAILS", Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = BrandBg, Location = new Point(25, 20) };

                Label tenantLabel = new Label
                {
                    AutoSize = true,
                    Text = $"Tenant: {selectedBilling.TenantName}    Billing Amount: ₱{selectedBilling.TotalDue:N2}    Paid: ₱{selectedBilling.TotalPaid:N2}    Balance: ₱{selectedBilling.OutstandingBalance:N2}",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(80, 75, 70),
                    Location = new Point(27, 58)
                };

                DataGridView dgvPayments = new DataGridView
                {
                    Location = new Point(25, 100),
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                    Width = form.ClientSize.Width - 50,
                    Height = form.ClientSize.Height - 125,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    AllowUserToResizeColumns = false,
                    ReadOnly = true,
                    MultiSelect = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoGenerateColumns = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    RowHeadersVisible = false,
                    EnableHeadersVisualStyles = false,
                    ColumnHeadersHeight = 34,
                    Font = new Font("Segoe UI", 8.5F)
                };

                dgvPayments.RowTemplate.Height = 30;

                // =================================================
                // PAYMENT HEADER STYLE
                // =================================================

                dgvPayments.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = BrandBg,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    WrapMode = DataGridViewTriState.False
                };

                // =================================================
                // PAYMENT ROW STYLE
                // =================================================

                dgvPayments.DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 8.5F),
                    BackColor = Color.White,
                    ForeColor = BrandBg,
                    SelectionBackColor = Color.FromArgb(232, 220, 199),
                    SelectionForeColor = BrandBg
                };

                dgvPayments.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(247, 244, 239)
                };

                // =================================================
                // PAYMENT DATE
                // =================================================

                dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { Name = "PaymentDateDisplay", HeaderText = "Date", DataPropertyName = "PaymentDateDisplay", FillWeight = 15 });

                // =================================================
                // AMOUNT
                // =================================================

                dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Amount",
                    HeaderText = "Amount",
                    DataPropertyName = "Amount",
                    FillWeight = 15,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "₱#,##0.00",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                });

                // =================================================
                // PAYMENT METHOD
                // =================================================

                dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { Name = "PaymentMethod", HeaderText = "Payment Method", DataPropertyName = "PaymentMethod", FillWeight = 18 });

                // =================================================
                // STATUS
                // =================================================

                dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 12 });

                // =================================================
                // REFERENCE
                // =================================================

                dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReferenceNumber", HeaderText = "Reference Number", DataPropertyName = "ReferenceNumber", FillWeight = 20 });

                // =================================================
                // NOTES
                // =================================================

                dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "Notes", DataPropertyName = "Notes", FillWeight = 20 });

                // =================================================
                // PREPARE DISPLAY DATES
                // =================================================

                foreach (PaymentDto payment in payments)
                {
                    payment.PaymentDateDisplay = payment.PaymentDate.ToString("MMM dd, yyyy");
                }

                dgvPayments.DataSource = payments;

                form.Controls.Add(title);
                form.Controls.Add(tenantLabel);
                form.Controls.Add(dgvPayments);

                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load payment details.\n\n{ex.Message}", "Payment Details", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // GET SELECTED BILLING
        // =========================================================

        private BillingDto? GetSelectedBilling()
        {
            if (dgvBilling.SelectedRows.Count == 0)
            {
                return null;
            }

            return dgvBilling.SelectedRows[0].DataBoundItem as BillingDto;
        }

        // =========================================================
        // CREATE INPUT LABEL
        // =========================================================

        private Label CreateInputLabel(string text, int x, int y)
        {
            return new Label
            {
                AutoSize = true,
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(x, y)
            };
        }

        // =========================================================
        // DTOs
        // =========================================================

        public class BillingDto
        {
            public int Id { get; set; }
            public int TenantId { get; set; }
            public string TenantName { get; set; } = string.Empty;
            public string? RoomNumber { get; set; }
            public string? BedNumber { get; set; }
            public decimal MonthlyRentalAmount { get; set; }
            public DateTime BillingPeriodStart { get; set; }
            public DateTime BillingPeriodEnd { get; set; }
            public DateTime DueDate { get; set; }
            public decimal TotalDue { get; set; }
            public decimal TotalPaid { get; set; }
            public decimal OutstandingBalance { get; set; }
            public string Status { get; set; } = string.Empty;
            public string? Notes { get; set; }
            public string BillingPeriod { get; set; } = string.Empty;
            public string DueDateDisplay { get; set; } = string.Empty;
        }

        public class PaymentDto
        {
            public int Id { get; set; }
            public int BillingId { get; set; }
            public int TenantId { get; set; }
            public string TenantName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public DateTime PaymentDate { get; set; }
            public string PaymentMethod { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string? ReferenceNumber { get; set; }
            public string? Notes { get; set; }
            public string PaymentDateDisplay { get; set; } = string.Empty;
        }

        public class BillingRequestDto
        {
            public int TenantId { get; set; }
            public decimal MonthlyRentalAmount { get; set; }
            public DateTime BillingPeriodStart { get; set; }
            public DateTime BillingPeriodEnd { get; set; }
            public DateTime DueDate { get; set; }
            public string? Notes { get; set; }
        }

        public class PaymentRequestDto
        {
            public decimal Amount { get; set; }
            public DateTime PaymentDate { get; set; }
            public string PaymentMethod { get; set; } = "Cash";
            public string? ReferenceNumber { get; set; }
            public string? Notes { get; set; }
        }

        public class TenantDto
        {
            public int Id { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        public class IdMessage
        {
            public string? Message { get; set; }
            public int BillingId { get; set; }
            public int PaymentId { get; set; }
        }
    }
}

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
    public class ManagerMaintenanceForm : Form
    {
        private readonly ApiService _apiService;
        private DataGridView dgvMaintenance = null!;
        private Button btnInProgress = null!;
        private Button btnResolve = null!;
        private Button btnViewDetails = null!;
        private Button btnRefresh = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbStatusFilter = null!;
        private Button btnClearSearch = null!;
        private Label lblSelectedInfo = null!;
        private List<MaintenanceRequestDto> _requests = new();

        private static readonly Color BrandBg = Color.FromArgb(32, 24, 18);
        private static readonly Color BrandAccent = Color.FromArgb(224, 194, 140);
        private static readonly Color CtaColor = Color.FromArgb(170, 130, 80);
        private static readonly Color PanelBg = Color.FromArgb(250, 247, 242);
        private static readonly Color GridAlternate = Color.FromArgb(247, 244, 239);
        private static readonly Color GridSelection = Color.FromArgb(232, 220, 199);
        private static readonly Color GridBorder = Color.FromArgb(225, 215, 200);

        private static readonly Color PendingBg = Color.FromArgb(255, 244, 204);
        private static readonly Color PendingText = Color.FromArgb(130, 95, 0);
        private static readonly Color InProgressBg = Color.FromArgb(218, 235, 252);
        private static readonly Color InProgressText = Color.FromArgb(35, 91, 145);
        private static readonly Color ResolvedBg = Color.FromArgb(218, 242, 224);
        private static readonly Color ResolvedText = Color.FromArgb(35, 110, 55);
        private static readonly Color CancelledBg = Color.FromArgb(250, 222, 222);
        private static readonly Color CancelledText = Color.FromArgb(150, 45, 45);

        public ManagerMaintenanceForm(ApiService apiService)
        {
            _apiService = apiService;
            Text = "Maintenance";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1250, 780);
            MinimumSize = new Size(1050, 680);
            BackColor = PanelBg;

            BuildInterface();

            Shown += async (_, _) => await LoadMaintenanceAsync();
        }

        private void BuildInterface()
        {
            var main = new Panel { Dock = DockStyle.Fill, BackColor = PanelBg };

            var header = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = BrandBg };
            var title = new Label { Text = "MAINTENANCE", ForeColor = Color.White, Font = new Font("Segoe UI", 22F, FontStyle.Bold), AutoSize = true, Location = new Point(25, 22) };
            var subtitle = new Label { Text = "Manage tenant maintenance requests and track their resolution.", ForeColor = BrandAccent, Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(28, 67) };
            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            var headerGap = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = PanelBg };

            var actionPanel = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = PanelBg };

            btnInProgress = CreateButton("Set In Progress", 130);
            btnInProgress.Location = new Point(20, 10);
            btnInProgress.Enabled = false;
            btnInProgress.Click += async (_, _) => await SetSelectedStatusAsync("In Progress");

            btnResolve = CreateButton("Resolve Request", 135);
            btnResolve.Location = new Point(160, 10);
            btnResolve.Enabled = false;
            btnResolve.Click += async (_, _) => await ResolveSelectedRequestAsync();

            btnViewDetails = CreateButton("View Details", 110);
            btnViewDetails.Location = new Point(305, 10);
            btnViewDetails.Enabled = false;
            btnViewDetails.Click += async (_, _) => await ViewSelectedRequestDetailsAsync();

            btnRefresh = CreateButton("Refresh", 95);
            btnRefresh.Location = new Point(425, 10);
            btnRefresh.Click += async (_, _) => await LoadMaintenanceAsync();

            actionPanel.Controls.Add(btnInProgress);
            actionPanel.Controls.Add(btnResolve);
            actionPanel.Controls.Add(btnViewDetails);
            actionPanel.Controls.Add(btnRefresh);

            var toolbarGap = new Panel { Dock = DockStyle.Top, Height = 15, BackColor = PanelBg };

            var filterPanel = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = PanelBg };

            var searchLabel = new Label { Text = "Search:", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = BrandBg, Location = new Point(20, 18) };
            txtSearch = new TextBox { Location = new Point(78, 12), Width = 290, Height = 30, Font = new Font("Segoe UI", 9F), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += (_, _) => ApplyFilters();

            var filterLabel = new Label { Text = "Status:", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = BrandBg, Location = new Point(390, 18) };
            cmbStatusFilter = new ComboBox { Location = new Point(445, 12), Width = 180, Height = 30, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F) };
            cmbStatusFilter.Items.Add("All Status");
            cmbStatusFilter.Items.Add("Pending");
            cmbStatusFilter.Items.Add("In Progress");
            cmbStatusFilter.Items.Add("Resolved");
            cmbStatusFilter.Items.Add("Cancelled");
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (_, _) => ApplyFilters();

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

            var filterGap = new Panel { Dock = DockStyle.Top, Height = 15, BackColor = PanelBg };

            var content = new Panel { Dock = DockStyle.Fill, BackColor = PanelBg, Padding = new Padding(20, 0, 20, 20) };

            lblSelectedInfo = new Label { Text = "MAINTENANCE REQUESTS", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = BrandBg, TextAlign = ContentAlignment.MiddleLeft };

            dgvMaintenance = CreateGrid();
            dgvMaintenance.Dock = DockStyle.Fill;
            dgvMaintenance.SelectionChanged += (_, _) =>
            {
                bool hasSelection = dgvMaintenance.SelectedRows.Count > 0;
                btnInProgress.Enabled = hasSelection;
                btnResolve.Enabled = hasSelection;
                btnViewDetails.Enabled = hasSelection;
            };
            dgvMaintenance.CellDoubleClick += async (_, e) =>
            {
                if (e.RowIndex >= 0) await ViewSelectedRequestDetailsAsync();
            };

            content.Controls.Add(dgvMaintenance);
            content.Controls.Add(lblSelectedInfo);

            main.Controls.Add(content);
            main.Controls.Add(filterGap);
            main.Controls.Add(filterPanel);
            main.Controls.Add(toolbarGap);
            main.Controls.Add(actionPanel);
            main.Controls.Add(headerGap);
            main.Controls.Add(header);

            Controls.Add(main);
        }

        private DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = GridBorder,
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
                ShowCellToolTips = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            };

            grid.RowTemplate.Height = 30;

            typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(grid, true, null);

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = BrandBg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.False
            };

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 8.5F),
                BackColor = Color.White,
                ForeColor = BrandBg,
                SelectionBackColor = GridSelection,
                SelectionForeColor = BrandBg,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.True,
                Padding = new Padding(8, 5, 8, 5)
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = GridAlternate,
                ForeColor = BrandBg,
                SelectionBackColor = GridSelection,
                SelectionForeColor = BrandBg,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.True,
                Padding = new Padding(8, 5, 8, 5)
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenantName", HeaderText = "Tenant", DataPropertyName = "TenantName", FillWeight = 20, MinimumWidth = 140, SortMode = DataGridViewColumnSortMode.NotSortable });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Request", HeaderText = "Request", DataPropertyName = "DisplayRequest", FillWeight = 18, MinimumWidth = 130, SortMode = DataGridViewColumnSortMode.NotSortable });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", DataPropertyName = "DisplayDescription", FillWeight = 30, MinimumWidth = 240, SortMode = DataGridViewColumnSortMode.NotSortable });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Priority", HeaderText = "Priority", DataPropertyName = "Priority", FillWeight = 10, MinimumWidth = 85, SortMode = DataGridViewColumnSortMode.NotSortable, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 12, MinimumWidth = 105, SortMode = DataGridViewColumnSortMode.NotSortable, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DateReportedDisplay", HeaderText = "Date Reported", DataPropertyName = "DateReportedDisplay", FillWeight = 13, MinimumWidth = 115, SortMode = DataGridViewColumnSortMode.NotSortable, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });

            grid.CellFormatting += DgvMaintenance_CellFormatting;

            return grid;
        }

        private void DgvMaintenance_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string columnName = dgvMaintenance.Columns[e.ColumnIndex].Name;

            if (columnName == "Status")
            {
                string status = e.Value?.ToString()?.Trim() ?? string.Empty;
                e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                switch (status.ToLowerInvariant())
                {
                    case "pending":
                        e.CellStyle.BackColor = PendingBg;
                        e.CellStyle.ForeColor = PendingText;
                        e.CellStyle.SelectionBackColor = PendingBg;
                        e.CellStyle.SelectionForeColor = PendingText;
                        break;
                    case "in progress":
                    case "in-progress":
                        e.CellStyle.BackColor = InProgressBg;
                        e.CellStyle.ForeColor = InProgressText;
                        e.CellStyle.SelectionBackColor = InProgressBg;
                        e.CellStyle.SelectionForeColor = InProgressText;
                        break;
                    case "resolved":
                    case "completed":
                        e.CellStyle.BackColor = ResolvedBg;
                        e.CellStyle.ForeColor = ResolvedText;
                        e.CellStyle.SelectionBackColor = ResolvedBg;
                        e.CellStyle.SelectionForeColor = ResolvedText;
                        break;
                    case "cancelled":
                    case "canceled":
                        e.CellStyle.BackColor = CancelledBg;
                        e.CellStyle.ForeColor = CancelledText;
                        e.CellStyle.SelectionBackColor = CancelledBg;
                        e.CellStyle.SelectionForeColor = CancelledText;
                        break;
                    default:
                        e.CellStyle.BackColor = Color.White;
                        e.CellStyle.ForeColor = BrandBg;
                        e.CellStyle.SelectionBackColor = GridSelection;
                        e.CellStyle.SelectionForeColor = BrandBg;
                        break;
                }
            }

            if (columnName == "Priority")
            {
                string priority = e.Value?.ToString()?.Trim() ?? string.Empty;
                e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

                switch (priority.ToLowerInvariant())
                {
                    case "urgent":
                        e.CellStyle.ForeColor = Color.FromArgb(160, 60, 45);
                        break;
                    case "high":
                        e.CellStyle.ForeColor = Color.FromArgb(175, 75, 55);
                        break;
                    case "normal":
                        e.CellStyle.ForeColor = CtaColor;
                        break;
                    case "low":
                        e.CellStyle.ForeColor = Color.FromArgb(90, 120, 90);
                        break;
                }
            }
        }

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

        private async Task LoadMaintenanceAsync()
        {
            try
            {
                btnRefresh.Enabled = false;

                var result = await _apiService.GetAsync<List<MaintenanceRequestDto>>("api/Maintenance");
                _requests = result ?? new List<MaintenanceRequestDto>();

                foreach (MaintenanceRequestDto request in _requests)
                {
                    request.TenantName = request.Tenant?.FullName ?? request.TenantName ?? $"Tenant #{request.TenantId}";
                    request.DisplayRequest = GetDisplayRequest(request.Title);
                    request.DisplayDescription = GetDisplayDescription(request.Description);
                    request.DateReportedDisplay = request.DateReported.ToLocalTime().ToString("MMM dd, yyyy");
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load maintenance records.\n\n{ex.Message}", "Maintenance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private string GetDisplayRequest(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;
            string cleanTitle = title.Trim();

            if (cleanTitle.StartsWith("Specific Issue:", StringComparison.OrdinalIgnoreCase))
            {
                cleanTitle = cleanTitle.Substring("Specific Issue:".Length).Trim();
            }

            int separator = cleanTitle.IndexOf(" - ", StringComparison.Ordinal);
            if (separator > 0)
            {
                cleanTitle = cleanTitle.Substring(0, separator).Trim();
            }

            return cleanTitle;
        }

        private string GetDisplayDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description)) return string.Empty;
            string text = description.Trim();

            int additionalIndex = text.IndexOf("Additional Details:", StringComparison.OrdinalIgnoreCase);
            if (additionalIndex >= 0)
            {
                return text.Substring(additionalIndex + "Additional Details:".Length).Trim();
            }

            int specificIndex = text.IndexOf("Specific Issue:", StringComparison.OrdinalIgnoreCase);
            if (specificIndex >= 0)
            {
                string afterSpecific = text.Substring(specificIndex + "Specific Issue:".Length).Trim();
                string[] lines = afterSpecific.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                if (lines.Length > 1)
                {
                    return string.Join(Environment.NewLine, lines.Skip(1)).Trim();
                }
                return string.Empty;
            }

            return text;
        }

        private void ApplyFilters()
        {
            if (dgvMaintenance == null) return;

            string search = txtSearch?.Text.Trim().ToLowerInvariant() ?? string.Empty;
            string selectedStatus = cmbStatusFilter?.SelectedItem?.ToString() ?? "All Status";

            IEnumerable<MaintenanceRequestDto> filtered = _requests;

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(request =>
                    (request.TenantName ?? string.Empty).ToLowerInvariant().Contains(search) ||
                    (request.DisplayRequest ?? string.Empty).ToLowerInvariant().Contains(search) ||
                    (request.DisplayDescription ?? string.Empty).ToLowerInvariant().Contains(search) ||
                    (request.Priority ?? string.Empty).ToLowerInvariant().Contains(search) ||
                    (request.Status ?? string.Empty).ToLowerInvariant().Contains(search) ||
                    (request.DateReportedDisplay ?? string.Empty).ToLowerInvariant().Contains(search));
            }

            if (!string.Equals(selectedStatus, "All Status", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(request => string.Equals(request.Status, selectedStatus, StringComparison.OrdinalIgnoreCase));
            }

            List<MaintenanceRequestDto> display = filtered.ToList();

            dgvMaintenance.DataSource = null;
            dgvMaintenance.DataSource = display;

            bool hasSelection = dgvMaintenance.SelectedRows.Count > 0;
            btnInProgress.Enabled = hasSelection;
            btnResolve.Enabled = hasSelection;
            btnViewDetails.Enabled = hasSelection;

            int count = display.Count;
            lblSelectedInfo.Text = count == 1 ? "MAINTENANCE REQUESTS  •  1 RECORD" : $"MAINTENANCE REQUESTS  •  {count} RECORDS";

            dgvMaintenance.ClearSelection();

            if (dgvMaintenance.Rows.Count > 0)
            {
                dgvMaintenance.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
            }
        }

        private MaintenanceRequestDto? GetSelectedRequest()
        {
            if (dgvMaintenance.SelectedRows.Count == 0) return null;
            return dgvMaintenance.SelectedRows[0].DataBoundItem as MaintenanceRequestDto;
        }

        private async Task SetSelectedStatusAsync(string status)
        {
            MaintenanceRequestDto? selectedRequest = GetSelectedRequest();
            if (selectedRequest == null)
            {
                MessageBox.Show("Please select a maintenance request first.", "Maintenance", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.Equals(selectedRequest.Status, "Resolved", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This maintenance request has already been resolved.", "Maintenance", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.Equals(selectedRequest.Status, status, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show($"This request is already marked as {status}.", "Maintenance", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                btnInProgress.Enabled = false;
                btnResolve.Enabled = false;
                btnViewDetails.Enabled = false;
                btnRefresh.Enabled = false;

                var result = await _apiService.PutAsync<MaintenanceRequestDto>(
                    $"api/Maintenance/{selectedRequest.Id}",
                    new
                    {
                        tenantId = selectedRequest.TenantId,
                        title = selectedRequest.Title,
                        description = selectedRequest.Description,
                        priority = selectedRequest.Priority,
                        status = status,
                        resolutionNotes = selectedRequest.ResolutionNotes
                    });

                if (result == null) return;

                MessageBox.Show($"Maintenance request has been marked as {status}.", "Maintenance Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadMaintenanceAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to update the maintenance request.\n\n{ex.Message}", "Maintenance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnInProgress.Enabled = true;
                btnResolve.Enabled = true;
                btnViewDetails.Enabled = true;
                btnRefresh.Enabled = true;
            }
        }

        private async Task ResolveSelectedRequestAsync()
        {
            MaintenanceRequestDto? selectedRequest = GetSelectedRequest();
            if (selectedRequest == null)
            {
                MessageBox.Show("Please select a maintenance request first.", "Resolve Request", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.Equals(selectedRequest.Status, "Resolved", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This maintenance request is already resolved.", "Resolve Request", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using ResolveMaintenanceDialog dialog = new ResolveMaintenanceDialog(selectedRequest.DisplayRequest, selectedRequest.DisplayDescription);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                btnInProgress.Enabled = false;
                btnResolve.Enabled = false;
                btnViewDetails.Enabled = false;
                btnRefresh.Enabled = false;

                var result = await _apiService.PutAsync<MaintenanceRequestDto>(
                    $"api/Maintenance/{selectedRequest.Id}",
                    new
                    {
                        tenantId = selectedRequest.TenantId,
                        title = selectedRequest.Title,
                        description = selectedRequest.Description,
                        priority = selectedRequest.Priority,
                        status = "Resolved",
                        resolutionNotes = dialog.ResolutionNotes
                    });

                if (result == null) return;

                MessageBox.Show("The maintenance request has been resolved.", "Maintenance Resolved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadMaintenanceAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to resolve the maintenance request.\n\n{ex.Message}", "Resolve Request", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnInProgress.Enabled = true;
                btnResolve.Enabled = true;
                btnViewDetails.Enabled = true;
                btnRefresh.Enabled = true;
            }
        }

        private async Task ViewSelectedRequestDetailsAsync()
        {
            MaintenanceRequestDto? selectedRequest = GetSelectedRequest();
            if (selectedRequest == null)
            {
                MessageBox.Show("Please select a maintenance request first.", "Maintenance Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string resolvedDate = selectedRequest.DateResolved.HasValue ? selectedRequest.DateResolved.Value.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt") : "Not resolved";
            string resolutionNotes = string.IsNullOrWhiteSpace(selectedRequest.ResolutionNotes) ? "No resolution notes." : selectedRequest.ResolutionNotes;

            using Form form = new Form
            {
                Text = "Maintenance Request Details",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(600, 500),
                BackColor = PanelBg
            };

            Label title = new Label { AutoSize = true, Text = "MAINTENANCE REQUEST DETAILS", Font = new Font("Segoe UI", 17F, FontStyle.Bold), ForeColor = BrandBg, Location = new Point(30, 25) };
            Label tenant = CreateDetailsLabel($"Tenant: {selectedRequest.TenantName}", 30, 75, 10F, FontStyle.Bold);
            Label request = CreateDetailsLabel($"Request: {selectedRequest.DisplayRequest}", 30, 110, 10F, FontStyle.Regular);
            Label descriptionTitle = CreateDetailsLabel("Description:", 30, 150, 10F, FontStyle.Bold);

            TextBox description = new TextBox
            {
                Location = new Point(30, 178),
                Width = 540,
                Height = 70,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = selectedRequest.DisplayDescription
            };

            Label priority = CreateDetailsLabel($"Priority: {selectedRequest.Priority}", 30, 265, 9.5F, FontStyle.Regular);
            Label status = CreateDetailsLabel($"Status: {selectedRequest.Status}", 30, 295, 9.5F, FontStyle.Bold);
            ApplyStatusLabelColor(status, selectedRequest.Status);

            Label dateReported = CreateDetailsLabel($"Date Reported: {selectedRequest.DateReported.ToLocalTime():MMM dd, yyyy hh:mm tt}", 30, 325, 9.5F, FontStyle.Regular);
            Label dateResolved = CreateDetailsLabel($"Date Resolved: {resolvedDate}", 30, 355, 9.5F, FontStyle.Regular);
            Label resolutionTitle = CreateDetailsLabel("Resolution Notes:", 30, 390, 9.5F, FontStyle.Bold);

            TextBox resolution = new TextBox
            {
                Location = new Point(30, 415),
                Width = 540,
                Height = 55,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = resolutionNotes
            };

            Button btnClose = CreateButton("Close", 100);
            btnClose.Location = new Point(470, 460);
            btnClose.DialogResult = DialogResult.Cancel;

            form.Controls.Add(title);
            form.Controls.Add(tenant);
            form.Controls.Add(request);
            form.Controls.Add(descriptionTitle);
            form.Controls.Add(description);
            form.Controls.Add(priority);
            form.Controls.Add(status);
            form.Controls.Add(dateReported);
            form.Controls.Add(dateResolved);
            form.Controls.Add(resolutionTitle);
            form.Controls.Add(resolution);
            form.Controls.Add(btnClose);

            form.CancelButton = btnClose;
            form.ShowDialog(this);

            await Task.CompletedTask;
        }

        private void ApplyStatusLabelColor(Label label, string? status)
        {
            switch (status?.Trim().ToLowerInvariant())
            {
                case "pending":
                    label.ForeColor = PendingText;
                    break;
                case "in progress":
                case "in-progress":
                    label.ForeColor = InProgressText;
                    break;
                case "resolved":
                case "completed":
                    label.ForeColor = ResolvedText;
                    break;
                case "cancelled":
                case "canceled":
                    label.ForeColor = CancelledText;
                    break;
                default:
                    label.ForeColor = BrandBg;
                    break;
            }
        }

        private Label CreateDetailsLabel(string text, int x, int y, float fontSize, FontStyle fontStyle)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", fontSize, fontStyle),
                ForeColor = BrandBg,
                Location = new Point(x, y)
            };
        }

        public class ResolveMaintenanceDialog : Form
        {
            private TextBox txtNotes = null!;
            private Button btnCancel = null!;
            private Button btnResolve = null!;

            public string ResolutionNotes => txtNotes.Text.Trim();

            public ResolveMaintenanceDialog(string title, string description)
            {
                Text = "Resolve Maintenance Request";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(500, 420);
                BackColor = PanelBg;

                BuildInterface(title, description);
            }

            private void BuildInterface(string title, string description)
            {
                Label lblTitle = CreateDialogLabel("Resolve Maintenance Request", 30, 25, 18F, FontStyle.Bold);
                Label lblRequest = CreateDialogLabel(title, 30, 70, 10F, FontStyle.Bold);
                Label lblDescription = CreateDialogLabel(description, 30, 98, 9F, FontStyle.Regular);
                lblDescription.Size = new Size(440, 55);
                lblDescription.AutoEllipsis = false;

                Label lblNotes = CreateDialogLabel("Resolution Notes", 30, 170, 9F, FontStyle.Bold);

                txtNotes = new TextBox
                {
                    Location = new Point(30, 195),
                    Width = 440,
                    Height = 105,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("Segoe UI", 10F),
                    BorderStyle = BorderStyle.FixedSingle
                };

                btnCancel = CreateDialogButton("Cancel", 130);
                btnCancel.Location = new Point(170, 350);
                btnCancel.DialogResult = DialogResult.Cancel;

                btnResolve = CreateDialogButton("Resolve", 145);
                btnResolve.Location = new Point(315, 350);
                btnResolve.Click += (_, _) =>
                {
                    if (string.IsNullOrWhiteSpace(txtNotes.Text))
                    {
                        MessageBox.Show("Please enter resolution notes.", "Resolution Notes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtNotes.Focus();
                        return;
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                };

                Controls.Add(lblTitle);
                Controls.Add(lblRequest);
                Controls.Add(lblDescription);
                Controls.Add(lblNotes);
                Controls.Add(txtNotes);
                Controls.Add(btnCancel);
                Controls.Add(btnResolve);

                AcceptButton = btnResolve;
                CancelButton = btnCancel;
            }

            private Label CreateDialogLabel(string text, int x, int y, float fontSize, FontStyle fontStyle)
            {
                return new Label
                {
                    Text = text,
                    AutoSize = true,
                    Font = new Font("Segoe UI", fontSize, fontStyle),
                    ForeColor = BrandBg,
                    Location = new Point(x, y)
                };
            }

            private Button CreateDialogButton(string text, int width)
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
        }

        public class MaintenanceRequestDto
        {
            public int Id { get; set; }
            public int TenantId { get; set; }
            public string TenantName { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Priority { get; set; } = "Normal";
            public string Status { get; set; } = "Pending";
            public DateTime DateReported { get; set; }
            public DateTime? DateResolved { get; set; }
            public string? ResolutionNotes { get; set; }
            public MaintenanceTenantDto? Tenant { get; set; }
                
            public string DisplayRequest { get; set; } = string.Empty;
            public string DisplayDescription { get; set; } = string.Empty;
            public string DateReportedDisplay { get; set; } = string.Empty;
        }

        public class MaintenanceTenantDto
        {
            public int Id { get; set; }
            public string FullName { get; set; } = string.Empty;
        }
    }
}
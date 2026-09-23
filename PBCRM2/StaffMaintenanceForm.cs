using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using PBCRM2.WinForms.Services;

namespace PBCRM2
{
    public class StaffMaintenanceForm : Form
    {
        private readonly ApiService _apiService;

        private DataGridView dgvMaintenance = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbStatusFilter = null!;
        private Button btnRefresh = null!;
        private Button btnNewRequest = null!;
        private Label lblRecordCount = null!;

        private List<StaffMaintenanceDto> _requests = new List<StaffMaintenanceDto>();

        // =========================================================
        // COLORS
        // =========================================================

        private static readonly Color BrandBg = Color.FromArgb(32, 24, 18);
        private static readonly Color BrandAccent = Color.FromArgb(224, 194, 140);
        private static readonly Color CtaColor = Color.FromArgb(170, 130, 80);
        private static readonly Color PanelBg = Color.FromArgb(250, 247, 242);
        private static readonly Color CardBg = Color.White;
        private static readonly Color CardBorder = Color.FromArgb(230, 224, 215);
        private static readonly Color SecondaryText = Color.FromArgb(105, 95, 85);

        // Status colors (soft)
        private static readonly Color PendingBg = Color.FromArgb(255, 248, 220);      // soft yellow
        private static readonly Color PendingText = Color.FromArgb(140, 100, 20);

        private static readonly Color InProgressBg = Color.FromArgb(220, 235, 250);   // soft blue
        private static readonly Color InProgressText = Color.FromArgb(30, 90, 160);

        private static readonly Color ResolvedBg = Color.FromArgb(220, 245, 230);     // soft green
        private static readonly Color ResolvedText = Color.FromArgb(20, 120, 70);

        private static readonly Color CancelledBg = Color.FromArgb(255, 230, 230);    // soft red
        private static readonly Color CancelledText = Color.FromArgb(160, 40, 40);

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public StaffMaintenanceForm(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            Text = "Maintenance";
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;
            BackColor = PanelBg;

            BuildInterface();

            Shown += async (_, _) => await LoadMaintenanceAsync();
        }

        // =========================================================
        // BUILD INTERFACE
        // =========================================================

        private void BuildInterface()
        {
            Panel main = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg
            };

            // Header
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = BrandBg
            };

            Label lblTitle = new Label
            {
                Text = "MAINTENANCE",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 22)
            };

            Label lblSubtitle = new Label
            {
                Text = "View and manage maintenance requests",
                ForeColor = BrandAccent,
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(28, 67)
            };

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSubtitle);

            // Header gap
            Panel headerGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = PanelBg
            };

            // Action toolbar
            Panel actionPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = PanelBg
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "Search tenant or request...",
                Font = new Font("Segoe UI", 9F),
                Location = new Point(20, 12),
                Size = new Size(240, 28),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (_, _) => ApplyFilters();

            cmbStatusFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(275, 10),
                Size = new Size(145, 32),
                BackColor = Color.White,
                ForeColor = BrandBg,
                FlatStyle = FlatStyle.Flat
            };
            cmbStatusFilter.Items.AddRange(new object[]
            {
                "All Status",
                "Pending",
                "In Progress",
                "Resolved",
                "Cancelled"
            });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (_, _) => ApplyFilters();

            btnRefresh = CreateButton("Refresh", 435, 10, 95, 32, CtaColor, Color.White);
            btnRefresh.Click += async (_, _) => await LoadMaintenanceAsync();

            btnNewRequest = CreateButton("New Request", 545, 10, 125, 32, CtaColor, Color.White);
            btnNewRequest.Click += async (_, _) => await CreateMaintenanceRequestAsync();

            lblRecordCount = new Label
            {
                Text = "0 records",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = BrandBg,
                AutoSize = true,
                Location = new Point(690, 18)
            };

            actionPanel.Controls.Add(txtSearch);
            actionPanel.Controls.Add(cmbStatusFilter);
            actionPanel.Controls.Add(btnRefresh);
            actionPanel.Controls.Add(btnNewRequest);
            actionPanel.Controls.Add(lblRecordCount);

            // Toolbar gap
            Panel toolbarGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 20,
                BackColor = PanelBg
            };

            // Content
            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg,
                Padding = new Padding(20, 0, 20, 20)
            };

            Label tableLabel = new Label
            {
                Text = "MAINTENANCE REQUESTS",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = BrandBg,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Panel gridCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(0)
            };

            dgvMaintenance = CreateGrid();
            AddColumns();

            // Status coloring
            dgvMaintenance.CellPainting += DgvMaintenance_CellPainting;

            gridCard.Controls.Add(dgvMaintenance);

            content.Controls.Add(gridCard);
            content.Controls.Add(tableLabel);

            main.Controls.Add(content);
            main.Controls.Add(toolbarGap);
            main.Controls.Add(actionPanel);
            main.Controls.Add(headerGap);
            main.Controls.Add(header);

            Controls.Add(main);
        }

        // =========================================================
        // CREATE BUTTON
        // =========================================================

        private Button CreateButton(string text, int x, int y, int width, int height, Color backColor, Color foreColor)
        {
            Button button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        // =========================================================
        // CREATE GRID
        // =========================================================

        private DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                ReadOnly = true,
                MultiSelect = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 38,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 9F),
                ForeColor = BrandBg,
                GridColor = Color.FromArgb(235, 230, 223),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                AllowUserToOrderColumns = false,
                ShowCellToolTips = false
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = BrandBg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.False,
                Padding = new Padding(8, 0, 8, 0)
            };

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = BrandBg,
                SelectionBackColor = Color.FromArgb(232, 220, 199),
                SelectionForeColor = BrandBg,
                Padding = new Padding(8, 7, 8, 7),
                WrapMode = DataGridViewTriState.True,
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(247, 244, 239),
                ForeColor = BrandBg,
                SelectionBackColor = Color.FromArgb(232, 220, 199),
                SelectionForeColor = BrandBg,
                Padding = new Padding(8, 7, 8, 7),
                WrapMode = DataGridViewTriState.True
            };

            grid.RowTemplate.Height = 48;

            typeof(DataGridView)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(grid, true, null);

            return grid;
        }

        // =========================================================
        // GRID COLUMNS
        // =========================================================

        private void AddColumns()
        {
            dgvMaintenance.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TenantName",
                HeaderText = "Tenant",
                FillWeight = 16,
                MinimumWidth = 120,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    WrapMode = DataGridViewTriState.True,
                    Padding = new Padding(8, 7, 8, 7)
                }
            });

            dgvMaintenance.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Title",
                HeaderText = "Request",
                FillWeight = 20,
                MinimumWidth = 150,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    WrapMode = DataGridViewTriState.True,
                    Padding = new Padding(8, 7, 8, 7)
                }
            });

            dgvMaintenance.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DisplayDescription",
                HeaderText = "Description",
                FillWeight = 34,
                MinimumWidth = 220,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    WrapMode = DataGridViewTriState.True,
                    Padding = new Padding(8, 7, 8, 7),
                    Alignment = DataGridViewContentAlignment.TopLeft
                }
            });

            dgvMaintenance.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = "Priority",
                FillWeight = 10,
                MinimumWidth = 85,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    WrapMode = DataGridViewTriState.True,
                    Padding = new Padding(8, 7, 8, 7)
                }
            });

            dgvMaintenance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                DataPropertyName = "Status",
                HeaderText = "Status",
                FillWeight = 12,
                MinimumWidth = 105,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    WrapMode = DataGridViewTriState.True,
                    Padding = new Padding(8, 7, 8, 7)
                }
            });

            dgvMaintenance.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DateReported",
                HeaderText = "Date Reported",
                FillWeight = 14,
                MinimumWidth = 115,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "MMM dd, yyyy",
                    WrapMode = DataGridViewTriState.True,
                    Padding = new Padding(8, 7, 8, 7)
                }
            });
        }

        // =========================================================
        // STATUS CELL COLORING (only the STATUS cell)
        // =========================================================

        private void DgvMaintenance_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dgvMaintenance.Columns[e.ColumnIndex].Name != "Status" &&
                dgvMaintenance.Columns[e.ColumnIndex].DataPropertyName != "Status")
                return;

            e.PaintBackground(e.CellBounds, true);

            string status = e.FormattedValue?.ToString() ?? "";

            Color bg;
            Color fg;

            if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                bg = PendingBg;
                fg = PendingText;
            }
            else if (status.Equals("In Progress", StringComparison.OrdinalIgnoreCase))
            {
                bg = InProgressBg;
                fg = InProgressText;
            }
            else if (status.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
            {
                bg = ResolvedBg;
                fg = ResolvedText;
            }
            else if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                bg = CancelledBg;
                fg = CancelledText;
            }
            else
            {
                // fallback
                bg = Color.FromArgb(240, 240, 240);
                fg = BrandBg;
            }

            // Soft rounded pill
            int pillHeight = 24;
            int pillWidth = Math.Min(e.CellBounds.Width - 16, 100);

            Rectangle pill = new Rectangle(
                e.CellBounds.X + 8,
                e.CellBounds.Y + (e.CellBounds.Height - pillHeight) / 2,
                pillWidth,
                pillHeight);

            using (SolidBrush bgBrush = new SolidBrush(bg))
            using (SolidBrush fgBrush = new SolidBrush(fg))
            {
                // Rounded rectangle
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 8;
                    int d = radius * 2;
                    path.AddArc(pill.X, pill.Y, d, d, 180, 90);
                    path.AddArc(pill.Right - d, pill.Y, d, d, 270, 90);
                    path.AddArc(pill.Right - d, pill.Bottom - d, d, d, 0, 90);
                    path.AddArc(pill.X, pill.Bottom - d, d, d, 90, 90);
                    path.CloseFigure();

                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(bgBrush, path);
                }

                using (StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    e.Graphics.DrawString(
                        status,
                        new Font("Segoe UI", 8F, FontStyle.Bold),
                        fgBrush,
                        pill,
                        sf);
                }
            }

            e.Handled = true;
        }

        // =========================================================
        // LOAD MAINTENANCE
        // =========================================================

        private async Task LoadMaintenanceAsync()
        {
            try
            {
                btnRefresh.Enabled = false;

                List<StaffMaintenanceDto>? result =
                    await _apiService.GetAsync<List<StaffMaintenanceDto>>("api/Maintenance");

                _requests = result ?? new List<StaffMaintenanceDto>();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load maintenance requests.\n\n" + ex.Message,
                    "Maintenance",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        // =========================================================
        // FILTERS
        // =========================================================

        private void ApplyFilters()
        {
            if (dgvMaintenance == null) return;

            string search = txtSearch?.Text?.Trim() ?? string.Empty;
            string status = cmbStatusFilter?.SelectedItem?.ToString() ?? "All Status";

            IEnumerable<StaffMaintenanceDto> filtered = _requests;

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(request =>
                    (request.TenantName ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (request.Title ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    GetDisplayDescription(request.Description).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (request.Priority ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (request.Status ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.Equals(status, "All Status", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(request =>
                    string.Equals(request.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            List<StaffMaintenanceDto> displayList = filtered
                .OrderByDescending(request => request.DateReported)
                .ToList();

            dgvMaintenance.DataSource = null;
            dgvMaintenance.DataSource = displayList;

            lblRecordCount.Text = $"{displayList.Count} record{(displayList.Count == 1 ? "" : "s")}";

            dgvMaintenance.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            foreach (DataGridViewRow row in dgvMaintenance.Rows)
            {
                if (row.Height < 48)
                    row.Height = 48;
            }
        }

        // =========================================================
        // DESCRIPTION DISPLAY
        // =========================================================

        private static string GetDisplayDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return "—";

            string value = description.Trim();
            const string marker = "Additional Details:";

            int markerIndex = value.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

            if (markerIndex >= 0)
            {
                string answer = value.Substring(markerIndex + marker.Length).Trim();
                if (!string.IsNullOrWhiteSpace(answer))
                    return answer;
                return "—";
            }

            return "—";
        }

        // =========================================================
        // CREATE MAINTENANCE REQUEST
        // =========================================================

        private async Task CreateMaintenanceRequestAsync()
        {
            try
            {
                List<StaffMaintenanceTenantDto>? tenants =
                    await _apiService.GetAsync<List<StaffMaintenanceTenantDto>>("api/Tenants");

                if (tenants == null || tenants.Count == 0)
                {
                    MessageBox.Show("No tenants were found.", "New Maintenance Request",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (StaffMaintenanceTenantDto tenant in tenants)
                {
                    if (string.IsNullOrWhiteSpace(tenant.FullName))
                        tenant.FullName = $"{tenant.FirstName} {tenant.LastName}".Trim();
                }

                List<StaffMaintenanceTenantDto> activeTenants = tenants
                    .Where(tenant =>
                        !tenant.IsArchived &&
                        !tenant.ActualMoveOutDate.HasValue &&
                        (string.IsNullOrWhiteSpace(tenant.Status) ||
                         string.Equals(tenant.Status, "Active", StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(tenant => tenant.FullName)
                    .ToList();

                if (activeTenants.Count == 0)
                {
                    MessageBox.Show(
                        "There are no active tenants available for a maintenance request.",
                        "New Maintenance Request",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                using Form form = new Form
                {
                    Text = "New Maintenance Request",
                    StartPosition = FormStartPosition.CenterParent,
                    Size = new Size(540, 650),
                    MinimumSize = new Size(540, 650),
                    MaximumSize = new Size(540, 650),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = PanelBg
                };

                Label lblTenant = CreateFieldLabel("Tenant", 25, 20);
                ComboBox cmbTenant = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 9F),
                    Location = new Point(25, 45),
                    Width = 470,
                    Height = 32
                };
                cmbTenant.DisplayMember = nameof(StaffMaintenanceTenantDto.FullName);
                foreach (var t in activeTenants) cmbTenant.Items.Add(t);
                if (cmbTenant.Items.Count > 0) cmbTenant.SelectedIndex = 0;

                Label lblType = CreateFieldLabel("Maintenance Type", 25, 88);
                ComboBox cmbType = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 9F),
                    Location = new Point(25, 113),
                    Width = 470,
                    Height = 32
                };
                cmbType.Items.AddRange(new object[]
                {
                    "Electrical", "Plumbing", "Air Conditioning", "Furniture",
                    "Room", "Bathroom", "Internet", "Cleaning", "Other"
                });
                cmbType.SelectedIndex = 0;

                Label lblIssues = CreateFieldLabel("Issues", 25, 156);
                CheckedListBox checkedIssues = new CheckedListBox
                {
                    Location = new Point(25, 181),
                    Width = 470,
                    Height = 115,
                    Font = new Font("Segoe UI", 9F),
                    BorderStyle = BorderStyle.FixedSingle,
                    CheckOnClick = true
                };

                Label lblDetails = CreateFieldLabel("Additional Details", 25, 307);
                TextBox txtDetails = new TextBox
                {
                    Location = new Point(25, 332),
                    Width = 470,
                    Height = 75,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("Segoe UI", 9F)
                };

                Label lblPriority = CreateFieldLabel("Priority", 25, 418);
                ComboBox cmbPriority = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 9F),
                    Location = new Point(25, 443),
                    Width = 200,
                    Height = 32
                };
                cmbPriority.Items.AddRange(new object[] { "Low", "Normal", "High", "Urgent" });
                cmbPriority.SelectedItem = "Normal";

                Button btnCancel = CreateButton("Cancel", 260, 500, 105, 38,
                    Color.FromArgb(229, 222, 212), BrandBg);
                Button btnSend = CreateButton("Send Request", 375, 500, 120, 38, CtaColor, Color.White);

                btnCancel.Click += (_, _) =>
                {
                    form.DialogResult = DialogResult.Cancel;
                    form.Close();
                };

                cmbType.SelectedIndexChanged += (_, _) =>
                {
                    LoadIssueOptions(checkedIssues, cmbType.SelectedItem?.ToString() ?? string.Empty);
                };
                LoadIssueOptions(checkedIssues, cmbType.SelectedItem?.ToString() ?? string.Empty);

                btnSend.Click += async (_, _) =>
                {
                    if (cmbTenant.SelectedItem is not StaffMaintenanceTenantDto selectedTenant)
                    {
                        MessageBox.Show("Please select a tenant.", "New Maintenance Request",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string maintenanceType = cmbType.SelectedItem?.ToString() ?? string.Empty;

                    List<string> selectedIssues = checkedIssues.CheckedItems
                        .Cast<object>()
                        .Select(item => item?.ToString() ?? string.Empty)
                        .Where(item => !string.IsNullOrWhiteSpace(item))
                        .ToList();

                    if (string.IsNullOrWhiteSpace(maintenanceType))
                    {
                        MessageBox.Show("Please select a maintenance type.", "New Maintenance Request",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (selectedIssues.Count == 0)
                    {
                        MessageBox.Show("Please select at least one issue.", "New Maintenance Request",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string issuesText = string.Join(", ", selectedIssues);
                    string title = $"{maintenanceType} - {issuesText}";
                    string details = txtDetails.Text.Trim();

                    string description = string.IsNullOrWhiteSpace(details)
                        ? issuesText
                        : $"{issuesText}\n\nAdditional Details:\n{details}";

                    MaintenanceRequestDto request = new MaintenanceRequestDto
                    {
                        TenantId = selectedTenant.Id,
                        Title = title,
                        Description = description,
                        Priority = cmbPriority.SelectedItem?.ToString() ?? "Normal"
                    };

                    btnSend.Enabled = false;
                    try
                    {
                        bool success = await _apiService.PostAsync("api/Maintenance", request);
                        if (!success)
                        {
                            MessageBox.Show("The maintenance request could not be created.",
                                "New Maintenance Request", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        MessageBox.Show("Maintenance request sent successfully.",
                            "New Maintenance Request", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        form.DialogResult = DialogResult.OK;
                        form.Close();
                    }
                    finally
                    {
                        btnSend.Enabled = true;
                    }
                };

                form.Controls.Add(lblTenant);
                form.Controls.Add(cmbTenant);
                form.Controls.Add(lblType);
                form.Controls.Add(cmbType);
                form.Controls.Add(lblIssues);
                form.Controls.Add(checkedIssues);
                form.Controls.Add(lblDetails);
                form.Controls.Add(txtDetails);
                form.Controls.Add(lblPriority);
                form.Controls.Add(cmbPriority);
                form.Controls.Add(btnCancel);
                form.Controls.Add(btnSend);

                form.ShowDialog(this);

                if (form.DialogResult == DialogResult.OK)
                    await LoadMaintenanceAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to create the maintenance request.\n\n" + ex.Message,
                    "Maintenance",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // ISSUE OPTIONS
        // =========================================================

        private void LoadIssueOptions(CheckedListBox checkedIssues, string maintenanceType)
        {
            checkedIssues.Items.Clear();

            switch (maintenanceType)
            {
                case "Electrical":
                    checkedIssues.Items.Add("Light not working");
                    checkedIssues.Items.Add("Outlet not working");
                    checkedIssues.Items.Add("Switch problem");
                    checkedIssues.Items.Add("Power interruption");
                    checkedIssues.Items.Add("Other electrical issue");
                    break;

                case "Plumbing":
                    checkedIssues.Items.Add("Leaking faucet");
                    checkedIssues.Items.Add("Clogged sink");
                    checkedIssues.Items.Add("Clogged toilet");
                    checkedIssues.Items.Add("Low water pressure");
                    checkedIssues.Items.Add("Water leak");
                    checkedIssues.Items.Add("Other plumbing issue");
                    break;

                case "Air Conditioning":
                    checkedIssues.Items.Add("Not cooling");
                    checkedIssues.Items.Add("Water leaking");
                    checkedIssues.Items.Add("Unusual noise");
                    checkedIssues.Items.Add("Remote problem");
                    checkedIssues.Items.Add("Other aircon issue");
                    break;

                case "Furniture":
                    checkedIssues.Items.Add("Broken bed");
                    checkedIssues.Items.Add("Broken chair");
                    checkedIssues.Items.Add("Broken table");
                    checkedIssues.Items.Add("Cabinet problem");
                    checkedIssues.Items.Add("Other furniture issue");
                    break;

                case "Room":
                    checkedIssues.Items.Add("Door problem");
                    checkedIssues.Items.Add("Window problem");
                    checkedIssues.Items.Add("Ceiling problem");
                    checkedIssues.Items.Add("Wall damage");
                    checkedIssues.Items.Add("Floor problem");
                    break;

                case "Bathroom":
                    checkedIssues.Items.Add("Shower problem");
                    checkedIssues.Items.Add("Toilet problem");
                    checkedIssues.Items.Add("Sink problem");
                    checkedIssues.Items.Add("Drain problem");
                    checkedIssues.Items.Add("Other bathroom issue");
                    break;

                case "Internet":
                    checkedIssues.Items.Add("No connection");
                    checkedIssues.Items.Add("Slow connection");
                    checkedIssues.Items.Add("Unstable connection");
                    checkedIssues.Items.Add("Router problem");
                    break;

                case "Cleaning":
                    checkedIssues.Items.Add("Room cleaning");
                    checkedIssues.Items.Add("Bathroom cleaning");
                    checkedIssues.Items.Add("Trash removal");
                    checkedIssues.Items.Add("Common area cleaning");
                    break;

                case "Other":
                    checkedIssues.Items.Add("General maintenance");
                    checkedIssues.Items.Add("Other issue");
                    break;
            }
        }

        // =========================================================
        // FIELD LABEL
        // =========================================================

        private Label CreateFieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = BrandBg
            };
        }

        // =========================================================
        // DTOs
        // =========================================================

        public class StaffMaintenanceDto
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
            public StaffMaintenanceTenantDto? Tenant { get; set; }

            public string DisplayDescription => GetDisplayDescription(Description);
        }

        public class StaffMaintenanceTenantDto
        {
            public int Id { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime? ActualMoveOutDate { get; set; }
            public bool IsArchived { get; set; }

            public override string ToString()
            {
                if (!string.IsNullOrWhiteSpace(FullName))
                    return FullName;
                return $"{FirstName} {LastName}".Trim();
            }
        }

        public class MaintenanceRequestDto
        {
            public int TenantId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Priority { get; set; } = "Normal";
        }
    }
}
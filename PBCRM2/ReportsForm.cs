using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PBCRM2
{
    public class ReportsForm : Form
    {
        private readonly ApiService _apiService;

        private DataGridView dgvReport = null!;

        private ComboBox cboBranch = null!;
        private ComboBox cboReportType = null!;

        private Button btnGenerate = null!;
        private Button btnRefresh = null!;
        private Button btnExport = null!;
        private Button btnPrint = null!;

        private Label lblReportTitle = null!;
        private Label lblReportDescription = null!;

        // =========================================================
        // SUMMARY CARD LABELS
        // =========================================================

        private Label lblSummaryTitle1 = null!;
        private Label lblSummaryTitle2 = null!;
        private Label lblSummaryTitle3 = null!;
        private Label lblSummaryTitle4 = null!;

        private Label lblSummary1 = null!;
        private Label lblSummary2 = null!;
        private Label lblSummary3 = null!;
        private Label lblSummary4 = null!;

        // =========================================================
        // REPORT DATA
        // =========================================================

        private readonly List<JsonElement> _tenantReport = new();
        private readonly List<JsonElement> _billingReport = new();
        private readonly List<JsonElement> _genericReport = new();

        private List<string> _genericHeaders = new();

        private List<BranchDto> _branches = new();

        private int? _selectedBranchId;

        // =========================================================
        // PRINT
        // =========================================================

        private readonly PrintDocument _printDocument;
        private int _printRowIndex;
        private string _printTitle = "Report";

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

        private static readonly Color TextMuted =
            Color.FromArgb(120, 110, 100);

        private static readonly Color CardBorder =
            Color.FromArgb(230, 224, 215);

        private static readonly Color GridSelection =
            Color.FromArgb(232, 220, 199);

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ReportsForm()
            : this(new ApiService())
        {
        }

        public ReportsForm(
            ApiService apiService)
        {
            this._apiService =
                apiService
                ?? new ApiService();

            this._printDocument = new PrintDocument();
            this._printDocument.PrintPage +=
                this.PrintDocument_PrintPage;

            this.Text = "Reports";

            this.StartPosition =
                FormStartPosition.CenterScreen;

            this.Size =
                new Size(1250, 780);

            this.MinimumSize =
                new Size(1050, 680);

            this.BackColor = PanelBg;

            this.BuildInterface();

            this.Shown += async (_, _) =>
                await this.InitializeReportsAsync();
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

            // =====================================================
            // HEADER
            // =====================================================

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = BrandBg
            };

            this.lblReportTitle = new Label
            {
                Text = "REPORTS",
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    22F,
                    FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 22)
            };

            this.lblReportDescription = new Label
            {
                Text =
                    "View business reports, occupancy, billing, revenue, and tenant performance.",
                ForeColor = BrandAccent,
                Font = new Font(
                    "Segoe UI",
                    10F),
                AutoSize = true,
                Location = new Point(28, 67)
            };

            header.Controls.Add(
                this.lblReportTitle);

            header.Controls.Add(
                this.lblReportDescription);

            // =====================================================
            // HEADER GAP
            // =====================================================

            Panel headerGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = PanelBg
            };

            // =====================================================
            // ACTION TOOLBAR
            // =====================================================

            Panel actionPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = PanelBg
            };

            this.btnGenerate =
                this.CreateButton(
                    "Generate",
                    100);

            this.btnGenerate.Location =
                new Point(20, 10);

            this.btnGenerate.Click +=
                async (_, _) =>
                    await this.GenerateReportAsync();

            this.btnRefresh =
                this.CreateButton(
                    "Refresh",
                    95);

            this.btnRefresh.Location =
                new Point(130, 10);

            this.btnRefresh.Click +=
                async (_, _) =>
                    await this.RefreshReportAsync();

            this.btnExport =
                this.CreateIconButton(
                    "\uE896",
                    "Export CSV");

            this.btnExport.Location =
                new Point(240, 10);

            this.btnExport.Click +=
                (_, _) =>
                    this.ExportCurrentReport();

            this.btnPrint =
                this.CreateIconButton(
                    "\uE749",
                    "Print Report");

            this.btnPrint.Location =
                new Point(285, 10);

            this.btnPrint.Click +=
                (_, _) =>
                    this.PrintCurrentReport();

            this.btnExport.Enabled = false;
            this.btnPrint.Enabled = false;

            actionPanel.Controls.Add(
                this.btnGenerate);

            actionPanel.Controls.Add(
                this.btnRefresh);

            actionPanel.Controls.Add(
                this.btnExport);

            actionPanel.Controls.Add(
                this.btnPrint);

            // =====================================================
            // TOOLBAR GAP
            // =====================================================

            Panel toolbarGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 15,
                BackColor = PanelBg
            };

            // =====================================================
            // FILTER PANEL
            // =====================================================

            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = PanelBg
            };

            Label branchLabel = new Label
            {
                Text = "Branch:",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(20, 18)
            };

            this.cboBranch = new ComboBox
            {
                Location = new Point(75, 12),
                Width = 220,
                Height = 30,
                DropDownStyle =
                    ComboBoxStyle.DropDownList,
                Font = new Font(
                    "Segoe UI",
                    9F)
            };

            this.cboBranch.SelectedIndexChanged +=
                this.CboBranch_SelectedIndexChanged;

            Label reportLabel = new Label
            {
                Text = "Report:",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(320, 18)
            };

            this.cboReportType = new ComboBox
            {
                Location = new Point(375, 12),
                Width = 280,
                Height = 30,
                DropDownStyle =
                    ComboBoxStyle.DropDownList,
                Font = new Font(
                    "Segoe UI",
                    9F)
            };

            this.cboReportType.Items.Add(
                "Tenant Report");

            this.cboReportType.Items.Add(
                "Occupancy Report");

            this.cboReportType.Items.Add(
                "Billing & Payments Report");

            this.cboReportType.Items.Add(
                "Revenue Report");

            this.cboReportType.Items.Add(
                "Branch Performance Report");

            this.cboReportType.Items.Add(
                "Maintenance Report");

            this.cboReportType.Items.Add(
                "Renewal & Retention Report");

            this.cboReportType.Items.Add(
                "Feedback & Satisfaction Report");

            this.cboReportType.SelectedIndexChanged +=
                this.CboReportType_SelectedIndexChanged;

            filterPanel.Controls.Add(
                branchLabel);

            filterPanel.Controls.Add(
                this.cboBranch);

            filterPanel.Controls.Add(
                reportLabel);

            filterPanel.Controls.Add(
                this.cboReportType);

            // =====================================================
            // FILTER GAP
            // =====================================================

            Panel filterGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 15,
                BackColor = PanelBg
            };

            // =====================================================
            // SUMMARY
            // =====================================================

            Panel summaryPanel =
                this.CreateSummaryPanel();

            Panel summaryGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 15,
                BackColor = PanelBg
            };

            // =====================================================
            // CONTENT
            // =====================================================

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg,
                Padding =
                    new Padding(
                        20,
                        0,
                        20,
                        20)
            };

            Label reportTableLabel = new Label
            {
                Text = "REPORT RECORDS",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold),
                ForeColor = BrandBg,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            this.dgvReport =
                this.CreateGrid();

            this.dgvReport.Dock =
                DockStyle.Fill;

            content.Controls.Add(
                this.dgvReport);

            content.Controls.Add(
                reportTableLabel);

            // =====================================================
            // ADD TO MAIN
            // =====================================================

            main.Controls.Add(content);
            main.Controls.Add(summaryGap);
            main.Controls.Add(summaryPanel);
            main.Controls.Add(filterGap);
            main.Controls.Add(filterPanel);
            main.Controls.Add(toolbarGap);
            main.Controls.Add(actionPanel);
            main.Controls.Add(headerGap);
            main.Controls.Add(header);

            this.Controls.Add(main);
        }

        // =========================================================
        // SUMMARY PANEL
        // =========================================================

        private Panel CreateSummaryPanel()
        {
            Panel summaryPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 105,
                BackColor = PanelBg,
                Padding =
                    new Padding(
                        20,
                        0,
                        20,
                        0)
            };

            TableLayoutPanel table =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 4,
                    RowCount = 1,
                    BackColor = PanelBg,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty
                };

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    25F));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    25F));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    25F));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    25F));

            Panel card1 =
                this.CreateSummaryCard(
                    "TOTAL",
                    "0",
                    out this.lblSummaryTitle1,
                    out this.lblSummary1);

            Panel card2 =
                this.CreateSummaryCard(
                    "ACTIVE",
                    "0",
                    out this.lblSummaryTitle2,
                    out this.lblSummary2);

            Panel card3 =
                this.CreateSummaryCard(
                    "PENDING",
                    "0",
                    out this.lblSummaryTitle3,
                    out this.lblSummary3);

            Panel card4 =
                this.CreateSummaryCard(
                    "OTHER",
                    "0",
                    out this.lblSummaryTitle4,
                    out this.lblSummary4);

            table.Controls.Add(
                card1,
                0,
                0);

            table.Controls.Add(
                card2,
                1,
                0);

            table.Controls.Add(
                card3,
                2,
                0);

            table.Controls.Add(
                card4,
                3,
                0);

            summaryPanel.Controls.Add(table);

            return summaryPanel;
        }

        // =========================================================
        // SUMMARY CARD
        // =========================================================

        private Panel CreateSummaryCard(
            string title,
            string value,
            out Label titleLabel,
            out Label valueLabel)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin =
                    new Padding(
                        0,
                        0,
                        10,
                        0),
                Padding =
                    new Padding(15)
            };

            card.BorderStyle =
                BorderStyle.FixedSingle;

            TableLayoutPanel layout =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    BackColor = Color.White,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty
                };

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    62F));

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    38F));

            titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                ForeColor = TextMuted,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            valueLabel = new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                Font = new Font(
                    "Segoe UI",
                    20F,
                    FontStyle.Bold),
                ForeColor = BrandBg,
                TextAlign =
                    ContentAlignment.MiddleRight
            };

            layout.Controls.Add(
                titleLabel,
                0,
                0);

            layout.Controls.Add(
                valueLabel,
                1,
                0);

            card.Controls.Add(layout);

            return card;
        }

        // =========================================================
        // CREATE GRID
        // =========================================================

        private DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle =
                    BorderStyle.FixedSingle,

                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,

                ReadOnly = true,

                MultiSelect = false,

                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,

                AutoGenerateColumns = false,

                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,

                RowHeadersVisible = false,

                EnableHeadersVisualStyles = false,

                ColumnHeadersHeight = 34,

                ShowCellToolTips = false
            };

            grid.RowTemplate.Height = 30;

            typeof(DataGridView)
                .GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(
                    grid,
                    true,
                    null);

            grid.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = BrandBg,
                    ForeColor = Color.White,
                    Font = new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold),
                    Alignment =
                        DataGridViewContentAlignment.MiddleLeft,
                    WrapMode =
                        DataGridViewTriState.False
                };

            grid.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    Font = new Font(
                        "Segoe UI",
                        8.5F),

                    BackColor = Color.White,

                    ForeColor = BrandBg,

                    SelectionBackColor =
                        GridSelection,

                    SelectionForeColor =
                        BrandBg,

                    Alignment =
                        DataGridViewContentAlignment.MiddleLeft,

                    WrapMode =
                        DataGridViewTriState.False
                };

            grid.AlternatingRowsDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor =
                        Color.FromArgb(
                            247,
                            244,
                            239)
                };

            return grid;
        }

        // =========================================================
        // CREATE STANDARD BUTTON
        // =========================================================

        private Button CreateButton(
            string text,
            int width)
        {
            Button button = new Button
            {
                Text = text,
                Width = width,
                Height = 32,

                FlatStyle =
                    FlatStyle.Flat,

                BackColor =
                    CtaColor,

                ForeColor =
                    Color.White,

                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),

                Cursor =
                    Cursors.Hand,

                UseVisualStyleBackColor =
                    false
            };

            button.FlatAppearance.BorderSize = 0;

            return button;
        }

        // =========================================================
        // CREATE ICON BUTTON
        // =========================================================

        private Button CreateIconButton(
            string icon,
            string tooltipText)
        {
            Button button = new Button
            {
                Text = icon,

                Width = 38,
                Height = 32,

                FlatStyle =
                    FlatStyle.Flat,

                BackColor =
                    CtaColor,

                ForeColor =
                    Color.White,

                Font = new Font(
                    "Segoe MDL2 Assets",
                    12F),

                Cursor =
                    Cursors.Hand,

                UseVisualStyleBackColor =
                    false,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                AccessibleName =
                    tooltipText
            };

            button.FlatAppearance.BorderSize = 0;

            ToolTip tooltip = new ToolTip
            {
                InitialDelay = 300,
                ReshowDelay = 100,
                AutoPopDelay = 3000
            };

            tooltip.SetToolTip(
                button,
                tooltipText);

            return button;
        }

        // =========================================================
        // INITIALIZE
        // =========================================================

        private async Task InitializeReportsAsync()
        {
            try
            {
                this.btnGenerate.Enabled = false;
                this.btnRefresh.Enabled = false;
                this.btnExport.Enabled = false;
                this.btnPrint.Enabled = false;

                await this.LoadBranchesAsync();

                this.ConfigureBranchAccess();

                if (this.cboReportType.Items.Count > 0)
                {
                    this.cboReportType.SelectedIndex = 0;
                }

                if (this._selectedBranchId.HasValue)
                {
                    await this.GenerateReportAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to initialize Reports.\n\n" +
                    ex.Message,
                    "Reports",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                this.UpdateMainButtonStates();
            }
        }

        // =========================================================
        // LOAD BRANCHES
        // =========================================================

        private async Task LoadBranchesAsync()
        {
            this._branches.Clear();
            this.cboBranch.Items.Clear();

            if (!IsAdminRole())
            {
                return;
            }

            if (!ApiService.CurrentCompanyId.HasValue)
            {
                return;
            }

            // Always silent — never popup 403 while opening Reports
            List<BranchDto>? branches =
                await this._apiService
                    .GetSilentAsync<List<BranchDto>>(
                        $"api/Users/companies/{ApiService.CurrentCompanyId.Value}/branches");

            if (branches == null)
            {
                branches = new List<BranchDto>();
            }

            this._branches =
                branches
                    .GroupBy(x => x.Id)
                    .Select(x => x.First())
                    .OrderBy(x => x.BranchName)
                    .ToList();
        }

        // =========================================================
        // CONFIGURE ROLE ACCESS
        // =========================================================

        private void ConfigureBranchAccess()
        {
            if (IsAdminRole())
            {
                this.ConfigureAdminAccess();
                return;
            }

            if (IsManagerRole())
            {
                this.ConfigureSingleBranchAccess();
                return;
            }

            if (IsStaffRole())
            {
                this.ConfigureSingleBranchAccess();
                return;
            }

            this.cboBranch.Enabled = false;
            this.cboReportType.Enabled = false;

            this._selectedBranchId = null;

            this.btnGenerate.Enabled = false;
        }

        // =========================================================
        // ADMIN ACCESS
        // =========================================================

        private void ConfigureAdminAccess()
        {
            this.cboBranch.Items.Clear();

            foreach (BranchDto branch in this._branches)
            {
                this.cboBranch.Items.Add(branch);
            }

            if (this._branches.Count == 0)
            {
                // Admin can still run company-wide reports without a branch list
                this.cboBranch.Items.Add(
                    new BranchDto
                    {
                        Id = 0,
                        BranchName = "All Branches"
                    });

                this.cboBranch.SelectedIndex = 0;
                this.cboBranch.Enabled = false;
                this.cboReportType.Enabled = true;
                this._selectedBranchId = 0;
                this.btnGenerate.Enabled = true;
                return;
            }

            this.cboBranch.Enabled = true;
            this.cboReportType.Enabled = true;

            this.cboBranch.SelectedIndex = 0;

            this._selectedBranchId =
                this._branches[0].Id;

            this.btnGenerate.Enabled = true;
        }

        // =========================================================
        // MANAGER / STAFF ACCESS
        // =========================================================

        private void ConfigureSingleBranchAccess()
        {
            int? currentBranchId =
                ApiService.CurrentBranchId;

            this.cboBranch.Items.Clear();

            if (!currentBranchId.HasValue)
            {
                this._selectedBranchId = null;

                this.cboBranch.Enabled = false;
                this.cboReportType.Enabled = false;
                this.btnGenerate.Enabled = false;

                MessageBox.Show(
                    "No branch is assigned to the current account.",
                    "Reports",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            this._selectedBranchId =
                currentBranchId.Value;

            BranchDto? matchingBranch =
                this._branches.FirstOrDefault(
                    x => x.Id == currentBranchId.Value);

            if (matchingBranch != null)
            {
                this.cboBranch.Items.Add(
                    matchingBranch);
            }
            else
            {
                this.cboBranch.Items.Add(
                    new BranchDto
                    {
                        Id = currentBranchId.Value,
                        BranchName = "Current Branch"
                    });
            }

            this.cboBranch.SelectedIndex = 0;

            this.cboBranch.Enabled = false;

            this.cboReportType.Enabled = true;
            this.btnGenerate.Enabled = true;
        }

        // =========================================================
        // BRANCH CHANGED
        // =========================================================

        private void CboBranch_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (!IsAdminRole())
            {
                return;
            }

            if (this.cboBranch.SelectedItem
                is BranchDto branch)
            {
                this._selectedBranchId =
                    branch.Id;

                this.cboReportType.Enabled = true;
                this.btnGenerate.Enabled = true;

                this.ClearReport();
            }
        }

        // =========================================================
        // REPORT TYPE CHANGED
        // =========================================================

        private void CboReportType_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            this.ClearReport();

            if (this._selectedBranchId.HasValue)
            {
                this.btnGenerate.Enabled = true;
            }
        }

        // =========================================================
        // GENERATE REPORT
        // =========================================================

        private async Task GenerateReportAsync()
        {
            if (!this._selectedBranchId.HasValue)
            {
                MessageBox.Show(
                    "Please select a branch first.",
                    "Reports",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // branchId 0 = All Branches is allowed for Admin

            if (this.cboReportType.SelectedItem == null)
            {
                return;
            }

            this.btnGenerate.Enabled = false;
            this.btnRefresh.Enabled = false;
            this.btnExport.Enabled = false;
            this.btnPrint.Enabled = false;

            this.Cursor = Cursors.WaitCursor;

            try
            {
                string reportType =
                    this.cboReportType.SelectedItem
                        .ToString() ?? string.Empty;

                switch (reportType)
                {
                    case "Tenant Report":
                        await this.GenerateTenantReportAsync();
                        break;

                    case "Occupancy Report":
                        await this.GenerateOccupancyReportAsync();
                        break;

                    case "Billing & Payments Report":
                        await this.GenerateBillingReportAsync();
                        break;

                    case "Revenue Report":
                        await this.GenerateRevenueReportAsync();
                        break;

                    case "Branch Performance Report":
                        await this.GenerateBranchPerformanceReportAsync();
                        break;

                    case "Maintenance Report":
                        await this.GenerateMaintenanceReportAsync();
                        break;

                    case "Renewal & Retention Report":
                        await this.GenerateRenewalReportAsync();
                        break;

                    case "Feedback & Satisfaction Report":
                        await this.GenerateFeedbackReportAsync();
                        break;

                    default:
                        this.ClearReport();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to generate the report.\n\n" +
                    ex.Message,
                    "Reports",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;

                this.UpdateMainButtonStates();
            }
        }

        // =========================================================
        // REFRESH
        // =========================================================

        private async Task RefreshReportAsync()
        {
            try
            {
                this.btnRefresh.Enabled = false;

                int? previousBranchId =
                    this._selectedBranchId;

                if (IsAdminRole())
                {
                    await this.LoadBranchesAsync();

                    this.ConfigureBranchAccess();

                    if (previousBranchId.HasValue)
                    {
                        BranchDto? branch =
                            this._branches.FirstOrDefault(
                                x => x.Id ==
                                     previousBranchId.Value);

                        if (branch != null)
                        {
                            for (
                                int i = 0;
                                i < this.cboBranch.Items.Count;
                                i++)
                            {
                                if (this.cboBranch.Items[i]
                                    is BranchDto item &&
                                    item.Id == branch.Id)
                                {
                                    this.cboBranch.SelectedIndex = i;
                                    this._selectedBranchId =
                                        branch.Id;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.ConfigureSingleBranchAccess();
                }

                if (this._selectedBranchId.HasValue)
                {
                    await this.GenerateReportAsync();
                }
                else
                {
                    this.ClearReport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to refresh the report.\n\n" +
                    ex.Message,
                    "Reports",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                this.UpdateMainButtonStates();
            }
        }

        // =========================================================
        // TENANT REPORT
        // =========================================================

        private async Task GenerateTenantReportAsync()
        {
            this.ClearReport();

            this.lblReportTitle.Text =
                "TENANT REPORT";

            this.lblReportDescription.Text =
                "View tenant records and current occupancy status.";

            this.SetSummaryTitles(
                "TOTAL TENANTS",
                "ACTIVE",
                "PENDING",
                "MOVED OUT");

            // Prefer dedicated report endpoint (handles Admin/Manager/Staff roles).
            // Fall back to api/Tenants if the report route is unavailable.
            List<JsonElement> tenants =
                await this.GetTenantReportDataAsync();

            tenants =
                this.FilterByBranch(
                    tenants);

            this._tenantReport.Clear();
            this._tenantReport.AddRange(tenants);

            this.CreateTenantTable();

            int total =
                tenants.Count;

            int active =
                tenants.Count(
                    IsTenantActive);

            int movedOut =
                tenants.Count(
                    IsTenantMovedOut);

            int pending =
                tenants.Count(
                    x =>
                        GetString(
                            x,
                            "status",
                            "Status")
                        .Equals(
                            "Pending",
                            StringComparison.OrdinalIgnoreCase));

            this.lblSummary1.Text =
                total.ToString();

            this.lblSummary2.Text =
                active.ToString();

            this.lblSummary3.Text =
                pending.ToString();

            this.lblSummary4.Text =
                movedOut.ToString();

            this.UpdateReportActions(
                tenants.Count > 0);
        }

        // =========================================================
        // TENANT TABLE
        // =========================================================

        private void CreateTenantTable()
        {
            this.dgvReport.Columns.Clear();
            this.dgvReport.DataSource = null;

            this.AddTextColumn(
                "Tenant",
                "Tenant",
                "Tenant");

            this.AddTextColumn(
                "Email",
                "Email",
                "Email");

            this.AddTextColumn(
                "Phone",
                "Phone",
                "Phone");

            this.AddTextColumn(
                "Branch",
                "Branch",
                "Branch");

            this.AddTextColumn(
                "Room",
                "Room",
                "Room");

            this.AddTextColumn(
                "Bed",
                "Bed",
                "Bed");

            this.AddTextColumn(
                "Status",
                "Status",
                "Status");

            this.AddTextColumn(
                "MoveOutDate",
                "Move Out",
                "MoveOut");

            foreach (JsonElement tenant
                     in this._tenantReport)
            {
                string firstName =
                    GetString(
                        tenant,
                        "firstName",
                        "FirstName");

                string lastName =
                    GetString(
                        tenant,
                        "lastName",
                        "LastName");

                string fullName =
                    GetString(
                        tenant,
                        "fullName",
                        "FullName");

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    fullName =
                        $"{firstName} {lastName}"
                        .Trim();
                }

                string branchName =
                    GetNestedString(
                        tenant,
                        "branch",
                        "Branch",
                        "branchName",
                        "BranchName");

                if (string.IsNullOrWhiteSpace(branchName))
                {
                    branchName =
                        GetString(
                            tenant,
                            "branchName",
                            "BranchName");
                }

                string room =
                    GetNestedString(
                        tenant,
                        "room",
                        "Room",
                        "roomNumber",
                        "RoomNumber");

                if (string.IsNullOrWhiteSpace(room))
                {
                    room =
                        GetString(
                            tenant,
                            "roomNumber",
                            "RoomNumber");
                }

                string bed =
                    GetNestedString(
                        tenant,
                        "bed",
                        "Bed",
                        "bedNumber",
                        "BedNumber");

                if (string.IsNullOrWhiteSpace(bed))
                {
                    bed =
                        GetString(
                            tenant,
                            "bedNumber",
                            "BedNumber");
                }

                string status =
                    GetString(
                        tenant,
                        "status",
                        "Status");

                string moveOut =
                    GetDateString(
                        tenant,
                        "actualMoveOutDate",
                        "ActualMoveOutDate");

                this.dgvReport.Rows.Add(
                    fullName,
                    GetString(
                        tenant,
                        "email",
                        "Email"),
                    GetString(
                        tenant,
                        "phoneNumber",
                        "PhoneNumber",
                        "phone",
                        "Phone"),
                    branchName,
                    room,
                    bed,
                    status,
                    moveOut);
            }
        }

        // =========================================================
        // OCCUPANCY REPORT
        // =========================================================

        private async Task GenerateOccupancyReportAsync()
        {
            this.ClearReport();

            this.lblReportTitle.Text =
                "OCCUPANCY REPORT";

            this.lblReportDescription.Text =
                "View room capacity, occupied beds, available beds, and maintenance beds.";

            this.SetSummaryTitles(
                "TOTAL ROOMS",
                "OCCUPIED",
                "AVAILABLE",
                "MAINTENANCE");

            List<JsonElement> rooms =
                await this.GetListAsync(
                    "api/Rooms");

            rooms =
                this.FilterByBranch(
                    rooms);

            this.dgvReport.Columns.Clear();

            this.AddTextColumn(
                "Branch",
                "Branch",
                "Branch");

            this.AddTextColumn(
                "Room",
                "Room",
                "Room");

            this.AddTextColumn(
                "Type",
                "Room Type",
                "Type");

            this.AddNumberColumn(
                "Capacity",
                "Capacity");

            this.AddNumberColumn(
                "Beds",
                "Beds");

            this.AddNumberColumn(
                "Occupied",
                "Occupied");

            this.AddNumberColumn(
                "Available",
                "Available");

            this.AddNumberColumn(
                "Maintenance",
                "Maintenance");

            this.AddNumberColumn(
                "Pending",
                "Pending Assignments");

            int totalOccupied = 0;
            int totalAvailable = 0;
            int totalMaintenance = 0;

            foreach (JsonElement room in rooms)
            {
                int capacity =
                    GetInt(
                        room,
                        "capacity",
                        "Capacity");

                int beds =
                    GetInt(
                        room,
                        "bedCount",
                        "BedCount");

                int occupied =
                    GetInt(
                        room,
                        "occupiedBeds",
                        "OccupiedBeds");

                int available =
                    GetInt(
                        room,
                        "availableBeds",
                        "AvailableBeds");

                int maintenance =
                    GetInt(
                        room,
                        "maintenanceBeds",
                        "MaintenanceBeds");

                int pending =
                    GetInt(
                        room,
                        "pendingAssignments",
                        "PendingAssignments");

                string branch =
                    GetString(
                        room,
                        "branchName",
                        "BranchName");

                string roomNumber =
                    GetString(
                        room,
                        "roomNumber",
                        "RoomNumber");

                string roomType =
                    GetString(
                        room,
                        "roomType",
                        "RoomType");

                this.dgvReport.Rows.Add(
                    branch,
                    roomNumber,
                    roomType,
                    capacity,
                    beds,
                    occupied,
                    available,
                    maintenance,
                    pending);

                totalOccupied += occupied;
                totalAvailable += available;
                totalMaintenance += maintenance;
            }

            this.lblSummary1.Text =
                rooms.Count.ToString();

            this.lblSummary2.Text =
                totalOccupied.ToString();

            this.lblSummary3.Text =
                totalAvailable.ToString();

            this.lblSummary4.Text =
                totalMaintenance.ToString();

            this.UpdateReportActions(
                rooms.Count > 0);
        }

        // =========================================================
        // BILLING REPORT
        // =========================================================

        private async Task GenerateBillingReportAsync()
        {
            this.ClearReport();

            this.lblReportTitle.Text =
                "BILLING & PAYMENTS REPORT";

            this.lblReportDescription.Text =
                "View billing amounts, payments, balances, and payment status.";

            this.SetSummaryTitles(
                "TOTAL RECORDS",
                "TOTAL PAID",
                "PENDING",
                "OUTSTANDING");

            List<JsonElement> billing =
                await this.GetListAsync(
                    "api/Billing");

            billing =
                await this.FilterBillingByBranchAsync(
                    billing);

            this._billingReport.Clear();
            this._billingReport.AddRange(billing);

            this.CreateBillingTable();

            decimal totalPaid =
                billing.Sum(
                    x =>
                        GetDecimal(
                            x,
                            "totalPaid",
                            "TotalPaid",
                            "paidAmount",
                            "PaidAmount"));

            decimal outstanding =
                billing.Sum(
                    x =>
                        GetDecimal(
                            x,
                            "outstandingBalance",
                            "OutstandingBalance",
                            "balance",
                            "Balance"));

            int pending =
                billing.Count(
                    x =>
                        GetString(
                            x,
                            "status",
                            "Status")
                        .Equals(
                            "Pending",
                            StringComparison.OrdinalIgnoreCase));

            this.lblSummary1.Text =
                billing.Count.ToString();

            this.lblSummary2.Text =
                totalPaid.ToString("N2");

            this.lblSummary3.Text =
                pending.ToString();

            this.lblSummary4.Text =
                outstanding.ToString("N2");

            this.UpdateReportActions(
                billing.Count > 0);
        }

        // =========================================================
        // BILLING TABLE
        // =========================================================

        private void CreateBillingTable()
        {
            this.dgvReport.Columns.Clear();
            this.dgvReport.DataSource = null;

            this.AddTextColumn(
                "Tenant",
                "Tenant",
                "Tenant");

            this.AddTextColumn(
                "Room",
                "Room",
                "Room");

            this.AddTextColumn(
                "Period",
                "Billing Period",
                "Period");

            this.AddTextColumn(
                "DueDate",
                "Due Date",
                "DueDate");

            this.AddTextColumn(
                "TotalDue",
                "Total Due",
                "TotalDue");

            this.AddTextColumn(
                "Paid",
                "Paid",
                "Paid");

            this.AddTextColumn(
                "Balance",
                "Outstanding",
                "Balance");

            this.AddTextColumn(
                "Status",
                "Status",
                "Status");

            foreach (JsonElement item
                     in this._billingReport)
            {
                string tenantName =
                    GetString(
                        item,
                        "tenantName",
                        "TenantName");

                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    tenantName =
                        GetNestedString(
                            item,
                            "tenant",
                            "Tenant",
                            "fullName",
                            "FullName");
                }

                this.dgvReport.Rows.Add(
                    tenantName,

                    GetString(
                        item,
                        "roomNumber",
                        "RoomNumber"),

                    GetString(
                        item,
                        "billingPeriod",
                        "BillingPeriod"),

                    GetDateString(
                        item,
                        "dueDate",
                        "DueDate"),

                    GetDecimal(
                        item,
                        "totalDue",
                        "TotalDue",
                        "amount",
                        "Amount")
                        .ToString("N2"),

                    GetDecimal(
                        item,
                        "totalPaid",
                        "TotalPaid",
                        "paidAmount",
                        "PaidAmount")
                        .ToString("N2"),

                    GetDecimal(
                        item,
                        "outstandingBalance",
                        "OutstandingBalance",
                        "balance",
                        "Balance")
                        .ToString("N2"),

                    GetString(
                        item,
                        "status",
                        "Status"));
            }
        }

        // =========================================================
        // REVENUE REPORT
        // =========================================================

        private async Task GenerateRevenueReportAsync()
        {
            this.ClearReport();

            this.lblReportTitle.Text =
                "REVENUE REPORT";

            this.lblReportDescription.Text =
                "View collected revenue and outstanding amounts.";

            this.SetSummaryTitles(
                "TOTAL RECORDS",
                "REVENUE",
                "TOTAL DUE",
                "OUTSTANDING");

            List<JsonElement> billing =
                await this.GetListAsync(
                    "api/Billing");

            billing =
                await this.FilterBillingByBranchAsync(
                    billing);

            decimal totalRevenue =
                billing.Sum(
                    x =>
                        GetDecimal(
                            x,
                            "totalPaid",
                            "TotalPaid",
                            "paidAmount",
                            "PaidAmount"));

            decimal totalDue =
                billing.Sum(
                    x =>
                        GetDecimal(
                            x,
                            "totalDue",
                            "TotalDue",
                            "amount",
                            "Amount"));

            decimal outstanding =
                billing.Sum(
                    x =>
                        GetDecimal(
                            x,
                            "outstandingBalance",
                            "OutstandingBalance",
                            "balance",
                            "Balance"));

            this.dgvReport.Columns.Clear();

            this.AddTextColumn(
                "Tenant",
                "Tenant",
                "Tenant");

            this.AddTextColumn(
                "PaymentDate",
                "Payment Date",
                "PaymentDate");

            this.AddTextColumn(
                "Amount",
                "Amount Paid",
                "Amount");

            this.AddTextColumn(
                "PaymentMethod",
                "Payment Method",
                "PaymentMethod");

            this.AddTextColumn(
                "Reference",
                "Reference",
                "Reference");

            this.AddTextColumn(
                "Status",
                "Status",
                "Status");

            foreach (JsonElement item
                     in billing)
            {
                string tenantName =
                    GetString(
                        item,
                        "tenantName",
                        "TenantName");

                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    tenantName =
                        GetNestedString(
                            item,
                            "tenant",
                            "Tenant",
                            "fullName",
                            "FullName");
                }

                decimal amount =
                    GetDecimal(
                        item,
                        "totalPaid",
                        "TotalPaid",
                        "paidAmount",
                        "PaidAmount");

                if (amount == 0)
                {
                    amount =
                        GetDecimal(
                            item,
                            "amount",
                            "Amount");
                }

                this.dgvReport.Rows.Add(
                    tenantName,

                    GetDateString(
                        item,
                        "paymentDate",
                        "PaymentDate"),

                    amount.ToString("N2"),

                    GetString(
                        item,
                        "paymentMethod",
                        "PaymentMethod"),

                    GetString(
                        item,
                        "referenceNumber",
                        "ReferenceNumber"),

                    GetString(
                        item,
                        "status",
                        "Status"));
            }

            this.lblSummary1.Text =
                billing.Count.ToString();

            this.lblSummary2.Text =
                totalRevenue.ToString("N2");

            this.lblSummary3.Text =
                totalDue.ToString("N2");

            this.lblSummary4.Text =
                outstanding.ToString("N2");

            this.UpdateReportActions(
                billing.Count > 0);
        }

        // =========================================================
        // BRANCH PERFORMANCE
        // =========================================================

        private async Task GenerateBranchPerformanceReportAsync()
        {
            this.ClearReport();

            this.lblReportTitle.Text =
                "BRANCH PERFORMANCE REPORT";

            this.lblReportDescription.Text =
                "View tenant, room, occupancy, and revenue performance.";

            this.SetSummaryTitles(
                "TOTAL TENANTS",
                "ACTIVE TENANTS",
                "OCCUPIED BEDS",
                "REVENUE");

            List<JsonElement> tenants =
                await this.GetListAsync(
                    "api/Tenants");

            List<JsonElement> rooms =
                await this.GetListAsync(
                    "api/Rooms");

            List<JsonElement> billing =
                await this.GetListAsync(
                    "api/Billing");

            tenants =
                this.FilterByBranch(
                    tenants);

            rooms =
                this.FilterByBranch(
                    rooms);

            billing =
                await this.FilterBillingByBranchAsync(
                    billing);

            int activeTenants =
                tenants.Count(
                    IsTenantActive);

            int occupiedBeds =
                rooms.Sum(
                    x =>
                        GetInt(
                            x,
                            "occupiedBeds",
                            "OccupiedBeds"));

            decimal revenue =
                billing.Sum(
                    x =>
                        GetDecimal(
                            x,
                            "totalPaid",
                            "TotalPaid",
                            "paidAmount",
                            "PaidAmount"));

            this.dgvReport.Columns.Clear();

            this.AddTextColumn(
                "Metric",
                "Metric",
                "Metric");

            this.AddTextColumn(
                "Value",
                "Value",
                "Value");

            this.dgvReport.Rows.Add(
                "Total Tenants",
                tenants.Count);

            this.dgvReport.Rows.Add(
                "Active Tenants",
                activeTenants);

            this.dgvReport.Rows.Add(
                "Total Rooms",
                rooms.Count);

            this.dgvReport.Rows.Add(
                "Occupied Beds",
                occupiedBeds);

            this.dgvReport.Rows.Add(
                "Collected Revenue",
                revenue.ToString("N2"));

            this.lblSummary1.Text =
                tenants.Count.ToString();

            this.lblSummary2.Text =
                activeTenants.ToString();

            this.lblSummary3.Text =
                occupiedBeds.ToString();

            this.lblSummary4.Text =
                revenue.ToString("N2");

            this.UpdateReportActions(
                this.dgvReport.Rows.Count > 0);
        }

        // =========================================================
        // MAINTENANCE REPORT
        // =========================================================

        private async Task GenerateMaintenanceReportAsync()
        {
            this.ClearReport();

            this.lblReportTitle.Text =
                "MAINTENANCE REPORT";

            this.lblReportDescription.Text =
                "View maintenance requests and their current status.";

            this.SetSummaryTitles(
                "TOTAL REQUESTS",
                "PENDING",
                "RESOLVED",
                "HIGH PRIORITY");

            List<JsonElement> requests =
                await this.GetListAsync(
                    "api/MaintenanceRequests");

            if (this._selectedBranchId.HasValue)
            {
                List<JsonElement> tenants =
                    await this.GetListAsync(
                        "api/Tenants");

                HashSet<int> branchTenantIds =
                    tenants
                        .Where(
                            x =>
                                GetInt(
                                    x,
                                    "branchId",
                                    "BranchId") ==
                                this._selectedBranchId.Value)
                        .Select(
                            x =>
                                GetInt(
                                    x,
                                    "id",
                                    "Id",
                                    "tenantId",
                                    "TenantId"))
                        .Where(
                            x => x > 0)
                        .ToHashSet();

                requests =
                    requests
                        .Where(
                            x =>
                                branchTenantIds.Contains(
                                    GetInt(
                                        x,
                                        "tenantId",
                                        "TenantId")))
                        .ToList();
            }

            this.dgvReport.Columns.Clear();

            this.AddTextColumn(
                "Tenant",
                "Tenant",
                "Tenant");

            this.AddTextColumn(
                "Title",
                "Title",
                "Title");

            this.AddTextColumn(
                "Priority",
                "Priority",
                "Priority");

            this.AddTextColumn(
                "Status",
                "Status",
                "Status");

            this.AddTextColumn(
                "DateReported",
                "Date Reported",
                "DateReported");

            this.AddTextColumn(
                "DateResolved",
                "Date Resolved",
                "DateResolved");

            foreach (JsonElement item
                     in requests)
            {
                string tenantName =
                    GetString(
                        item,
                        "tenantName",
                        "TenantName");

                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    tenantName =
                        GetNestedString(
                            item,
                            "tenant",
                            "Tenant",
                            "fullName",
                            "FullName");
                }

                this.dgvReport.Rows.Add(
                    tenantName,

                    GetString(
                        item,
                        "title",
                        "Title"),

                    GetString(
                        item,
                        "priority",
                        "Priority"),

                    GetString(
                        item,
                        "status",
                        "Status"),

                    GetDateString(
                        item,
                        "dateReported",
                        "DateReported"),

                    GetDateString(
                        item,
                        "dateResolved",
                        "DateResolved"));
            }

            int pending =
                requests.Count(
                    x =>
                        GetString(
                            x,
                            "status",
                            "Status")
                        .Equals(
                            "Pending",
                            StringComparison.OrdinalIgnoreCase));

            int resolved =
                requests.Count(
                    x =>
                    {
                        string status =
                            GetString(
                                x,
                                "status",
                                "Status");

                        return
                            status.Equals(
                                "Resolved",
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            status.Equals(
                                "Completed",
                                StringComparison.OrdinalIgnoreCase);
                    });

            int highPriority =
                requests.Count(
                    x =>
                        GetString(
                            x,
                            "priority",
                            "Priority")
                        .Equals(
                            "High",
                            StringComparison.OrdinalIgnoreCase));

            this.lblSummary1.Text =
                requests.Count.ToString();

            this.lblSummary2.Text =
                pending.ToString();

            this.lblSummary3.Text =
                resolved.ToString();

            this.lblSummary4.Text =
                highPriority.ToString();

            this.UpdateReportActions(
                requests.Count > 0);
        }

        // =========================================================
        // RENEWAL REPORT
        // =========================================================

        private async Task GenerateRenewalReportAsync()
        {
            this.ClearReport();

            this.lblReportTitle.Text =
                "RENEWAL & RETENTION REPORT";

            this.lblReportDescription.Text =
                "View tenant renewal activity and retention information.";

            this.SetSummaryTitles(
                "TOTAL RENEWALS",
                "ACTIVE",
                "COMPLETED",
                "RETENTION");

            List<JsonElement> renewals =
                await this.GetListAsync(
                    "api/Renewals");

            if (this._selectedBranchId.HasValue)
            {
                List<JsonElement> tenants =
                    await this.GetListAsync(
                        "api/Tenants");

                HashSet<int> branchTenantIds =
                    tenants
                        .Where(
                            x =>
                                GetInt(
                                    x,
                                    "branchId",
                                    "BranchId") ==
                                this._selectedBranchId.Value)
                        .Select(
                            x =>
                                GetInt(
                                    x,
                                    "id",
                                    "Id",
                                    "tenantId",
                                    "TenantId"))
                        .Where(
                            x => x > 0)
                        .ToHashSet();

                renewals =
                    renewals
                        .Where(
                            x =>
                                branchTenantIds.Contains(
                                    GetInt(
                                        x,
                                        "tenantId",
                                        "TenantId")))
                        .ToList();
            }

            this.dgvReport.Columns.Clear();

            this.AddTextColumn(
                "Tenant",
                "Tenant",
                "Tenant");

            this.AddTextColumn(
                "StartDate",
                "Start Date",
                "StartDate");

            this.AddTextColumn(
                "EndDate",
                "End Date",
                "EndDate");

            this.AddTextColumn(
                "Status",
                "Status",
                "Status");

            this.AddTextColumn(
                "Notes",
                "Notes",
                "Notes");

            foreach (JsonElement item
                     in renewals)
            {
                string tenantName =
                    GetString(
                        item,
                        "tenantName",
                        "TenantName");

                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    tenantName =
                        GetNestedString(
                            item,
                            "tenant",
                            "Tenant",
                            "fullName",
                            "FullName");
                }

                this.dgvReport.Rows.Add(
                    tenantName,

                    GetDateString(
                        item,
                        "startDate",
                        "StartDate",
                        "renewalStartDate",
                        "RenewalStartDate"),

                    GetDateString(
                        item,
                        "endDate",
                        "EndDate",
                        "renewalEndDate",
                        "RenewalEndDate"),

                    GetString(
                        item,
                        "status",
                        "Status"),

                    GetString(
                        item,
                        "notes",
                        "Notes"));
            }

            int total =
                renewals.Count;

            int active =
                renewals.Count(
                    x =>
                        GetString(
                            x,
                            "status",
                            "Status")
                        .Equals(
                            "Active",
                            StringComparison.OrdinalIgnoreCase));

            int completed =
                renewals.Count(
                    x =>
                    {
                        string status =
                            GetString(
                                x,
                                "status",
                                "Status");

                        return
                            status.Equals(
                                "Completed",
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            status.Equals(
                                "Renewed",
                                StringComparison.OrdinalIgnoreCase);
                    });

            this.lblSummary1.Text =
                total.ToString();

            this.lblSummary2.Text =
                active.ToString();

            this.lblSummary3.Text =
                completed.ToString();

            this.lblSummary4.Text =
                CalculateRetentionPercentage(
                    total,
                    completed);

            this.UpdateReportActions(
                renewals.Count > 0);
        }

        // =========================================================
        // FEEDBACK REPORT
        // =========================================================

        private async Task GenerateFeedbackReportAsync()
        {
            this.ClearReport();

            this.lblReportTitle.Text =
                "FEEDBACK & SATISFACTION REPORT";

            this.lblReportDescription.Text =
                "View tenant feedback, ratings, and satisfaction information.";

            this.SetSummaryTitles(
                "TOTAL FEEDBACK",
                "AVERAGE RATING",
                "POSITIVE",
                "NEGATIVE");

            // =====================================================
            // IMPORTANT:
            // Actual controller is FeedbackController.
            // Route is:
            //
            // GET api/Feedback
            //
            // NOT api/Feedbacks
            // =====================================================

            List<JsonElement> feedback =
                await this.GetListAsync(
                    "api/Feedback");

            if (this._selectedBranchId.HasValue)
            {
                List<JsonElement> tenants =
                    await this.GetListAsync(
                        "api/Tenants");

                HashSet<int> branchTenantIds =
                    tenants
                        .Where(
                            x =>
                                GetInt(
                                    x,
                                    "branchId",
                                    "BranchId") ==
                                this._selectedBranchId.Value)
                        .Select(
                            x =>
                                GetInt(
                                    x,
                                    "id",
                                    "Id",
                                    "tenantId",
                                    "TenantId"))
                        .Where(
                            x => x > 0)
                        .ToHashSet();

                feedback =
                    feedback
                        .Where(
                            x =>
                                branchTenantIds.Contains(
                                    GetInt(
                                        x,
                                        "tenantId",
                                        "TenantId")))
                        .ToList();
            }

            // =====================================================
            // FEEDBACK COLUMNS
            // =====================================================

            this.dgvReport.Columns.Clear();

            DataGridViewTextBoxColumn dateColumn =
                this.AddTextColumn(
                    "Date",
                    "Date",
                    "Date");

            dateColumn.FillWeight = 18F;
            dateColumn.MinimumWidth = 145;

            this.AddTextColumn(
                "Tenant",
                "Tenant",
                "Tenant");

            DataGridViewTextBoxColumn ratingColumn =
                this.AddTextColumn(
                    "Rating",
                    "Rating",
                    "Rating");

            ratingColumn.FillWeight = 10F;
            ratingColumn.MinimumWidth = 75;

            ratingColumn.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            DataGridViewTextBoxColumn commentsColumn =
                this.AddTextColumn(
                    "Comments",
                    "Comments",
                    "Comments");

            commentsColumn.FillWeight = 55F;
            commentsColumn.MinimumWidth = 350;

            commentsColumn.DefaultCellStyle.WrapMode =
                DataGridViewTriState.True;

            commentsColumn.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.TopLeft;

            DataGridViewTextBoxColumn statusColumn =
                this.AddTextColumn(
                    "Status",
                    "Status",
                    "Status");

            statusColumn.FillWeight = 15F;
            statusColumn.MinimumWidth = 100;

            foreach (JsonElement item
                     in feedback)
            {
                // -------------------------------------------------
                // ACTUAL FeedbackController RESPONSE:
                //
                // TenantName
                // TenantEmail
                // Rating
                // Comment
                // SubmittedAt
                // -------------------------------------------------

                string tenantName =
                    GetString(
                        item,
                        "tenantName",
                        "TenantName");

                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    tenantName =
                        GetNestedString(
                            item,
                            "tenant",
                            "Tenant",
                            "fullName",
                            "FullName");
                }

                int rating =
                    GetInt(
                        item,
                        "rating",
                        "Rating",
                        "score",
                        "Score");

                string ratingText =
                    rating > 0
                        ? $"{rating}.0/5"
                        : "";

                string comments =
                    GetString(
                        item,
                        "comments",
                        "Comments",
                        "comment",
                        "Comment",
                        "message",
                        "Message");

                string status =
                    GetString(
                        item,
                        "status",
                        "Status",
                        "feedbackStatus",
                        "FeedbackStatus",
                        "reviewStatus",
                        "ReviewStatus");

                // FeedbackController does not currently return
                // a Status property, so use Submitted.
                if (string.IsNullOrWhiteSpace(status))
                {
                    status = "Submitted";
                }

                string date =
                    GetDateTimeString(
                        item,
                        "submittedAt",
                        "SubmittedAt",
                        "createdAt",
                        "CreatedAt",
                        "dateCreated",
                        "DateCreated",
                        "date",
                        "Date");

                this.dgvReport.Rows.Add(
                    date,
                    tenantName,
                    ratingText,
                    comments,
                    status);
            }

            // =====================================================
            // RESIZE FEEDBACK ROWS ONCE
            // =====================================================

            this.ResizeFeedbackRows();

            List<int> ratings =
                feedback
                    .Select(
                        x =>
                            GetInt(
                                x,
                                "rating",
                                "Rating",
                                "score",
                                "Score"))
                    .Where(
                        x => x > 0)
                    .ToList();

            double average =
                ratings.Count > 0
                    ? ratings.Average()
                    : 0;

            int positive =
                ratings.Count(
                    x => x >= 4);

            int negative =
                ratings.Count(
                    x => x <= 2);

            this.lblSummary1.Text =
                feedback.Count.ToString();

            this.lblSummary2.Text =
                average.ToString("0.00");

            this.lblSummary3.Text =
                positive.ToString();

            this.lblSummary4.Text =
                negative.ToString();

            this.UpdateReportActions(
                feedback.Count > 0);
        }

        // =========================================================
        // RESIZE FEEDBACK ROWS ONCE
        // =========================================================

        private void ResizeFeedbackRows()
        {
            if (this.dgvReport.Rows.Count == 0)
            {
                return;
            }

            int commentsColumnIndex = -1;

            for (
                int i = 0;
                i < this.dgvReport.Columns.Count;
                i++)
            {
                if (string.Equals(
                        this.dgvReport.Columns[i].Name,
                        "Comments",
                        StringComparison.OrdinalIgnoreCase))
                {
                    commentsColumnIndex = i;
                    break;
                }
            }

            if (commentsColumnIndex < 0)
            {
                return;
            }

            DataGridViewColumn commentsColumn =
                this.dgvReport.Columns[
                    commentsColumnIndex];

            int availableWidth =
                commentsColumn.Width - 12;

            if (availableWidth < 100)
            {
                availableWidth = 100;
            }

            using Font font =
                new Font(
                    "Segoe UI",
                    8.5F);

            foreach (DataGridViewRow row
                     in this.dgvReport.Rows)
            {
                string text =
                    row.Cells[
                        commentsColumnIndex]
                        .Value
                        ?.ToString()
                        ?? "";

                if (string.IsNullOrWhiteSpace(text))
                {
                    row.Height = 44;
                    continue;
                }

                Size measured =
                    TextRenderer.MeasureText(
                        text,
                        font,
                        new Size(
                            availableWidth,
                            1000),
                        TextFormatFlags.WordBreak);

                int height =
                    Math.Max(
                        44,
                        measured.Height + 16);

                height =
                    Math.Min(
                        100,
                        height);

                row.Height = height;
            }
        }

        // =========================================================
        // SET SUMMARY TITLES
        // =========================================================

        private void SetSummaryTitles(
            string title1,
            string title2,
            string title3,
            string title4)
        {
            if (this.lblSummaryTitle1 != null)
            {
                this.lblSummaryTitle1.Text =
                    title1;
            }

            if (this.lblSummaryTitle2 != null)
            {
                this.lblSummaryTitle2.Text =
                    title2;
            }

            if (this.lblSummaryTitle3 != null)
            {
                this.lblSummaryTitle3.Text =
                    title3;
            }

            if (this.lblSummaryTitle4 != null)
            {
                this.lblSummaryTitle4.Text =
                    title4;
            }
        }

        // =========================================================
        // CLEAR REPORT
        // =========================================================

        private void ClearReport()
        {
            this._tenantReport.Clear();
            this._billingReport.Clear();
            this._genericReport.Clear();
            this._genericHeaders.Clear();

            if (this.dgvReport != null)
            {
                this.dgvReport.DataSource = null;
                this.dgvReport.Rows.Clear();
                this.dgvReport.Columns.Clear();
            }

            if (this.lblSummary1 != null)
            {
                this.lblSummary1.Text = "0";
            }

            if (this.lblSummary2 != null)
            {
                this.lblSummary2.Text = "0";
            }

            if (this.lblSummary3 != null)
            {
                this.lblSummary3.Text = "0";
            }

            if (this.lblSummary4 != null)
            {
                this.lblSummary4.Text = "0";
            }

            if (this.lblSummaryTitle1 != null)
            {
                this.lblSummaryTitle1.Text = "TOTAL";
            }

            if (this.lblSummaryTitle2 != null)
            {
                this.lblSummaryTitle2.Text = "ACTIVE";
            }

            if (this.lblSummaryTitle3 != null)
            {
                this.lblSummaryTitle3.Text = "PENDING";
            }

            if (this.lblSummaryTitle4 != null)
            {
                this.lblSummaryTitle4.Text = "OTHER";
            }

            this.UpdateReportActions(false);
        }

        // =========================================================
        // UPDATE REPORT ACTIONS
        // =========================================================

        private void UpdateReportActions(
            bool hasData)
        {
            this.btnExport.Enabled =
                hasData;

            this.btnPrint.Enabled =
                hasData;

            this.btnRefresh.Enabled =
                this._selectedBranchId.HasValue;

            this.btnGenerate.Enabled =
                this._selectedBranchId.HasValue &&
                this.cboReportType.Enabled;
        }

        // =========================================================
        // UPDATE MAIN BUTTON STATES
        // =========================================================

        private void UpdateMainButtonStates()
        {
            this.btnRefresh.Enabled =
                this._selectedBranchId.HasValue;

            this.btnGenerate.Enabled =
                this._selectedBranchId.HasValue &&
                this.cboReportType.Enabled;

            bool hasData =
                this.dgvReport != null &&
                this.dgvReport.Columns.Count > 0 &&
                this.dgvReport.Rows.Count > 0;

            this.btnExport.Enabled =
                hasData;

            this.btnPrint.Enabled =
                hasData;
        }

        // =========================================================
        // ADD TEXT COLUMN
        // =========================================================

        private DataGridViewTextBoxColumn AddTextColumn(
            string name,
            string headerText,
            string propertyName)
        {
            DataGridViewTextBoxColumn column =
                new DataGridViewTextBoxColumn
                {
                    Name = name,
                    HeaderText = headerText,
                    DataPropertyName = propertyName,
                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                };

            this.dgvReport.Columns.Add(column);

            return column;
        }

        // =========================================================
        // ADD NUMBER COLUMN
        // =========================================================

        private void AddNumberColumn(
            string name,
            string headerText)
        {
            DataGridViewTextBoxColumn column =
                new DataGridViewTextBoxColumn
                {
                    Name = name,
                    HeaderText = headerText,
                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                };

            column.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            this.dgvReport.Columns.Add(column);
        }

        // =========================================================
        // GET API LIST
        // =========================================================


        // =========================================================
        // TENANT REPORT DATA (api/Reports/tenants)
        // =========================================================

        private async Task<List<JsonElement>> GetTenantReportDataAsync()
        {
            try
            {
                // Response shape: { summary: {...}, data: [ ... ] }
                JsonElement result =
                    await this._apiService
                        .GetSilentAsync<JsonElement>(
                            "api/Reports/tenants");

                // Silent failure returns default (Undefined)
                if (result.ValueKind ==
                    JsonValueKind.Undefined ||
                    result.ValueKind ==
                    JsonValueKind.Null)
                {
                    return await this.GetListAsync(
                        "api/Tenants");
                }

                if (result.ValueKind ==
                    JsonValueKind.Object)
                {
                    if (result.TryGetProperty(
                            "data",
                            out JsonElement data) &&
                        data.ValueKind ==
                        JsonValueKind.Array)
                    {
                        return data
                            .EnumerateArray()
                            .ToList();
                    }

                    if (result.TryGetProperty(
                            "Data",
                            out JsonElement data2) &&
                        data2.ValueKind ==
                        JsonValueKind.Array)
                    {
                        return data2
                            .EnumerateArray()
                            .ToList();
                    }
                }
                else if (result.ValueKind ==
                         JsonValueKind.Array)
                {
                    return result
                        .EnumerateArray()
                        .ToList();
                }
            }
            catch
            {
                // Fall through to api/Tenants
            }

            return await this.GetListAsync(
                "api/Tenants");
        }

        private async Task<List<JsonElement>> GetListAsync(
            string endpoint)
        {
            try
            {
                // Silent — avoid 403/404 MessageBox spam in Reports
                List<JsonElement>? result =
                    await this._apiService
                        .GetSilentAsync<List<JsonElement>>(
                            endpoint);

                return result ??
                       new List<JsonElement>();
            }
            catch
            {
                return new List<JsonElement>();
            }
        }

        // =========================================================
        // FILTER BY BRANCH
        // =========================================================

        private List<JsonElement> FilterByBranch(
            List<JsonElement> items)
        {
            if (!this._selectedBranchId.HasValue
                || this._selectedBranchId.Value <= 0)
            {
                // null or 0 = All Branches (Admin)
                return items;
            }

            int branchId =
                this._selectedBranchId.Value;

            List<JsonElement> filtered =
                items
                    .Where(
                        x =>
                            GetInt(
                                x,
                                "branchId",
                                "BranchId") ==
                            branchId)
                    .ToList();

            if (
                filtered.Count == 0 &&
                !IsAdminRole())
            {
                bool hasAnyBranchId =
                    items.Any(
                        x =>
                            GetInt(
                                x,
                                "branchId",
                                "BranchId") > 0);

                if (!hasAnyBranchId)
                {
                    return items;
                }
            }

            return filtered;
        }

        // =========================================================
        // FILTER BILLING BY BRANCH
        // =========================================================

        private async Task<List<JsonElement>>
            FilterBillingByBranchAsync(
                List<JsonElement> billing)
        {
            if (!this._selectedBranchId.HasValue)
            {
                return billing;
            }

            int branchId =
                this._selectedBranchId.Value;

            List<JsonElement> tenants =
                await this.GetListAsync(
                    "api/Tenants");

            HashSet<int> branchTenantIds =
                tenants
                    .Where(
                        x =>
                            GetInt(
                                x,
                                "branchId",
                                "BranchId") ==
                            branchId)
                    .Select(
                        x =>
                            GetInt(
                                x,
                                "id",
                                "Id",
                                "tenantId",
                                "TenantId"))
                    .Where(
                        x => x > 0)
                    .ToHashSet();

            return billing
                .Where(
                    x =>
                        branchTenantIds.Contains(
                            GetInt(
                                x,
                                "tenantId",
                                "TenantId")))
                .ToList();
        }

        // =========================================================
        // GET STRING
        // =========================================================

        private static string GetString(
            JsonElement element,
            params string[] names)
        {
            foreach (string name in names)
            {
                if (!element.TryGetProperty(
                        name,
                        out JsonElement property))
                {
                    continue;
                }

                if (property.ValueKind ==
                    JsonValueKind.String)
                {
                    return property.GetString() ?? "";
                }

                if (
                    property.ValueKind ==
                        JsonValueKind.Number ||
                    property.ValueKind ==
                        JsonValueKind.True ||
                    property.ValueKind ==
                        JsonValueKind.False)
                {
                    return property.ToString();
                }
            }

            return "";
        }

        // =========================================================
        // GET NESTED STRING
        // =========================================================

        private static string GetNestedString(
            JsonElement element,
            string objectName1,
            string objectName2,
            params string[] propertyNames)
        {
            JsonElement nested;

            if (!element.TryGetProperty(
                    objectName1,
                    out nested))
            {
                if (!element.TryGetProperty(
                        objectName2,
                        out nested))
                {
                    return "";
                }
            }

            if (nested.ValueKind !=
                JsonValueKind.Object)
            {
                return "";
            }

            return GetString(
                nested,
                propertyNames);
        }

        // =========================================================
        // GET INT
        // =========================================================

        private static int GetInt(
            JsonElement element,
            params string[] names)
        {
            foreach (string name in names)
            {
                if (!element.TryGetProperty(
                        name,
                        out JsonElement property))
                {
                    continue;
                }

                if (
                    property.ValueKind ==
                        JsonValueKind.Number &&
                    property.TryGetInt32(
                        out int value))
                {
                    return value;
                }

                if (
                    property.ValueKind ==
                        JsonValueKind.String &&
                    int.TryParse(
                        property.GetString(),
                        out value))
                {
                    return value;
                }
            }

            return 0;
        }

        // =========================================================
        // GET DECIMAL
        // =========================================================

        private static decimal GetDecimal(
            JsonElement element,
            params string[] names)
        {
            foreach (string name in names)
            {
                if (!element.TryGetProperty(
                        name,
                        out JsonElement property))
                {
                    continue;
                }

                if (
                    property.ValueKind ==
                        JsonValueKind.Number &&
                    property.TryGetDecimal(
                        out decimal value))
                {
                    return value;
                }

                if (
                    property.ValueKind ==
                        JsonValueKind.String &&
                    decimal.TryParse(
                        property.GetString(),
                        out value))
                {
                    return value;
                }
            }

            return 0m;
        }

        // =========================================================
        // GET DATE STRING
        // =========================================================

        private static string GetDateString(
            JsonElement element,
            params string[] names)
        {
            foreach (string name in names)
            {
                if (!element.TryGetProperty(
                        name,
                        out JsonElement property))
                {
                    continue;
                }

                if (property.ValueKind !=
                    JsonValueKind.String)
                {
                    continue;
                }

                string? value =
                    property.GetString();

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (DateTime.TryParse(
                        value,
                        out DateTime date))
                {
                    return date.ToString(
                        "MMM dd, yyyy");
                }

                return value;
            }

            return "";
        }

        // =========================================================
        // GET DATE + TIME STRING
        // =========================================================

        private static string GetDateTimeString(
            JsonElement element,
            params string[] names)
        {
            foreach (string name in names)
            {
                if (!element.TryGetProperty(
                        name,
                        out JsonElement property))
                {
                    continue;
                }

                if (property.ValueKind !=
                    JsonValueKind.String)
                {
                    continue;
                }

                string? value =
                    property.GetString();

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (DateTime.TryParse(
                        value,
                        out DateTime date))
                {
                    return date.ToString(
                        "MMM dd, yyyy hh:mm tt");
                }

                return value;
            }

            return "";
        }

        // =========================================================
        // TENANT MOVED OUT
        // =========================================================

        private static bool IsTenantMovedOut(
            JsonElement tenant)
        {
            string status =
                GetString(
                    tenant,
                    "status",
                    "Status");

            if (
                status.Equals(
                    "Moved Out",
                    StringComparison.OrdinalIgnoreCase)
                ||
                status.Equals(
                    "MovedOut",
                    StringComparison.OrdinalIgnoreCase)
                ||
                status.Equals(
                    "Archived",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string moveOutDate =
                GetString(
                    tenant,
                    "actualMoveOutDate",
                    "ActualMoveOutDate");

            return
                !string.IsNullOrWhiteSpace(
                    moveOutDate);
        }

        // =========================================================
        // TENANT ACTIVE
        // =========================================================

        private static bool IsTenantActive(
            JsonElement tenant)
        {
            if (IsTenantMovedOut(tenant))
            {
                return false;
            }

            string status =
                GetString(
                    tenant,
                    "status",
                    "Status");

            return
                status.Equals(
                    "Active",
                    StringComparison.OrdinalIgnoreCase)
                ||
                status.Equals(
                    "Approved",
                    StringComparison.OrdinalIgnoreCase)
                ||
                status.Equals(
                    "Occupied",
                    StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // RETENTION
        // =========================================================

        private static string
            CalculateRetentionPercentage(
                int total,
                int retained)
        {
            if (total <= 0)
            {
                return "0%";
            }

            double percentage =
                (double)retained /
                total *
                100;

            return percentage.ToString("0.0") +
                   "%";
        }

        // =========================================================
        // ROLE CHECKS
        // =========================================================

        private static bool IsAdminRole()
        {
            return string.Equals(
                ApiService.CurrentRole,
                "Admin",
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsManagerRole()
        {
            return string.Equals(
                ApiService.CurrentRole,
                "Manager",
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStaffRole()
        {
            return string.Equals(
                ApiService.CurrentRole,
                "Staff",
                StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // EXPORT CSV
        // =========================================================

        private void ExportCurrentReport()
        {
            if (
                this.dgvReport.Columns.Count == 0 ||
                this.dgvReport.Rows.Count == 0)
            {
                MessageBox.Show(
                    "There is no report data to export.",
                    "Export",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using SaveFileDialog dialog =
                new SaveFileDialog
                {
                    Filter =
                        "CSV Files (*.csv)|*.csv",

                    DefaultExt = "csv",

                    AddExtension = true,

                    FileName =
                        $"{this.GetSafeReportFileName()}_" +
                        $"{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

            if (dialog.ShowDialog() !=
                DialogResult.OK)
            {
                return;
            }

            try
            {
                StringBuilder csv =
                    new StringBuilder();

                for (
                    int i = 0;
                    i < this.dgvReport.Columns.Count;
                    i++)
                {
                    if (i > 0)
                    {
                        csv.Append(",");
                    }

                    csv.Append(
                        EscapeCsv(
                            this.dgvReport
                                .Columns[i]
                                .HeaderText));
                }

                csv.AppendLine();

                foreach (
                    DataGridViewRow row
                    in this.dgvReport.Rows)
                {
                    if (row.IsNewRow)
                    {
                        continue;
                    }

                    for (
                        int i = 0;
                        i < this.dgvReport.Columns.Count;
                        i++)
                    {
                        if (i > 0)
                        {
                            csv.Append(",");
                        }

                        object? value =
                            row.Cells[i].Value;

                        csv.Append(
                            EscapeCsv(
                                value?.ToString()
                                ?? ""));
                    }

                    csv.AppendLine();
                }

                File.WriteAllText(
                    dialog.FileName,
                    csv.ToString(),
                    Encoding.UTF8);

                MessageBox.Show(
                    "Report exported successfully.",
                    "Export",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to export the report.\n\n" +
                    ex.Message,
                    "Export",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // ESCAPE CSV
        // =========================================================

        private static string EscapeCsv(
            string value)
        {
            if (
                value.Contains(",") ||
                value.Contains("\"") ||
                value.Contains("\r") ||
                value.Contains("\n"))
            {
                return
                    "\"" +
                    value.Replace(
                        "\"",
                        "\"\"") +
                    "\"";
            }

            return value;
        }

        // =========================================================
        // PRINT
        // =========================================================

        private void PrintCurrentReport()
        {
            if (
                this.dgvReport.Columns.Count == 0 ||
                this.dgvReport.Rows.Count == 0)
            {
                MessageBox.Show(
                    "There is no report data to print.",
                    "Print",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            this._printRowIndex = 0;

            this._printTitle =
                this.cboReportType.SelectedItem
                    ?.ToString()
                ?? "Report";

            this._printDocument
                .DefaultPageSettings
                .Landscape =
                this.dgvReport.Columns.Count >= 6;

            try
            {
                using PrintDialog dialog =
                    new PrintDialog
                    {
                        Document =
                            this._printDocument,

                        UseEXDialog = true
                    };

                if (dialog.ShowDialog() ==
                    DialogResult.OK)
                {
                    this._printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to print the report.\n\n" +
                    ex.Message,
                    "Print",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // PRINT PAGE
        // =========================================================

        private void PrintDocument_PrintPage(
            object? sender,
            PrintPageEventArgs e)
        {
            Graphics graphics =
                e.Graphics;

            Rectangle bounds =
                e.MarginBounds;

            using Font titleFont =
                new Font(
                    "Segoe UI",
                    16F,
                    FontStyle.Bold);

            using Font infoFont =
                new Font(
                    "Segoe UI",
                    9F);

            using Font headerFont =
                new Font(
                    "Segoe UI",
                    8F,
                    FontStyle.Bold);

            using Font rowFont =
                new Font(
                    "Segoe UI",
                    7.5F);

            float y =
                bounds.Top;

            graphics.DrawString(
                this._printTitle,
                titleFont,
                Brushes.Black,
                bounds.Left,
                y);

            y += 32;

            string branchName =
                this.cboBranch.SelectedItem
                    ?.ToString()
                ?? "Current Branch";

            graphics.DrawString(
                $"Branch: {branchName}",
                infoFont,
                Brushes.Black,
                bounds.Left,
                y);

            y += 18;

            graphics.DrawString(
                $"Generated: {DateTime.Now:MMM dd, yyyy hh:mm tt}",
                infoFont,
                Brushes.Black,
                bounds.Left,
                y);

            y += 25;

            int columnCount =
                this.dgvReport.Columns.Count;

            if (columnCount <= 0)
            {
                e.HasMorePages = false;
                return;
            }

            int totalColumnWidth =
                this.dgvReport.Columns
                    .Cast<DataGridViewColumn>()
                    .Sum(
                        column =>
                            Math.Max(
                                column.Width,
                                40));

            int rowHeight = 24;

            // =====================================================
            // HEADER
            // =====================================================

            float x =
                bounds.Left;

            for (
                int columnIndex = 0;
                columnIndex < columnCount;
                columnIndex++)
            {
                DataGridViewColumn column =
                    this.dgvReport.Columns[
                        columnIndex];

                int cellWidth =
                    (int)(
                        (double)
                        Math.Max(
                            column.Width,
                            40)
                        /
                        totalColumnWidth
                        *
                        bounds.Width);

                if (
                    columnIndex ==
                    columnCount - 1)
                {
                    cellWidth =
                        bounds.Right -
                        (int)x;
                }

                Rectangle headerRect =
                    new Rectangle(
                        (int)x,
                        (int)y,
                        cellWidth,
                        rowHeight);

                using Brush headerBrush =
                    new SolidBrush(
                        BrandBg);

                graphics.FillRectangle(
                    headerBrush,
                    headerRect);

                using Brush headerTextBrush =
                    new SolidBrush(
                        Color.White);

                string headerText =
                    TrimToWidth(
                        graphics,
                        column.HeaderText,
                        headerFont,
                        cellWidth - 8);

                graphics.DrawString(
                    headerText,
                    headerFont,
                    headerTextBrush,
                    new RectangleF(
                        x + 4,
                        y + 4,
                        cellWidth - 8,
                        rowHeight - 4));

                x += cellWidth;
            }

            y += rowHeight;

            // =====================================================
            // ROWS
            // =====================================================

            while (
                this._printRowIndex <
                this.dgvReport.Rows.Count)
            {
                DataGridViewRow row =
                    this.dgvReport.Rows[
                        this._printRowIndex];

                if (row.IsNewRow)
                {
                    this._printRowIndex++;
                    continue;
                }

                if (y + rowHeight >
                    bounds.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }

                x =
                    bounds.Left;

                for (
                    int columnIndex = 0;
                    columnIndex < columnCount;
                    columnIndex++)
                {
                    DataGridViewColumn column =
                        this.dgvReport.Columns[
                            columnIndex];

                    int cellWidth =
                        (int)(
                            (double)
                            Math.Max(
                                column.Width,
                                40)
                            /
                            totalColumnWidth
                            *
                            bounds.Width);

                    if (
                        columnIndex ==
                        columnCount - 1)
                    {
                        cellWidth =
                            bounds.Right -
                            (int)x;
                    }

                    Rectangle cellRect =
                        new Rectangle(
                            (int)x,
                            (int)y,
                            cellWidth,
                            rowHeight);

                    if (
                        this._printRowIndex %
                        2 == 1)
                    {
                        using Brush alternateBrush =
                            new SolidBrush(
                                Color.FromArgb(
                                    247,
                                    244,
                                    239));

                        graphics.FillRectangle(
                            alternateBrush,
                            cellRect);
                    }
                    else
                    {
                        graphics.FillRectangle(
                            Brushes.White,
                            cellRect);
                    }

                    using Pen borderPen =
                        new Pen(
                            Color.FromArgb(
                                225,
                                215,
                                200));

                    graphics.DrawRectangle(
                        borderPen,
                        cellRect);

                    string value =
                        row.Cells[
                                columnIndex]
                            .Value
                            ?.ToString()
                            ?? "";

                    value =
                        TrimToWidth(
                            graphics,
                            value,
                            rowFont,
                            cellWidth - 8);

                    graphics.DrawString(
                        value,
                        rowFont,
                        Brushes.Black,
                        new RectangleF(
                            x + 4,
                            y + 4,
                            cellWidth - 8,
                            rowHeight - 4));

                    x += cellWidth;
                }

                y += rowHeight;

                this._printRowIndex++;
            }

            e.HasMorePages = false;
        }

        // =========================================================
        // TRIM PRINT TEXT
        // =========================================================

        private static string TrimToWidth(
            Graphics graphics,
            string text,
            Font font,
            float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            if (graphics.MeasureString(
                    text,
                    font).Width <= maxWidth)
            {
                return text;
            }

            string result = text;

            while (
                result.Length > 3 &&
                graphics.MeasureString(
                    result + "...",
                    font).Width > maxWidth)
            {
                result =
                    result.Substring(
                        0,
                        result.Length - 1);
            }

            return result + "...";
        }

        // =========================================================
        // SAFE FILE NAME
        // =========================================================

        private string GetSafeReportFileName()
        {
            string reportType =
                this.cboReportType
                    ?.SelectedItem
                    ?.ToString()
                ?? "Report";

            foreach (
                char invalid
                in Path.GetInvalidFileNameChars())
            {
                reportType =
                    reportType.Replace(
                        invalid,
                        '_');
            }

            return reportType.Replace(
                " ",
                "_");
        }

        // =========================================================
        // DISPOSE
        // =========================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                this._printDocument.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
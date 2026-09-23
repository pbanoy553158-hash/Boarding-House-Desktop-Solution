using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PBCRM2.WinForms.Forms
{
    public class AdminRenewalRetentionForm : Form
    {
        // ============================================================
        // SERVICES
        // ============================================================

        private readonly ApiService _apiService;

        // ============================================================
        // MAIN CONTROLS
        // ============================================================

        private TabControl tabMain = null!;

        private Label lblHeaderDate = null!;
        private Label lblLoadStatus = null!;
        private Button btnRefresh = null!;

        // ============================================================
        // OVERVIEW
        // ============================================================

        private Label lblTotalRenewals = null!;
        private Label lblUpcoming = null!;
        private Label lblDueSoon = null!;
        private Label lblExpired = null!;
        private Label lblCompleted = null!;
        private Label lblRetentionRate = null!;

        private Label lblOverviewActivity = null!;
        private Label lblOverviewInsights = null!;

        // ============================================================
        // BRANCH PERFORMANCE
        // ============================================================

        private DataGridView dgvBranchPerformance = null!;
        private Label lblBranchCount = null!;

        // ============================================================
        // RENEWAL MONITORING
        // ============================================================

        private DataGridView dgvRenewals = null!;
        private TextBox txtRenewalSearch = null!;
        private ComboBox cmbRenewalBranch = null!;
        private ComboBox cmbRenewalStatus = null!;
        private Button btnClearRenewalFilters = null!;
        private Label lblRenewalCount = null!;

        // ============================================================
        // PROMOTIONS
        // ============================================================

        private DataGridView dgvPromotions = null!;
        private TextBox txtPromotionSearch = null!;
        private ComboBox cmbPromotionStatus = null!;
        private Button btnAddPromotion = null!;
        private Button btnEditPromotion = null!;
        private Button btnArchivePromotion = null!;
        private Label lblPromotionCount = null!;

        // ============================================================
        // DATA
        // ============================================================

        private List<RenewalDto> _renewals = new();
        private List<BranchRenewalRecord> _branchRecords = new();
        private List<PromotionDto> _promotions = new();

        // ============================================================
        // COLORS
        // ============================================================

        private static readonly Color BrandBg =
            Color.FromArgb(32, 24, 18);

        private static readonly Color BrandAccent =
            Color.FromArgb(224, 194, 140);

        private static readonly Color CtaColor =
            Color.FromArgb(170, 130, 80);

        private static readonly Color PanelBg =
            Color.FromArgb(250, 247, 242);

        private static readonly Color GridAlternate =
            Color.FromArgb(247, 244, 239);

        private static readonly Color GridSelection =
            Color.FromArgb(232, 220, 199);

        private static readonly Color GridBorder =
            Color.FromArgb(225, 215, 200);

        private static readonly Color TextDark =
            Color.FromArgb(45, 38, 32);

        private static readonly Color TextMuted =
            Color.FromArgb(115, 105, 95);

        private static readonly Color SuccessColor =
            Color.FromArgb(46, 125, 50);

        private static readonly Color WarningColor =
            Color.FromArgb(183, 110, 0);

        private static readonly Color DangerColor =
            Color.FromArgb(183, 52, 52);

        private static readonly Color InfoColor =
            Color.FromArgb(53, 91, 125);

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public AdminRenewalRetentionForm(ApiService apiService)
        {
            _apiService = apiService;

            Text = "Renewal & Retention";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1250, 780);
            MinimumSize = new Size(1000, 680);
            BackColor = PanelBg;

            BuildInterface();

            Shown += async (_, _) =>
            {
                await LoadRenewalDataAsync();
            };
        }

        // ============================================================
        // BUILD MAIN INTERFACE
        // ============================================================

        private void BuildInterface()
        {
            var main = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg
            };

            // ========================================================
            // HEADER
            // ========================================================

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = BrandBg
            };

            var title = new Label
            {
                Text = "RENEWAL & RETENTION",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 22)
            };

            var subtitle = new Label
            {
                Text = "Monitor tenant renewals, retention performance, and promotional offers.",
                ForeColor = BrandAccent,
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(28, 67)
            };

            lblHeaderDate = new Label
            {
                Text = DateTime.Now.ToString("MMMM dd, yyyy"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnRefresh = CreateButton("Refresh", 95);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += async (_, _) =>
            {
                await LoadRenewalDataAsync();
            };

            lblLoadStatus = new Label
            {
                Text = "Ready",
                ForeColor = BrandAccent,
                Font = new Font("Segoe UI", 8.5F),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            header.Controls.Add(lblHeaderDate);
            header.Controls.Add(btnRefresh);
            header.Controls.Add(lblLoadStatus);

            header.Resize += (_, _) =>
            {
                lblHeaderDate.Location =
                    new Point(header.ClientSize.Width - lblHeaderDate.Width - 25, 18);

                btnRefresh.Location =
                    new Point(header.ClientSize.Width - btnRefresh.Width - 25, 48);

                lblLoadStatus.Location =
                    new Point(
                        header.ClientSize.Width - lblLoadStatus.Width - 140,
                        18);
            };

            // ========================================================
            // HEADER GAP
            // ========================================================

            var headerGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = PanelBg
            };

            // ========================================================
            // TABS
            // ========================================================

            BuildTabs();

            // ========================================================
            // ADD TO MAIN
            // ========================================================

            main.Controls.Add(tabMain);
            main.Controls.Add(headerGap);
            main.Controls.Add(header);

            Controls.Add(main);

            header.BringToFront();

            // Force initial header layout.
            header.PerformLayout();
        }

        // ============================================================
        // TABS
        // ============================================================

        private void BuildTabs()
        {
            tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Padding = new Point(14, 6)
            };

            var tabOverview = new TabPage("Overview")
            {
                BackColor = PanelBg,
                Padding = new Padding(20, 10, 20, 20)
            };

            var tabBranch = new TabPage("Branch Performance")
            {
                BackColor = PanelBg,
                Padding = new Padding(20, 10, 20, 20)
            };

            var tabMonitoring = new TabPage("Renewal Monitoring")
            {
                BackColor = PanelBg,
                Padding = new Padding(20, 10, 20, 20)
            };

            var tabPromotions = new TabPage("Promotions & Offers")
            {
                BackColor = PanelBg,
                Padding = new Padding(20, 10, 20, 20)
            };

            BuildOverviewTab(tabOverview);
            BuildBranchTab(tabBranch);
            BuildMonitoringTab(tabMonitoring);
            BuildPromotionsTab(tabPromotions);

            tabMain.TabPages.Add(tabOverview);
            tabMain.TabPages.Add(tabBranch);
            tabMain.TabPages.Add(tabMonitoring);
            tabMain.TabPages.Add(tabPromotions);

            tabMain.SelectedIndexChanged += (_, _) =>
            {
                UpdateCurrentTab();
            };
        }

        // ============================================================
        // OVERVIEW TAB
        // ============================================================

        private void BuildOverviewTab(TabPage tab)
        {
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg
            };

            // --------------------------------------------------------
            // TITLE
            // --------------------------------------------------------

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = PanelBg
            };

            var title = new Label
            {
                Text = "RENEWAL OVERVIEW",
                AutoSize = true,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(0, 2)
            };

            var subtitle = new Label
            {
                Text = "Review current renewal activity and tenant retention performance.",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMuted,
                Location = new Point(2, 33)
            };

            headerPanel.Controls.Add(title);
            headerPanel.Controls.Add(subtitle);

            // --------------------------------------------------------
            // METRICS
            // --------------------------------------------------------

            var metricsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = PanelBg,
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(0)
            };

            for (int i = 0; i < 6; i++)
            {
                metricsPanel.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 16.6667F));
            }

            metricsPanel.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            Panel card1 = CreateMetricCard(
                "TOTAL RENEWALS",
                "0",
                "All renewal records",
                out lblTotalRenewals);

            Panel card2 = CreateMetricCard(
                "UPCOMING",
                "0",
                "Future renewals",
                out lblUpcoming);

            Panel card3 = CreateMetricCard(
                "DUE SOON",
                "0",
                "Within 7 days",
                out lblDueSoon);

            Panel card4 = CreateMetricCard(
                "EXPIRED",
                "0",
                "Past renewal date",
                out lblExpired);

            Panel card5 = CreateMetricCard(
                "COMPLETED",
                "0",
                "Successfully renewed",
                out lblCompleted);

            Panel card6 = CreateMetricCard(
                "RETENTION",
                "0%",
                "Completed vs expired",
                out lblRetentionRate);

            metricsPanel.Controls.Add(card1, 0, 0);
            metricsPanel.Controls.Add(card2, 1, 0);
            metricsPanel.Controls.Add(card3, 2, 0);
            metricsPanel.Controls.Add(card4, 3, 0);
            metricsPanel.Controls.Add(card5, 4, 0);
            metricsPanel.Controls.Add(card6, 5, 0);

            // --------------------------------------------------------
            // OVERVIEW PANELS
            // --------------------------------------------------------

            var overviewBody = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 15, 0, 0)
            };

            overviewBody.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50F));

            overviewBody.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50F));

            overviewBody.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            Panel activityPanel = CreateWhitePanel(
                "RENEWAL ACTIVITY",
                out lblOverviewActivity);

            Panel insightsPanel = CreateWhitePanel(
                "RETENTION INSIGHTS",
                out lblOverviewInsights);

            activityPanel.Margin = new Padding(0, 0, 8, 0);
            insightsPanel.Margin = new Padding(8, 0, 0, 0);

            overviewBody.Controls.Add(activityPanel, 0, 0);
            overviewBody.Controls.Add(insightsPanel, 1, 0);

            content.Controls.Add(overviewBody);
            content.Controls.Add(metricsPanel);
            content.Controls.Add(headerPanel);

            tab.Controls.Add(content);

            // Responsive overview layout.
            content.Resize += (_, _) =>
            {
                if (content.ClientSize.Width < 900)
                {
                    metricsPanel.ColumnCount = 3;
                    metricsPanel.RowCount = 2;
                    metricsPanel.Height = 220;

                    metricsPanel.ColumnStyles.Clear();
                    metricsPanel.RowStyles.Clear();

                    for (int i = 0; i < 3; i++)
                    {
                        metricsPanel.ColumnStyles.Add(
                            new ColumnStyle(SizeType.Percent, 33.3333F));
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        metricsPanel.RowStyles.Add(
                            new RowStyle(SizeType.Percent, 50F));
                    }

                    overviewBody.ColumnCount = 1;
                    overviewBody.RowCount = 2;

                    overviewBody.ColumnStyles.Clear();
                    overviewBody.RowStyles.Clear();

                    overviewBody.ColumnStyles.Add(
                        new ColumnStyle(SizeType.Percent, 100F));

                    overviewBody.RowStyles.Add(
                        new RowStyle(SizeType.Percent, 50F));

                    overviewBody.RowStyles.Add(
                        new RowStyle(SizeType.Percent, 50F));

                    activityPanel.Margin = new Padding(0, 0, 0, 8);
                    insightsPanel.Margin = new Padding(0, 8, 0, 0);
                }
                else
                {
                    metricsPanel.ColumnCount = 6;
                    metricsPanel.RowCount = 1;
                    metricsPanel.Height = 110;

                    metricsPanel.ColumnStyles.Clear();
                    metricsPanel.RowStyles.Clear();

                    for (int i = 0; i < 6; i++)
                    {
                        metricsPanel.ColumnStyles.Add(
                            new ColumnStyle(SizeType.Percent, 16.6667F));
                    }

                    metricsPanel.RowStyles.Add(
                        new RowStyle(SizeType.Percent, 100F));

                    overviewBody.ColumnCount = 2;
                    overviewBody.RowCount = 1;

                    overviewBody.ColumnStyles.Clear();
                    overviewBody.RowStyles.Clear();

                    overviewBody.ColumnStyles.Add(
                        new ColumnStyle(SizeType.Percent, 50F));

                    overviewBody.ColumnStyles.Add(
                        new ColumnStyle(SizeType.Percent, 50F));

                    overviewBody.RowStyles.Add(
                        new RowStyle(SizeType.Percent, 100F));

                    activityPanel.Margin = new Padding(0, 0, 8, 0);
                    insightsPanel.Margin = new Padding(8, 0, 0, 0);
                }
            };
        }

        // ============================================================
        // BRANCH PERFORMANCE TAB
        // ============================================================

        private void BuildBranchTab(TabPage tab)
        {
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg
            };

            dgvBranchPerformance = CreateGrid();

            dgvBranchPerformance.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Branch",
                    HeaderText = "Branch",
                    DataPropertyName = "BranchName",
                    FillWeight = 24,
                    MinimumWidth = 150,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });

            dgvBranchPerformance.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "TotalRenewals",
                    HeaderText = "Total Renewals",
                    DataPropertyName = "TotalRenewals",
                    FillWeight = 14,
                    MinimumWidth = 110,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = CenterCellStyle()
                });

            dgvBranchPerformance.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Upcoming",
                    HeaderText = "Upcoming",
                    DataPropertyName = "Upcoming",
                    FillWeight = 12,
                    MinimumWidth = 90,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = CenterCellStyle()
                });

            dgvBranchPerformance.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "DueSoon",
                    HeaderText = "Due Soon",
                    DataPropertyName = "DueSoon",
                    FillWeight = 12,
                    MinimumWidth = 90,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = CenterCellStyle()
                });

            dgvBranchPerformance.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Expired",
                    HeaderText = "Expired",
                    DataPropertyName = "Expired",
                    FillWeight = 12,
                    MinimumWidth = 90,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = CenterCellStyle()
                });

            dgvBranchPerformance.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Completed",
                    HeaderText = "Completed",
                    DataPropertyName = "Completed",
                    FillWeight = 12,
                    MinimumWidth = 90,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = CenterCellStyle()
                });

            dgvBranchPerformance.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Retention",
                    HeaderText = "Retention Rate",
                    DataPropertyName = "RetentionRate",
                    FillWeight = 14,
                    MinimumWidth = 110,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = CenterCellStyle()
                });

            dgvBranchPerformance.Dock = DockStyle.Fill;

            dgvBranchPerformance.CellFormatting += DgvBranchPerformance_CellFormatting;

            // Add grid FIRST.
            content.Controls.Add(dgvBranchPerformance);

            // --------------------------------------------------------
            // TOP CONTENT PANEL
            // --------------------------------------------------------

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = PanelBg
            };

            var title = new Label
            {
                Text = "BRANCH PERFORMANCE",
                AutoSize = true,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(0, 2)
            };

            var subtitle = new Label
            {
                Text = "Compare renewal activity and retention results across branches.",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMuted,
                Location = new Point(2, 34)
            };

            lblBranchCount = new Label
            {
                Text = "0 RECORDS",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = BrandBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            topPanel.Controls.Add(title);
            topPanel.Controls.Add(subtitle);
            topPanel.Controls.Add(lblBranchCount);

            topPanel.Resize += (_, _) =>
            {
                lblBranchCount.Location =
                    new Point(
                        topPanel.ClientSize.Width - lblBranchCount.Width,
                        34);
            };

            content.Controls.Add(topPanel);

            tab.Controls.Add(content);
        }

        // ============================================================
        // RENEWAL MONITORING TAB
        // ============================================================

        private void BuildMonitoringTab(TabPage tab)
        {
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg
            };

            dgvRenewals = CreateGrid();

            dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Tenant",
                    HeaderText = "Tenant",
                    DataPropertyName = "TenantName",
                    FillWeight = 19,
                    MinimumWidth = 145,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });

            dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Email",
                    HeaderText = "Email",
                    DataPropertyName = "Email",
                    FillWeight = 18,
                    MinimumWidth = 145,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });

            dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "StartDate",
                    HeaderText = "Start Date",
                    DataPropertyName = "StartDateDisplay",
                    FillWeight = 12,
                    MinimumWidth = 100,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = CenterCellStyle()
                });

            dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "EndDate",
                    HeaderText = "End Date",
                    DataPropertyName = "EndDateDisplay",
                    FillWeight = 12,
                    MinimumWidth = 100,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = CenterCellStyle()
                });

            dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "RenewalDate",
                    HeaderText = "Renewal Date",
                    DataPropertyName = "RenewalDateDisplay",
                    FillWeight = 13,
                    MinimumWidth = 105,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = CenterCellStyle()
                });

            dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Status",
                    DataPropertyName = "DisplayStatus",
                    FillWeight = 12,
                    MinimumWidth = 105,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
                    }
                });

            dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Notes",
                    HeaderText = "Notes",
                    DataPropertyName = "Notes",
                    FillWeight = 18,
                    MinimumWidth = 150,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });

            dgvRenewals.CellFormatting += DgvRenewals_CellFormatting;
            dgvRenewals.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    ShowRenewalDetails();
                }
            };

            dgvRenewals.SelectionChanged += (_, _) =>
            {
                // Intentionally kept for future row actions.
            };

            // Grid FIRST.
            content.Controls.Add(dgvRenewals);

            // --------------------------------------------------------
            // TOP FILTER PANEL
            // --------------------------------------------------------

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = PanelBg
            };

            var title = new Label
            {
                Text = "RENEWAL MONITORING",
                AutoSize = true,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(0, 2)
            };

            var subtitle = new Label
            {
                Text = "Search and monitor individual tenant renewal records.",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMuted,
                Location = new Point(2, 34)
            };

            var searchLabel = new Label
            {
                Text = "Search:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(0, 78)
            };

            txtRenewalSearch = new TextBox
            {
                Location = new Point(58, 72),
                Width = 270,
                Height = 30,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };

            txtRenewalSearch.TextChanged += (_, _) =>
            {
                ApplyRenewalFilters();
            };

            var branchLabel = new Label
            {
                Text = "Branch:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(345, 78)
            };

            cmbRenewalBranch = new ComboBox
            {
                Location = new Point(405, 72),
                Width = 170,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };

            cmbRenewalBranch.Items.Add("All Branches");
            cmbRenewalBranch.SelectedIndex = 0;

            cmbRenewalBranch.SelectedIndexChanged += (_, _) =>
            {
                ApplyRenewalFilters();
            };

            var statusLabel = new Label
            {
                Text = "Status:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(595, 78)
            };

            cmbRenewalStatus = new ComboBox
            {
                Location = new Point(650, 72),
                Width = 165,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };

            cmbRenewalStatus.Items.Add("All Status");
            cmbRenewalStatus.Items.Add("Upcoming");
            cmbRenewalStatus.Items.Add("Due Soon");
            cmbRenewalStatus.Items.Add("Expired");
            cmbRenewalStatus.Items.Add("Completed");
            cmbRenewalStatus.Items.Add("Approved");
            cmbRenewalStatus.Items.Add("Declined");
            cmbRenewalStatus.Items.Add("Pending");
            cmbRenewalStatus.SelectedIndex = 0;

            cmbRenewalStatus.SelectedIndexChanged += (_, _) =>
            {
                ApplyRenewalFilters();
            };

            btnClearRenewalFilters = CreateButton("Clear", 80);
            btnClearRenewalFilters.Location = new Point(830, 71);

            btnClearRenewalFilters.Click += (_, _) =>
            {
                txtRenewalSearch.Clear();
                cmbRenewalBranch.SelectedIndex = 0;
                cmbRenewalStatus.SelectedIndex = 0;
            };

            lblRenewalCount = new Label
            {
                Text = "0 RECORDS",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = BrandBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            topPanel.Controls.Add(title);
            topPanel.Controls.Add(subtitle);
            topPanel.Controls.Add(searchLabel);
            topPanel.Controls.Add(txtRenewalSearch);
            topPanel.Controls.Add(branchLabel);
            topPanel.Controls.Add(cmbRenewalBranch);
            topPanel.Controls.Add(statusLabel);
            topPanel.Controls.Add(cmbRenewalStatus);
            topPanel.Controls.Add(btnClearRenewalFilters);
            topPanel.Controls.Add(lblRenewalCount);

            topPanel.Resize += (_, _) =>
            {
                int width = topPanel.ClientSize.Width;

                if (width >= 1100)
                {
                    searchLabel.Location = new Point(0, 78);
                    txtRenewalSearch.Location = new Point(58, 72);
                    txtRenewalSearch.Width = 270;

                    branchLabel.Location = new Point(345, 78);
                    cmbRenewalBranch.Location = new Point(405, 72);
                    cmbRenewalBranch.Width = 170;

                    statusLabel.Location = new Point(595, 78);
                    cmbRenewalStatus.Location = new Point(650, 72);
                    cmbRenewalStatus.Width = 165;

                    btnClearRenewalFilters.Location = new Point(830, 71);

                    lblRenewalCount.Location =
                        new Point(width - lblRenewalCount.Width, 78);

                    topPanel.Height = 120;
                }
                else
                {
                    topPanel.Height = 155;

                    searchLabel.Location = new Point(0, 76);
                    txtRenewalSearch.Location = new Point(58, 70);
                    txtRenewalSearch.Width = Math.Max(220, width - 58);

                    branchLabel.Location = new Point(0, 113);
                    cmbRenewalBranch.Location = new Point(58, 107);
                    cmbRenewalBranch.Width = 170;

                    statusLabel.Location = new Point(245, 113);
                    cmbRenewalStatus.Location = new Point(300, 107);
                    cmbRenewalStatus.Width = 160;

                    btnClearRenewalFilters.Location = new Point(470, 106);

                    lblRenewalCount.Location =
                        new Point(width - lblRenewalCount.Width, 113);
                }
            };

            content.Controls.Add(topPanel);

            tab.Controls.Add(content);
        }

        // ============================================================
        // PROMOTIONS TAB
        // ============================================================

        private void BuildPromotionsTab(TabPage tab)
        {
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg
            };

            dgvPromotions = CreateGrid();

            dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Promotion",
                    HeaderText = "Promotion",
                    DataPropertyName = "Name",
                    FillWeight = 18,
                    MinimumWidth = 140,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });

            dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Offer",
                    HeaderText = "Offer",
                    DataPropertyName = "Offer",
                    FillWeight = 20,
                    MinimumWidth = 150,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });

            dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "ValidUntil",
                    HeaderText = "Valid Until",
                    DataPropertyName = "ValidUntilDisplay",
                    FillWeight = 13,
                    MinimumWidth = 110,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = CenterCellStyle()
                });

            dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Usage",
                    HeaderText = "Usage",
                    DataPropertyName = "UsageDisplay",
                    FillWeight = 10,
                    MinimumWidth = 80,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = CenterCellStyle()
                });

            dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Status",
                    DataPropertyName = "Status",
                    FillWeight = 12,
                    MinimumWidth = 100,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
                    }
                });

            dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Description",
                    HeaderText = "Description",
                    DataPropertyName = "Description",
                    FillWeight = 27,
                    MinimumWidth = 180,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });

            dgvPromotions.CellFormatting += DgvPromotions_CellFormatting;

            // Grid FIRST.
            content.Controls.Add(dgvPromotions);

            // --------------------------------------------------------
            // TOP PANEL
            // --------------------------------------------------------

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = PanelBg
            };

            var title = new Label
            {
                Text = "PROMOTIONS & OFFERS",
                AutoSize = true,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(0, 2)
            };

            var subtitle = new Label
            {
                Text = "Manage renewal offers and promotional campaigns for tenants.",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMuted,
                Location = new Point(2, 34)
            };

            var searchLabel = new Label
            {
                Text = "Search:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(0, 78)
            };

            txtPromotionSearch = new TextBox
            {
                Location = new Point(58, 72),
                Width = 270,
                Height = 30,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };

            txtPromotionSearch.TextChanged += (_, _) =>
            {
                ApplyPromotionFilters();
            };

            var statusLabel = new Label
            {
                Text = "Status:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(345, 78)
            };

            cmbPromotionStatus = new ComboBox
            {
                Location = new Point(400, 72),
                Width = 160,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };

            cmbPromotionStatus.Items.Add("All Status");
            cmbPromotionStatus.Items.Add("Active");
            cmbPromotionStatus.Items.Add("Expired");
            cmbPromotionStatus.Items.Add("Archived");
            cmbPromotionStatus.SelectedIndex = 0;

            cmbPromotionStatus.SelectedIndexChanged += (_, _) =>
            {
                ApplyPromotionFilters();
            };

            btnAddPromotion = CreateButton("Add Promotion", 120);
            btnEditPromotion = CreateButton("Edit", 80);
            btnArchivePromotion = CreateButton("Archive", 90);

            btnAddPromotion.Location = new Point(580, 71);
            btnEditPromotion.Location = new Point(710, 71);
            btnArchivePromotion.Location = new Point(800, 71);

            btnEditPromotion.Enabled = false;
            btnArchivePromotion.Enabled = false;

            btnAddPromotion.Click += (_, _) =>
            {
                ShowPromotionEditor(null);
            };

            btnEditPromotion.Click += (_, _) =>
            {
                if (dgvPromotions.SelectedRows.Count == 0)
                    return;

                var selected =
                    dgvPromotions.SelectedRows[0].DataBoundItem as PromotionDto;

                if (selected != null)
                {
                    ShowPromotionEditor(selected);
                }
            };

            btnArchivePromotion.Click += (_, _) =>
            {
                ArchiveSelectedPromotion();
            };

            lblPromotionCount = new Label
            {
                Text = "0 RECORDS",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = BrandBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            dgvPromotions.SelectionChanged += (_, _) =>
            {
                bool selected = dgvPromotions.SelectedRows.Count > 0;

                btnEditPromotion.Enabled = selected;
                btnArchivePromotion.Enabled = selected;
            };

            topPanel.Controls.Add(title);
            topPanel.Controls.Add(subtitle);
            topPanel.Controls.Add(searchLabel);
            topPanel.Controls.Add(txtPromotionSearch);
            topPanel.Controls.Add(statusLabel);
            topPanel.Controls.Add(cmbPromotionStatus);
            topPanel.Controls.Add(btnAddPromotion);
            topPanel.Controls.Add(btnEditPromotion);
            topPanel.Controls.Add(btnArchivePromotion);
            topPanel.Controls.Add(lblPromotionCount);

            topPanel.Resize += (_, _) =>
            {
                int width = topPanel.ClientSize.Width;

                if (width >= 1100)
                {
                    topPanel.Height = 120;

                    searchLabel.Location = new Point(0, 78);
                    txtPromotionSearch.Location = new Point(58, 72);
                    txtPromotionSearch.Width = 270;

                    statusLabel.Location = new Point(345, 78);
                    cmbPromotionStatus.Location = new Point(400, 72);

                    btnAddPromotion.Location = new Point(580, 71);
                    btnEditPromotion.Location = new Point(710, 71);
                    btnArchivePromotion.Location = new Point(800, 71);

                    lblPromotionCount.Location =
                        new Point(width - lblPromotionCount.Width, 78);
                }
                else
                {
                    topPanel.Height = 155;

                    searchLabel.Location = new Point(0, 76);
                    txtPromotionSearch.Location = new Point(58, 70);
                    txtPromotionSearch.Width =
                        Math.Max(220, width - 58);

                    statusLabel.Location = new Point(0, 113);
                    cmbPromotionStatus.Location = new Point(55, 107);

                    btnAddPromotion.Location = new Point(230, 106);
                    btnEditPromotion.Location = new Point(360, 106);
                    btnArchivePromotion.Location = new Point(450, 106);

                    lblPromotionCount.Location =
                        new Point(width - lblPromotionCount.Width, 113);
                }
            };

            content.Controls.Add(topPanel);

            tab.Controls.Add(content);

            SeedDefaultPromotions();
        }

        // ============================================================
        // METRIC CARD
        // ============================================================

        private Panel CreateMetricCard(
            string title,
            string value,
            string subtitle,
            out Label valueLabel)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(5),
                Padding = new Padding(12)
            };

            card.Paint += (_, e) =>
            {
                using Pen pen = new Pen(GridBorder);

                e.Graphics.DrawRectangle(
                    pen,
                    0,
                    0,
                    card.Width - 1,
                    card.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 20,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = TextMuted,
                TextAlign = ContentAlignment.MiddleLeft
            };

            valueLabel = new Label
            {
                Text = value,
                Dock = DockStyle.Top,
                Height = 42,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = BrandBg,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblSubtitle = new Label
            {
                Text = subtitle,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = TextMuted,
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(lblSubtitle);
            card.Controls.Add(valueLabel);
            card.Controls.Add(lblTitle);

            return card;
        }

        // ============================================================
        // WHITE PANEL
        // ============================================================

        private Panel CreateWhitePanel(
            string title,
            out Label contentLabel)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(18)
            };

            panel.Paint += (_, e) =>
            {
                using Pen pen = new Pen(GridBorder);

                e.Graphics.DrawRectangle(
                    pen,
                    0,
                    0,
                    panel.Width - 1,
                    panel.Height - 1);
            };

            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = BrandBg,
                TextAlign = ContentAlignment.MiddleLeft
            };

            contentLabel = new Label
            {
                Text = "Loading...",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextDark,
                TextAlign = ContentAlignment.TopLeft,
                AutoEllipsis = false
            };

            panel.Controls.Add(contentLabel);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        // ============================================================
        // GRID
        // ============================================================

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
                RowTemplate = new DataGridViewRow
                {
                    Height = 32
                },

                ShowCellToolTips = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,

                ScrollBars = ScrollBars.Both
            };

            typeof(DataGridView)
                .GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(grid, true, null);

            grid.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = BrandBg,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    WrapMode = DataGridViewTriState.False
                };

            grid.DefaultCellStyle =
                new DataGridViewCellStyle
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

            grid.AlternatingRowsDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = GridAlternate,
                    ForeColor = BrandBg,
                    SelectionBackColor = GridSelection,
                    SelectionForeColor = BrandBg,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    WrapMode = DataGridViewTriState.True,
                    Padding = new Padding(8, 5, 8, 5)
                };

            return grid;
        }

        private DataGridViewCellStyle CenterCellStyle()
        {
            return new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
        }

        // ============================================================
        // BUTTON
        // ============================================================

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

        // ============================================================
        // LOAD DATA
        // ============================================================

        private async Task LoadRenewalDataAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                lblLoadStatus.Text = "Loading...";

                var result =
                    await _apiService.GetAsync<List<RenewalDto>>(
                        "api/Renewals");

                _renewals = result ?? new List<RenewalDto>();

                foreach (RenewalDto renewal in _renewals)
                {
                    renewal.TenantName =
                        renewal.Tenant?.FullName
                        ?? renewal.TenantName
                        ?? $"Tenant #{renewal.TenantId}";

                    renewal.Email =
                        renewal.Tenant?.Email
                        ?? renewal.Email
                        ?? string.Empty;

                    renewal.StartDateDisplay =
                        renewal.StartDate.HasValue
                            ? renewal.StartDate.Value
                                .ToLocalTime()
                                .ToString("MMM dd, yyyy")
                            : "-";

                    renewal.EndDateDisplay =
                        renewal.EndDate.HasValue
                            ? renewal.EndDate.Value
                                .ToLocalTime()
                                .ToString("MMM dd, yyyy")
                            : "-";

                    renewal.RenewalDateDisplay =
                        renewal.RenewalDate.HasValue
                            ? renewal.RenewalDate.Value
                                .ToLocalTime()
                                .ToString("MMM dd, yyyy")
                            : "-";

                    renewal.DisplayStatus =
                        CalculateDisplayStatus(renewal);

                    renewal.BranchName = "Company-wide";
                }

                BuildBranchStatistics();

                UpdateOverview();

                PopulateBranchFilter();

                ApplyRenewalFilters();

                ApplyPromotionFilters();

                lblLoadStatus.Text =
                    $"{_renewals.Count} renewal records";

                lblHeaderDate.Text =
                    DateTime.Now.ToString("MMMM dd, yyyy");
            }
            catch (Exception ex)
            {
                lblLoadStatus.Text = "Load failed";

                MessageBox.Show(
                    $"Unable to load renewal records.\n\n{ex.Message}",
                    "Renewal & Retention",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        // ============================================================
        // STATUS CALCULATION
        // ============================================================

        private string CalculateDisplayStatus(RenewalDto renewal)
        {
            string original =
                renewal.Status?.Trim() ?? string.Empty;

            if (string.Equals(
                original,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Completed";
            }

            if (string.Equals(
                original,
                "Declined",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Declined";
            }

            if (string.Equals(
                original,
                "Approved",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Approved";
            }

            if (renewal.EndDate.HasValue)
            {
                DateTime endDate =
                    renewal.EndDate.Value.ToLocalTime().Date;

                int days =
                    (endDate - DateTime.Now.Date).Days;

                if (days < 0)
                    return "Expired";

                if (days <= 7)
                    return "Due Soon";

                return "Upcoming";
            }

            return "Pending";
        }

        // ============================================================
        // OVERVIEW
        // ============================================================

        private void UpdateOverview()
        {
            int total = _renewals.Count;

            int upcoming =
                _renewals.Count(x =>
                    string.Equals(
                        x.DisplayStatus,
                        "Upcoming",
                        StringComparison.OrdinalIgnoreCase));

            int dueSoon =
                _renewals.Count(x =>
                    string.Equals(
                        x.DisplayStatus,
                        "Due Soon",
                        StringComparison.OrdinalIgnoreCase));

            int expired =
                _renewals.Count(x =>
                    string.Equals(
                        x.DisplayStatus,
                        "Expired",
                        StringComparison.OrdinalIgnoreCase));

            int completed =
                _renewals.Count(x =>
                    string.Equals(
                        x.DisplayStatus,
                        "Completed",
                        StringComparison.OrdinalIgnoreCase));

            int retentionBase =
                completed + expired;

            double retention =
                retentionBase == 0
                    ? 0
                    : completed * 100.0 / retentionBase;

            lblTotalRenewals.Text = total.ToString();
            lblUpcoming.Text = upcoming.ToString();
            lblDueSoon.Text = dueSoon.ToString();
            lblExpired.Text = expired.ToString();
            lblCompleted.Text = completed.ToString();
            lblRetentionRate.Text =
                $"{retention:0.#}%";

            lblOverviewActivity.Text =
                $"Total renewal records: {total}\r\n\r\n" +
                $"Upcoming renewals: {upcoming}\r\n" +
                $"Due within 7 days: {dueSoon}\r\n" +
                $"Expired renewals: {expired}\r\n" +
                $"Completed renewals: {completed}";

            lblOverviewInsights.Text =
                $"Completed renewals: {completed}\r\n" +
                $"Expired renewals: {expired}\r\n\r\n" +
                $"Current retention rate: {retention:0.#}%\r\n\r\n" +
                GetRetentionInsight(retention);
        }

        private string GetRetentionInsight(double retention)
        {
            if (retention <= 0)
            {
                return "No completed renewal activity is currently available.";
            }

            if (retention < 50)
            {
                return "Review expired renewals and follow up with tenants who have not renewed.";
            }

            if (retention < 75)
            {
                return "Renewal activity is being recorded. Continue monitoring upcoming and due-soon tenants.";
            }

            return "Most completed or expired renewal records are currently completed.";
        }

        // ============================================================
        // BRANCH STATISTICS
        // ============================================================

        private void BuildBranchStatistics()
        {
            _branchRecords = new List<BranchRenewalRecord>();

            if (_renewals.Count == 0)
                return;

            var grouped =
                _renewals
                    .GroupBy(x =>
                        string.IsNullOrWhiteSpace(x.BranchName)
                            ? "Company-wide"
                            : x.BranchName)
                    .OrderBy(x => x.Key);

            foreach (var group in grouped)
            {
                int total = group.Count();

                int upcoming =
                    group.Count(x =>
                        x.DisplayStatus == "Upcoming");

                int dueSoon =
                    group.Count(x =>
                        x.DisplayStatus == "Due Soon");

                int expired =
                    group.Count(x =>
                        x.DisplayStatus == "Expired");

                int completed =
                    group.Count(x =>
                        x.DisplayStatus == "Completed");

                int retentionBase =
                    completed + expired;

                double retention =
                    retentionBase == 0
                        ? 0
                        : completed * 100.0 / retentionBase;

                _branchRecords.Add(
                    new BranchRenewalRecord
                    {
                        BranchName = group.Key,
                        TotalRenewals = total,
                        Upcoming = upcoming,
                        DueSoon = dueSoon,
                        Expired = expired,
                        Completed = completed,
                        RetentionRate = $"{retention:0.#}%"
                    });
            }

            dgvBranchPerformance.DataSource = null;
            dgvBranchPerformance.DataSource = _branchRecords;

            lblBranchCount.Text =
                _branchRecords.Count == 1
                    ? "1 RECORD"
                    : $"{_branchRecords.Count} RECORDS";
        }

        // ============================================================
        // BRANCH FILTER
        // ============================================================

        private void PopulateBranchFilter()
        {
            if (cmbRenewalBranch == null)
                return;

            string current =
                cmbRenewalBranch.SelectedItem?.ToString()
                ?? "All Branches";

            cmbRenewalBranch.Items.Clear();

            cmbRenewalBranch.Items.Add("All Branches");

            foreach (string branch in
                _renewals
                    .Select(x =>
                        string.IsNullOrWhiteSpace(x.BranchName)
                            ? "Company-wide"
                            : x.BranchName)
                    .Distinct()
                    .OrderBy(x => x))
            {
                cmbRenewalBranch.Items.Add(branch);
            }

            int index =
                cmbRenewalBranch.Items.IndexOf(current);

            cmbRenewalBranch.SelectedIndex =
                index >= 0 ? index : 0;
        }

        // ============================================================
        // RENEWAL FILTERS
        // ============================================================

        private void ApplyRenewalFilters()
        {
            if (dgvRenewals == null)
                return;

            string search =
                txtRenewalSearch?.Text
                    .Trim()
                    .ToLowerInvariant()
                ?? string.Empty;

            string selectedBranch =
                cmbRenewalBranch?.SelectedItem?.ToString()
                ?? "All Branches";

            string selectedStatus =
                cmbRenewalStatus?.SelectedItem?.ToString()
                ?? "All Status";

            IEnumerable<RenewalDto> filtered =
                _renewals;

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered =
                    filtered.Where(x =>
                        (x.TenantName ?? string.Empty)
                            .ToLowerInvariant()
                            .Contains(search)
                        ||
                        (x.Email ?? string.Empty)
                            .ToLowerInvariant()
                            .Contains(search)
                        ||
                        (x.Notes ?? string.Empty)
                            .ToLowerInvariant()
                            .Contains(search)
                        ||
                        (x.DisplayStatus ?? string.Empty)
                            .ToLowerInvariant()
                            .Contains(search)
                        ||
                        (x.BranchName ?? string.Empty)
                            .ToLowerInvariant()
                            .Contains(search));
            }

            if (!string.Equals(
                selectedBranch,
                "All Branches",
                StringComparison.OrdinalIgnoreCase))
            {
                filtered =
                    filtered.Where(x =>
                        string.Equals(
                            x.BranchName,
                            selectedBranch,
                            StringComparison.OrdinalIgnoreCase));
            }

            if (!string.Equals(
                selectedStatus,
                "All Status",
                StringComparison.OrdinalIgnoreCase))
            {
                filtered =
                    filtered.Where(x =>
                        string.Equals(
                            x.DisplayStatus,
                            selectedStatus,
                            StringComparison.OrdinalIgnoreCase));
            }

            List<RenewalDto> display =
                filtered.ToList();

            dgvRenewals.DataSource = null;
            dgvRenewals.DataSource = display;

            dgvRenewals.ClearSelection();

            lblRenewalCount.Text =
                display.Count == 1
                    ? "1 RECORD"
                    : $"{display.Count} RECORDS";
        }

        // ============================================================
        // GRID STATUS FORMATTING
        // ============================================================

        private void DgvRenewals_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            string columnName =
                dgvRenewals.Columns[e.ColumnIndex].Name;

            if (columnName != "Status")
                return;

            string status =
                e.Value?.ToString()?.Trim()
                ?? string.Empty;

            e.CellStyle.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            e.CellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            switch (status.ToLowerInvariant())
            {
                case "completed":
                    e.CellStyle.BackColor =
                        Color.FromArgb(218, 242, 224);

                    e.CellStyle.ForeColor =
                        SuccessColor;

                    e.CellStyle.SelectionBackColor =
                        Color.FromArgb(218, 242, 224);

                    e.CellStyle.SelectionForeColor =
                        SuccessColor;
                    break;

                case "due soon":
                    e.CellStyle.BackColor =
                        Color.FromArgb(255, 244, 204);

                    e.CellStyle.ForeColor =
                        WarningColor;

                    e.CellStyle.SelectionBackColor =
                        Color.FromArgb(255, 244, 204);

                    e.CellStyle.SelectionForeColor =
                        WarningColor;
                    break;

                case "expired":
                    e.CellStyle.BackColor =
                        Color.FromArgb(250, 222, 222);

                    e.CellStyle.ForeColor =
                        DangerColor;

                    e.CellStyle.SelectionBackColor =
                        Color.FromArgb(250, 222, 222);

                    e.CellStyle.SelectionForeColor =
                        DangerColor;
                    break;

                case "upcoming":
                    e.CellStyle.BackColor =
                        Color.FromArgb(218, 235, 252);

                    e.CellStyle.ForeColor =
                        InfoColor;

                    e.CellStyle.SelectionBackColor =
                        Color.FromArgb(218, 235, 252);

                    e.CellStyle.SelectionForeColor =
                        InfoColor;
                    break;

                case "approved":
                    e.CellStyle.BackColor =
                        Color.FromArgb(230, 240, 250);

                    e.CellStyle.ForeColor =
                        InfoColor;

                    e.CellStyle.SelectionBackColor =
                        Color.FromArgb(230, 240, 250);

                    e.CellStyle.SelectionForeColor =
                        InfoColor;
                    break;

                default:
                    e.CellStyle.BackColor = Color.White;
                    e.CellStyle.ForeColor = BrandBg;
                    e.CellStyle.SelectionBackColor =
                        GridSelection;
                    e.CellStyle.SelectionForeColor =
                        BrandBg;
                    break;
            }
        }

        private void DgvBranchPerformance_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            string columnName =
                dgvBranchPerformance.Columns[e.ColumnIndex].Name;

            if (columnName != "Retention")
                return;

            e.CellStyle.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            e.CellStyle.ForeColor =
                SuccessColor;

            e.CellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
        }

        // ============================================================
        // PROMOTION FILTERS
        // ============================================================

        private void ApplyPromotionFilters()
        {
            if (dgvPromotions == null)
                return;

            string search =
                txtPromotionSearch?.Text
                    .Trim()
                    .ToLowerInvariant()
                ?? string.Empty;

            string selectedStatus =
                cmbPromotionStatus?.SelectedItem?.ToString()
                ?? "All Status";

            IEnumerable<PromotionDto> filtered =
                _promotions;

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered =
                    filtered.Where(x =>
                        (x.Name ?? string.Empty)
                            .ToLowerInvariant()
                            .Contains(search)
                        ||
                        (x.Offer ?? string.Empty)
                            .ToLowerInvariant()
                            .Contains(search)
                        ||
                        (x.Description ?? string.Empty)
                            .ToLowerInvariant()
                            .Contains(search));
            }

            if (!string.Equals(
                selectedStatus,
                "All Status",
                StringComparison.OrdinalIgnoreCase))
            {
                filtered =
                    filtered.Where(x =>
                        string.Equals(
                            x.Status,
                            selectedStatus,
                            StringComparison.OrdinalIgnoreCase));
            }

            List<PromotionDto> display =
                filtered.ToList();

            dgvPromotions.DataSource = null;
            dgvPromotions.DataSource = display;

            dgvPromotions.ClearSelection();

            btnEditPromotion.Enabled = false;
            btnArchivePromotion.Enabled = false;

            lblPromotionCount.Text =
                display.Count == 1
                    ? "1 RECORD"
                    : $"{display.Count} RECORDS";
        }

        // ============================================================
        // PROMOTION FORMATTING
        // ============================================================

        private void DgvPromotions_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            string columnName =
                dgvPromotions.Columns[e.ColumnIndex].Name;

            if (columnName != "Status")
                return;

            string status =
                e.Value?.ToString()?.Trim()
                ?? string.Empty;

            e.CellStyle.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            e.CellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            switch (status.ToLowerInvariant())
            {
                case "active":
                    e.CellStyle.BackColor =
                        Color.FromArgb(218, 242, 224);

                    e.CellStyle.ForeColor =
                        SuccessColor;
                    break;

                case "expired":
                    e.CellStyle.BackColor =
                        Color.FromArgb(255, 244, 204);

                    e.CellStyle.ForeColor =
                        WarningColor;
                    break;

                case "archived":
                    e.CellStyle.BackColor =
                        Color.FromArgb(235, 231, 226);

                    e.CellStyle.ForeColor =
                        TextMuted;
                    break;

                default:
                    e.CellStyle.BackColor = Color.White;
                    e.CellStyle.ForeColor = BrandBg;
                    break;
            }
        }

        // ============================================================
        // DEFAULT PROMOTIONS
        // ============================================================

        private void SeedDefaultPromotions()
        {
            _promotions = new List<PromotionDto>
            {
                new PromotionDto
                {
                    Id = 1,
                    Name = "Early Renewal",
                    Offer = "5% Renewal Discount",
                    ValidUntil = DateTime.Now.AddDays(30),
                    Usage = 0,
                    Status = "Active",
                    Description =
                        "Discount available to tenants who renew before their current stay expires."
                },

                new PromotionDto
                {
                    Id = 2,
                    Name = "Long Stay Renewal",
                    Offer = "10% Renewal Discount",
                    ValidUntil = DateTime.Now.AddDays(60),
                    Usage = 0,
                    Status = "Active",
                    Description =
                        "Offer for tenants who commit to a longer renewal period."
                },

                new PromotionDto
                {
                    Id = 3,
                    Name = "Returning Tenant",
                    Offer = "Special Renewal Rate",
                    ValidUntil = DateTime.Now.AddDays(90),
                    Usage = 0,
                    Status = "Active",
                    Description =
                        "Special rate for qualified returning tenants."
                }
            };

            foreach (PromotionDto promotion in _promotions)
            {
                promotion.ValidUntilDisplay =
                    promotion.ValidUntil.ToString(
                        "MMM dd, yyyy");

                promotion.UsageDisplay =
                    promotion.Usage.ToString();
            }

            if (dgvPromotions != null)
            {
                ApplyPromotionFilters();
            }
        }

        // ============================================================
        // PROMOTION EDITOR
        // ============================================================

        private void ShowPromotionEditor(
            PromotionDto? existing)
        {
            bool isEdit = existing != null;

            using Form form = new Form
            {
                Text = isEdit
                    ? "Edit Promotion"
                    : "Add Promotion",

                StartPosition =
                    FormStartPosition.CenterParent,

                FormBorderStyle =
                    FormBorderStyle.FixedDialog,

                MaximizeBox = false,
                MinimizeBox = false,

                ClientSize =
                    new Size(560, 470),

                BackColor = PanelBg
            };

            Label title = new Label
            {
                Text = isEdit
                    ? "EDIT PROMOTION"
                    : "ADD PROMOTION",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        17F,
                        FontStyle.Bold),

                ForeColor = BrandBg,

                Location =
                    new Point(30, 25)
            };

            Label lblName =
                CreateEditorLabel(
                    "Promotion Name",
                    30,
                    75);

            TextBox txtName =
                new TextBox
                {
                    Location = new Point(30, 98),
                    Width = 500,
                    Height = 30,
                    Font = new Font("Segoe UI", 9.5F)
                };

            Label lblOffer =
                CreateEditorLabel(
                    "Offer",
                    30,
                    140);

            TextBox txtOffer =
                new TextBox
                {
                    Location = new Point(30, 163),
                    Width = 500,
                    Height = 30,
                    Font = new Font("Segoe UI", 9.5F)
                };

            Label lblValidUntil =
                CreateEditorLabel(
                    "Valid Until",
                    30,
                    205);

            DateTimePicker dtpValidUntil =
                new DateTimePicker
                {
                    Location = new Point(30, 228),
                    Width = 220,
                    Height = 30,
                    Format = DateTimePickerFormat.Short,
                    Font = new Font("Segoe UI", 9.5F)
                };

            Label lblStatus =
                CreateEditorLabel(
                    "Status",
                    280,
                    205);

            ComboBox cmbStatus =
                new ComboBox
                {
                    Location = new Point(280, 228),
                    Width = 250,
                    Height = 30,
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 9.5F)
                };

            cmbStatus.Items.Add("Active");
            cmbStatus.Items.Add("Expired");
            cmbStatus.Items.Add("Archived");

            Label lblDescription =
                CreateEditorLabel(
                    "Description",
                    30,
                    270);

            TextBox txtDescription =
                new TextBox
                {
                    Location = new Point(30, 293),
                    Width = 500,
                    Height = 70,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("Segoe UI", 9.5F)
                };

            Button btnCancel =
                CreateButton(
                    "Cancel",
                    100);

            btnCancel.Location =
                new Point(320, 415);

            btnCancel.DialogResult =
                DialogResult.Cancel;

            Button btnSave =
                CreateButton(
                    isEdit
                        ? "Save Changes"
                        : "Add Promotion",
                    140);

            btnSave.Location =
                new Point(420, 415);

            if (existing != null)
            {
                txtName.Text =
                    existing.Name;

                txtOffer.Text =
                    existing.Offer;

                dtpValidUntil.Value =
                    existing.ValidUntil;

                txtDescription.Text =
                    existing.Description;

                int statusIndex =
                    cmbStatus.Items.IndexOf(
                        existing.Status);

                cmbStatus.SelectedIndex =
                    statusIndex >= 0
                        ? statusIndex
                        : 0;
            }
            else
            {
                cmbStatus.SelectedIndex = 0;
                dtpValidUntil.Value =
                    DateTime.Now.AddDays(30);
            }

            btnSave.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(
                    txtName.Text))
                {
                    MessageBox.Show(
                        "Please enter a promotion name.",
                        "Promotion",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(
                    txtOffer.Text))
                {
                    MessageBox.Show(
                        "Please enter the offer.",
                        "Promotion",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtOffer.Focus();
                    return;
                }

                if (existing == null)
                {
                    int nextId =
                        _promotions.Count == 0
                            ? 1
                            : _promotions.Max(x => x.Id) + 1;

                    existing =
                        new PromotionDto
                        {
                            Id = nextId,
                            Usage = 0
                        };

                    _promotions.Add(existing);
                }

                existing.Name =
                    txtName.Text.Trim();

                existing.Offer =
                    txtOffer.Text.Trim();

                existing.ValidUntil =
                    dtpValidUntil.Value;

                existing.Status =
                    cmbStatus.SelectedItem?.ToString()
                    ?? "Active";

                existing.Description =
                    txtDescription.Text.Trim();

                existing.ValidUntilDisplay =
                    existing.ValidUntil.ToString(
                        "MMM dd, yyyy");

                existing.UsageDisplay =
                    existing.Usage.ToString();

                ApplyPromotionFilters();

                form.DialogResult =
                    DialogResult.OK;

                form.Close();
            };

            form.Controls.Add(title);
            form.Controls.Add(lblName);
            form.Controls.Add(txtName);
            form.Controls.Add(lblOffer);
            form.Controls.Add(txtOffer);
            form.Controls.Add(lblValidUntil);
            form.Controls.Add(dtpValidUntil);
            form.Controls.Add(lblStatus);
            form.Controls.Add(cmbStatus);
            form.Controls.Add(lblDescription);
            form.Controls.Add(txtDescription);
            form.Controls.Add(btnCancel);
            form.Controls.Add(btnSave);

            form.CancelButton = btnCancel;

            form.ShowDialog(this);
        }

        // ============================================================
        // ARCHIVE PROMOTION
        // ============================================================

        private void ArchiveSelectedPromotion()
        {
            if (dgvPromotions.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select a promotion first.",
                    "Promotion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            PromotionDto? selected =
                dgvPromotions
                    .SelectedRows[0]
                    .DataBoundItem as PromotionDto;

            if (selected == null)
                return;

            DialogResult result =
                MessageBox.Show(
                    $"Archive the promotion \"{selected.Name}\"?",
                    "Archive Promotion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            selected.Status = "Archived";

            selected.ValidUntilDisplay =
                selected.ValidUntil.ToString(
                    "MMM dd, yyyy");

            ApplyPromotionFilters();
        }

        // ============================================================
        // RENEWAL DETAILS
        // ============================================================

        private void ShowRenewalDetails()
        {
            if (dgvRenewals.SelectedRows.Count == 0)
            {
                return;
            }

            RenewalDto? selected =
                dgvRenewals
                    .SelectedRows[0]
                    .DataBoundItem as RenewalDto;

            if (selected == null)
                return;

            string startDate =
                selected.StartDate.HasValue
                    ? selected.StartDate.Value
                        .ToLocalTime()
                        .ToString("MMM dd, yyyy")
                    : "Not available";

            string endDate =
                selected.EndDate.HasValue
                    ? selected.EndDate.Value
                        .ToLocalTime()
                        .ToString("MMM dd, yyyy")
                    : "Not available";

            string renewalDate =
                selected.RenewalDate.HasValue
                    ? selected.RenewalDate.Value
                        .ToLocalTime()
                        .ToString("MMM dd, yyyy")
                    : "Not available";

            using Form form = new Form
            {
                Text = "Renewal Details",
                StartPosition =
                    FormStartPosition.CenterParent,

                FormBorderStyle =
                    FormBorderStyle.FixedDialog,

                MaximizeBox = false,
                MinimizeBox = false,

                ClientSize =
                    new Size(600, 500),

                BackColor = PanelBg
            };

            Label title =
                CreateDetailsLabel(
                    "RENEWAL DETAILS",
                    30,
                    25,
                    17F,
                    FontStyle.Bold);

            Label tenant =
                CreateDetailsLabel(
                    $"Tenant: {selected.TenantName}",
                    30,
                    75,
                    10F,
                    FontStyle.Bold);

            Label email =
                CreateDetailsLabel(
                    $"Email: {selected.Email}",
                    30,
                    108,
                    9.5F,
                    FontStyle.Regular);

            Label branch =
                CreateDetailsLabel(
                    $"Branch: {selected.BranchName}",
                    30,
                    138,
                    9.5F,
                    FontStyle.Regular);

            Label status =
                CreateDetailsLabel(
                    $"Status: {selected.DisplayStatus}",
                    30,
                    168,
                    9.5F,
                    FontStyle.Bold);

            ApplyStatusLabelColor(
                status,
                selected.DisplayStatus);

            Label start =
                CreateDetailsLabel(
                    $"Start Date: {startDate}",
                    30,
                    200,
                    9.5F,
                    FontStyle.Regular);

            Label end =
                CreateDetailsLabel(
                    $"End Date: {endDate}",
                    30,
                    230,
                    9.5F,
                    FontStyle.Regular);

            Label renewal =
                CreateDetailsLabel(
                    $"Renewal Date: {renewalDate}",
                    30,
                    260,
                    9.5F,
                    FontStyle.Regular);

            Label notesTitle =
                CreateDetailsLabel(
                    "Notes:",
                    30,
                    300,
                    9.5F,
                    FontStyle.Bold);

            TextBox notes =
                new TextBox
                {
                    Location = new Point(30, 325),
                    Width = 540,
                    Height = 90,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("Segoe UI", 9.5F),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Text =
                        string.IsNullOrWhiteSpace(
                            selected.Notes)
                            ? "No notes available."
                            : selected.Notes
                };

            Button btnClose =
                CreateButton(
                    "Close",
                    100);

            btnClose.Location =
                new Point(470, 445);

            btnClose.DialogResult =
                DialogResult.Cancel;

            form.Controls.Add(title);
            form.Controls.Add(tenant);
            form.Controls.Add(email);
            form.Controls.Add(branch);
            form.Controls.Add(status);
            form.Controls.Add(start);
            form.Controls.Add(end);
            form.Controls.Add(renewal);
            form.Controls.Add(notesTitle);
            form.Controls.Add(notes);
            form.Controls.Add(btnClose);

            form.CancelButton =
                btnClose;

            form.ShowDialog(this);
        }

        // ============================================================
        // STATUS LABEL
        // ============================================================

        private void ApplyStatusLabelColor(
            Label label,
            string? status)
        {
            switch (
                status?
                    .Trim()
                    .ToLowerInvariant())
            {
                case "completed":
                    label.ForeColor =
                        SuccessColor;
                    break;

                case "due soon":
                    label.ForeColor =
                        WarningColor;
                    break;

                case "expired":
                    label.ForeColor =
                        DangerColor;
                    break;

                case "upcoming":
                case "approved":
                    label.ForeColor =
                        InfoColor;
                    break;

                default:
                    label.ForeColor =
                        BrandBg;
                    break;
            }
        }

        // ============================================================
        // LABEL HELPERS
        // ============================================================

        private Label CreateEditorLabel(
            string text,
            int x,
            int y)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                ForeColor = BrandBg,
                Location = new Point(x, y)
            };
        }

        private Label CreateDetailsLabel(
            string text,
            int x,
            int y,
            float fontSize,
            FontStyle fontStyle)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font =
                    new Font(
                        "Segoe UI",
                        fontSize,
                        fontStyle),
                ForeColor = BrandBg,
                Location = new Point(x, y)
            };
        }

        // ============================================================
        // TAB UPDATE
        // ============================================================

        private void UpdateCurrentTab()
        {
            if (tabMain == null)
                return;

            if (tabMain.SelectedIndex == 0)
            {
                UpdateOverview();
            }
            else if (tabMain.SelectedIndex == 1)
            {
                BuildBranchStatistics();
            }
            else if (tabMain.SelectedIndex == 2)
            {
                ApplyRenewalFilters();
            }
            else if (tabMain.SelectedIndex == 3)
            {
                ApplyPromotionFilters();
            }
        }

        // ============================================================
        // DTOs
        // ============================================================

        public class RenewalDto
        {
            public int Id { get; set; }

            public int TenantId { get; set; }

            public string TenantName { get; set; } =
                string.Empty;

            public string Email { get; set; } =
                string.Empty;

            public string Status { get; set; } =
                "Pending";

            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }

            public DateTime? RenewalDate { get; set; }

            public string? Notes { get; set; }

            public RenewalTenantDto? Tenant { get; set; }

            public string BranchName { get; set; } =
                "Company-wide";

            public string DisplayStatus { get; set; } =
                string.Empty;

            public string StartDateDisplay { get; set; } =
                string.Empty;

            public string EndDateDisplay { get; set; } =
                string.Empty;

            public string RenewalDateDisplay { get; set; } =
                string.Empty;
        }

        public class RenewalTenantDto
        {
            public int Id { get; set; }

            public string FullName { get; set; } =
                string.Empty;

            public string Email { get; set; } =
                string.Empty;
        }

        public class BranchRenewalRecord
        {
            public string BranchName { get; set; } =
                string.Empty;

            public int TotalRenewals { get; set; }

            public int Upcoming { get; set; }

            public int DueSoon { get; set; }

            public int Expired { get; set; }

            public int Completed { get; set; }

            public string RetentionRate { get; set; } =
                "0%";
        }

        public class PromotionDto
        {
            public int Id { get; set; }

            public string Name { get; set; } =
                string.Empty;

            public string Offer { get; set; } =
                string.Empty;

            public DateTime ValidUntil { get; set; }

            public int Usage { get; set; }

            public string Status { get; set; } =
                "Active";

            public string Description { get; set; } =
                string.Empty;

            public string ValidUntilDisplay { get; set; } =
                string.Empty;

            public string UsageDisplay { get; set; } =
                string.Empty;
        }
    }
}
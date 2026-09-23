using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PBCRM2
{
    // ============================================================
    // MANAGER RENEWAL & RETENTION
    // ============================================================

    public class ManagerRenewalRetentionForm : Form
    {
        // ========================================================
        // COLORS
        // ========================================================

        private static readonly Color BrandBg =
            Color.FromArgb(32, 24, 18);

        private static readonly Color BrandAccent =
            Color.FromArgb(224, 194, 140);

        private static readonly Color CtaColor =
            Color.FromArgb(170, 130, 80);

        private static readonly Color ContentBg =
            Color.FromArgb(247, 244, 239);

        private static readonly Color CardBg =
            Color.White;

        private static readonly Color TextDark =
            Color.FromArgb(55, 39, 20);

        private static readonly Color TextMuted =
            Color.FromArgb(120, 120, 120);

        private static readonly Color BorderColor =
            Color.FromArgb(225, 215, 200);

        private static readonly Color SuccessColor =
            Color.FromArgb(54, 122, 77);

        private static readonly Color WarningColor =
            Color.FromArgb(190, 130, 45);

        private static readonly Color DangerColor =
            Color.FromArgb(180, 70, 70);

        private static readonly Color InfoColor =
            Color.FromArgb(74, 110, 145);

        // ========================================================
        // SERVICES
        // ========================================================

        private readonly ApiService _apiService;

        // ========================================================
        // MAIN CONTROLS
        // ========================================================

        private Panel pnlMain = null!;
        private Panel pnlHeader = null!;

        private Label lblTitle = null!;
        private Label lblSubtitle = null!;
        private Label lblDate = null!;

        private Button btnRefresh = null!;

        private TabControl tabMain = null!;

        // ========================================================
        // OVERVIEW
        // ========================================================

        private TabPage tabOverview = null!;

        private Label lblTotalValue = null!;
        private Label lblUpcomingValue = null!;
        private Label lblDueValue = null!;
        private Label lblExpiredValue = null!;
        private Label lblRenewedValue = null!;
        private Label lblRetentionRateValue = null!;

        private Panel pnlRenewalTrend = null!;
        private Panel pnlRetentionInsights = null!;

        // ========================================================
        // FOLLOW-UP
        // ========================================================

        private TabPage tabFollowUp = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbStatus = null!;
        private DataGridView dgvRenewals = null!;

        private Label lblFollowUpCount = null!;

        // ========================================================
        // PROMOTIONS
        // ========================================================

        private TabPage tabPromotions = null!;

        private DataGridView dgvPromotions = null!;

        private TextBox txtPromotionSearch = null!;

        private ComboBox cmbPromotionStatus = null!;

        private Label lblPromotionCount = null!;

        private Button btnAddPromotion = null!;
        private Button btnEditPromotion = null!;
        private Button btnArchivePromotion = null!;

        // ========================================================
        // DATA
        // ========================================================

        private readonly List<RenewalRecord> _renewals = new();

        private readonly List<PromotionRecord> _promotions = new();

        // ========================================================
        // CONSTRUCTOR
        // ========================================================

        public ManagerRenewalRetentionForm(
            ApiService apiService)
        {
            this._apiService =
                apiService
                ?? throw new ArgumentNullException(
                    nameof(apiService)
                );

            this.BuildForm();
        }

        // ========================================================
        // BUILD FORM
        // ========================================================

        private void BuildForm()
        {
            this.Text =
                "PBCRM2 - Renewal & Retention";

            this.StartPosition =
                FormStartPosition.CenterScreen;

            this.ClientSize =
                new Size(1150, 760);

            this.MinimumSize =
                new Size(950, 650);

            this.BackColor =
                ContentBg;

            this.FormBorderStyle =
                FormBorderStyle.None;

            this.DoubleBuffered = true;

            this.BuildHeader();

            this.BuildTabs();

            this.LoadDefaultPromotions();

            this.Resize +=
                this.ManagerRenewalRetentionForm_Resize;

            this.Shown += async (sender, e) =>
            {
                this.PerformLayoutResponsive();

                await this.LoadRenewalsAsync();
            };
        }

        // ========================================================
        // HEADER
        // ========================================================

        private void BuildHeader()
        {
            this.pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 105,
                BackColor = ContentBg
            };

            this.pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentBg,
                AutoScroll = true
            };

            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);

            this.lblTitle = new Label
            {
                AutoSize = true,
                Text = "Renewal & Retention",
                Font = new Font(
                    "Segoe UI",
                    21F,
                    FontStyle.Bold
                ),
                ForeColor = TextDark,
                Location = new Point(30, 20)
            };

            this.lblSubtitle = new Label
            {
                AutoSize = true,
                Text =
                    "Keep tenants longer through renewal follow-up, offers, and retention activities.",
                Font = new Font(
                    "Segoe UI",
                    9.5F
                ),
                ForeColor = TextMuted,
                Location = new Point(32, 59)
            };

            this.lblDate = new Label
            {
                AutoSize = true,
                Text = DateTime.Now.ToString(
                    "MMMM dd, yyyy"
                ),
                Font = new Font(
                    "Segoe UI",
                    9F
                ),
                ForeColor = TextMuted
            };

            this.btnRefresh =
                this.CreateButton(
                    "Refresh",
                    CtaColor,
                    Color.White
                );

            this.btnRefresh.Click +=
                async (sender, e) =>
                {
                    await this.LoadRenewalsAsync();
                };

            this.pnlHeader.Controls.Add(
                this.lblTitle
            );

            this.pnlHeader.Controls.Add(
                this.lblSubtitle
            );

            this.pnlHeader.Controls.Add(
                this.lblDate
            );

            this.pnlHeader.Controls.Add(
                this.btnRefresh
            );
        }

        // ========================================================
        // TABS
        // ========================================================

        private void BuildTabs()
        {
            this.tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font(
                    "Segoe UI",
                    9F
                ),
                Padding = new Point(18, 6),
                BackColor = ContentBg
            };

            this.tabOverview =
                new TabPage(
                    "Overview"
                );

            this.tabOverview.BackColor =
                ContentBg;

            this.tabFollowUp =
                new TabPage(
                    "Renewal Follow-up"
                );

            this.tabFollowUp.BackColor =
                ContentBg;

            this.tabPromotions =
                new TabPage(
                    "Promotions & Offers"
                );

            this.tabPromotions.BackColor =
                ContentBg;

            this.BuildOverviewTab();

            this.BuildFollowUpTab();

            this.BuildPromotionsTab();

            this.tabMain.TabPages.Add(
                this.tabOverview
            );

            this.tabMain.TabPages.Add(
                this.tabFollowUp
            );

            this.tabMain.TabPages.Add(
                this.tabPromotions
            );

            this.pnlMain.Controls.Add(
                this.tabMain
            );
        }

        // ========================================================
        // OVERVIEW TAB
        // ========================================================

        private void BuildOverviewTab()
        {
            Panel summaryPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 125,
                BackColor = ContentBg
            };

            this.tabOverview.Controls.Add(
                summaryPanel
            );

            Panel total =
                this.CreateMetricCard(
                    "TOTAL RENEWALS",
                    "0",
                    "Renewal records"
                );

            Panel upcoming =
                this.CreateMetricCard(
                    "UPCOMING",
                    "0",
                    "Within 30 days"
                );

            Panel due =
                this.CreateMetricCard(
                    "DUE / PENDING",
                    "0",
                    "Needs follow-up"
                );

            Panel expired =
                this.CreateMetricCard(
                    "EXPIRED",
                    "0",
                    "Retention follow-up"
                );

            Panel renewed =
                this.CreateMetricCard(
                    "RENEWED",
                    "0",
                    "Successfully retained"
                );

            Panel retention =
                this.CreateMetricCard(
                    "RETENTION RATE",
                    "0%",
                    "Based on renewal records"
                );

            this.lblTotalValue =
                this.GetMetricValue(total);

            this.lblUpcomingValue =
                this.GetMetricValue(upcoming);

            this.lblDueValue =
                this.GetMetricValue(due);

            this.lblExpiredValue =
                this.GetMetricValue(expired);

            this.lblRenewedValue =
                this.GetMetricValue(renewed);

            this.lblRetentionRateValue =
                this.GetMetricValue(retention);

            summaryPanel.Controls.Add(total);
            summaryPanel.Controls.Add(upcoming);
            summaryPanel.Controls.Add(due);
            summaryPanel.Controls.Add(expired);
            summaryPanel.Controls.Add(renewed);
            summaryPanel.Controls.Add(retention);

            // ----------------------------------------------------
            // RENEWAL ACTIVITY PANEL
            // ----------------------------------------------------

            this.pnlRenewalTrend =
                this.CreateWhitePanel();

            this.pnlRenewalTrend.Location =
                new Point(20, 140);

            this.pnlRenewalTrend.Size =
                new Size(520, 390);

            Label trendTitle = new Label
            {
                AutoSize = true,
                Text = "Renewal Activity",
                Font = new Font(
                    "Segoe UI",
                    13F,
                    FontStyle.Bold
                ),
                ForeColor = TextDark,
                Location = new Point(20, 18)
            };

            Label trendSubtitle = new Label
            {
                AutoSize = true,
                Text = "Current renewal distribution",
                Font = new Font(
                    "Segoe UI",
                    8.5F
                ),
                ForeColor = TextMuted,
                Location = new Point(20, 45)
            };

            this.pnlRenewalTrend.Controls.Add(
                trendTitle
            );

            this.pnlRenewalTrend.Controls.Add(
                trendSubtitle
            );

            // ----------------------------------------------------
            // RETENTION INSIGHTS PANEL
            // ----------------------------------------------------

            this.pnlRetentionInsights =
                this.CreateWhitePanel();

            this.pnlRetentionInsights.Location =
                new Point(560, 140);

            this.pnlRetentionInsights.Size =
                new Size(520, 390);

            Label insightTitle = new Label
            {
                AutoSize = true,
                Text = "Retention Insights",
                Font = new Font(
                    "Segoe UI",
                    13F,
                    FontStyle.Bold
                ),
                ForeColor = TextDark,
                Location = new Point(20, 18)
            };

            Label insightSubtitle = new Label
            {
                AutoSize = true,
                Text =
                    "Areas that may require Manager attention",
                Font = new Font(
                    "Segoe UI",
                    8.5F
                ),
                ForeColor = TextMuted,
                Location = new Point(20, 45)
            };

            this.pnlRetentionInsights.Controls.Add(
                insightTitle
            );

            this.pnlRetentionInsights.Controls.Add(
                insightSubtitle
            );

            this.tabOverview.Controls.Add(
                this.pnlRenewalTrend
            );

            this.tabOverview.Controls.Add(
                this.pnlRetentionInsights
            );

            this.BuildOverviewPlaceholders();
        }

        // ========================================================
        // OVERVIEW PLACEHOLDERS
        // ========================================================

        private void BuildOverviewPlaceholders()
        {
            this.pnlRenewalTrend.Controls.Add(
                this.CreateInsightRow(
                    "Renewed Tenants",
                    "0",
                    SuccessColor,
                    90
                )
            );

            this.pnlRenewalTrend.Controls.Add(
                this.CreateInsightRow(
                    "Upcoming Renewals",
                    "0",
                    InfoColor,
                    145
                )
            );

            this.pnlRenewalTrend.Controls.Add(
                this.CreateInsightRow(
                    "Due Soon",
                    "0",
                    WarningColor,
                    200
                )
            );

            this.pnlRenewalTrend.Controls.Add(
                this.CreateInsightRow(
                    "Expired",
                    "0",
                    DangerColor,
                    255
                )
            );

            this.pnlRenewalTrend.Controls.Add(
                this.CreateInsightRow(
                    "Active Promotions",
                    "0",
                    CtaColor,
                    310
                )
            );

            this.pnlRetentionInsights.Controls.Add(
                this.CreateRetentionInsight(
                    "Follow up with tenants whose renewal is due within 7 days.",
                    WarningColor,
                    90
                )
            );

            this.pnlRetentionInsights.Controls.Add(
                this.CreateRetentionInsight(
                    "Use active promotions when discussing renewal options.",
                    CtaColor,
                    150
                )
            );

            this.pnlRetentionInsights.Controls.Add(
                this.CreateRetentionInsight(
                    "Review expired renewals and record the reason for non-renewal.",
                    DangerColor,
                    210
                )
            );

            this.pnlRetentionInsights.Controls.Add(
                this.CreateRetentionInsight(
                    "Keep successful renewal activity documented for retention analysis.",
                    SuccessColor,
                    270
                )
            );
        }

        // ========================================================
        // INSIGHT ROW
        // ========================================================

        private Panel CreateInsightRow(
            string title,
            string value,
            Color accent,
            int top)
        {
            Panel panel = new Panel
            {
                Location = new Point(
                    20,
                    top
                ),
                Size = new Size(
                    470,
                    45
                ),
                BackColor = Color.FromArgb(
                    250,
                    247,
                    242
                )
            };

            Panel strip = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = accent
            };

            Label lblTitle = new Label
            {
                AutoSize = true,
                Text = title,
                Font = new Font(
                    "Segoe UI",
                    9F
                ),
                ForeColor = TextDark,
                Location = new Point(
                    15,
                    13
                )
            };

            Label lblValue = new Label
            {
                AutoSize = true,
                Text = value,
                Font = new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold
                ),
                ForeColor = accent,
                Location = new Point(
                    390,
                    11
                )
            };

            panel.Controls.Add(
                lblValue
            );

            panel.Controls.Add(
                lblTitle
            );

            panel.Controls.Add(
                strip
            );

            return panel;
        }

        // ========================================================
        // RETENTION INSIGHT
        // ========================================================

        private Panel CreateRetentionInsight(
            string text,
            Color accent,
            int top)
        {
            Panel panel = new Panel
            {
                Location = new Point(
                    20,
                    top
                ),
                Size = new Size(
                    470,
                    48
                ),
                BackColor = Color.FromArgb(
                    250,
                    247,
                    242
                )
            };

            Panel strip = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = accent
            };

            Label label = new Label
            {
                AutoSize = false,
                Width = 430,
                Height = 45,
                Text = text,
                Font = new Font(
                    "Segoe UI",
                    8.5F
                ),
                ForeColor = TextDark,
                Location = new Point(
                    15,
                    4
                )
            };

            panel.Controls.Add(label);

            panel.Controls.Add(strip);

            return panel;
        }

        // ========================================================
        // FOLLOW-UP TAB
        // ========================================================

        private void BuildFollowUpTab()
        {
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 105,
                BackColor = ContentBg
            };

            this.tabFollowUp.Controls.Add(
                topPanel
            );

            Label title = new Label
            {
                AutoSize = true,
                Text = "Renewal Follow-up",
                Font = new Font(
                    "Segoe UI",
                    14F,
                    FontStyle.Bold
                ),
                ForeColor = TextDark,
                Location = new Point(
                    20,
                    15
                )
            };

            Label subtitle = new Label
            {
                AutoSize = true,
                Text =
                    "Track tenants who are approaching renewal or need retention follow-up.",
                Font = new Font(
                    "Segoe UI",
                    8.5F
                ),
                ForeColor = TextMuted,
                Location = new Point(
                    20,
                    43
                )
            };

            Label searchLabel = new Label
            {
                AutoSize = true,
                Text = "Search",
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold
                ),
                ForeColor = TextMuted,
                Location = new Point(
                    20,
                    72
                )
            };

            this.txtSearch = new TextBox
            {
                Width = 230,
                Height = 28,
                Font = new Font(
                    "Segoe UI",
                    9F
                ),
                Location = new Point(
                    75,
                    68
                )
            };

            this.txtSearch.TextChanged +=
                (sender, e) =>
                {
                    this.ApplyRenewalFilters();
                };

            Label statusLabel = new Label
            {
                AutoSize = true,
                Text = "Status",
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold
                ),
                ForeColor = TextMuted,
                Location = new Point(
                    330,
                    72
                )
            };

            this.cmbStatus = new ComboBox
            {
                Width = 160,
                Height = 28,
                DropDownStyle =
                    ComboBoxStyle.DropDownList,
                Font = new Font(
                    "Segoe UI",
                    9F
                ),
                Location = new Point(
                    380,
                    68
                )
            };

            this.cmbStatus.Items.Add(
                "All Statuses"
            );

            this.cmbStatus.Items.Add(
                "Upcoming"
            );

            this.cmbStatus.Items.Add(
                "Due"
            );

            this.cmbStatus.Items.Add(
                "Expired"
            );

            this.cmbStatus.Items.Add(
                "Renewed"
            );

            this.cmbStatus.Items.Add(
                "Cancelled"
            );

            this.cmbStatus.SelectedIndex = 0;

            this.cmbStatus.SelectedIndexChanged +=
                (sender, e) =>
                {
                    this.ApplyRenewalFilters();
                };

            this.lblFollowUpCount = new Label
            {
                AutoSize = true,
                Text = "0 records",
                Font = new Font(
                    "Segoe UI",
                    8.5F
                ),
                ForeColor = TextMuted
            };

            topPanel.Controls.Add(title);
            topPanel.Controls.Add(subtitle);
            topPanel.Controls.Add(searchLabel);
            topPanel.Controls.Add(this.txtSearch);
            topPanel.Controls.Add(statusLabel);
            topPanel.Controls.Add(this.cmbStatus);
            topPanel.Controls.Add(this.lblFollowUpCount);

            this.dgvRenewals =
                new DataGridView
                {
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    AutoGenerateColumns = false,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    CellBorderStyle =
                        DataGridViewCellBorderStyle.SingleHorizontal,
                    ColumnHeadersBorderStyle =
                        DataGridViewHeaderBorderStyle.None,
                    EnableHeadersVisualStyles = false,
                    GridColor = Color.FromArgb(
                        235,
                        231,
                        225
                    ),
                    MultiSelect = false,
                    ReadOnly = true,
                    RowHeadersVisible = false,
                    SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect,
                    Font = new Font(
                        "Segoe UI",
                        9F
                    ),
                    Dock = DockStyle.Fill
                };

            this.dgvRenewals.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(
                        250,
                        247,
                        242
                    ),
                    ForeColor = TextDark,
                    Font = new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold
                    ),
                    Padding = new Padding(
                        8,
                        0,
                        8,
                        0
                    )
                };

            this.dgvRenewals.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = TextDark,
                    SelectionBackColor =
                        Color.FromArgb(
                            246,
                            240,
                            231
                        ),
                    SelectionForeColor = TextDark,
                    Padding = new Padding(
                        8,
                        5,
                        8,
                        5
                    )
                };

            this.dgvRenewals.AlternatingRowsDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(
                        252,
                        250,
                        247
                    )
                };

            this.dgvRenewals.RowTemplate.Height = 42;

            this.AddRenewalColumns();

            this.dgvRenewals.CellFormatting +=
                this.DgvRenewals_CellFormatting;

            this.dgvRenewals.CellDoubleClick +=
                this.DgvRenewals_CellDoubleClick;

            this.tabFollowUp.Controls.Add(
                this.dgvRenewals
            );
        }

        // ========================================================
        // RENEWAL COLUMNS
        // ========================================================

        private void AddRenewalColumns()
        {
            this.dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "TenantName",
                    HeaderText = "TENANT",
                    DataPropertyName = "TenantName",
                    Width = 180
                }
            );

            this.dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Room",
                    HeaderText = "ROOM / BED",
                    DataPropertyName = "Room",
                    Width = 100
                }
            );

            this.dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "RenewalDate",
                    HeaderText = "RENEWAL DATE",
                    DataPropertyName = "RenewalDate",
                    Width = 130
                }
            );

            this.dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Days",
                    HeaderText = "TIME",
                    DataPropertyName = "DaysText",
                    Width = 115
                }
            );

            this.dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "STATUS",
                    DataPropertyName = "Status",
                    Width = 105
                }
            );

            this.dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "TenantStatus",
                    HeaderText = "TENANT",
                    DataPropertyName = "TenantStatus",
                    Width = 110
                }
            );

            this.dgvRenewals.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Notes",
                    HeaderText = "FOLLOW-UP NOTES",
                    DataPropertyName = "Notes",
                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill
                }
            );
        }

        // ========================================================
        // PROMOTIONS TAB
        // ========================================================

        private void BuildPromotionsTab()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = ContentBg
            };

            this.tabPromotions.Controls.Add(
                header
            );

            Label title = new Label
            {
                AutoSize = true,
                Text = "Promotions & Offers",
                Font = new Font(
                    "Segoe UI",
                    14F,
                    FontStyle.Bold
                ),
                ForeColor = TextDark,
                Location = new Point(
                    20,
                    15
                )
            };

            Label subtitle = new Label
            {
                AutoSize = true,
                Text =
                    "Create retention offers that Managers can use when following up with tenants.",
                Font = new Font(
                    "Segoe UI",
                    8.5F
                ),
                ForeColor = TextMuted,
                Location = new Point(
                    20,
                    43
                )
            };

            Label searchLabel = new Label
            {
                AutoSize = true,
                Text = "Search",
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold
                ),
                ForeColor = TextMuted,
                Location = new Point(
                    20,
                    76
                )
            };

            this.txtPromotionSearch =
                new TextBox
                {
                    Width = 200,
                    Height = 28,
                    Font = new Font(
                        "Segoe UI",
                        9F
                    ),
                    Location = new Point(
                        75,
                        72
                    )
                };

            this.txtPromotionSearch.TextChanged +=
                (sender, e) =>
                {
                    this.ApplyPromotionFilters();
                };

            Label statusLabel = new Label
            {
                AutoSize = true,
                Text = "Status",
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold
                ),
                ForeColor = TextMuted,
                Location = new Point(
                    295,
                    76
                )
            };

            this.cmbPromotionStatus =
                new ComboBox
                {
                    Width = 140,
                    Height = 28,
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Font = new Font(
                        "Segoe UI",
                        9F
                    ),
                    Location = new Point(
                        345,
                        72
                    )
                };

            this.cmbPromotionStatus.Items.Add(
                "All"
            );

            this.cmbPromotionStatus.Items.Add(
                "Active"
            );

            this.cmbPromotionStatus.Items.Add(
                "Archived"
            );

            this.cmbPromotionStatus.SelectedIndex = 0;

            this.cmbPromotionStatus.SelectedIndexChanged +=
                (sender, e) =>
                {
                    this.ApplyPromotionFilters();
                };

            this.lblPromotionCount =
                new Label
                {
                    AutoSize = true,
                    Text = "0 promotions",
                    Font = new Font(
                        "Segoe UI",
                        8.5F
                    ),
                    ForeColor = TextMuted
                };

            this.btnAddPromotion =
                this.CreateButton(
                    "Add Promotion",
                    CtaColor,
                    Color.White
                );

            this.btnAddPromotion.Size =
                new Size(
                    125,
                    34
                );

            this.btnAddPromotion.Click +=
                (sender, e) =>
                {
                    this.ShowPromotionEditor(
                        null
                    );
                };

            this.btnEditPromotion =
                this.CreateButton(
                    "Edit",
                    Color.FromArgb(
                        245,
                        241,
                        235
                    ),
                    TextDark
                );

            this.btnEditPromotion.Size =
                new Size(
                    85,
                    34
                );

            this.btnEditPromotion.Click +=
                (sender, e) =>
                {
                    this.EditSelectedPromotion();
                };

            this.btnArchivePromotion =
                this.CreateButton(
                    "Archive",
                    Color.FromArgb(
                        245,
                        241,
                        235
                    ),
                    TextDark
                );

            this.btnArchivePromotion.Size =
                new Size(
                    95,
                    34
                );

            this.btnArchivePromotion.Click +=
                (sender, e) =>
                {
                    this.ArchiveSelectedPromotion();
                };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            header.Controls.Add(searchLabel);
            header.Controls.Add(this.txtPromotionSearch);
            header.Controls.Add(statusLabel);
            header.Controls.Add(this.cmbPromotionStatus);
            header.Controls.Add(this.lblPromotionCount);
            header.Controls.Add(this.btnAddPromotion);
            header.Controls.Add(this.btnEditPromotion);
            header.Controls.Add(this.btnArchivePromotion);

            // ----------------------------------------------------
            // PROMOTION TABLE
            // ----------------------------------------------------

            this.dgvPromotions =
                new DataGridView
                {
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    AutoGenerateColumns = false,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    CellBorderStyle =
                        DataGridViewCellBorderStyle.SingleHorizontal,
                    ColumnHeadersBorderStyle =
                        DataGridViewHeaderBorderStyle.None,
                    EnableHeadersVisualStyles = false,
                    GridColor = Color.FromArgb(
                        235,
                        231,
                        225
                    ),
                    MultiSelect = false,
                    ReadOnly = true,
                    RowHeadersVisible = false,
                    SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect,
                    Font = new Font(
                        "Segoe UI",
                        9F
                    ),
                    Dock = DockStyle.Fill
                };

            this.dgvPromotions.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(
                        250,
                        247,
                        242
                    ),
                    ForeColor = TextDark,
                    Font = new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold
                    ),
                    Padding = new Padding(
                        8,
                        0,
                        8,
                        0
                    )
                };

            this.dgvPromotions.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = TextDark,
                    SelectionBackColor =
                        Color.FromArgb(
                            246,
                            240,
                            231
                        ),
                    SelectionForeColor = TextDark,
                    Padding = new Padding(
                        8,
                        5,
                        8,
                        5
                    )
                };

            this.dgvPromotions.AlternatingRowsDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(
                        252,
                        250,
                        247
                    )
                };

            this.dgvPromotions.RowTemplate.Height = 42;

            this.AddPromotionColumns();

            this.dgvPromotions.CellDoubleClick +=
                (sender, e) =>
                {
                    this.EditSelectedPromotion();
                };

            this.tabPromotions.Controls.Add(
                this.dgvPromotions
            );
        }

        // ========================================================
        // PROMOTION COLUMNS
        // ========================================================

        private void AddPromotionColumns()
        {
            this.dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "PromotionName",
                    HeaderText = "PROMOTION",
                    DataPropertyName = "Name",
                    Width = 180
                }
            );

            this.dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Offer",
                    HeaderText = "OFFER",
                    DataPropertyName = "Offer",
                    Width = 180
                }
            );

            this.dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "ValidUntil",
                    HeaderText = "VALID UNTIL",
                    DataPropertyName = "ValidUntil",
                    Width = 130
                }
            );

            this.dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Usage",
                    HeaderText = "USAGE",
                    DataPropertyName = "Usage",
                    Width = 80
                }
            );

            this.dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "STATUS",
                    DataPropertyName = "Status",
                    Width = 100
                }
            );

            this.dgvPromotions.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Description",
                    HeaderText = "DESCRIPTION",
                    DataPropertyName = "Description",
                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill
                }
            );
        }

        // ========================================================
        // DEFAULT PROMOTIONS
        // ========================================================

        private void LoadDefaultPromotions()
        {
            this._promotions.Clear();

            this._promotions.Add(
                new PromotionRecord
                {
                    Name = "Early Renewal",
                    Offer = "10% renewal discount",
                    ValidUntil =
                        DateTime.Today.AddMonths(1),
                    Usage = 0,
                    Status = "Active",
                    Description =
                        "Offer tenants a discount when they renew before their current stay expires."
                }
            );

            this._promotions.Add(
                new PromotionRecord
                {
                    Name = "Long-Term Renewal",
                    Offer =
                        "Special rate for 6-month renewal",
                    ValidUntil =
                        DateTime.Today.AddMonths(2),
                    Usage = 0,
                    Status = "Active",
                    Description =
                        "Encourage tenants to commit to a longer stay."
                }
            );

            this._promotions.Add(
                new PromotionRecord
                {
                    Name = "Loyal Tenant Offer",
                    Offer =
                        "Special renewal rate",
                    ValidUntil =
                        DateTime.Today.AddMonths(3),
                    Usage = 0,
                    Status = "Active",
                    Description =
                        "Retention offer for tenants with a positive stay history."
                }
            );

            this.ApplyPromotionFilters();
        }

        // ========================================================
        // LOAD RENEWALS
        // ========================================================

        private async Task LoadRenewalsAsync()
        {
            try
            {
                this.SetLoadingState(true);

                this._renewals.Clear();

                JsonElement response =
                    await this._apiService
                        .GetSilentAsync<JsonElement>(
                            "api/Renewals"
                        );

                JsonElement array =
                    this.ExtractArray(response);

                if (array.ValueKind ==
                    JsonValueKind.Array)
                {
                    foreach (
                        JsonElement item
                        in array.EnumerateArray())
                    {
                        RenewalRecord? renewal =
                            this.ParseRenewal(item);

                        if (renewal != null)
                        {
                            this._renewals.Add(
                                renewal
                            );
                        }
                    }
                }

                this.UpdateOverview();

                this.ApplyRenewalFilters();
            }
            catch
            {
                this.UpdateOverview();

                this.ApplyRenewalFilters();
            }
            finally
            {
                this.SetLoadingState(false);
            }
        }

        // ========================================================
        // PARSE RENEWAL
        // ========================================================

        private RenewalRecord? ParseRenewal(
            JsonElement item)
        {
            if (item.ValueKind !=
                JsonValueKind.Object)
            {
                return null;
            }

            string tenantName =
                this.GetString(
                    item,
                    "tenantName",
                    "TenantName",
                    "tenantFullName",
                    "fullName"
                );

            if (string.IsNullOrWhiteSpace(
                tenantName))
            {
                if (
                    item.TryGetProperty(
                        "tenant",
                        out JsonElement tenant
                    ) &&
                    tenant.ValueKind ==
                    JsonValueKind.Object
                )
                {
                    string firstName =
                        this.GetString(
                            tenant,
                            "firstName",
                            "FirstName"
                        );

                    string lastName =
                        this.GetString(
                            tenant,
                            "lastName",
                            "LastName"
                        );

                    tenantName =
                        $"{firstName} {lastName}"
                            .Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(
                tenantName))
            {
                tenantName =
                    "Unknown Tenant";
            }

            string room =
                this.GetString(
                    item,
                    "roomName",
                    "RoomName",
                    "roomNumber",
                    "RoomNumber",
                    "bedName",
                    "BedName"
                );

            DateTime? renewalDate =
                this.GetDate(
                    item,
                    "renewalDate",
                    "RenewalDate",
                    "renewalDueDate",
                    "RenewalDueDate",
                    "endDate",
                    "EndDate",
                    "expiryDate",
                    "ExpiryDate"
                );

            string status =
                this.GetString(
                    item,
                    "status",
                    "Status"
                );

            string tenantStatus =
                this.GetString(
                    item,
                    "tenantStatus",
                    "TenantStatus"
                );

            string notes =
                this.GetString(
                    item,
                    "notes",
                    "Notes",
                    "remarks",
                    "Remarks"
                );

            return new RenewalRecord
            {
                TenantName = tenantName,

                Room =
                    string.IsNullOrWhiteSpace(room)
                        ? "-"
                        : room,

                RenewalDate = renewalDate,

                Status =
                    this.CalculateStatus(
                        status,
                        renewalDate
                    ),

                TenantStatus =
                    string.IsNullOrWhiteSpace(
                        tenantStatus)
                        ? "-"
                        : tenantStatus,

                Notes =
                    string.IsNullOrWhiteSpace(notes)
                        ? "-"
                        : notes
            };
        }

        // ========================================================
        // STATUS
        // ========================================================

        private string CalculateStatus(
            string status,
            DateTime? renewalDate)
        {
            string normalized =
                status?.Trim()
                ?? string.Empty;

            if (normalized.Equals(
                    "Renewed",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Renewed";
            }

            if (normalized.Equals(
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Cancelled";
            }

            if (!renewalDate.HasValue)
            {
                return string.IsNullOrWhiteSpace(
                    normalized)
                    ? "Pending"
                    : normalized;
            }

            int days =
                (
                    renewalDate.Value.Date
                    - DateTime.Today
                ).Days;

            if (days < 0)
            {
                return "Expired";
            }

            if (days <= 7)
            {
                return "Due";
            }

            return "Upcoming";
        }

        // ========================================================
        // UPDATE OVERVIEW
        // ========================================================

        private void UpdateOverview()
        {
            int total =
                this._renewals.Count;

            int upcoming =
                this._renewals.Count(
                    x =>
                        x.Status.Equals(
                            "Upcoming",
                            StringComparison.OrdinalIgnoreCase
                        ) &&
                        x.RenewalDate.HasValue &&
                        (
                            x.RenewalDate.Value.Date
                            - DateTime.Today
                        ).Days <= 30
                );

            int due =
                this._renewals.Count(
                    x =>
                        x.Status.Equals(
                            "Due",
                            StringComparison.OrdinalIgnoreCase
                        ) ||
                        x.Status.Equals(
                            "Pending",
                            StringComparison.OrdinalIgnoreCase
                        )
                );

            int expired =
                this._renewals.Count(
                    x =>
                        x.Status.Equals(
                            "Expired",
                            StringComparison.OrdinalIgnoreCase
                        )
                );

            int renewed =
                this._renewals.Count(
                    x =>
                        x.Status.Equals(
                            "Renewed",
                            StringComparison.OrdinalIgnoreCase
                        )
                );

            int completed =
                renewed + expired;

            double retentionRate =
                completed == 0
                    ? 0
                    : (
                        (double)renewed
                        / completed
                    ) * 100;

            this.lblTotalValue.Text =
                total.ToString();

            this.lblUpcomingValue.Text =
                upcoming.ToString();

            this.lblDueValue.Text =
                due.ToString();

            this.lblExpiredValue.Text =
                expired.ToString();

            this.lblRenewedValue.Text =
                renewed.ToString();

            this.lblRetentionRateValue.Text =
                $"{retentionRate:0.#}%";

            this.UpdateOverviewInsightValues(
                renewed,
                upcoming,
                due,
                expired
            );
        }

        // ========================================================
        // UPDATE OVERVIEW INSIGHTS
        // ========================================================

        private void UpdateOverviewInsightValues(
            int renewed,
            int upcoming,
            int due,
            int expired)
        {
            if (this.pnlRenewalTrend == null)
            {
                return;
            }

            foreach (
                Control control
                in this.pnlRenewalTrend.Controls)
            {
                if (control is not Panel panel)
                {
                    continue;
                }

                Label? title =
                    panel.Controls
                        .OfType<Label>()
                        .FirstOrDefault(
                            x =>
                                x.Location.X < 100
                        );

                Label? value =
                    panel.Controls
                        .OfType<Label>()
                        .FirstOrDefault(
                            x =>
                                x.Location.X > 300
                        );

                if (title == null ||
                    value == null)
                {
                    continue;
                }

                switch (title.Text)
                {
                    case "Renewed Tenants":

                        value.Text =
                            renewed.ToString();

                        break;

                    case "Upcoming Renewals":

                        value.Text =
                            upcoming.ToString();

                        break;

                    case "Due Soon":

                        value.Text =
                            due.ToString();

                        break;

                    case "Expired":

                        value.Text =
                            expired.ToString();

                        break;

                    case "Active Promotions":

                        value.Text =
                            this._promotions.Count(
                                x =>
                                    x.Status.Equals(
                                        "Active",
                                        StringComparison.OrdinalIgnoreCase
                                    )
                            ).ToString();

                        break;
                }
            }
        }

        // ========================================================
        // RENEWAL FILTERS
        // ========================================================

        private void ApplyRenewalFilters()
        {
            if (this.dgvRenewals == null)
            {
                return;
            }

            IEnumerable<RenewalRecord> query =
                this._renewals;

            string search =
                this.txtSearch?.Text
                ?.Trim()
                ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(
                search))
            {
                query =
                    query.Where(
                        x =>
                            x.TenantName.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase
                            ) ||
                            x.Room.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase
                            ) ||
                            x.Status.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase
                            )
                    );
            }

            string selectedStatus =
                this.cmbStatus?.SelectedItem
                    ?.ToString()
                ?? "All Statuses";

            if (!selectedStatus.Equals(
                    "All Statuses",
                    StringComparison.OrdinalIgnoreCase))
            {
                query =
                    query.Where(
                        x =>
                            x.Status.Equals(
                                selectedStatus,
                                StringComparison.OrdinalIgnoreCase
                            )
                    );
            }

            List<RenewalRecord> filtered =
                query
                    .OrderBy(
                        x =>
                            x.RenewalDate
                            ?? DateTime.MaxValue
                    )
                    .ToList();

            this.dgvRenewals.DataSource = null;

            this.dgvRenewals.DataSource =
                filtered;

            this.lblFollowUpCount.Text =
                $"{filtered.Count} record" +
                (
                    filtered.Count == 1
                        ? ""
                        : "s"
                );
        }

        // ========================================================
        // PROMOTION FILTERS
        // ========================================================

        private void ApplyPromotionFilters()
        {
            if (this.dgvPromotions == null)
            {
                return;
            }

            IEnumerable<PromotionRecord> query =
                this._promotions;

            string search =
                this.txtPromotionSearch?.Text
                ?.Trim()
                ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(
                search))
            {
                query =
                    query.Where(
                        x =>
                            x.Name.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase
                            ) ||
                            x.Offer.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase
                            ) ||
                            x.Description.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase
                            )
                    );
            }

            string status =
                this.cmbPromotionStatus
                    ?.SelectedItem
                    ?.ToString()
                ?? "All";

            if (!status.Equals(
                    "All",
                    StringComparison.OrdinalIgnoreCase))
            {
                query =
                    query.Where(
                        x =>
                            x.Status.Equals(
                                status,
                                StringComparison.OrdinalIgnoreCase
                            )
                    );
            }

            List<PromotionRecord> filtered =
                query.ToList();

            this.dgvPromotions.DataSource = null;

            this.dgvPromotions.DataSource =
                filtered;

            this.lblPromotionCount.Text =
                $"{filtered.Count} promotion" +
                (
                    filtered.Count == 1
                        ? ""
                        : "s"
                );

            this.UpdateOverview();
        }

        // ========================================================
        // PROMOTION EDITOR
        // ========================================================

        private void ShowPromotionEditor(
            PromotionRecord? existing)
        {
            bool editing =
                existing != null;

            Form form = new Form
            {
                Text =
                    editing
                        ? "Edit Promotion"
                        : "Create Promotion",

                StartPosition =
                    FormStartPosition.CenterParent,

                Size =
                    new Size(
                        550,
                        520
                    ),

                MinimumSize =
                    new Size(
                        500,
                        480
                    ),

                BackColor =
                    ContentBg,

                FormBorderStyle =
                    FormBorderStyle.FixedDialog,

                MaximizeBox = false,

                MinimizeBox = false
            };

            Label title = new Label
            {
                AutoSize = true,

                Text =
                    editing
                        ? "Edit Promotion"
                        : "Create Promotion",

                Font =
                    new Font(
                        "Segoe UI",
                        18F,
                        FontStyle.Bold
                    ),

                ForeColor =
                    TextDark,

                Location =
                    new Point(
                        25,
                        22
                    )
            };

            form.Controls.Add(title);

            TextBox txtName =
                this.CreateEditorTextBox(
                    form,
                    "Promotion Name",
                    75,
                    existing?.Name ?? ""
                );

            TextBox txtOffer =
                this.CreateEditorTextBox(
                    form,
                    "Offer",
                    145,
                    existing?.Offer ?? ""
                );

            DateTimePicker datePicker =
                new DateTimePicker
                {
                    Format =
                        DateTimePickerFormat.Long,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F
                        ),

                    Width = 460,

                    Location =
                        new Point(
                            25,
                            215
                        ),

                    Value =
                        existing?.ValidUntil
                        ?? DateTime.Today.AddMonths(1)
                };

            Label dateLabel = new Label
            {
                AutoSize = true,

                Text =
                    "Valid Until",

                Font =
                    new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold
                    ),

                ForeColor =
                    TextMuted,

                Location =
                    new Point(
                        25,
                        195
                    )
            };

            form.Controls.Add(
                dateLabel
            );

            form.Controls.Add(
                datePicker
            );

            Label descriptionLabel =
                new Label
                {
                    AutoSize = true,

                    Text =
                        "Description",

                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F,
                            FontStyle.Bold
                        ),

                    ForeColor =
                        TextMuted,

                    Location =
                        new Point(
                            25,
                            270
                        )
                };

            TextBox txtDescription =
                new TextBox
                {
                    Multiline = true,

                    ScrollBars =
                        ScrollBars.Vertical,

                    Width = 460,

                    Height = 90,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F
                        ),

                    Location =
                        new Point(
                            25,
                            290
                        ),

                    Text =
                        existing?.Description
                        ?? ""
                };

            form.Controls.Add(
                descriptionLabel
            );

            form.Controls.Add(
                txtDescription
            );

            Button btnCancel =
                this.CreateButton(
                    "Cancel",
                    Color.FromArgb(
                        235,
                        230,
                        223
                    ),
                    TextDark
                );

            btnCancel.Size =
                new Size(
                    100,
                    38
                );

            btnCancel.Location =
                new Point(
                    275,
                    405
                );

            btnCancel.Click +=
                (sender, e) =>
                {
                    form.Close();
                };

            Button btnSave =
                this.CreateButton(
                    editing
                        ? "Save Changes"
                        : "Create Promotion",
                    CtaColor,
                    Color.White
                );

            btnSave.Size =
                new Size(
                    140,
                    38
                );

            btnSave.Location =
                new Point(
                    345,
                    405
                );

            btnSave.Click +=
                (sender, e) =>
                {
                    string name =
                        txtName.Text.Trim();

                    string offer =
                        txtOffer.Text.Trim();

                    if (string.IsNullOrWhiteSpace(
                        name))
                    {
                        MessageBox.Show(
                            "Please enter a promotion name.",
                            "Promotion",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        return;
                    }

                    if (string.IsNullOrWhiteSpace(
                        offer))
                    {
                        MessageBox.Show(
                            "Please enter the offer.",
                            "Promotion",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        return;
                    }

                    if (editing &&
                        existing != null)
                    {
                        existing.Name =
                            name;

                        existing.Offer =
                            offer;

                        existing.ValidUntil =
                            datePicker.Value.Date;

                        existing.Description =
                            txtDescription.Text.Trim();
                    }
                    else
                    {
                        this._promotions.Add(
                            new PromotionRecord
                            {
                                Name = name,

                                Offer = offer,

                                ValidUntil =
                                    datePicker.Value.Date,

                                Usage = 0,

                                Status = "Active",

                                Description =
                                    txtDescription
                                        .Text
                                        .Trim()
                            }
                        );
                    }

                    this.ApplyPromotionFilters();

                    form.Close();
                };

            form.Controls.Add(
                btnCancel
            );

            form.Controls.Add(
                btnSave
            );

            form.ShowDialog(this);
        }

        // ========================================================
        // EDITOR TEXTBOX
        // ========================================================

        private TextBox CreateEditorTextBox(
            Form form,
            string labelText,
            int top,
            string value)
        {
            Label label = new Label
            {
                AutoSize = true,

                Text = labelText,

                Font =
                    new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold
                    ),

                ForeColor =
                    TextMuted,

                Location =
                    new Point(
                        25,
                        top
                    )
            };

            TextBox textBox = new TextBox
            {
                Width = 460,

                Height = 28,

                Font =
                    new Font(
                        "Segoe UI",
                        9F
                    ),

                Location =
                    new Point(
                        25,
                        top + 20
                    ),

                Text = value
            };

            form.Controls.Add(label);

            form.Controls.Add(textBox);

            return textBox;
        }

        // ========================================================
        // EDIT PROMOTION
        // ========================================================

        private void EditSelectedPromotion()
        {
            if (this.dgvPromotions.CurrentRow
                    ?.DataBoundItem
                is not PromotionRecord promotion)
            {
                MessageBox.Show(
                    "Please select a promotion first.",
                    "Promotion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            this.ShowPromotionEditor(
                promotion
            );
        }

        // ========================================================
        // ARCHIVE PROMOTION
        // ========================================================

        private void ArchiveSelectedPromotion()
        {
            if (this.dgvPromotions.CurrentRow
                    ?.DataBoundItem
                is not PromotionRecord promotion)
            {
                MessageBox.Show(
                    "Please select a promotion first.",
                    "Promotion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            if (promotion.Status.Equals(
                    "Archived",
                    StringComparison.OrdinalIgnoreCase))
            {
                promotion.Status =
                    "Active";
            }
            else
            {
                DialogResult result =
                    MessageBox.Show(
                        $"Archive '{promotion.Name}'?",
                        "Archive Promotion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                if (result !=
                    DialogResult.Yes)
                {
                    return;
                }

                promotion.Status =
                    "Archived";
            }

            this.ApplyPromotionFilters();
        }

        // ========================================================
        // RENEWAL DETAILS
        // ========================================================

        private void DgvRenewals_CellDoubleClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (this.dgvRenewals.Rows[e.RowIndex]
                    .DataBoundItem
                is not RenewalRecord renewal)
            {
                return;
            }

            this.ShowRenewalDetails(
                renewal
            );
        }

        private void ShowRenewalDetails(
            RenewalRecord renewal)
        {
            Form form = new Form
            {
                Text =
                    "Renewal Details",

                StartPosition =
                    FormStartPosition.CenterParent,

                Size =
                    new Size(
                        520,
                        450
                    ),

                BackColor =
                    ContentBg,

                FormBorderStyle =
                    FormBorderStyle.FixedDialog,

                MaximizeBox = false,

                MinimizeBox = false
            };

            Label title = new Label
            {
                AutoSize = true,

                Text =
                    "Renewal Details",

                Font =
                    new Font(
                        "Segoe UI",
                        18F,
                        FontStyle.Bold
                    ),

                ForeColor =
                    TextDark,

                Location =
                    new Point(
                        25,
                        22
                    )
            };

            form.Controls.Add(title);

            int y = 75;

            this.AddDetailRow(
                form,
                "Tenant",
                renewal.TenantName,
                ref y
            );

            this.AddDetailRow(
                form,
                "Room / Bed",
                renewal.Room,
                ref y
            );

            this.AddDetailRow(
                form,
                "Renewal Date",
                renewal.RenewalDate.HasValue
                    ? renewal.RenewalDate.Value
                        .ToString(
                            "MMMM dd, yyyy"
                        )
                    : "Not specified",
                ref y
            );

            this.AddDetailRow(
                form,
                "Status",
                renewal.Status,
                ref y
            );

            this.AddDetailRow(
                form,
                "Tenant Status",
                renewal.TenantStatus,
                ref y
            );

            this.AddDetailRow(
                form,
                "Notes",
                renewal.Notes,
                ref y
            );

            Button close =
                this.CreateButton(
                    "Close",
                    CtaColor,
                    Color.White
                );

            close.Size =
                new Size(
                    100,
                    36
                );

            close.Location =
                new Point(
                    380,
                    355
                );

            close.Click +=
                (sender, e) =>
                {
                    form.Close();
                };

            form.Controls.Add(close);

            form.ShowDialog(this);
        }

        // ========================================================
        // DETAIL ROW
        // ========================================================

        private void AddDetailRow(
            Form form,
            string title,
            string value,
            ref int y)
        {
            Label titleLabel = new Label
            {
                AutoSize = true,

                Text = title,

                Font =
                    new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold
                    ),

                ForeColor =
                    TextMuted,

                Location =
                    new Point(
                        25,
                        y
                    )
            };

            Label valueLabel = new Label
            {
                AutoSize = false,

                Width = 430,

                Height = 30,

                Text = value,

                Font =
                    new Font(
                        "Segoe UI",
                        10F
                    ),

                ForeColor =
                    TextDark,

                Location =
                    new Point(
                        25,
                        y + 18
                    )
            };

            form.Controls.Add(
                valueLabel
            );

            form.Controls.Add(
                titleLabel
            );

            y += 52;
        }

        // ========================================================
        // RENEWAL GRID FORMATTING
        // ========================================================

        private void DgvRenewals_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            string columnName =
                this.dgvRenewals
                    .Columns[e.ColumnIndex]
                    .Name;

            if (columnName ==
                "RenewalDate")
            {
                if (e.Value is DateTime date)
                {
                    e.Value =
                        date.ToString(
                            "MMM dd, yyyy"
                        );

                    e.FormattingApplied = true;
                }
            }

            if (columnName ==
                "Status")
            {
                string status =
                    e.Value?.ToString()
                    ?? "";

                e.CellStyle.Font =
                    new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold
                    );

                if (status.Equals(
                        "Renewed",
                        StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor =
                        SuccessColor;
                }
                else if (status.Equals(
                        "Upcoming",
                        StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor =
                        InfoColor;
                }
                else if (
                    status.Equals(
                        "Due",
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    status.Equals(
                        "Pending",
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    e.CellStyle.ForeColor =
                        WarningColor;
                }
                else if (status.Equals(
                    "Expired",
                    StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor =
                        DangerColor;
                }
            }

            if (columnName ==
                "Days")
            {
                if (
                    this.dgvRenewals
                        .Rows[e.RowIndex]
                        .DataBoundItem
                    is RenewalRecord renewal
                )
                {
                    if (!renewal.RenewalDate.HasValue)
                    {
                        e.Value = "-";
                    }
                    else
                    {
                        int days =
                            (
                                renewal.RenewalDate
                                    .Value.Date
                                - DateTime.Today
                            ).Days;

                        if (days < 0)
                        {
                            e.Value =
                                $"{Math.Abs(days)} day(s) overdue";

                            e.CellStyle.ForeColor =
                                DangerColor;
                        }
                        else if (days == 0)
                        {
                            e.Value = "Today";

                            e.CellStyle.ForeColor =
                                DangerColor;
                        }
                        else
                        {
                            e.Value =
                                $"{days} day(s)";

                            if (days <= 7)
                            {
                                e.CellStyle.ForeColor =
                                    WarningColor;
                            }
                        }
                    }

                    e.FormattingApplied = true;
                }
            }
        }

        // ========================================================
        // JSON ARRAY
        // ========================================================

        private JsonElement ExtractArray(
            JsonElement response)
        {
            if (response.ValueKind ==
                JsonValueKind.Array)
            {
                return response;
            }

            if (response.ValueKind !=
                JsonValueKind.Object)
            {
                return default;
            }

            string[] names =
            {
                "data",
                "items",
                "results",
                "renewals"
            };

            foreach (string name in names)
            {
                if (
                    response.TryGetProperty(
                        name,
                        out JsonElement value
                    ) &&
                    value.ValueKind ==
                    JsonValueKind.Array
                )
                {
                    return value;
                }
            }

            return default;
        }

        // ========================================================
        // GET STRING
        // ========================================================

        private string GetString(
            JsonElement element,
            params string[] names)
        {
            foreach (string name in names)
            {
                if (
                    !element.TryGetProperty(
                        name,
                        out JsonElement value
                    )
                )
                {
                    continue;
                }

                if (value.ValueKind ==
                    JsonValueKind.String)
                {
                    return value.GetString()
                        ?? string.Empty;
                }

                if (value.ValueKind ==
                    JsonValueKind.Number)
                {
                    return value.ToString();
                }
            }

            return string.Empty;
        }

        // ========================================================
        // GET DATE
        // ========================================================

        private DateTime? GetDate(
            JsonElement element,
            params string[] names)
        {
            foreach (string name in names)
            {
                if (
                    !element.TryGetProperty(
                        name,
                        out JsonElement value
                    )
                )
                {
                    continue;
                }

                if (
                    value.ValueKind ==
                        JsonValueKind.String &&
                    DateTime.TryParse(
                        value.GetString(),
                        out DateTime date
                    )
                )
                {
                    return date;
                }
            }

            return null;
        }

        // ========================================================
        // METRIC CARD
        // ========================================================

        private Panel CreateMetricCard(
            string title,
            string value,
            string description)
        {
            Panel card = new Panel
            {
                BackColor =
                    CardBg,

                BorderStyle =
                    BorderStyle.FixedSingle,

                Size =
                    new Size(
                        175,
                        95
                    )
            };

            Panel accent = new Panel
            {
                Dock =
                    DockStyle.Left,

                Width = 4,

                BackColor =
                    CtaColor
            };

            Label titleLabel = new Label
            {
                AutoSize = true,

                Text = title,

                Font =
                    new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold
                    ),

                ForeColor =
                    TextMuted,

                Location =
                    new Point(
                        15,
                        9
                    )
            };

            Label valueLabel = new Label
            {
                AutoSize = true,

                Text = value,

                Font =
                    new Font(
                        "Segoe UI",
                        19F,
                        FontStyle.Bold
                    ),

                ForeColor =
                    TextDark,

                Location =
                    new Point(
                        15,
                        27
                    )
            };

            Label descriptionLabel = new Label
            {
                AutoSize = true,

                Text = description,

                Font =
                    new Font(
                        "Segoe UI",
                        8F
                    ),

                ForeColor =
                    TextMuted,

                Location =
                    new Point(
                        15,
                        68
                    )
            };

            card.Controls.Add(
                descriptionLabel
            );

            card.Controls.Add(
                valueLabel
            );

            card.Controls.Add(
                titleLabel
            );

            card.Controls.Add(
                accent
            );

            return card;
        }

        private Label GetMetricValue(
            Panel card)
        {
            return card.Controls
                .OfType<Label>()
                .First(
                    x =>
                        x.Font.Size >= 18
                );
        }

        // ========================================================
        // WHITE PANEL
        // ========================================================

        private Panel CreateWhitePanel()
        {
            return new Panel
            {
                BackColor =
                    CardBg,

                BorderStyle =
                    BorderStyle.FixedSingle
            };
        }

        // ========================================================
        // BUTTON
        // ========================================================

        private Button CreateButton(
            string text,
            Color backColor,
            Color foreColor)
        {
            Button button = new Button
            {
                Text = text,

                BackColor =
                    backColor,

                ForeColor =
                    foreColor,

                FlatStyle =
                    FlatStyle.Flat,

                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold
                    ),

                Cursor =
                    Cursors.Hand,

                Size =
                    new Size(
                        100,
                        36
                    ),

                TabStop = false
            };

            button.FlatAppearance.BorderSize =
                0;

            return button;
        }

        // ========================================================
        // LOADING STATE
        // ========================================================

        private void SetLoadingState(
            bool loading)
        {
            if (this.btnRefresh == null)
            {
                return;
            }

            this.btnRefresh.Enabled =
                !loading;

            this.btnRefresh.Text =
                loading
                    ? "Loading..."
                    : "Refresh";

            this.Cursor =
                loading
                    ? Cursors.WaitCursor
                    : Cursors.Default;
        }

        // ========================================================
        // RESPONSIVE LAYOUT
        // ========================================================

        private void PerformLayoutResponsive()
        {
            if (this.pnlHeader == null ||
                this.tabMain == null)
            {
                return;
            }

            int width =
                this.ClientSize.Width;

            this.lblDate.Location =
                new Point(
                    Math.Max(
                        30,
                        width - 255
                    ),
                    28
                );

            this.btnRefresh.Location =
                new Point(
                    Math.Max(
                        30,
                        width - 135
                    ),
                    56
                );

            this.LayoutOverviewCards();

            this.LayoutOverviewPanels();

            this.LayoutFollowUp();

            this.LayoutPromotions();
        }

        private void LayoutOverviewCards()
        {
            if (this.tabOverview == null)
            {
                return;
            }

            Panel? summary =
                this.tabOverview.Controls
                    .OfType<Panel>()
                    .FirstOrDefault(
                        x =>
                            x.Height == 125
                    );

            if (summary == null)
            {
                return;
            }

            List<Panel> cards =
                summary.Controls
                    .OfType<Panel>()
                    .ToList();

            if (cards.Count != 6)
            {
                return;
            }

            int availableWidth =
                this.tabOverview.ClientSize.Width
                - 40;

            int gap = 12;

            int cardWidth =
                (
                    availableWidth
                    - (gap * 5)
                ) / 6;

            cardWidth =
                Math.Max(
                    130,
                    cardWidth
                );

            for (
                int i = 0;
                i < cards.Count;
                i++)
            {
                cards[i].Width =
                    cardWidth;

                cards[i].Location =
                    new Point(
                        20 +
                        i *
                        (
                            cardWidth
                            + gap
                        ),
                        10
                    );
            }
        }

        private void LayoutOverviewPanels()
        {
            if (this.pnlRenewalTrend == null ||
                this.pnlRetentionInsights == null)
            {
                return;
            }

            int width =
                this.tabOverview.ClientSize.Width;

            int height =
                this.tabOverview.ClientSize.Height;

            int gap = 15;

            int panelWidth =
                (
                    width - 40 - gap
                ) / 2;

            panelWidth =
                Math.Max(
                    400,
                    panelWidth
                );

            this.pnlRenewalTrend.Location =
                new Point(
                    20,
                    140
                );

            this.pnlRenewalTrend.Size =
                new Size(
                    panelWidth,
                    Math.Max(
                        350,
                        height - 165
                    )
                );

            this.pnlRetentionInsights.Location =
                new Point(
                    20 +
                    panelWidth +
                    gap,
                    140
                );

            this.pnlRetentionInsights.Size =
                new Size(
                    panelWidth,
                    Math.Max(
                        350,
                        height - 165
                    )
                );
        }

        private void LayoutFollowUp()
        {
            if (this.dgvRenewals == null)
            {
                return;
            }

            this.dgvRenewals.Dock =
                DockStyle.Fill;
        }

        private void LayoutPromotions()
        {
            if (this.dgvPromotions == null)
            {
                return;
            }

            this.dgvPromotions.Dock =
                DockStyle.Fill;
        }

        private void ManagerRenewalRetentionForm_Resize(
            object? sender,
            EventArgs e)
        {
            this.PerformLayoutResponsive();
        }

        // ========================================================
        // RENEWAL MODEL
        // ========================================================

        private class RenewalRecord
        {
            public string TenantName { get; set; } =
                string.Empty;

            public string Room { get; set; } =
                string.Empty;

            public DateTime? RenewalDate { get; set; }

            public string Status { get; set; } =
                string.Empty;

            public string TenantStatus { get; set; } =
                string.Empty;

            public string Notes { get; set; } =
                string.Empty;

            public string DaysText
            {
                get
                {
                    if (!this.RenewalDate.HasValue)
                    {
                        return "-";
                    }

                    int days =
                        (
                            this.RenewalDate.Value.Date
                            - DateTime.Today
                        ).Days;

                    if (days < 0)
                    {
                        return
                            $"{Math.Abs(days)} day(s) overdue";
                    }

                    if (days == 0)
                    {
                        return "Today";
                    }

                    return $"{days} day(s)";
                }
            }
        }

        // ========================================================
        // PROMOTION MODEL
        // ========================================================

        private class PromotionRecord
        {
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
        }
    }
}
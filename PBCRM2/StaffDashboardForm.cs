using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PBCRM2
{
    public class StaffDashboardForm : Form
    {
        private readonly string _fullName;
        private readonly ApiService _apiService;
        private readonly List<Button> _sidebarButtons =
        new List<Button>();

        private readonly List<Panel> _summaryCards =
            new List<Panel>();

        // =========================================================
        // COLORS
        // =========================================================

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

        private static readonly Color SidebarHover =
            Color.FromArgb(58, 44, 31);

        private static readonly Color SidebarSelected =
            Color.FromArgb(80, 59, 40);

        private static readonly Color BorderColor =
            Color.FromArgb(230, 225, 218);

        // =========================================================
        // CONTROLS
        // =========================================================

        private Panel pnlSidebar = null!;
        private Panel pnlContent = null!;

        private Label lblPageTitle = null!;
        private Label lblWelcome = null!;
        private Label lblDate = null!;

        private Panel pnlOperations = null!;
        private Panel pnlActivity = null!;

        private Button btnRefreshDashboard = null!;

        private Form? _currentModuleForm;

        // =========================================================
        // DASHBOARD VALUES
        // =========================================================

        private Label? lblTenantsValue;
        private Label? lblRoomsBedsValue;
        private Label? lblMaintenanceValue;
        private Label? lblSupportValue;

        private Label? lblTenantStatusValue;
        private Label? lblMaintenanceStatusValue;
        private Label? lblSupportStatusValue;
        private Label? lblFeedbackStatusValue;
        private Label? lblRoomsBedsStatusValue;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public StaffDashboardForm(ApiService apiService)
        {
            _apiService =
                apiService
                ?? throw new ArgumentNullException(
                    nameof(apiService));

            _fullName = "Staff";

            BuildDashboard();
        }

        // =========================================================
        // CONSTRUCTOR WITH FULL NAME
        // =========================================================

        public StaffDashboardForm(
            ApiService apiService,
            string fullName)
        {
            _apiService =
                apiService
                ?? throw new ArgumentNullException(
                    nameof(apiService));

            _fullName =
                string.IsNullOrWhiteSpace(fullName)
                    ? "Staff"
                    : fullName;

            BuildDashboard();
        }

        // =========================================================
        // BUILD DASHBOARD
        // =========================================================

        private void BuildDashboard()
        {
            Text = "PBCRM2 - Staff Dashboard";

            StartPosition =
                FormStartPosition.CenterScreen;

            Size =
                new Size(
                    1250,
                    720);

            MinimumSize =
                new Size(
                    850,
                    600);

            BackColor = ContentBg;

            FormBorderStyle =
                FormBorderStyle.Sizable;

            MaximizeBox = true;
            MinimizeBox = true;

            DoubleBuffered = true;

            BuildSidebar();
            BuildContentPanel();

            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);

            Resize +=
                (s, e) =>
                {
                    PerformResponsiveLayout();
                };

            Shown +=
                async (s, e) =>
                {
                    await LoadDashboardDataAsync();
                };

            ShowDashboard();
        }

        // =========================================================
        // SIDEBAR
        // =========================================================

        private void BuildSidebar()
        {
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = BrandBg
            };

            // =====================================================
            // LOGO
            // =====================================================

            var picLogo = new PictureBox
            {
                SizeMode =
                    PictureBoxSizeMode.Zoom,

                Size =
                    new Size(
                        190,
                        65),

                Location =
                    new Point(
                        30,
                        18),

                BackColor =
                    Color.Transparent
            };

            try
            {
                string logoPath =
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Resources",
                        "logo.png");

                if (File.Exists(logoPath))
                {
                    using var stream =
                        new FileStream(
                            logoPath,
                            FileMode.Open,
                            FileAccess.Read);

                    picLogo.Image =
                        Image.FromStream(stream);
                }
            }
            catch
            {
                // Ignore logo loading errors.
            }

            // =====================================================
            // SUBTITLE
            // =====================================================

            var lblSubtitle = new Label
            {
                Text = "BOARDING HOUSE CRM",

                ForeColor =
                    Color.FromArgb(
                        175,
                        155,
                        130),

                Font =
                    new Font(
                        "Segoe UI",
                        8,
                        FontStyle.Bold),

                AutoSize = false,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Size =
                    new Size(
                        250,
                        25),

                Location =
                    new Point(
                        0,
                        88)
            };

            var divider = new Panel
            {
                Size =
                    new Size(
                        190,
                        1),

                Location =
                    new Point(
                        30,
                        120),

                BackColor =
                    Color.FromArgb(
                        85,
                        65,
                        45)
            };

            pnlSidebar.Controls.Add(picLogo);
            pnlSidebar.Controls.Add(lblSubtitle);
            pnlSidebar.Controls.Add(divider);

            // =====================================================
            // MENU
            // =====================================================

            int y = 140;

            AddSidebarButton(
                "⌂",
                "Dashboard",
                y,
                true);

            y += 55;

            AddSidebarButton(
                "♙",
                "Tenant Management",
                y);

            y += 55;

            AddSidebarButton(
                "▣",
                "Rooms & Beds",
                y);

            y += 55;

            AddSidebarButton(
                "⚒",
                "Maintenance & Services",
                y);

            y += 55;

            AddSidebarButton(
                "☷",
                "Tenant Support & Communication",
                y);

            y += 55;

            AddSidebarButton(
                "★",
                "Feedback & Satisfaction",
                y);

            BuildUserPanel();
        }

        // =========================================================
        // SIDEBAR BUTTON
        // =========================================================

        private void AddSidebarButton(
            string icon,
            string text,
            int y,
            bool selected = false)
        {
            var button = new Button
            {
                Text =
                    $"  {icon}    {text}",

                Location =
                    new Point(
                        15,
                        y),

                Size =
                    new Size(
                        220,
                        45),

                FlatStyle =
                    FlatStyle.Flat,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                Font =
                    new Font(
                        "Segoe UI",
                        9.5f),

                Cursor =
                    Cursors.Hand,

                BackColor =
                    selected
                        ? SidebarSelected
                        : Color.Transparent,

                ForeColor =
                    selected
                        ? BrandAccent
                        : Color.FromArgb(
                            215,
                            205,
                            192),

                TabStop = false,

                Tag = text
            };

            button.FlatAppearance.BorderSize = 0;

            button.MouseEnter +=
                (s, e) =>
                {
                    if (!IsButtonSelected(button))
                    {
                        button.BackColor =
                            SidebarHover;

                        button.ForeColor =
                            BrandAccent;
                    }
                };

            button.MouseLeave +=
                (s, e) =>
                {
                    if (!IsButtonSelected(button))
                    {
                        button.BackColor =
                            Color.Transparent;

                        button.ForeColor =
                            Color.FromArgb(
                                215,
                                205,
                                192);
                    }
                };

            button.Click +=
                async (s, e) =>
                {
                    SetActiveButton(button);

                    await OpenModuleAsync(text);
                };

            _sidebarButtons.Add(button);

            pnlSidebar.Controls.Add(button);
        }

        // =========================================================
        // CHECK SELECTED BUTTON
        // =========================================================

        private bool IsButtonSelected(Button btn)
        {
            return btn.BackColor ==
                   SidebarSelected;
        }

        // =========================================================
        // SET ACTIVE BUTTON
        // =========================================================

        private void SetActiveButton(
            Button activeBtn)
        {
            foreach (var btn in _sidebarButtons)
            {
                if (btn == activeBtn)
                {
                    btn.BackColor =
                        SidebarSelected;

                    btn.ForeColor =
                        BrandAccent;
                }
                else
                {
                    btn.BackColor =
                        Color.Transparent;

                    btn.ForeColor =
                        Color.FromArgb(
                            215,
                            205,
                            192);
                }
            }
        }

        // =========================================================
        // HIGHLIGHT MENU BUTTON
        // =========================================================

        private void HighlightMenuButton(
            string moduleName)
        {
            foreach (var btn in _sidebarButtons)
            {
                if (btn.Tag?.ToString() ==
                    moduleName)
                {
                    SetActiveButton(btn);
                    break;
                }
            }
        }

        // =========================================================
        // CONTENT PANEL
        // =========================================================

        private void BuildContentPanel()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,

                BackColor = ContentBg,

                AutoScroll = true,

                Padding =
                    new Padding(0)
            };
        }

        // =========================================================
        // CLEAR CURRENT MODULE
        // =========================================================

        private void ClearCurrentModule()
        {
            if (_currentModuleForm != null)
            {
                try
                {
                    _currentModuleForm.Close();
                    _currentModuleForm.Dispose();
                }
                catch
                {
                    // Ignore cleanup errors.
                }

                _currentModuleForm = null;
            }

            pnlContent.Controls.Clear();
        }

        // =========================================================
        // DASHBOARD
        // =========================================================

        private void ShowDashboard()
        {
            ClearCurrentModule();

            HighlightMenuButton(
                "Dashboard");

            BuildDashboardContent();

            PerformResponsiveLayout();
        }

        // =========================================================
        // DASHBOARD DATA
        // =========================================================

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                var tenantsTask =
                    _apiService
                        .GetAsync<List<StaffDashboardTenantDto>>(
                            "api/Tenants");

                var roomsTask =
                    _apiService
                        .GetAsync<List<StaffDashboardRoomDto>>(
                            "api/Rooms");

                var maintenanceTask =
                    _apiService
                        .GetAsync<List<StaffDashboardMaintenanceDto>>(
                            "api/Maintenance");

                var feedbackTask =
                    _apiService
                        .GetAsync<List<StaffDashboardFeedbackDto>>(
                            "api/Feedback");

                await Task.WhenAll(
                    tenantsTask,
                    roomsTask,
                    maintenanceTask,
                    feedbackTask);

                var tenants =
                    await tenantsTask
                    ?? new List<StaffDashboardTenantDto>();

                var rooms =
                    await roomsTask
                    ?? new List<StaffDashboardRoomDto>();

                var maintenance =
                    await maintenanceTask
                    ?? new List<StaffDashboardMaintenanceDto>();

                var feedback =
                    await feedbackTask
                    ?? new List<StaffDashboardFeedbackDto>();

                int activeTenants =
                    tenants.Count(t =>
                        string.Equals(
                            t.Status,
                            "Active",
                            StringComparison.OrdinalIgnoreCase));

                int openMaintenance =
                    maintenance.Count(m =>
                        !IsCompletedMaintenanceStatus(
                            m.Status));

                int feedbackCount =
                    feedback.Count;

                int totalRooms =
                    rooms.Count;

                int totalBeds =
                    rooms.Sum(r =>
                        GetRoomBedCount(r));

                int totalRoomsAndBeds =
                    totalRooms + totalBeds;

                UpdateDashboardValue(
                    lblTenantsValue,
                    activeTenants.ToString());

                UpdateDashboardValue(
                    lblRoomsBedsValue,
                    totalRoomsAndBeds.ToString());

                UpdateDashboardValue(
                    lblMaintenanceValue,
                    openMaintenance.ToString());

                UpdateDashboardValue(
                    lblSupportValue,
                    feedbackCount.ToString());

                UpdateDashboardValue(
                    lblTenantStatusValue,
                    activeTenants.ToString());

                UpdateDashboardValue(
                    lblMaintenanceStatusValue,
                    openMaintenance.ToString());

                UpdateDashboardValue(
                    lblSupportStatusValue,
                    "0");

                UpdateDashboardValue(
                    lblFeedbackStatusValue,
                    feedbackCount.ToString());

                UpdateDashboardValue(
                    lblRoomsBedsStatusValue,
                    totalRoomsAndBeds.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load dashboard information.\n\n{ex.Message}",
                    "Staff Dashboard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // =========================================================
        // UPDATE LABEL
        // =========================================================

        private void UpdateDashboardValue(
            Label? label,
            string value)
        {
            if (label == null ||
                label.IsDisposed)
            {
                return;
            }

            label.Text = value;
        }

        // =========================================================
        // MAINTENANCE STATUS
        // =========================================================

        private bool IsCompletedMaintenanceStatus(
            string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            return
                status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase)
                ||
                status.Equals(
                    "Resolved",
                    StringComparison.OrdinalIgnoreCase)
                ||
                status.Equals(
                    "Closed",
                    StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // GET BED COUNT
        // =========================================================

        private int GetRoomBedCount(
            StaffDashboardRoomDto room)
        {
            if (room.Beds != null)
                return room.Beds.Count;

            if (room.BedCount.HasValue)
                return room.BedCount.Value;

            return 0;
        }

        // =========================================================
        // TENANT MANAGEMENT
        // =========================================================

        private void ShowTenantManagement()
        {
            ClearCurrentModule();

            HighlightMenuButton(
                "Tenant Management");

            pnlContent.SuspendLayout();

            var tenantForm =
                new TenantManagementForm(
                    _apiService)
                {
                    TopLevel = false,

                    FormBorderStyle =
                        FormBorderStyle.None,

                    Dock =
                        DockStyle.Fill,

                    Margin =
                        new Padding(0)
                };

            _currentModuleForm =
                tenantForm;

            pnlContent.Controls.Add(
                tenantForm);

            tenantForm.Show();

            tenantForm.BringToFront();

            pnlContent.ResumeLayout();
        }

        // =========================================================
        // ROOMS & BEDS
        // =========================================================

        private void ShowRoomsBeds()
        {
            ClearCurrentModule();

            HighlightMenuButton(
                "Rooms & Beds");

            pnlContent.SuspendLayout();

            var roomsForm =
                new StaffDashboardRoomsBedsForm(
                    _apiService)
                {
                    TopLevel = false,

                    FormBorderStyle =
                        FormBorderStyle.None,

                    Dock =
                        DockStyle.Fill,

                    Margin =
                        new Padding(0)
                };

            _currentModuleForm =
                roomsForm;

            pnlContent.Controls.Add(
                roomsForm);

            roomsForm.Show();

            roomsForm.BringToFront();

            pnlContent.ResumeLayout();
        }

        // =========================================================
        // MAINTENANCE
        // =========================================================

        private void ShowMaintenance()
        {
            ClearCurrentModule();

            HighlightMenuButton(
                "Maintenance & Services");

            pnlContent.SuspendLayout();

            var maintenanceForm =
                new StaffMaintenanceForm(
                    _apiService)
                {
                    TopLevel = false,

                    FormBorderStyle =
                        FormBorderStyle.None,

                    Dock =
                        DockStyle.Fill,

                    Margin =
                        new Padding(0),

                    BackColor =
                        ContentBg
                };

            _currentModuleForm =
                maintenanceForm;

            pnlContent.Controls.Add(
                maintenanceForm);

            maintenanceForm.Show();

            maintenanceForm.BringToFront();

            pnlContent.ResumeLayout();
        }

        // =========================================================
        // FEEDBACK
        // =========================================================

        private void ShowFeedback()
        {
            ClearCurrentModule();

            HighlightMenuButton(
                "Feedback & Satisfaction");

            pnlContent.SuspendLayout();

            var feedbackForm =
                new StaffFeedbackForm(
                    _apiService)
                {
                    TopLevel = false,

                    FormBorderStyle =
                        FormBorderStyle.None,

                    Dock =
                        DockStyle.Fill,

                    Margin =
                        new Padding(0),

                    BackColor =
                        ContentBg
                };

            _currentModuleForm =
                feedbackForm;

            pnlContent.Controls.Add(
                feedbackForm);

            feedbackForm.Show();

            feedbackForm.BringToFront();

            pnlContent.ResumeLayout();
        }

        // =========================================================
        // TENANT SUPPORT
        // =========================================================

        private void ShowTenantSupport()
        {
            ClearCurrentModule();

            HighlightMenuButton(
                "Tenant Support & Communication");

            pnlContent.SuspendLayout();

            var supportForm =
                new StaffTenantSupportForm(
                    _apiService)
                {
                    TopLevel = false,

                    FormBorderStyle =
                        FormBorderStyle.None,

                    Dock =
                        DockStyle.Fill,

                    Margin =
                        new Padding(0),

                    BackColor =
                        ContentBg
                };

            _currentModuleForm =
                supportForm;

            pnlContent.Controls.Add(
                supportForm);

            supportForm.Show();

            supportForm.BringToFront();

            pnlContent.ResumeLayout();
        }

        // =========================================================
        // USER PANEL
        // =========================================================

        private void BuildUserPanel()
        {
            var userPanel = new Panel
            {
                Dock =
                    DockStyle.Bottom,

                Height = 105,

                BackColor =
                    Color.FromArgb(
                        42,
                        31,
                        23)
            };

            var lblUser = new Label
            {
                Text = _fullName,

                ForeColor =
                    Color.White,

                Font =
                    new Font(
                        "Segoe UI",
                        9,
                        FontStyle.Bold),

                AutoSize = false,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                Size =
                    new Size(
                        200,
                        25),

                Location =
                    new Point(
                        25,
                        15)
            };

            var lblRole = new Label
            {
                Text = "Staff",

                ForeColor =
                    BrandAccent,

                Font =
                    new Font(
                        "Segoe UI",
                        8),

                AutoSize = false,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                Size =
                    new Size(
                        200,
                        20),

                Location =
                    new Point(
                        25,
                        40)
            };

            var btnLogout = new Button
            {
                Text = "LOG OUT",

                Location =
                    new Point(
                        25,
                        67),

                Size =
                    new Size(
                        90,
                        28),

                FlatStyle =
                    FlatStyle.Flat,

                BackColor =
                    Color.Transparent,

                ForeColor =
                    Color.FromArgb(
                        210,
                        190,
                        165),

                Font =
                    new Font(
                        "Segoe UI",
                        8,
                        FontStyle.Bold),

                Cursor =
                    Cursors.Hand
            };

            btnLogout.FlatAppearance.BorderColor =
                Color.FromArgb(
                    100,
                    80,
                    60);

            btnLogout.FlatAppearance.BorderSize = 1;

            btnLogout.Click +=
                (s, e) =>
                {
                    var result =
                        MessageBox.Show(
                            "Are you sure you want to log out?",
                            "Logout",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                    if (result ==
                        DialogResult.Yes)
                    {
                        DialogResult =
                            DialogResult.Retry;

                        Close();
                    }
                };

            userPanel.Controls.Add(lblUser);
            userPanel.Controls.Add(lblRole);
            userPanel.Controls.Add(btnLogout);

            pnlSidebar.Controls.Add(userPanel);
        }

        // =========================================================
        // DASHBOARD CONTENT
        // =========================================================

        private void BuildDashboardContent()
        {
            pnlContent.SuspendLayout();

            pnlContent.Controls.Clear();

            _summaryCards.Clear();

            lblPageTitle = new Label
            {
                Text = "Staff Dashboard",

                ForeColor =
                    TextDark,

                Font =
                    new Font(
                        "Segoe UI",
                        24,
                        FontStyle.Bold),

                AutoSize = true,

                Location =
                    new Point(
                        35,
                        30)
            };

            lblWelcome = new Label
            {
                Text =
                    $"Welcome back, {_fullName}",

                ForeColor =
                    TextMuted,

                Font =
                    new Font(
                        "Segoe UI",
                        10.5f),

                AutoSize = true,

                Location =
                    new Point(
                        38,
                        78)
            };

            lblDate = new Label
            {
                Text =
                    DateTime.Now.ToString(
                        "dddd, MMMM dd, yyyy"),

                ForeColor =
                    TextMuted,

                Font =
                    new Font(
                        "Segoe UI",
                        9),

                AutoSize = true
            };

            btnRefreshDashboard =
                new Button
                {
                    Text = "REFRESH",

                    Size =
                        new Size(
                            90,
                            30),

                    FlatStyle =
                        FlatStyle.Flat,

                    BackColor =
                        Color.Transparent,

                    ForeColor =
                        CtaColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            8,
                            FontStyle.Bold),

                    Cursor =
                        Cursors.Hand,

                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Right
                };

            btnRefreshDashboard.FlatAppearance.BorderColor =
                CtaColor;

            btnRefreshDashboard.FlatAppearance.BorderSize = 1;

            btnRefreshDashboard.Click +=
                async (s, e) =>
                {
                    btnRefreshDashboard.Enabled = false;

                    try
                    {
                        await LoadDashboardDataAsync();
                    }
                    finally
                    {
                        btnRefreshDashboard.Enabled = true;
                    }
                };

            pnlContent.Controls.Add(
                lblPageTitle);

            pnlContent.Controls.Add(
                lblWelcome);

            pnlContent.Controls.Add(
                lblDate);

            pnlContent.Controls.Add(
                btnRefreshDashboard);

            BuildSummaryCards();

            BuildMainPanels();

            pnlContent.ResumeLayout();
        }

        // =========================================================
        // SUMMARY CARDS
        // =========================================================

        private void BuildSummaryCards()
        {
            var tenantsCard =
                CreateSummaryCardPanel(
                    "TENANTS",
                    "0",
                    "Active tenant records");

            lblTenantsValue =
                FindValueLabel(tenantsCard);

            _summaryCards.Add(
                tenantsCard);

            var roomsCard =
                CreateSummaryCardPanel(
                    "ROOMS & BEDS",
                    "0",
                    "Rooms and beds");

            lblRoomsBedsValue =
                FindValueLabel(roomsCard);

            _summaryCards.Add(
                roomsCard);

            var maintenanceCard =
                CreateSummaryCardPanel(
                    "MAINTENANCE",
                    "0",
                    "Open requests");

            lblMaintenanceValue =
                FindValueLabel(
                    maintenanceCard);

            _summaryCards.Add(
                maintenanceCard);

            var supportCard =
                CreateSummaryCardPanel(
                    "FEEDBACK",
                    "0",
                    "Feedback responses");

            lblSupportValue =
                FindValueLabel(
                    supportCard);

            _summaryCards.Add(
                supportCard);

            foreach (var card in _summaryCards)
            {
                pnlContent.Controls.Add(card);
            }
        }

        // =========================================================
        // CREATE SUMMARY CARD
        // =========================================================

        private Panel CreateSummaryCardPanel(
            string title,
            string value,
            string description)
        {
            var card = new Panel
            {
                Size =
                    new Size(
                        220,
                        120),

                BackColor =
                    CardBg
            };

            var lblTitle = new Label
            {
                Text = title,

                Font =
                    new Font(
                        "Segoe UI",
                        8,
                        FontStyle.Bold),

                ForeColor =
                    TextMuted,

                AutoSize = false,

                Size =
                    new Size(
                        190,
                        20),

                Location =
                    new Point(
                        15,
                        12)
            };

            var lblValue = new Label
            {
                Text = value,

                Font =
                    new Font(
                        "Segoe UI",
                        20,
                        FontStyle.Bold),

                ForeColor =
                    CtaColor,

                AutoSize = false,

                Size =
                    new Size(
                        190,
                        38),

                Location =
                    new Point(
                        15,
                        35),

                Tag = "summary-value"
            };

            var lblDescription = new Label
            {
                Text = description,

                Font =
                    new Font(
                        "Segoe UI",
                        8),

                ForeColor =
                    TextMuted,

                AutoSize = false,

                Size =
                    new Size(
                        190,
                        20),

                Location =
                    new Point(
                        15,
                        82)
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblDescription);

            return card;
        }

        // =========================================================
        // FIND VALUE LABEL
        // =========================================================

        private Label? FindValueLabel(
            Panel panel)
        {
            foreach (Control control
                     in panel.Controls)
            {
                if (control is Label label &&
                    label.Tag?.ToString() ==
                    "summary-value")
                {
                    return label;
                }
            }

            return null;
        }

        // =========================================================
        // MAIN PANELS
        // =========================================================

        private void BuildMainPanels()
        {
            BuildStaffOperationsPanel();

            BuildStaffActivityPanel();
        }

        // =========================================================
        // STAFF OPERATIONS
        // =========================================================

        private void BuildStaffOperationsPanel()
        {
            pnlOperations = new Panel
            {
                BackColor =
                    Color.White
            };

            var lblTitle = new Label
            {
                Text = "Staff Operations",

                ForeColor =
                    TextDark,

                Font =
                    new Font(
                        "Segoe UI",
                        14,
                        FontStyle.Bold),

                AutoSize = true,

                Location =
                    new Point(
                        25,
                        20)
            };

            var lblSubtitle = new Label
            {
                Text =
                    "Handle assigned boarding house tasks and tenant support.",

                ForeColor =
                    TextMuted,

                Font =
                    new Font(
                        "Segoe UI",
                        9),

                AutoSize = false,

                Size =
                    new Size(
                        450,
                        30),

                Location =
                    new Point(
                        25,
                        52)
            };

            pnlOperations.Controls.Add(
                lblTitle);

            pnlOperations.Controls.Add(
                lblSubtitle);

            AddOperationRow(
                pnlOperations,
                "TENANTS",
                "Manage assigned tenant information.",
                95);

            AddOperationRow(
                pnlOperations,
                "ROOMS & BEDS",
                "View rooms, beds and request assignments.",
                145);

            AddOperationRow(
                pnlOperations,
                "MAINTENANCE",
                "Submit and monitor maintenance requests.",
                195);

            AddOperationRow(
                pnlOperations,
                "SUPPORT",
                "View tenant contact information and communicate.",
                245);

            AddOperationRow(
                pnlOperations,
                "FEEDBACK",
                "Send feedback links to tenants.",
                295);

            pnlContent.Controls.Add(
                pnlOperations);
        }

        // =========================================================
        // OPERATION ROW
        // =========================================================

        private void AddOperationRow(
            Panel parent,
            string title,
            string description,
            int y)
        {
            var lblTitle = new Label
            {
                Text = title,

                ForeColor =
                    TextDark,

                Font =
                    new Font(
                        "Segoe UI",
                        8.5f,
                        FontStyle.Bold),

                AutoSize = false,

                Size =
                    new Size(
                        125,
                        20),

                Location =
                    new Point(
                        25,
                        y)
            };

            var lblDescription = new Label
            {
                Text = description,

                ForeColor =
                    TextMuted,

                Font =
                    new Font(
                        "Segoe UI",
                        8),

                AutoSize = false,

                Size =
                    new Size(
                        250,
                        30),

                Location =
                    new Point(
                        145,
                        y - 2)
            };

            var btnOpen = new Button
            {
                Text = "OPEN",

                Size =
                    new Size(
                        65,
                        27),

                FlatStyle =
                    FlatStyle.Flat,

                BackColor =
                    Color.Transparent,

                ForeColor =
                    CtaColor,

                Font =
                    new Font(
                        "Segoe UI",
                        7.5f,
                        FontStyle.Bold),

                Cursor =
                    Cursors.Hand,

                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Right,

                Tag =
                    "operation-button"
            };

            btnOpen.FlatAppearance.BorderColor =
                CtaColor;

            btnOpen.FlatAppearance.BorderSize = 1;

            btnOpen.Click +=
                async (s, e) =>
                {
                    switch (title)
                    {
                        case "TENANTS":
                            ShowTenantManagement();
                            break;

                        case "ROOMS & BEDS":
                            ShowRoomsBeds();
                            break;

                        case "MAINTENANCE":
                            ShowMaintenance();
                            break;

                        case "SUPPORT":
                            ShowTenantSupport();
                            break;

                        case "FEEDBACK":
                            ShowFeedback();
                            break;
                    }

                    await Task.CompletedTask;
                };

            parent.Controls.Add(
                lblTitle);

            parent.Controls.Add(
                lblDescription);

            parent.Controls.Add(
                btnOpen);
        }

        // =========================================================
        // STAFF ACTIVITY
        // =========================================================

        private void BuildStaffActivityPanel()
        {
            pnlActivity = new Panel
            {
                BackColor =
                    Color.White
            };

            var lblTitle = new Label
            {
                Text = "Staff Overview",

                ForeColor =
                    TextDark,

                Font =
                    new Font(
                        "Segoe UI",
                        14,
                        FontStyle.Bold),

                AutoSize = true,

                Location =
                    new Point(
                        25,
                        20)
            };

            var lblSubtitle = new Label
            {
                Text =
                    "Assigned areas requiring attention.",

                ForeColor =
                    TextMuted,

                Font =
                    new Font(
                        "Segoe UI",
                        9),

                AutoSize = true,

                Location =
                    new Point(
                        25,
                        52)
            };

            pnlActivity.Controls.Add(
                lblTitle);

            pnlActivity.Controls.Add(
                lblSubtitle);

            lblTenantStatusValue =
                AddStatusItem(
                    pnlActivity,
                    "Tenant Records",
                    "0",
                    "Active tenant records.",
                    95);

            lblMaintenanceStatusValue =
                AddStatusItem(
                    pnlActivity,
                    "Maintenance",
                    "0",
                    "Maintenance requests to monitor.",
                    145);

            lblSupportStatusValue =
                AddStatusItem(
                    pnlActivity,
                    "Support",
                    "0",
                    "Tenant concerns requiring assistance.",
                    195);

            lblFeedbackStatusValue =
                AddStatusItem(
                    pnlActivity,
                    "Feedback",
                    "0",
                    "Feedback responses received.",
                    245);

            lblRoomsBedsStatusValue =
                AddStatusItem(
                    pnlActivity,
                    "Rooms & Beds",
                    "0",
                    "Rooms and beds in your branch.",
                    295);

            pnlContent.Controls.Add(
                pnlActivity);
        }

        // =========================================================
        // STATUS ITEM
        // =========================================================

        private Label AddStatusItem(
            Panel parent,
            string title,
            string value,
            string description,
            int y)
        {
            var lblValue = new Label
            {
                Text = value,

                ForeColor =
                    CtaColor,

                Font =
                    new Font(
                        "Segoe UI",
                        13,
                        FontStyle.Bold),

                AutoSize = false,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Size =
                    new Size(
                        45,
                        35),

                Location =
                    new Point(
                        25,
                        y)
            };

            var lblTitle = new Label
            {
                Text = title,

                ForeColor =
                    TextDark,

                Font =
                    new Font(
                        "Segoe UI",
                        9,
                        FontStyle.Bold),

                AutoSize = false,

                Size =
                    new Size(
                        200,
                        20),

                Location =
                    new Point(
                        85,
                        y)
            };

            var lblDescription = new Label
            {
                Text = description,

                ForeColor =
                    TextMuted,

                Font =
                    new Font(
                        "Segoe UI",
                        8),

                AutoSize = false,

                Size =
                    new Size(
                        390,
                        20),

                Location =
                    new Point(
                        85,
                        y + 19)
            };

            parent.Controls.Add(
                lblValue);

            parent.Controls.Add(
                lblTitle);

            parent.Controls.Add(
                lblDescription);

            return lblValue;
        }

        // =========================================================
        // RESPONSIVE LAYOUT
        // =========================================================

        private void PerformResponsiveLayout()
        {
            if (pnlContent == null)
                return;

            if (_currentModuleForm != null)
                return;

            int visibleWidth =
                pnlContent.ClientSize.Width;

            if (visibleWidth <= 0)
                return;

            pnlContent.SuspendLayout();

            const int leftMargin = 35;
            const int rightMargin = 35;
            const int gap = 20;

            const int minimumCardWidth = 180;

            int cardWidth =
                Math.Max(
                    minimumCardWidth,
                    (
                        visibleWidth -
                        leftMargin -
                        rightMargin -
                        (gap * 3)
                    ) / 4);

            int requiredWidth =
                leftMargin +
                (cardWidth * 4) +
                (gap * 3) +
                rightMargin;

            if (lblDate != null)
            {
                lblDate.Location =
                    new Point(
                        Math.Max(
                            leftMargin,
                            visibleWidth -
                            lblDate.Width -
                            rightMargin -
                            105),
                        35);
            }

            if (btnRefreshDashboard != null)
            {
                btnRefreshDashboard.Location =
                    new Point(
                        Math.Max(
                            leftMargin,
                            visibleWidth -
                            btnRefreshDashboard.Width -
                            rightMargin),
                        30);
            }

            for (
                int i = 0;
                i < _summaryCards.Count;
                i++)
            {
                int x =
                    leftMargin +
                    i *
                    (cardWidth + gap);

                _summaryCards[i].Location =
                    new Point(
                        x,
                        135);

                _summaryCards[i].Size =
                    new Size(
                        cardWidth,
                        120);

                ResizeSummaryCardContents(
                    _summaryCards[i],
                    cardWidth);
            }

            int panelsY = 275;

            if (pnlOperations != null &&
                pnlActivity != null)
            {
                if (visibleWidth >= 1000)
                {
                    int panelWidth =
                        (
                            visibleWidth -
                            leftMargin -
                            rightMargin -
                            gap
                        ) / 2;

                    panelWidth =
                        Math.Max(
                            350,
                            panelWidth);

                    pnlOperations.Location =
                        new Point(
                            leftMargin,
                            panelsY);

                    pnlActivity.Location =
                        new Point(
                            leftMargin +
                            panelWidth +
                            gap,
                            panelsY);

                    pnlOperations.Size =
                        new Size(
                            panelWidth,
                            335);

                    pnlActivity.Size =
                        new Size(
                            panelWidth,
                            335);

                    ResizeOperationControls(
                        pnlOperations,
                        panelWidth);

                    ResizeActivityControls(
                        pnlActivity,
                        panelWidth);
                }
                else
                {
                    int panelWidth =
                        Math.Max(
                            350,
                            visibleWidth -
                            leftMargin -
                            rightMargin);

                    pnlOperations.Location =
                        new Point(
                            leftMargin,
                            panelsY);

                    pnlOperations.Size =
                        new Size(
                            panelWidth,
                            335);

                    pnlActivity.Location =
                        new Point(
                            leftMargin,
                            panelsY +
                            335 +
                            gap);

                    pnlActivity.Size =
                        new Size(
                            panelWidth,
                            335);

                    ResizeOperationControls(
                        pnlOperations,
                        panelWidth);

                    ResizeActivityControls(
                        pnlActivity,
                        panelWidth);
                }
            }

            int bottom =
                pnlActivity != null
                    ? pnlActivity.Bottom + 40
                    : 700;

            int contentWidth =
                Math.Max(
                    visibleWidth,
                    requiredWidth);

            int contentHeight =
                Math.Max(
                    pnlContent.ClientSize.Height,
                    bottom);

            pnlContent.AutoScrollMinSize =
                new Size(
                    contentWidth,
                    contentHeight);

            pnlContent.ResumeLayout();
        }

        // =========================================================
        // RESIZE CARD CONTENT
        // =========================================================

        private void ResizeSummaryCardContents(
            Panel card,
            int width)
        {
            int innerWidth =
                Math.Max(
                    100,
                    width - 30);

            foreach (
                Control control
                in card.Controls)
            {
                if (control is Label label)
                {
                    label.Width =
                        innerWidth;
                }
            }
        }

        // =========================================================
        // RESIZE OPERATION CONTROLS
        // =========================================================

        private void ResizeOperationControls(
            Panel panel,
            int panelWidth)
        {
            foreach (
                Control control
                in panel.Controls)
            {
                if (
                    control.Tag?.ToString() ==
                    "operation-button")
                {
                    control.Location =
                        new Point(
                            Math.Max(
                                250,
                                panelWidth -
                                control.Width -
                                25),
                            control.Location.Y);
                }
            }
        }

        // =========================================================
        // RESIZE ACTIVITY CONTROLS
        // =========================================================

        private void ResizeActivityControls(
            Panel panel,
            int panelWidth)
        {
            foreach (
                Control control
                in panel.Controls)
            {
                if (control is Label label)
                {
                    if (label.Location.X >= 85)
                    {
                        label.Width =
                            Math.Max(
                                150,
                                panelWidth -
                                label.Location.X -
                                25);
                    }
                }
            }
        }

        // =========================================================
        // OPEN MODULE
        // =========================================================

        private async Task OpenModuleAsync(
            string moduleName)
        {
            switch (moduleName)
            {
                case "Dashboard":
                    ShowDashboard();

                    await LoadDashboardDataAsync();

                    break;

                case "Tenant Management":
                    ShowTenantManagement();
                    break;

                case "Rooms & Beds":
                    ShowRoomsBeds();
                    break;

                case "Maintenance & Services":
                    ShowMaintenance();
                    break;

                case "Tenant Support & Communication":
                    ShowTenantSupport();
                    break;

                case "Feedback & Satisfaction":
                    ShowFeedback();
                    break;
            }
        }

        // =========================================================
        // STAFF DASHBOARD TENANT DTO
        // =========================================================

        private class StaffDashboardTenantDto
        {
            public int Id { get; set; }

            public string FullName { get; set; } =
                string.Empty;

            public string Status { get; set; } =
                string.Empty;
        }

        // =========================================================
        // STAFF DASHBOARD ROOM DTO
        // =========================================================

        private class StaffDashboardRoomDto
        {
            public int Id { get; set; }

            public string RoomNumber { get; set; } =
                string.Empty;

            public int? BedCount { get; set; }

            public List<StaffDashboardBedDto>? Beds { get; set; }
        }

        // =========================================================
        // STAFF DASHBOARD BED DTO
        // =========================================================

        private class StaffDashboardBedDto
        {
            public int Id { get; set; }

            public string BedNumber { get; set; } =
                string.Empty;
        }

        // =========================================================
        // STAFF DASHBOARD MAINTENANCE DTO
        // =========================================================

        private class StaffDashboardMaintenanceDto
        {
            public int Id { get; set; }

            public string Status { get; set; } =
                string.Empty;
        }

        // =========================================================
        // STAFF DASHBOARD FEEDBACK DTO
        // =========================================================

        private class StaffDashboardFeedbackDto
        {
            public int Id { get; set; }

            public int Rating { get; set; }

            public string? Comment { get; set; }

            public DateTime SubmittedAt { get; set; }
        }
    }

    // =============================================================
    // STAFF TENANT SUPPORT FORM
    // =============================================================

    public class StaffTenantSupportForm : Form
    {
        private readonly ApiService _apiService;

        private DataGridView dgvTenants = null!;

        private TextBox txtSearch = null!;

        private Button btnRefresh = null!;

        private List<StaffSupportTenantDto> _tenants =
            new List<StaffSupportTenantDto>();

        private int _hoveredRowIndex = -1;
        private int _hoveredActionIndex = -1;

        private readonly ToolTip actionToolTip =
            new ToolTip();

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

        private static readonly Color TextDark =
            Color.FromArgb(55, 39, 20);

        private static readonly Color TextMuted =
            Color.FromArgb(120, 120, 120);

        private static readonly Color BorderColor =
            Color.FromArgb(225, 215, 200);

        private static readonly Color RowHoverBg =
            Color.FromArgb(248, 244, 238);

        private static readonly Color RowSelectedBg =
            Color.FromArgb(232, 220, 199);

        private static readonly Color ActiveBg =
            Color.FromArgb(232, 248, 240);

        private static readonly Color ActiveText =
            Color.FromArgb(16, 135, 91);

        private static readonly Color MovedBg =
            Color.FromArgb(245, 245, 245);

        private static readonly Color MovedText =
            Color.FromArgb(105, 105, 105);

        private static readonly Color ArchivedBg =
            Color.FromArgb(250, 235, 235);

        private static readonly Color ArchivedText =
            Color.FromArgb(175, 55, 55);

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public StaffTenantSupportForm(
            ApiService apiService)
        {
            _apiService =
                apiService
                ?? throw new ArgumentNullException(
                    nameof(apiService));

            Text =
                "Tenant Support & Communication";

            BackColor = PanelBg;

            FormBorderStyle =
                FormBorderStyle.None;

            Dock = DockStyle.Fill;

            DoubleBuffered = true;

            BuildInterface();

            Shown +=
                async (s, e) =>
                {
                    await LoadTenantsAsync();
                };
        }

        // =========================================================
        // BUILD INTERFACE
        // =========================================================

        private void BuildInterface()
        {
            // =====================================================
            // HEADER
            // =====================================================

            var header = new Panel
            {
                Dock = DockStyle.Top,

                // Slightly taller so the whole module
                // has more breathing room.
                Height = 130,

                BackColor = BrandBg
            };

            var title = new Label
            {
                // TITLE CASE - NOT ALL CAPS
                Text =
                    "Tenant Support & Communication",

                ForeColor =
                    Color.White,

                Font =
                    new Font(
                        "Segoe UI",
                        21F,
                        FontStyle.Bold),

                AutoSize = false,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                Size =
                    new Size(
                        900,
                        38),

                Location =
                    new Point(
                        25,
                        20)
            };

            var subtitle = new Label
            {
                Text =
                    "View tenant contact information and communicate with assigned tenants.",

                ForeColor =
                    BrandAccent,

                Font =
                    new Font(
                        "Segoe UI",
                        10F),

                AutoSize = false,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                Size =
                    new Size(
                        900,
                        25),

                Location =
                    new Point(
                        28,
                        67)
            };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            // =====================================================
            // TOOLBAR
            // =====================================================

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,

                Height = 88,

                BackColor = PanelBg,

                // Do not use padding for the controls.
                // We use the exact same X coordinate as
                // the table below.
                Padding = new Padding(0)
            };

            // =====================================================
            // SEARCH BAR
            // =====================================================

            txtSearch = new TextBox
            {
                // EXACT SAME LEFT POSITION AS TABLE
                Location =
                    new Point(
                        25,
                        20),

                Size =
                    new Size(
                        350,
                        34),

                Font =
                    new Font(
                        "Segoe UI",
                        9.5f),

                BorderStyle =
                    BorderStyle.FixedSingle,

                BackColor =
                    Color.White,

                ForeColor =
                    TextDark
            };

            txtSearch.PlaceholderText =
                "Search tenant name, email or phone...";

            txtSearch.TextChanged +=
                (s, e) =>
                {
                    ApplySearch();
                };

            // =====================================================
            // REFRESH BUTTON
            // =====================================================

            btnRefresh =
                CreateToolbarButton(
                    "REFRESH",
                    90);

            btnRefresh.Location =
                new Point(
                    390,
                    20);

            btnRefresh.Click +=
                async (s, e) =>
                {
                    btnRefresh.Enabled = false;

                    try
                    {
                        await LoadTenantsAsync();
                    }
                    finally
                    {
                        btnRefresh.Enabled = true;
                    }
                };

            toolbar.Controls.Add(txtSearch);
            toolbar.Controls.Add(btnRefresh);

            // =====================================================
            // TABLE CONTAINER
            // =====================================================

            var tablePanel = new Panel
            {
                Dock = DockStyle.Fill,

                BackColor = PanelBg,

                // EXACT SAME LEFT/RIGHT MARGIN AS SEARCH BAR
                //
                // Top = 10 moves the table slightly lower
                // while keeping the left edge aligned.
                Padding =
                    new Padding(
                        25,
                        10,
                        25,
                        25)
            };

            // =====================================================
            // GRID
            // =====================================================

            dgvTenants =
                new DataGridView
                {
                    Dock = DockStyle.Fill,

                    BackgroundColor =
                        Color.White,

                    BorderStyle =
                        BorderStyle.FixedSingle,

                    CellBorderStyle =
                        DataGridViewCellBorderStyle.SingleHorizontal,

                    GridColor =
                        BorderColor,

                    AllowUserToAddRows = false,

                    AllowUserToDeleteRows = false,

                    AllowUserToResizeRows = false,

                    AllowUserToResizeColumns = false,

                    ReadOnly = true,

                    MultiSelect = false,

                    SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect,

                    AutoGenerateColumns = false,

                    RowHeadersVisible = false,

                    AutoSizeColumnsMode =
                        DataGridViewAutoSizeColumnsMode.Fill,

                    AutoSizeRowsMode =
                        DataGridViewAutoSizeRowsMode.None,

                    RowTemplate =
                    {
                        Height = 52
                    },

                    EnableHeadersVisualStyles = false,

                    ColumnHeadersHeight = 44,

                    ColumnHeadersHeightSizeMode =
                        DataGridViewColumnHeadersHeightSizeMode.DisableResizing
                };

            // =====================================================
            // COLUMNS
            // =====================================================

            dgvTenants.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Tenant",

                    // DISPLAYED HEADER
                    // Internal Name remains Tenant
                    // so the existing code still works.
                    HeaderText = "Name",

                    DataPropertyName = "Tenant",

                    FillWeight = 25,

                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                });

            dgvTenants.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Email",

                    HeaderText = "Email Address",

                    DataPropertyName = "Email",

                    FillWeight = 27,

                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                });

            dgvTenants.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Phone",

                    HeaderText = "Contact Number",

                    DataPropertyName = "Phone",

                    FillWeight = 20,

                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                });

            dgvTenants.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",

                    HeaderText = "Status",

                    DataPropertyName = "Status",

                    FillWeight = 13,

                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                });

            dgvTenants.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Action",

                    HeaderText = "Action",

                    DataPropertyName = "Action",

                    FillWeight = 15,

                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                });

            // =====================================================
            // HEADER STYLE
            // =====================================================

            dgvTenants.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor =
                        BrandBg,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),

                    Alignment =
                        DataGridViewContentAlignment.MiddleLeft,

                    Padding =
                        new Padding(
                            10,
                            0,
                            10,
                            0),

                    SelectionBackColor =
                        BrandBg,

                    SelectionForeColor =
                        Color.White
                };

            // =====================================================
            // DEFAULT CELL STYLE
            // =====================================================

            dgvTenants.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    Font =
                        new Font(
                            "Segoe UI",
                            9F),

                    ForeColor =
                        TextDark,

                    BackColor =
                        Color.White,

                    SelectionBackColor =
                        RowSelectedBg,

                    SelectionForeColor =
                        TextDark,

                    Alignment =
                        DataGridViewContentAlignment.MiddleLeft,

                    Padding =
                        new Padding(
                            10,
                            0,
                            10,
                            0),

                    WrapMode =
                        DataGridViewTriState.False
                };

            // =====================================================
            // ALTERNATING ROW
            // =====================================================

            dgvTenants.AlternatingRowsDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor =
                        Color.FromArgb(
                            252,
                            250,
                            247),

                    ForeColor =
                        TextDark,

                    SelectionBackColor =
                        RowSelectedBg,

                    SelectionForeColor =
                        TextDark
                };

            // =====================================================
            // EVENTS
            // =====================================================

            dgvTenants.CellPainting +=
                DgvTenants_CellPainting;

            dgvTenants.CellMouseMove +=
                DgvTenants_CellMouseMove;

            dgvTenants.CellMouseLeave +=
                DgvTenants_CellMouseLeave;

            dgvTenants.CellClick +=
                DgvTenants_CellClick;

            dgvTenants.CellDoubleClick +=
                (s, e) =>
                {
                    if (e.RowIndex >= 0)
                    {
                        ViewSelectedTenant();
                    }
                };

            tablePanel.Controls.Add(
                dgvTenants);

            // =====================================================
            // CONTROL ORDER
            // =====================================================

            // Table is added first because it is Fill.
            // Toolbar and header are then docked above it.
            Controls.Add(
                tablePanel);

            Controls.Add(
                toolbar);

            Controls.Add(
                header);
        }

        // =========================================================
        // CREATE TOOLBAR BUTTON
        // =========================================================

        private Button CreateToolbarButton(
            string text,
            int width)
        {
            var button = new Button
            {
                Text = text,

                Size =
                    new Size(
                        width,
                        34),

                FlatStyle =
                    FlatStyle.Flat,

                BackColor =
                    Color.White,

                ForeColor =
                    CtaColor,

                Font =
                    new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold),

                Cursor =
                    Cursors.Hand,

                TabStop = false
            };

            button.FlatAppearance.BorderColor =
                CtaColor;

            button.FlatAppearance.BorderSize = 1;

            button.MouseEnter +=
                (s, e) =>
                {
                    if (button.Enabled)
                    {
                        button.BackColor =
                            Color.FromArgb(
                                245,
                                237,
                                225);

                        button.ForeColor =
                            TextDark;
                    }
                };

            button.MouseLeave +=
                (s, e) =>
                {
                    button.BackColor =
                        Color.White;

                    button.ForeColor =
                        CtaColor;
                };

            return button;
        }

        // =========================================================
        // LOAD TENANTS
        // =========================================================

        private async Task LoadTenantsAsync()
        {
            try
            {
                var result =
                    await _apiService
                        .GetAsync<List<StaffSupportTenantDto>>(
                            "api/Tenants");

                if (result == null)
                    return;

                _tenants =
                    result
                        .OrderBy(
                            t =>
                                t.FullName)
                        .ToList();

                ApplySearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load tenant support information.\n\n{ex.Message}",
                    "Tenant Support",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // =========================================================
        // SEARCH
        // =========================================================

        private void ApplySearch()
        {
            if (dgvTenants == null)
                return;

            string search =
                txtSearch?.Text
                ?.Trim()
                ?? string.Empty;

            IEnumerable<StaffSupportTenantDto> filtered =
                _tenants;

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered =
                    _tenants.Where(
                        t =>
                            Contains(
                                t.FullName,
                                search)
                            ||
                            Contains(
                                t.Email,
                                search)
                            ||
                            Contains(
                                t.ContactNumber,
                                search));
            }

            dgvTenants.DataSource =
                filtered
                    .Select(
                        t =>
                            new
                            {
                                Tenant =
                                    t.FullName,

                                Email =
                                    DisplayValue(
                                        t.Email),

                                Phone =
                                    DisplayValue(
                                        t.ContactNumber),

                                Status =
                                    GetDisplayStatus(
                                        t.Status),

                                Action =
                                    "VIEW / EMAIL"
                            })
                    .ToList();

            dgvTenants.ClearSelection();

            _hoveredRowIndex = -1;
            _hoveredActionIndex = -1;

            dgvTenants.Invalidate();
        }

        // =========================================================
        // GET DISPLAY STATUS
        // =========================================================

        private string GetDisplayStatus(
            string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "Active";

            if (status.Equals(
                    "Moved Out",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Moved Out";
            }

            if (status.Equals(
                    "Archived",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Archived";
            }

            return "Active";
        }

        // =========================================================
        // CONTAINS
        // =========================================================

        private bool Contains(
            string? value,
            string search)
        {
            return
                !string.IsNullOrWhiteSpace(value)
                &&
                value.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // CELL PAINTING
        // =========================================================

        private void DgvTenants_CellPainting(
            object? sender,
            DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (e.ColumnIndex < 0)
                return;

            string columnName =
                dgvTenants.Columns[
                    e.ColumnIndex]
                .Name;

            // =====================================================
            // STATUS PILL
            // =====================================================

            if (columnName == "Status")
            {
                e.Handled = true;

                e.PaintBackground(
                    e.CellBounds,
                    true);

                string status =
                    e.Value?.ToString()
                    ?? "Active";

                Color bg;
                Color fg;

                if (status.Equals(
                        "Moved Out",
                        StringComparison.OrdinalIgnoreCase))
                {
                    bg = MovedBg;
                    fg = MovedText;
                }
                else if (status.Equals(
                             "Archived",
                             StringComparison.OrdinalIgnoreCase))
                {
                    bg = ArchivedBg;
                    fg = ArchivedText;
                }
                else
                {
                    bg = ActiveBg;
                    fg = ActiveText;
                }

                int pillWidth =
                    status == "Moved Out"
                        ? 82
                        : status == "Archived"
                            ? 74
                            : 58;

                int pillHeight = 22;

                pillWidth =
                    Math.Max(
                        1,
                        Math.Min(
                            Math.Max(
                                1,
                                e.CellBounds.Width - 16),
                            pillWidth));

                pillHeight =
                    Math.Min(
                        22,
                        Math.Max(
                            1,
                            e.CellBounds.Height - 8));

                var pill =
                    new Rectangle(
                        e.CellBounds.X + 8,
                        e.CellBounds.Y +
                        Math.Max(
                            0,
                            (
                                e.CellBounds.Height -
                                pillHeight
                            ) / 2),
                        pillWidth,
                        pillHeight);

                using var bgBrush =
                    new SolidBrush(bg);

                using var fgBrush =
                    new SolidBrush(fg);

                FillRoundedRectangle(
                    e.Graphics,
                    bgBrush,
                    pill,
                    8);

                using var sf =
                    new StringFormat
                    {
                        Alignment =
                            StringAlignment.Center,

                        LineAlignment =
                            StringAlignment.Center
                    };

                using var statusFont =
                    new Font(
                        "Segoe UI",
                        7.5F,
                        FontStyle.Bold);

                e.Graphics.DrawString(
                    status,
                    statusFont,
                    fgBrush,
                    pill,
                    sf);

                return;
            }

            // =====================================================
            // ACTION CELL
            // =====================================================

            if (columnName == "Action")
            {
                e.Handled = true;

                e.PaintBackground(
                    e.CellBounds,
                    true);

                int width =
                    Math.Max(
                        1,
                        e.CellBounds.Width);

                int actionWidth =
                    Math.Max(
                        1,
                        width / 2);

                var viewRect =
                    new Rectangle(
                        e.CellBounds.X + 8,
                        e.CellBounds.Y + 9,
                        Math.Max(
                            1,
                            actionWidth - 12),
                        Math.Max(
                            1,
                            e.CellBounds.Height - 18));

                var emailRect =
                    new Rectangle(
                        e.CellBounds.X +
                        actionWidth + 2,
                        e.CellBounds.Y + 9,
                        Math.Max(
                            1,
                            width -
                            actionWidth -
                            10),
                        Math.Max(
                            1,
                            e.CellBounds.Height - 18));

                int hoveredAction =
                    e.RowIndex == _hoveredRowIndex
                        ? _hoveredActionIndex
                        : -1;

                if (hoveredAction == 0)
                {
                    using var hoverBrush =
                        new SolidBrush(
                            Color.FromArgb(
                                245,
                                237,
                                225));

                    FillRoundedRectangle(
                        e.Graphics,
                        hoverBrush,
                        viewRect,
                        7);
                }
                else if (hoveredAction == 1)
                {
                    using var hoverBrush =
                        new SolidBrush(
                            Color.FromArgb(
                                245,
                                237,
                                225));

                    FillRoundedRectangle(
                        e.Graphics,
                        hoverBrush,
                        emailRect,
                        7);
                }

                using var actionFont =
                    new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold);

                using var actionBrush =
                    new SolidBrush(
                        CtaColor);

                using var sf =
                    new StringFormat
                    {
                        Alignment =
                            StringAlignment.Center,

                        LineAlignment =
                            StringAlignment.Center
                    };

                e.Graphics.DrawString(
                    "VIEW",
                    actionFont,
                    actionBrush,
                    viewRect,
                    sf);

                e.Graphics.DrawString(
                    "EMAIL",
                    actionFont,
                    actionBrush,
                    emailRect,
                    sf);

                return;
            }
        }

        // =========================================================
        // CELL MOUSE MOVE
        // =========================================================

        private void DgvTenants_CellMouseMove(
            object? sender,
            DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
            {
                ResetHover();
                return;
            }

            string columnName =
                dgvTenants.Columns[
                    e.ColumnIndex]
                .Name;

            if (columnName != "Action")
            {
                ResetHover();

                dgvTenants.Cursor =
                    Cursors.Default;

                return;
            }

            int actionIndex =
                GetActionIndex(
                    e.X,
                    dgvTenants
                        .Columns[e.ColumnIndex]
                        .Width);

            if (actionIndex < 0)
            {
                ResetHover();
                return;
            }

            bool changed =
                _hoveredRowIndex != e.RowIndex
                ||
                _hoveredActionIndex != actionIndex;

            _hoveredRowIndex =
                e.RowIndex;

            _hoveredActionIndex =
                actionIndex;

            dgvTenants.Cursor =
                Cursors.Hand;

            if (changed)
            {
                actionToolTip.SetToolTip(
                    dgvTenants,
                    actionIndex == 0
                        ? "View Tenant"
                        : "Email Tenant");

                dgvTenants.InvalidateCell(
                    e.ColumnIndex,
                    e.RowIndex);
            }
        }

        // =========================================================
        // CELL MOUSE LEAVE
        // =========================================================

        private void DgvTenants_CellMouseLeave(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            ResetHover();
        }

        // =========================================================
        // RESET HOVER
        // =========================================================

        private void ResetHover()
        {
            if (_hoveredRowIndex >= 0)
            {
                int row =
                    _hoveredRowIndex;

                _hoveredRowIndex = -1;
                _hoveredActionIndex = -1;

                if (dgvTenants != null &&
                    !dgvTenants.IsDisposed)
                {
                    dgvTenants.InvalidateRow(row);
                }
            }
            else
            {
                _hoveredActionIndex = -1;
            }

            if (dgvTenants != null &&
                !dgvTenants.IsDisposed)
            {
                dgvTenants.Cursor =
                    Cursors.Default;
            }

            actionToolTip.SetToolTip(
                dgvTenants,
                string.Empty);
        }

        // =========================================================
        // GET ACTION INDEX
        // =========================================================

        private int GetActionIndex(
            int mouseX,
            int cellWidth)
        {
            int width =
                Math.Max(
                    1,
                    cellWidth);

            int x =
                Math.Max(
                    0,
                    Math.Min(
                        width - 1,
                        mouseX));

            if (x < width / 2)
                return 0;

            return 1;
        }

        // =========================================================
        // CELL CLICK
        // =========================================================

        private void DgvTenants_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
            {
                return;
            }

            if (dgvTenants.Columns[
                    e.ColumnIndex]
                .Name != "Action")
            {
                return;
            }

            var row =
                dgvTenants.Rows[
                    e.RowIndex];

            if (row == null)
                return;

            string? tenantName =
                row.Cells["Tenant"]
                    .Value
                    ?.ToString();

            if (string.IsNullOrWhiteSpace(
                    tenantName))
            {
                return;
            }

            var tenant =
                _tenants.FirstOrDefault(
                    t =>
                        string.Equals(
                            t.FullName,
                            tenantName,
                            StringComparison.OrdinalIgnoreCase));

            if (tenant == null)
                return;

            int actionIndex =
                GetActionIndex(
                    e.ColumnIndex >= 0
                        ? dgvTenants
                            .PointToClient(
                                Cursor.Position)
                            .X -
                          dgvTenants
                            .GetCellDisplayRectangle(
                                e.ColumnIndex,
                                e.RowIndex,
                                false)
                            .X
                        : 0,
                    dgvTenants
                        .Columns[e.ColumnIndex]
                        .Width);

            if (actionIndex == 0)
            {
                ViewSelectedTenant(
                    tenant);
            }
            else
            {
                EmailSelectedTenant(
                    tenant);
            }
        }

        // =========================================================
        // GET SELECTED TENANT
        // =========================================================

        private StaffSupportTenantDto?
            GetSelectedTenant()
        {
            if (dgvTenants.CurrentRow == null)
                return null;

            string? name =
                dgvTenants.CurrentRow
                    .Cells["Tenant"]
                    .Value
                    ?.ToString();

            if (string.IsNullOrWhiteSpace(name))
                return null;

            return
                _tenants.FirstOrDefault(
                    t =>
                        string.Equals(
                            t.FullName,
                            name,
                            StringComparison.OrdinalIgnoreCase));
        }

        // =========================================================
        // VIEW SELECTED TENANT
        // =========================================================

        private void ViewSelectedTenant()
        {
            var tenant =
                GetSelectedTenant();

            if (tenant == null)
            {
                MessageBox.Show(
                    "Please select a tenant first.",
                    "Tenant Support",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            ViewSelectedTenant(tenant);
        }

        // =========================================================
        // VIEW TENANT
        // =========================================================

        private void ViewSelectedTenant(
            StaffSupportTenantDto tenant)
        {
            MessageBox.Show(
                $"TENANT INFORMATION\n\n" +
                $"Name: {tenant.FullName}\n\n" +
                $"Email: {DisplayValue(tenant.Email)}\n\n" +
                $"Phone: {DisplayValue(tenant.ContactNumber)}\n\n" +
                $"Status: {DisplayValue(tenant.Status)}",
                "Tenant Support",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =========================================================
        // EMAIL SELECTED TENANT
        // =========================================================

        private void EmailSelectedTenant()
        {
            var tenant =
                GetSelectedTenant();

            if (tenant == null)
            {
                MessageBox.Show(
                    "Please select a tenant first.",
                    "Tenant Support",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            EmailSelectedTenant(tenant);
        }

        // =========================================================
        // EMAIL TENANT
        // =========================================================

        private void EmailSelectedTenant(
            StaffSupportTenantDto tenant)
        {
            if (string.IsNullOrWhiteSpace(
                    tenant.Email))
            {
                MessageBox.Show(
                    "This tenant does not have an email address.",
                    "Tenant Support",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName =
                            $"mailto:{tenant.Email}",

                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open the email application.\n\n{ex.Message}",
                    "Tenant Support",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // DISPLAY VALUE
        // =========================================================

        private string DisplayValue(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "Not provided"
                : value;
        }

        // =========================================================
        // ROUNDED RECTANGLE
        // =========================================================

        private static void FillRoundedRectangle(
            Graphics graphics,
            Brush brush,
            Rectangle rect,
            int radius)
        {
            if (graphics == null ||
                brush == null)
            {
                return;
            }

            if (rect.Width <= 0 ||
                rect.Height <= 0)
            {
                return;
            }

            int maxRadius =
                Math.Min(
                    rect.Width,
                    rect.Height) / 2;

            if (maxRadius <= 0)
            {
                graphics.FillRectangle(
                    brush,
                    rect);

                return;
            }

            radius =
                Math.Max(
                    0,
                    Math.Min(
                        radius,
                        maxRadius));

            if (radius == 0)
            {
                graphics.FillRectangle(
                    brush,
                    rect);

                return;
            }

            int diameter =
                radius * 2;

            using var path =
                new GraphicsPath();

            path.AddArc(
                new Rectangle(
                    rect.X,
                    rect.Y,
                    diameter,
                    diameter),
                180,
                90);

            path.AddArc(
                new Rectangle(
                    rect.Right -
                    diameter,
                    rect.Y,
                    diameter,
                    diameter),
                270,
                90);

            path.AddArc(
                new Rectangle(
                    rect.Right -
                    diameter,
                    rect.Bottom -
                    diameter,
                    diameter,
                    diameter),
                0,
                90);

            path.AddArc(
                new Rectangle(
                    rect.X,
                    rect.Bottom -
                    diameter,
                    diameter,
                    diameter),
                90,
                90);

            path.CloseFigure();

            graphics.FillPath(
                brush,
                path);
        }

        // =========================================================
        // DTO
        // =========================================================

        private class StaffSupportTenantDto
        {
            public int Id { get; set; }

            public string FullName { get; set; } =
                string.Empty;

            public string Email { get; set; } =
                string.Empty;

            public string ContactNumber { get; set; } =
                string.Empty;

            public string Status { get; set; } =
                string.Empty;
        }
    }
}
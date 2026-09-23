using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PBCRM2
{
    public class SuperAdminDashboardForm : Form
    {
        private readonly string _fullName;
        private readonly ApiService _apiService;

        private readonly List<Button> _sidebarButtons =
            new List<Button>();

        private readonly List<Panel> _summaryCards =
            new List<Panel>();

        // =========================
        // COLORS
        // =========================

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

        // =========================
        // CONTROLS
        // =========================

        private Panel pnlSidebar = null!;
        private Panel pnlContent = null!;

        private Label lblPageTitle = null!;
        private Label lblWelcome = null!;
        private Label lblDate = null!;

        private Panel pnlOperations = null!;
        private Panel pnlActivity = null!;

        private Form? _currentModuleForm;

        // =========================
        // CONSTRUCTOR
        // =========================

        public SuperAdminDashboardForm(
            ApiService apiService)
        {
            _apiService = apiService
                ?? throw new ArgumentNullException(
                    nameof(apiService));

            _fullName = "Super Admin";

            BuildDashboard();
        }

        // =========================
        // BUILD DASHBOARD
        // =========================

        private void BuildDashboard()
        {
            Text =
                "PBCRM2 - Super Admin Dashboard";

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

            BackColor =
                ContentBg;

            FormBorderStyle =
                FormBorderStyle.Sizable;

            MaximizeBox = true;
            MinimizeBox = true;

            DoubleBuffered = true;

            BuildSidebar();
            BuildContentPanel();

            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);

            Resize += (s, e) =>
            {
                PerformResponsiveLayout();
            };

            ShowDashboard();
        }

        // =========================
        // SIDEBAR
        // =========================

        private void BuildSidebar()
        {
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = BrandBg
            };

            // =========================
            // LOGO
            // =========================

            PictureBox picLogo = new PictureBox
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
                    using Image tempImage =
                        Image.FromFile(logoPath);

                    picLogo.Image =
                        new Bitmap(tempImage);
                }
            }
            catch
            {
                // Ignore logo loading errors
            }

            // =========================
            // SUBTITLE
            // =========================

            Label lblSubtitle = new Label
            {
                Text =
                    "BOARDING HOUSE CRM",

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

            Panel divider = new Panel
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

            // =========================
            // MENU
            // =========================

            int y = 140;

            AddSidebarButton(
                "⌂",
                "Dashboard",
                y,
                true);

            y += 55;

            AddSidebarButton(
                "♙",
                "User & Role Management",
                y);

            y += 55;

            AddSidebarButton(
                "⚙",
                "System Settings",
                y);

            BuildUserPanel();
        }

        // =========================
        // SIDEBAR BUTTON
        // =========================

        private void AddSidebarButton(
            string icon,
            string text,
            int y,
            bool selected = false)
        {
            Button button = new Button
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

            button.MouseEnter += (s, e) =>
            {
                if (!IsButtonSelected(button))
                {
                    button.BackColor =
                        SidebarHover;

                    button.ForeColor =
                        BrandAccent;
                }
            };

            button.MouseLeave += (s, e) =>
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

            button.Click += (s, e) =>
            {
                SetActiveButton(button);

                OpenModule(text);
            };

            _sidebarButtons.Add(button);

            pnlSidebar.Controls.Add(button);
        }

        // =========================
        // SELECTED BUTTON
        // =========================

        private bool IsButtonSelected(
            Button btn)
        {
            return btn.BackColor ==
                   SidebarSelected;
        }

        private void SetActiveButton(
            Button activeBtn)
        {
            foreach (Button btn in _sidebarButtons)
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

        private void HighlightMenuButton(
            string moduleName)
        {
            foreach (Button btn in _sidebarButtons)
            {
                if (btn.Tag?.ToString() ==
                    moduleName)
                {
                    SetActiveButton(btn);
                    break;
                }
            }
        }

        // =========================
        // CONTENT PANEL
        // =========================

        private void BuildContentPanel()
        {
            pnlContent = new Panel
            {
                Dock =
                    DockStyle.Fill,

                BackColor =
                    ContentBg,

                AutoScroll =
                    true,

                Padding =
                    new Padding(0)
            };
        }

        // =========================
        // MODULE CLEANUP
        // =========================

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
                    // Ignore cleanup errors
                }

                _currentModuleForm = null;
            }

            pnlContent.Controls.Clear();
        }

        // =========================
        // DASHBOARD
        // =========================

        private void ShowDashboard()
        {
            ClearCurrentModule();

            HighlightMenuButton(
                "Dashboard");

            BuildDashboardContent();

            PerformResponsiveLayout();
        }

        // =========================
        // USER MANAGEMENT
        // =========================
        //
        // IMPORTANT:
        // This now opens the ACTUAL
        // UserManagementForm directly.
        //
        // It no longer displays the
        // old white placeholder card.
        // =========================

        private void ShowUserManagement()
        {
            OpenActualUserManagement();
        }

        // =========================
        // ACTUAL USER MANAGEMENT
        // =========================

        private void OpenActualUserManagement()
        {
            try
            {
                ClearCurrentModule();

                HighlightMenuButton(
                    "User & Role Management");

                UserManagementForm form =
                    new UserManagementForm(
                        _apiService);

                form.TopLevel =
                    false;

                form.FormBorderStyle =
                    FormBorderStyle.None;

                form.Dock =
                    DockStyle.Fill;

                form.Margin =
                    new Padding(0);

                form.BackColor =
                    ContentBg;

                _currentModuleForm =
                    form;

                pnlContent.Controls.Add(
                    form);

                form.Show();
                form.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to open User & Role Management.\n\n" +
                    ex.Message,
                    "User & Role Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================
        // SYSTEM SETTINGS
        // =========================

        private void ShowSystemSettings()
        {
            ClearCurrentModule();

            HighlightMenuButton(
                "System Settings");

            Panel panel = new Panel
            {
                Dock =
                    DockStyle.Fill,

                BackColor =
                    ContentBg
            };

            Label title = new Label
            {
                Text =
                    "System Settings",

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

            Label subtitle = new Label
            {
                Text =
                    "System-level configuration for PBCRM2.",

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

            Panel card = new Panel
            {
                Location =
                    new Point(
                        35,
                        135),

                Size =
                    new Size(
                        800,
                        250),

                BackColor =
                    CardBg
            };

            Label cardTitle = new Label
            {
                Text =
                    "System Configuration",

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
                        25)
            };

            Label description = new Label
            {
                Text =
                    "System Settings is reserved for Super Admin functions.\n\n" +
                    "• System information\n" +
                    "• Security configuration\n" +
                    "• Application preferences\n" +
                    "• Database and system configuration",

                ForeColor =
                    TextMuted,

                Font =
                    new Font(
                        "Segoe UI",
                        9.5f),

                AutoSize = true,

                Location =
                    new Point(
                        25,
                        65)
            };

            card.Controls.Add(
                cardTitle);

            card.Controls.Add(
                description);

            panel.Controls.Add(
                title);

            panel.Controls.Add(
                subtitle);

            panel.Controls.Add(
                card);

            pnlContent.Controls.Add(
                panel);
        }

        // =========================
        // USER PANEL
        // =========================

        private void BuildUserPanel()
        {
            Panel userPanel = new Panel
            {
                Dock =
                    DockStyle.Bottom,

                Height =
                    105,

                BackColor =
                    Color.FromArgb(
                        42,
                        31,
                        23)
            };

            Label lblUser = new Label
            {
                Text =
                    _fullName,

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

            Label lblRole = new Label
            {
                Text =
                    "Super Admin",

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

            Button btnLogout = new Button
            {
                Text =
                    "LOG OUT",

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
                    Logout();
                };

            userPanel.Controls.Add(
                lblUser);

            userPanel.Controls.Add(
                lblRole);

            userPanel.Controls.Add(
                btnLogout);

            pnlSidebar.Controls.Add(
                userPanel);
        }

        // =========================
        // DASHBOARD CONTENT
        // =========================

        private void BuildDashboardContent()
        {
            pnlContent.SuspendLayout();

            pnlContent.Controls.Clear();

            _summaryCards.Clear();

            // =========================
            // TITLE
            // =========================

            lblPageTitle = new Label
            {
                Text =
                    "Super Admin Dashboard",

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

            // =========================
            // WELCOME
            // =========================

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

            // =========================
            // DATE
            // =========================

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

            pnlContent.Controls.Add(
                lblPageTitle);

            pnlContent.Controls.Add(
                lblWelcome);

            pnlContent.Controls.Add(
                lblDate);

            BuildSummaryCards();

            BuildMainPanels();

            pnlContent.ResumeLayout();
        }

        // =========================
        // SUMMARY CARDS
        // =========================

        private void BuildSummaryCards()
        {
            _summaryCards.Add(
                CreateSummaryCardPanel(
                    "SYSTEM USERS",
                    "0",
                    "Registered system accounts"));

            _summaryCards.Add(
                CreateSummaryCardPanel(
                    "ROLES",
                    "4",
                    "Available system roles"));

            _summaryCards.Add(
                CreateSummaryCardPanel(
                    "COMPANIES",
                    "0",
                    "System tenant companies"));

            _summaryCards.Add(
                CreateSummaryCardPanel(
                    "BRANCHES",
                    "0",
                    "Registered boarding branches"));

            foreach (Panel card in _summaryCards)
            {
                pnlContent.Controls.Add(
                    card);
            }
        }

        private Panel CreateSummaryCardPanel(
            string title,
            string value,
            string description)
        {
            Panel card = new Panel
            {
                Size =
                    new Size(
                        220,
                        120),

                BackColor =
                    CardBg
            };

            Label lblTitle = new Label
            {
                Text =
                    title,

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

            Label lblValue = new Label
            {
                Text =
                    value,

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
                        35)
            };

            Label lblDescription = new Label
            {
                Text =
                    description,

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

            card.Controls.Add(
                lblTitle);

            card.Controls.Add(
                lblValue);

            card.Controls.Add(
                lblDescription);

            return card;
        }

        // =========================
        // MAIN PANELS
        // =========================

        private void BuildMainPanels()
        {
            BuildSystemOperationsPanel();

            BuildSystemOverviewPanel();
        }

        // =========================
        // SYSTEM OPERATIONS
        // =========================

        private void BuildSystemOperationsPanel()
        {
            pnlOperations = new Panel
            {
                BackColor =
                    Color.White
            };

            Label lblTitle = new Label
            {
                Text =
                    "System Administration",

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

            Label lblSubtitle = new Label
            {
                Text =
                    "Manage system-level access and configuration.",

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
                "USERS",
                "Manage system user accounts and roles.",
                95);

            AddOperationRow(
                pnlOperations,
                "ROLES",
                "Control Admin, Manager, and Staff access.",
                145);

            AddOperationRow(
                pnlOperations,
                "COMPANIES",
                "Manage system tenant company assignments.",
                195);

            AddOperationRow(
                pnlOperations,
                "BRANCHES",
                "Manage Manager and Staff branch assignments.",
                245);

            AddOperationRow(
                pnlOperations,
                "SETTINGS",
                "Configure system-level preferences.",
                295);

            pnlContent.Controls.Add(
                pnlOperations);
        }

        // =========================
        // OPERATION ROW
        // =========================

        private void AddOperationRow(
            Panel parent,
            string title,
            string description,
            int y)
        {
            Label lblTitle = new Label
            {
                Text =
                    title,

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

            Label lblDescription = new Label
            {
                Text =
                    description,

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

            Button btnOpen = new Button
            {
                Text =
                    "OPEN",

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
                (s, e) =>
                {
                    if (title == "USERS" ||
                        title == "ROLES" ||
                        title == "COMPANIES" ||
                        title == "BRANCHES")
                    {
                        ShowUserManagement();
                        return;
                    }

                    if (title == "SETTINGS")
                    {
                        ShowSystemSettings();
                        return;
                    }
                };

            parent.Controls.Add(
                lblTitle);

            parent.Controls.Add(
                lblDescription);

            parent.Controls.Add(
                btnOpen);
        }

        // =========================
        // SYSTEM OVERVIEW
        // =========================

        private void BuildSystemOverviewPanel()
        {
            pnlActivity = new Panel
            {
                BackColor =
                    Color.White
            };

            Label lblTitle = new Label
            {
                Text =
                    "System Overview",

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

            Label lblSubtitle = new Label
            {
                Text =
                    "System-level areas under Super Admin control.",

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

            AddStatusItem(
                pnlActivity,
                "System Users",
                "0",
                "Registered user accounts.",
                95);

            AddStatusItem(
                pnlActivity,
                "Roles",
                "4",
                "Super Admin, Admin, Manager, Staff.",
                145);

            AddStatusItem(
                pnlActivity,
                "Companies",
                "0",
                "System tenant companies.",
                195);

            AddStatusItem(
                pnlActivity,
                "Branches",
                "0",
                "Registered branch records.",
                245);

            AddStatusItem(
                pnlActivity,
                "Security",
                "ON",
                "System access protection is enabled.",
                295);

            pnlContent.Controls.Add(
                pnlActivity);
        }

        // =========================
        // STATUS ITEM
        // =========================

        private void AddStatusItem(
            Panel parent,
            string title,
            string value,
            string description,
            int y)
        {
            Label lblValue = new Label
            {
                Text =
                    value,

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

            Label lblTitle = new Label
            {
                Text =
                    title,

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

            Label lblDescription = new Label
            {
                Text =
                    description,

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
        }

        // =========================
        // RESPONSIVE LAYOUT
        // =========================

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

            // =========================
            // DATE
            // =========================

            if (lblDate != null)
            {
                lblDate.Location =
                    new Point(
                        Math.Max(
                            leftMargin,
                            visibleWidth -
                            lblDate.Width -
                            rightMargin),
                        35);
            }

            // =========================
            // SUMMARY CARDS
            // =========================

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

            // =========================
            // MAIN PANELS
            // =========================

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

            // =========================
            // SCROLL SIZE
            // =========================

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

        // =========================
        // RESIZE SUMMARY CARDS
        // =========================

        private void ResizeSummaryCardContents(
            Panel card,
            int width)
        {
            int innerWidth =
                Math.Max(
                    100,
                    width - 30);

            foreach (Control control
                in card.Controls)
            {
                if (control is Label label)
                {
                    label.Width =
                        innerWidth;
                }
            }
        }

        // =========================
        // RESIZE OPERATION BUTTONS
        // =========================

        private void ResizeOperationControls(
            Panel panel,
            int panelWidth)
        {
            foreach (Control control
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

        // =========================
        // RESIZE ACTIVITY
        // =========================

        private void ResizeActivityControls(
            Panel panel,
            int panelWidth)
        {
            foreach (Control control
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

        // =========================
        // OPEN MODULE
        // =========================

        private void OpenModule(
            string moduleName)
        {
            switch (moduleName)
            {
                case "Dashboard":

                    ShowDashboard();

                    break;

                case "User & Role Management":

                    // OPEN THE REAL USER MANAGEMENT FORM
                    OpenActualUserManagement();

                    break;

                case "System Settings":

                    ShowSystemSettings();

                    break;
            }
        }

        // =========================
        // LOGOUT
        // =========================

        private void Logout()
        {
            DialogResult result =
                MessageBox.Show(
                    "Are you sure you want to log out?",
                    "Logout",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result !=
                DialogResult.Yes)
            {
                return;
            }

            Hide();

            LoginForm loginForm =
                new LoginForm();

            loginForm.FormClosed +=
                (s, e) =>
                {
                    Close();
                };

            loginForm.Show();
        }
    }
}
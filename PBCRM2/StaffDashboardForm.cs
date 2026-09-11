using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PBCRM2
{
    public class StaffDashboardForm : Form
    {
        private readonly string _fullName;

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

        // =========================
        // CONSTRUCTOR
        // =========================
        public StaffDashboardForm(string fullName)
        {
            _fullName = string.IsNullOrWhiteSpace(fullName)
                ? "Staff"
                : fullName;

            BuildDashboard();
        }

        // =========================
        // BUILD DASHBOARD
        // =========================
        private void BuildDashboard()
        {
            Text = "PBCRM2 - Staff Dashboard";

            StartPosition =
                FormStartPosition.CenterScreen;

            ClientSize =
                new Size(1250, 720);

            MinimumSize =
                new Size(1100, 650);

            BackColor = ContentBg;

            FormBorderStyle =
                FormBorderStyle.Sizable;

            MaximizeBox = true;
            MinimizeBox = true;
            DoubleBuffered = true;

            BuildSidebar();
            BuildContent();

            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);

            Resize += (s, e) =>
            {
                PerformResponsiveLayout();
            };

            PerformResponsiveLayout();
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
            var picLogo = new PictureBox
            {
                SizeMode =
                    PictureBoxSizeMode.Zoom,

                Size =
                    new Size(190, 65),

                Location =
                    new Point(30, 18),

                BackColor =
                    Color.Transparent
            };

            try
            {
                string logoPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Resources",
                    "logo.png");

                if (File.Exists(logoPath))
                {
                    picLogo.Image =
                        Image.FromFile(logoPath);
                }
            }
            catch
            {
                // Keep dashboard running
                // if the logo cannot be loaded.
            }

            // =========================
            // SUBTITLE
            // =========================
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
                    new Size(250, 25),

                Location =
                    new Point(0, 88)
            };

            pnlSidebar.Controls.Add(picLogo);
            pnlSidebar.Controls.Add(lblSubtitle);

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

        // =========================
        // SIDEBAR BUTTON
        // =========================
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
                    new Point(15, y),

                Size =
                    new Size(220, 45),

                FlatStyle =
                    FlatStyle.Flat,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                Font =
                    new Font(
                        "Segoe UI",
                        9.5f,
                        FontStyle.Regular),

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

                TabStop = false
            };

            button.FlatAppearance.BorderSize = 0;

            button.MouseEnter += (s, e) =>
            {
                if (!selected)
                {
                    button.BackColor =
                        SidebarHover;

                    button.ForeColor =
                        BrandAccent;
                }
            };

            button.MouseLeave += (s, e) =>
            {
                if (!selected)
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

            if (text != "Dashboard")
            {
                button.Click += (s, e) =>
                {
                    MessageBox.Show(
                        $"{text} module is ready for implementation.",
                        text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                };
            }

            pnlSidebar.Controls.Add(button);
        }

        // =========================
        // USER PANEL
        // =========================
        private void BuildUserPanel()
        {
            var userPanel = new Panel
            {
                Dock = DockStyle.Bottom,

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
                    new Size(180, 25),

                Location =
                    new Point(25, 15)
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
                    new Size(180, 20),

                Location =
                    new Point(25, 40)
            };

            var btnLogout = new Button
            {
                Text = "LOG OUT",

                Location =
                    new Point(25, 67),

                Size =
                    new Size(90, 28),

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

            btnLogout.Click += (s, e) =>
            {
                Close();
            };

            userPanel.Controls.Add(lblUser);
            userPanel.Controls.Add(lblRole);
            userPanel.Controls.Add(btnLogout);

            pnlSidebar.Controls.Add(userPanel);
        }

        // =========================
        // CONTENT
        // =========================
        private void BuildContent()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,

                BackColor =
                    ContentBg,

                AutoScroll = true
            };

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
                    new Point(35, 30)
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
                    new Point(38, 78)
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

            pnlContent.Controls.Add(
                lblPageTitle);

            pnlContent.Controls.Add(
                lblWelcome);

            pnlContent.Controls.Add(
                lblDate);

            BuildSummaryCards();
            BuildMainPanels();
        }

        // =========================
        // SUMMARY CARDS
        // =========================
        private void BuildSummaryCards()
        {
            CreateSummaryCard(
                "TENANTS",
                "0",
                "Assigned tenant records",
                35,
                135);

            CreateSummaryCard(
                "ROOMS & BEDS",
                "0",
                "Room and bed information",
                275,
                135);

            CreateSummaryCard(
                "MAINTENANCE",
                "0",
                "Open requests",
                515,
                135);

            CreateSummaryCard(
                "SUPPORT",
                "0",
                "Pending concerns",
                755,
                135);
        }

        // =========================
        // SUMMARY CARD
        // =========================
        private void CreateSummaryCard(
            string title,
            string value,
            string description,
            int x,
            int y)
        {
            var card = new Panel
            {
                Location =
                    new Point(x, y),

                Size =
                    new Size(220, 120),

                BackColor =
                    CardBg
            };

            // No painted border.

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
                    new Size(190, 20),

                Location =
                    new Point(15, 12)
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
                    new Size(190, 38),

                Location =
                    new Point(15, 35)
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
                    new Size(190, 20),

                Location =
                    new Point(15, 82)
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblDescription);

            pnlContent.Controls.Add(card);
        }

        // =========================
        // MAIN PANELS
        // =========================
        private void BuildMainPanels()
        {
            BuildStaffOperationsPanel();
            BuildStaffActivityPanel();
        }

        // =========================
        // STAFF OPERATIONS
        // =========================
        private void BuildStaffOperationsPanel()
        {
            pnlOperations = new Panel
            {
                Location =
                    new Point(35, 285),

                Size =
                    new Size(535, 335),

                BackColor =
                    Color.White
            };

            // No painted border.

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
                    new Point(25, 20)
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
                    new Size(470, 30),

                Location =
                    new Point(25, 52)
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
                "View room and bed information.",
                145);

            AddOperationRow(
                pnlOperations,
                "MAINTENANCE",
                "Submit and monitor maintenance requests.",
                195);

            AddOperationRow(
                pnlOperations,
                "SUPPORT",
                "Assist with tenant concerns.",
                245);

            AddOperationRow(
                pnlOperations,
                "FEEDBACK",
                "Send feedback links to tenants.",
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
                    new Size(125, 20),

                Location =
                    new Point(25, y)
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
                    new Size(270, 30),

                Location =
                    new Point(145, y - 2)
            };

            var btnOpen = new Button
            {
                Text = "OPEN",

                Size =
                    new Size(65, 27),

                Location =
                    new Point(440, y - 4),

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
                    Cursors.Hand
            };

            btnOpen.FlatAppearance.BorderColor =
                CtaColor;

            btnOpen.FlatAppearance.BorderSize = 1;

            btnOpen.Click += (s, e) =>
            {
                MessageBox.Show(
                    $"{title} module is ready for implementation.",
                    title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            };

            parent.Controls.Add(
                lblTitle);

            parent.Controls.Add(
                lblDescription);

            parent.Controls.Add(
                btnOpen);
        }

        // =========================
        // STAFF ACTIVITY
        // =========================
        private void BuildStaffActivityPanel()
        {
            pnlActivity = new Panel
            {
                Location =
                    new Point(590, 285),

                Size =
                    new Size(535, 335),

                BackColor =
                    Color.White
            };

            // No painted border.

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
                    new Point(25, 20)
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
                    new Point(25, 52)
            };

            pnlActivity.Controls.Add(
                lblTitle);

            pnlActivity.Controls.Add(
                lblSubtitle);

            AddStatusItem(
                pnlActivity,
                "Tenant Records",
                "0",
                "Assigned tenant records.",
                95);

            AddStatusItem(
                pnlActivity,
                "Maintenance",
                "0",
                "Maintenance requests to monitor.",
                145);

            AddStatusItem(
                pnlActivity,
                "Support",
                "0",
                "Tenant concerns requiring assistance.",
                195);

            AddStatusItem(
                pnlActivity,
                "Feedback",
                "0",
                "Feedback links and responses.",
                245);

            AddStatusItem(
                pnlActivity,
                "Rooms & Beds",
                "0",
                "Room and bed information.",
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
                    new Size(45, 35),

                Location =
                    new Point(25, y)
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
                    new Size(200, 20),

                Location =
                    new Point(85, y)
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
                    new Size(390, 20),

                Location =
                    new Point(85, y + 19)
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
            if (pnlContent == null ||
                pnlContent.Width <= 0)
            {
                return;
            }

            pnlContent.SuspendLayout();

            int sidePadding = 35;
            int gap = 20;

            int availableWidth =
                pnlContent.ClientSize.Width -
                (sidePadding * 2);

            // =========================
            // DATE
            // =========================
            if (lblDate != null)
            {
                lblDate.Location =
                    new Point(
                        pnlContent.ClientSize.Width -
                        lblDate.Width -
                        sidePadding,
                        82);
            }

            // =========================
            // MAIN PANELS
            // =========================
            int panelY = 285;

            int panelWidth =
                Math.Max(
                    450,
                    (availableWidth - gap) / 2);

            pnlOperations.Location =
                new Point(
                    sidePadding,
                    panelY);

            pnlOperations.Size =
                new Size(
                    panelWidth,
                    335);

            pnlActivity.Location =
                new Point(
                    sidePadding +
                    panelWidth +
                    gap,
                    panelY);

            pnlActivity.Size =
                new Size(
                    panelWidth,
                    335);

            pnlContent.ResumeLayout();
        }
    }
}
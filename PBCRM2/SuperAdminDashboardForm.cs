using System.Drawing;
using System.Windows.Forms;
using System;

namespace PBCRM2
{
    public class SuperAdminDashboardForm : Form
    {
        private readonly string _fullName;

        private static readonly Color BrandBg = Color.FromArgb(32, 24, 18);
        private static readonly Color BrandAccent = Color.FromArgb(224, 194, 140);
        private static readonly Color ContentBg = Color.FromArgb(247, 244, 239);
        private static readonly Color CardBg = Color.White;
        private static readonly Color TextDark = Color.FromArgb(55, 39, 20);
        private static readonly Color TextMuted = Color.FromArgb(120, 120, 120);

        public SuperAdminDashboardForm(string fullName)
        {
            _fullName = string.IsNullOrWhiteSpace(fullName) ? "Super Admin" : fullName;

            InitializeForm();
            BuildInterface();
        }

        private void InitializeForm()
        {
            Text = "PBCRM2 - Super Admin Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1250, 720);
            MinimumSize = new Size(1100, 650);
            BackColor = ContentBg;
        }

        private void BuildInterface()
        {
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 255,
                BackColor = BrandBg
            };

            var logo = new Label
            {
                Text = "PBCRM2",
                ForeColor = BrandAccent,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Location = new Point(25, 25),
                AutoSize = true
            };

            var subtitle = new Label
            {
                Text = "SUPER ADMIN PORTAL",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(28, 62),
                AutoSize = true
            };

            sidebar.Controls.Add(logo);
            sidebar.Controls.Add(subtitle);

            string[] menuItems =
            {
                "Dashboard",
                "User & Role Management",
                "System Settings",
                "Branch Management",
                "Reports"
            };

            int y = 120;

            foreach (string item in menuItems)
            {
                Button button = CreateMenuButton(item, y);
                sidebar.Controls.Add(button);
                y += 48;
            }

            var userPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 105,
                BackColor = Color.FromArgb(48, 36, 27)
            };

            var userName = new Label
            {
                Text = _fullName,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            var userRole = new Label
            {
                Text = "Super Admin",
                ForeColor = BrandAccent,
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 40),
                AutoSize = true
            };

            var logout = new Button
            {
                Text = "Log Out",
                Location = new Point(20, 65),
                Size = new Size(210, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(170, 130, 80),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };

            logout.FlatAppearance.BorderSize = 0;
            logout.Click += (s, e) => Close();

            userPanel.Controls.Add(userName);
            userPanel.Controls.Add(userRole);
            userPanel.Controls.Add(logout);

            sidebar.Controls.Add(userPanel);

            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentBg,
                AutoScroll = true
            };

            var title = new Label
            {
                Text = "Super Admin Dashboard",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(35, 30),
                AutoSize = true
            };

            var welcome = new Label
            {
                Text = $"Welcome, {_fullName}",
                Font = new Font("Segoe UI", 11),
                ForeColor = TextMuted,
                Location = new Point(38, 72),
                AutoSize = true
            };

            content.Controls.Add(title);
            content.Controls.Add(welcome);

            CreateSummaryCard(content, "USERS", "0", "Registered system users", 35, 125);
            CreateSummaryCard(content, "ADMINS", "0", "Admin accounts", 260, 125);
            CreateSummaryCard(content, "MANAGERS", "0", "Manager accounts", 485, 125);
            CreateSummaryCard(content, "STAFF", "0", "Staff accounts", 710, 125);

            var managementPanel = new Panel
            {
                Location = new Point(35, 270),
                Size = new Size(750, 280),
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            var panelTitle = new Label
            {
                Text = "System Management",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(20, 20),
                AutoSize = true
            };

            var panelText = new Label
            {
                Text =
                    "• Manage system users and roles\r\n\r\n" +
                    "• Configure system settings\r\n\r\n" +
                    "• Manage boarding house branches\r\n\r\n" +
                    "• Monitor system-wide reports\r\n\r\n" +
                    "• Maintain overall CRM configuration",
                Font = new Font("Segoe UI", 10),
                ForeColor = TextMuted,
                Location = new Point(25, 65),
                AutoSize = true
            };

            managementPanel.Controls.Add(panelTitle);
            managementPanel.Controls.Add(panelText);
            content.Controls.Add(managementPanel);

            Controls.Add(content);
            Controls.Add(sidebar);
        }

        private Button CreateMenuButton(string text, int y)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(15, y),
                Size = new Size(225, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = BrandBg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;

            button.Click += (s, e) =>
            {
                if (text != "Dashboard")
                {
                    MessageBox.Show(
                        $"{text} module is ready for implementation.",
                        "PBCRM2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            };

            return button;
        }

        private void CreateSummaryCard(
            Panel parent,
            string title,
            string value,
            string description,
            int x,
            int y)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(205, 115),
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = TextMuted,
                Location = new Point(15, 15),
                AutoSize = true
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 38),
                AutoSize = true
            };

            var lblDescription = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 8),
                ForeColor = TextMuted,
                Location = new Point(15, 80),
                AutoSize = true
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblDescription);

            parent.Controls.Add(card);
        }
    }
}
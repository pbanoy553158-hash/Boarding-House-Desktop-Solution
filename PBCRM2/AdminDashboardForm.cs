using System.Drawing.Drawing2D;

namespace PBCRM2
{
    public class AdminDashboardForm : Form
    {
        private static readonly Color BrandBg = Color.FromArgb(32, 24, 18);
        private static readonly Color BrandAccent = Color.FromArgb(224, 194, 140);
        private static readonly Color CtaColor = Color.FromArgb(170, 130, 80);

        private static readonly Color ContentBg = Color.FromArgb(247, 244, 239);
        private static readonly Color CardBg = Color.White;
        private static readonly Color TextDark = Color.FromArgb(55, 39, 20);
        private static readonly Color TextMuted = Color.FromArgb(120, 120, 120);

        private Panel pnlSidebar = null!;
        private Panel pnlContent = null!;
        private Panel pnlOverview = null!;
        private Label lblOverviewInfo = null!;
        private Label lblWelcome = null!;

        private readonly List<Panel> _summaryCards = new();
        private readonly Dictionary<Panel, (Label title, Label value, Label desc)> _cardLabels = new();
        private readonly string _fullName;

        public AdminDashboardForm(string fullName)
        {
            _fullName = fullName;
            BuildDashboard();
        }

        private void BuildDashboard()
        {
            Text = "PBCRM2 - Admin Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1250, 720);
            MinimumSize = new Size(1100, 650);
            BackColor = ContentBg;

            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            DoubleBuffered = true;

            BuildSidebar();
            BuildContent();

            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);

            Resize += (s, e) => PerformResponsiveLayout();
            PerformResponsiveLayout();
        }

        private void BuildSidebar()
        {
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = BrandBg
            };

            var picLogo = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(190, 65),
                Location = new Point(30, 18),
                BackColor = Color.Transparent
            };

            try
            {
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo.png");
                if (File.Exists(logoPath))
                {
                    picLogo.Image = Image.FromFile(logoPath);
                }
            }
            catch
            {
            }

            var lblSubtitle = new Label
            {
                Text = "BOARDING HOUSE CRM",
                ForeColor = Color.FromArgb(175, 155, 130),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(250, 25),
                Location = new Point(0, 88)
            };

            var divider = new Panel
            {
                Size = new Size(190, 1),
                Location = new Point(30, 120),
                BackColor = Color.FromArgb(85, 65, 45)
            };

            pnlSidebar.Controls.Add(picLogo);
            pnlSidebar.Controls.Add(lblSubtitle);
            pnlSidebar.Controls.Add(divider);

            int y = 140;
            AddSidebarButton("⌂", "Dashboard", y, true);
            y += 55;
            AddSidebarButton("♙", "Tenants", y);
            y += 55;
            AddSidebarButton("▣", "Rooms & Beds", y);
            y += 55;
            AddSidebarButton("₱", "Billing & Payments", y);
            y += 55;
            AddSidebarButton("⚒", "Maintenance & Services", y); // Corrected to match Use Case Diagram
            y += 55;
            AddSidebarButton("☷", "Feedback", y);
            y += 55;
            AddSidebarButton("▤", "Reports", y);

            var userPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 105,
                BackColor = Color.FromArgb(42, 31, 23)
            };

            var lblUser = new Label
            {
                Text = _fullName,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(180, 25),
                Location = new Point(25, 15)
            };

            var lblRole = new Label
            {
                Text = "Administrator",
                ForeColor = BrandAccent,
                Font = new Font("Segoe UI", 8),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(180, 20),
                Location = new Point(25, 40)
            };

            var btnLogout = new Button
            {
                Text = "LOG OUT",
                Location = new Point(25, 67),
                Size = new Size(90, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(210, 190, 165),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnLogout.FlatAppearance.BorderColor = Color.FromArgb(100, 80, 60);
            btnLogout.FlatAppearance.BorderSize = 1;
            btnLogout.Click += (s, e) => Close();

            userPanel.Controls.Add(lblUser);
            userPanel.Controls.Add(lblRole);
            userPanel.Controls.Add(btnLogout);

            pnlSidebar.Controls.Add(userPanel);
        }

        private void AddSidebarButton(string icon, string text, int y, bool selected = false)
        {
            var button = new Button
            {
                Text = $"  {icon}    {text}",
                Location = new Point(15, y),
                Size = new Size(220, 45),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Cursor = Cursors.Hand,
                BackColor = selected ? Color.FromArgb(80, 59, 40) : Color.Transparent,
                ForeColor = selected ? BrandAccent : Color.FromArgb(215, 205, 192)
            };

            button.FlatAppearance.BorderSize = 0;

            button.MouseEnter += (s, e) =>
            {
                if (!selected)
                {
                    button.BackColor = Color.FromArgb(58, 44, 31);
                    button.ForeColor = BrandAccent;
                }
            };

            button.MouseLeave += (s, e) =>
            {
                if (!selected)
                {
                    button.BackColor = Color.Transparent;
                    button.ForeColor = Color.FromArgb(215, 205, 192);
                }
            };

            pnlSidebar.Controls.Add(button);
        }

        private void BuildContent()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentBg,
                AutoScroll = true
            };

            var lblPageTitle = new Label
            {
                Text = "Dashboard",
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(35, 30)
            };

            lblWelcome = new Label
            {
                Text = $"Welcome back, {_fullName}",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(38, 78)
            };

            pnlContent.Controls.Add(lblPageTitle);
            pnlContent.Controls.Add(lblWelcome);

            BuildSummaryCards();
            BuildQuickOverview();
        }

        private void BuildSummaryCards()
        {
            var cardData = new[]
            {
                ("TENANTS", "0", "Registered tenants"),
                ("ROOMS", "0", "Available rooms"),
                ("BEDS", "0", "Occupied beds"),
                ("PAYMENTS", "₱0.00", "This month")
            };

            foreach (var (title, value, desc) in cardData)
            {
                var card = CreateSummaryCard(title, value, desc);
                _summaryCards.Add(card);
                pnlContent.Controls.Add(card);
            }
        }

        private Panel CreateSummaryCard(string title, string value, string description)
        {
            var card = new Panel
            {
                Size = new Size(200, 120),
                BackColor = CardBg
            };

            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(225, 215, 200));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = title,
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(170, 20),
                Location = new Point(15, 12)
            };

            var lblValue = new Label
            {
                Text = value,
                ForeColor = CtaColor,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(170, 38),
                Location = new Point(15, 35)
            };

            var lblDescription = new Label
            {
                Text = description,
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8),
                AutoSize = false,
                Size = new Size(170, 20),
                Location = new Point(15, 82)
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblDescription);

            _cardLabels[card] = (lblTitle, lblValue, lblDescription);

            return card;
        }

        private void BuildQuickOverview()
        {
            pnlOverview = new Panel
            {
                Size = new Size(850, 310),
                BackColor = Color.White
            };

            pnlOverview.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(225, 215, 200));
                e.Graphics.DrawRectangle(pen, 0, 0, pnlOverview.Width - 1, pnlOverview.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = "Quick Overview",
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 22)
            };

            lblOverviewInfo = new Label
            {
                Text =
                    "Use the navigation menu to manage your boarding house operations.\n\n" +
                    "• Manage tenant records and registrations\n" +
                    "• Monitor rooms and bed occupancy\n" +
                    "• Track billing and payments\n" +
                    "• Review maintenance requests\n" +
                    "• Monitor tenant feedback\n" +
                    "• Generate management reports",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 10),
                AutoSize = false,
                Size = new Size(750, 210),
                Location = new Point(25, 70)
            };

            pnlOverview.Controls.Add(lblTitle);
            pnlOverview.Controls.Add(lblOverviewInfo);

            pnlContent.Controls.Add(pnlOverview);
        }

        private void PerformResponsiveLayout()
        {
            if (pnlContent == null || pnlContent.Width <= 0) return;

            pnlContent.SuspendLayout();

            int sidePadding = 35;
            int gap = 20;
            int availableWidth = pnlContent.ClientSize.Width - (sidePadding * 2);

            if (_summaryCards.Count == 4)
            {
                int cardWidth = Math.Max(200, (availableWidth - (gap * 3)) / 4);
                int cardHeight = 120;
                int startY = 135;

                for (int i = 0; i < _summaryCards.Count; i++)
                {
                    var card = _summaryCards[i];
                    int x = sidePadding + i * (cardWidth + gap);
                    card.Location = new Point(x, startY);
                    card.Size = new Size(cardWidth, cardHeight);

                    if (_cardLabels.TryGetValue(card, out var labels))
                    {
                        int innerWidth = cardWidth - 30;
                        labels.title.Width = innerWidth;
                        labels.value.Width = innerWidth;
                        labels.desc.Width = innerWidth;
                    }

                    card.Invalidate();
                }
            }

            int overviewY = 285;
            pnlOverview.Location = new Point(sidePadding, overviewY);
            pnlOverview.Width = Math.Max(600, availableWidth);
            lblOverviewInfo.Width = pnlOverview.Width - 50;
            pnlOverview.Invalidate();

            pnlContent.ResumeLayout();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace PBCRM2
{
    public class BorderedPanel : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.FromArgb(225, 215, 200);

        public BorderedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(BackColor);
            using var pen = new Pen(BorderColor);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }

    public class ManagerDashboardForm : Form
    {
        private static readonly Color BrandBg = Color.FromArgb(32, 24, 18);
        private static readonly Color BrandAccent = Color.FromArgb(224, 194, 140);
        private static readonly Color CtaColor = Color.FromArgb(170, 130, 80);

        private static readonly Color ContentBg = Color.FromArgb(247, 244, 239);
        private static readonly Color CardBg = Color.White;
        private static readonly Color TextDark = Color.FromArgb(55, 39, 20);
        private static readonly Color TextMuted = Color.FromArgb(120, 120, 120);
        private static readonly Color BorderColor = Color.FromArgb(225, 215, 200);

        private static readonly Color SidebarHover = Color.FromArgb(58, 44, 31);
        private static readonly Color SidebarSelected = Color.FromArgb(80, 59, 40);

        private Panel pnlSidebar = null!;
        private Panel pnlContent = null!;
        private Label lblPageTitle = null!;
        private Label lblWelcome = null!;
        private Label lblDate = null!;

        private BorderedPanel pnlOperations = null!;
        private BorderedPanel pnlActivity = null!;

        private readonly List<BorderedPanel> _summaryCards = new();
        private readonly Dictionary<Panel, (Label title, Label value, Label desc)> _cardLabels = new();
        private readonly string _fullName;

        public ManagerDashboardForm(string fullName)
        {
            _fullName = string.IsNullOrWhiteSpace(fullName) ? "Branch Manager" : fullName;
            BuildDashboard();
        }

        private void BuildDashboard()
        {
            Text = "PBCRM2 - Manager Dashboard";
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
            catch { }

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
            AddSidebarButton("♙", "Tenant Management", y);
            y += 55;
            AddSidebarButton("▣", "Rooms & Beds", y);
            y += 55;
            AddSidebarButton("₱", "Billing & Payments", y);
            y += 55;
            AddSidebarButton("⚒", "Maintenance", y);
            y += 55;
            AddSidebarButton("☷", "Tenant Support", y);
            y += 55;
            AddSidebarButton("★", "Feedback & Satisfaction", y);
            y += 55;
            AddSidebarButton("↻", "Renewal & Retention", y);
            y += 55;
            AddSidebarButton("▤", "Reports", y);
            y += 55;
            AddSidebarButton("⌂", "Branch Management", y);

            BuildUserPanel();
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
                BackColor = selected ? SidebarSelected : Color.Transparent,
                ForeColor = selected ? BrandAccent : Color.FromArgb(215, 205, 192)
            };

            button.FlatAppearance.BorderSize = 0;

            button.MouseEnter += (s, e) =>
            {
                if (!selected)
                {
                    button.BackColor = SidebarHover;
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

            if (text != "Dashboard")
            {
                button.Click += (s, e) =>
                {
                    MessageBox.Show($"{text} module is ready for implementation.", text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
            }

            pnlSidebar.Controls.Add(button);
        }

        private void BuildUserPanel()
        {
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
                Text = "Branch Manager",
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

        private void BuildContent()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentBg,
                AutoScroll = true
            };

            lblPageTitle = new Label
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
                Font = new Font("Segoe UI", 10.5f),
                AutoSize = true,
                Location = new Point(38, 78)
            };

            lblDate = new Label
            {
                Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy"),
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 9),
                AutoSize = true
            };

            pnlContent.Controls.Add(lblPageTitle);
            pnlContent.Controls.Add(lblWelcome);
            pnlContent.Controls.Add(lblDate);

            BuildSummaryCards();
            BuildMainPanels();
        }

        private void BuildSummaryCards()
        {
            var cardData = new[]
            {
                ("TENANTS", "0", "Active tenants"),
                ("ROOMS", "0", "Total rooms"),
                ("OCCUPIED BEDS", "0", "Currently occupied"),
                ("PAYMENTS", "₱0.00", "This month")
            };

            foreach (var (title, value, desc) in cardData)
            {
                var card = CreateSummaryCard(title, value, desc);
                _summaryCards.Add(card);
                pnlContent.Controls.Add(card);
            }
        }

        private BorderedPanel CreateSummaryCard(string title, string value, string description)
        {
            var card = new BorderedPanel
            {
                Size = new Size(200, 120),
                BackColor = CardBg,
                BorderColor = BorderColor
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

        private void BuildMainPanels()
        {
            BuildOperationsPanel();
            BuildActivityPanel();
        }

        private void BuildOperationsPanel()
        {
            pnlOperations = new BorderedPanel
            {
                Size = new Size(535, 345),
                BackColor = Color.White,
                BorderColor = BorderColor
            };

            var lblTitle = new Label
            {
                Text = "Branch Operations",
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 20)
            };

            var lblSubtitle = new Label
            {
                Text = "Monitor and manage daily boarding house activities.",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 9),
                AutoSize = false,
                Size = new Size(470, 30),
                Location = new Point(25, 52)
            };

            pnlOperations.Controls.Add(lblTitle);
            pnlOperations.Controls.Add(lblSubtitle);

            AddOperationRow(pnlOperations, "TENANTS", "Manage registrations and tenant records.", "OPEN", 95);
            AddOperationRow(pnlOperations, "ROOMS & BEDS", "Monitor room and bed occupancy.", "OPEN", 145);
            AddOperationRow(pnlOperations, "PAYMENTS", "Review billing and payment activity.", "OPEN", 195);
            AddOperationRow(pnlOperations, "MAINTENANCE", "Review tenant maintenance requests.", "OPEN", 245);
            AddOperationRow(pnlOperations, "SUPPORT", "Handle tenant concerns and communication.", "OPEN", 295);

            pnlContent.Controls.Add(pnlOperations);
        }

        private void AddOperationRow(Panel parent, string title, string description, string buttonText, int y)
        {
            var lblTitle = new Label
            {
                Text = title,
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(125, 20),
                Location = new Point(25, y)
            };

            var lblDescription = new Label
            {
                Text = description,
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8),
                AutoSize = false,
                Size = new Size(270, 30),
                Location = new Point(145, y - 2)
            };

            var btnOpen = new Button
            {
                Text = buttonText,
                Size = new Size(65, 27),
                Location = new Point(440, y - 4),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = CtaColor,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnOpen.FlatAppearance.BorderColor = CtaColor;
            btnOpen.FlatAppearance.BorderSize = 1;

            btnOpen.Click += (s, e) =>
            {
                MessageBox.Show($"{title} module is ready for implementation.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            parent.Controls.Add(lblTitle);
            parent.Controls.Add(lblDescription);
            parent.Controls.Add(btnOpen);
        }

        private void BuildActivityPanel()
        {
            pnlActivity = new BorderedPanel
            {
                Size = new Size(535, 345),
                BackColor = Color.White,
                BorderColor = BorderColor
            };

            var lblTitle = new Label
            {
                Text = "Manager Overview",
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 20)
            };

            var lblSubtitle = new Label
            {
                Text = "Key areas requiring attention.",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Location = new Point(25, 52)
            };

            pnlActivity.Controls.Add(lblTitle);
            pnlActivity.Controls.Add(lblSubtitle);

            AddStatusItem(pnlActivity, "Pending Registrations", "0", "Review new tenant applications.", 95);
            AddStatusItem(pnlActivity, "Maintenance Requests", "0", "Requests awaiting action.", 145);
            AddStatusItem(pnlActivity, "Pending Payments", "0", "Payments requiring attention.", 195);
            AddStatusItem(pnlActivity, "Renewals", "0", "Tenants approaching renewal.", 245);
            AddStatusItem(pnlActivity, "Feedback", "0", "Recent tenant feedback.", 295);

            pnlContent.Controls.Add(pnlActivity);
        }

        private void AddStatusItem(Panel parent, string title, string value, string description, int y)
        {
            var lblValue = new Label
            {
                Text = value,
                ForeColor = CtaColor,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(45, 35),
                Location = new Point(25, y)
            };

            var lblTitle = new Label
            {
                Text = title,
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(200, 20),
                Location = new Point(85, y)
            };

            var lblDescription = new Label
            {
                Text = description,
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8),
                AutoSize = false,
                Size = new Size(390, 20),
                Location = new Point(85, y + 19)
            };

            parent.Controls.Add(lblValue);
            parent.Controls.Add(lblTitle);
            parent.Controls.Add(lblDescription);
        }

        private void PerformResponsiveLayout()
        {
            if (pnlContent == null || pnlContent.Width <= 0) return;

            pnlContent.SuspendLayout();

            int sidePadding = 35;
            int gap = 20;
            int availableWidth = pnlContent.ClientSize.Width - (sidePadding * 2);

            if (lblDate != null)
            {
                lblDate.Location = new Point(pnlContent.ClientSize.Width - lblDate.Width - sidePadding, 82);
            }

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

            int panelY = 285;
            int panelWidth = Math.Max(450, (availableWidth - gap) / 2);

            pnlOperations.Location = new Point(sidePadding, panelY);
            pnlOperations.Size = new Size(panelWidth, 345);
            pnlOperations.Invalidate();

            pnlActivity.Location = new Point(sidePadding + panelWidth + gap, panelY);
            pnlActivity.Size = new Size(panelWidth, 345);
            pnlActivity.Invalidate();

            pnlContent.ResumeLayout();
        }
    }
}
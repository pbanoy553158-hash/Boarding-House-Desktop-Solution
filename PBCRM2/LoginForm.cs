using PBCRM2.WinForms.Services;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace PBCRM2
{
    public partial class LoginForm : Form
    {
        private static readonly Color BrandBg = Color.FromArgb(32, 24, 18);
        private static readonly Color BrandBgTop = Color.FromArgb(72, 51, 32);
        private static readonly Color BrandAccent = Color.FromArgb(224, 194, 140);
        private static readonly Color BrandAccentDim = Color.FromArgb(170, 145, 105);
        private static readonly Color CtaColor = Color.FromArgb(170, 130, 80);
        private static readonly Color CtaColorDisabled = Color.FromArgb(200, 184, 163);
        private static readonly Color CtaColorHover = Color.FromArgb(186, 148, 100);
        private static readonly Color FieldBorderFocus = Color.FromArgb(170, 130, 80);
        private static readonly Color PanelBg = Color.FromArgb(250, 247, 242);
        private static readonly Color PanelBgWarm = Color.FromArgb(245, 237, 225);
        private static readonly Color TextDark = Color.FromArgb(55, 39, 20);
        private static readonly Color TextMuted = Color.FromArgb(120, 120, 120);
        private static readonly Color FieldLabel = Color.FromArgb(60, 60, 60);
        private static readonly Color FieldBorder = Color.FromArgb(225, 215, 200);

        private const int BrandPanelWidth = 420;
        private const int FormWidth = 1000;
        private const int FormHeight = 620;
        private const int RightContentLeft = 60;
        private const int CardWidth = 460;
        private const int FieldHeight = 48;

        private readonly ApiService _apiService;
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Label lblEyeToggle = null!;
        private Label lblMessage = null!;

        public LoginForm()
        {
            InitializeComponent();
            _apiService = new ApiService();
            BuildLoginForm();
        }

        private void BuildLoginForm()
        {
            Text = "PBCRM2 - Login";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(FormWidth, FormHeight);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            BackColor = PanelBg;
            AutoScaleMode = AutoScaleMode.Dpi;

            Controls.Add(BuildBrandPanel());
            Controls.Add(BuildLoginPanel());

            var divider = new Panel
            {
                Location = new Point(BrandPanelWidth - 2, 0),
                Size = new Size(3, FormHeight),
                BackColor = BrandAccentDim
            };

            Controls.Add(divider);
            divider.BringToFront();
            AcceptButton = btnLogin;
        }

        private sealed class GradientPanel : Panel
        {
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Color TopColor { get; set; } = Color.Black;

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Color BottomColor { get; set; } = Color.Black;

            public GradientPanel()
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                using var brush = new LinearGradientBrush(ClientRectangle, TopColor, BottomColor, LinearGradientMode.ForwardDiagonal);
                e.Graphics.FillRectangle(brush, ClientRectangle);
                base.OnPaint(e);
            }
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private sealed class RoundedPanel : Panel
        {
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int CornerRadius { get; set; } = 12;

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Color BorderColor { get; set; } = Color.LightGray;

            public RoundedPanel()
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                var rect = new Rectangle(0, 0, Width - 1, Height - 1);

                using var path = CreateRoundedPath(rect, CornerRadius);
                using var fill = new SolidBrush(BackColor);
                using var pen = new Pen(BorderColor, 1.4f);

                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(pen, path);

                base.OnPaint(e);
            }
        }

        private Panel BuildBrandPanel()
        {
            var panel = new GradientPanel
            {
                Location = new Point(0, 0),
                Size = new Size(BrandPanelWidth, FormHeight),
                BackColor = BrandBg,
                TopColor = BrandBgTop,
                BottomColor = BrandBg
            };

            var logoSize = new Size(180, 180);
            var logoLocation = new Point((BrandPanelWidth - logoSize.Width) / 2, 40);

            var logo = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = logoSize,
                Location = logoLocation,
                BackColor = Color.Transparent
            };

            var fallback = new Label
            {
                Text = "PB",
                ForeColor = BrandAccent,
                Font = new Font("Segoe UI", 44, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = logoSize,
                Location = logoLocation,
                BackColor = Color.Transparent,
                Visible = false
            };

            var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "logo.png");

            if (File.Exists(logoPath))
            {
                try
                {
                    using var image = Image.FromFile(logoPath);
                    logo.Image = new Bitmap(image);
                }
                catch
                {
                    logo.Visible = false;
                    fallback.Visible = true;
                }
            }
            else
            {
                logo.Visible = false;
                fallback.Visible = true;
            }

            var title = new Label
            {
                Text = "PERCY'S BOARDING HOUSE",
                ForeColor = BrandAccent,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(BrandPanelWidth, 24),
                Location = new Point(0, 235),
                BackColor = Color.Transparent
            };

            var rule = new Panel
            {
                Size = new Size(60, 2),
                Location = new Point((BrandPanelWidth - 60) / 2, 262),
                BackColor = BrandAccent
            };

            var description = new Label
            {
                Text = "A centralized system for managing tenants, rooms, payments, maintenance, feedback, and boarding house operations.",
                ForeColor = Color.FromArgb(220, 215, 205),
                Font = new Font("Segoe UI", 10f),
                TextAlign = ContentAlignment.TopCenter,
                AutoSize = false,
                Size = new Size(340, 100),
                Location = new Point((BrandPanelWidth - 340) / 2, 280),
                BackColor = Color.Transparent
            };

            panel.Controls.Add(logo);
            panel.Controls.Add(fallback);
            panel.Controls.Add(title);
            panel.Controls.Add(rule);
            panel.Controls.Add(description);

            AddBrandBadges(panel);

            return panel;
        }

        private void AddBrandBadges(Panel parent)
        {
            var badges = new (string Icon, string Label)[]
            {
                ("\U0001F6E1", "SECURE"),
                ("\u2601", "CENTRALIZED"),
                ("\u26A1", "EFFICIENT")
            };

            const int slotWidth = 140;
            const int rowY = 520;

            for (int i = 0; i < badges.Length; i++)
            {
                var badge = badges[i];
                var x = i * slotWidth;

                var icon = new Label
                {
                    Text = badge.Icon,
                    Font = new Font("Segoe UI Emoji", 14),
                    ForeColor = BrandAccent,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(slotWidth, 26),
                    Location = new Point(x, rowY),
                    BackColor = Color.Transparent
                };

                var caption = new Label
                {
                    Text = badge.Label,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    ForeColor = BrandAccentDim,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(slotWidth, 18),
                    Location = new Point(x, rowY + 28),
                    BackColor = Color.Transparent
                };

                parent.Controls.Add(icon);
                parent.Controls.Add(caption);

                if (i < badges.Length - 1)
                {
                    parent.Controls.Add(new Panel
                    {
                        Size = new Size(1, 40),
                        Location = new Point(x + slotWidth, rowY + 3),
                        BackColor = Color.FromArgb(90, 70, 50)
                    });
                }
            }
        }

        private Panel BuildLoginPanel()
        {
            var panel = new GradientPanel
            {
                Location = new Point(BrandPanelWidth, 0),
                Size = new Size(FormWidth - BrandPanelWidth, FormHeight),
                BackColor = PanelBg,
                TopColor = PanelBg,
                BottomColor = PanelBgWarm
            };

            var rightWidth = FormWidth - BrandPanelWidth;

            panel.Controls.Add(new Label
            {
                Text = "Welcome Back",
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(RightContentLeft - 3, 60),
                BackColor = Color.Transparent
            });

            panel.Controls.Add(new Label
            {
                Text = "Sign in to access your CRM dashboard",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Location = new Point(RightContentLeft, 108),
                BackColor = Color.Transparent
            });

            var card = BuildLoginCard();
            card.Location = new Point(RightContentLeft, 170);
            panel.Controls.Add(card);

            lblMessage = new Label
            {
                Text = "",
                Location = new Point(RightContentLeft, 480),
                Size = new Size(CardWidth, 30),
                ForeColor = Color.Firebrick,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            panel.Controls.Add(lblMessage);

            panel.Controls.Add(new Label
            {
                Text = "PBCRM2  •  Boarding House CRM System",
                ForeColor = Color.DarkGray,
                Font = new Font("Segoe UI", 8),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(rightWidth, 20),
                Location = new Point(0, 545),
                BackColor = Color.Transparent
            });

            return panel;
        }

        private RoundedPanel BuildLoginCard()
        {
            const int padding = 28;
            const int contentWidth = CardWidth - padding * 2;

            var card = new RoundedPanel
            {
                Size = new Size(CardWidth, 300),
                BackColor = Color.White,
                BorderColor = FieldBorder,
                CornerRadius = 18
            };

            card.Controls.Add(new Label
            {
                Text = "USERNAME",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(padding, 24)
            });

            var usernameField = BuildFieldContainer(contentWidth, out txtUsername, "\U0001F464");
            usernameField.Location = new Point(padding, 46);
            card.Controls.Add(usernameField);

            card.Controls.Add(new Label
            {
                Text = "PASSWORD",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(padding, 120)
            });

            var passwordField = BuildPasswordFieldContainer(contentWidth);
            passwordField.Location = new Point(padding, 142);
            card.Controls.Add(passwordField);

            btnLogin = new Button
            {
                Text = "SIGN IN",
                Location = new Point(padding, 220),
                Size = new Size(contentWidth, 50),
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                BackColor = CtaColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Region = new Region(CreateRoundedPath(new Rectangle(0, 0, btnLogin.Width, btnLogin.Height), 10));

            btnLogin.MouseEnter += (s, e) =>
            {
                if (btnLogin.Enabled)
                    btnLogin.BackColor = CtaColorHover;
            };

            btnLogin.MouseLeave += (s, e) =>
            {
                if (btnLogin.Enabled)
                    btnLogin.BackColor = CtaColor;
            };

            btnLogin.Click += BtnLogin_Click;
            card.Controls.Add(btnLogin);

            return card;
        }

        private RoundedPanel BuildFieldContainer(int width, out TextBox textBox, string iconGlyph)
        {
            var container = new RoundedPanel
            {
                Size = new Size(width, FieldHeight),
                BackColor = Color.White,
                BorderColor = FieldBorder,
                CornerRadius = 12
            };

            var icon = new Label
            {
                Text = iconGlyph,
                Font = new Font("Segoe UI Emoji", 12),
                ForeColor = FieldLabel,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(28, 28),
                Location = new Point(10, (FieldHeight - 28) / 2)
            };

            var field = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11),
                Location = new Point(46, (FieldHeight - 24) / 2),
                Size = new Size(width - 60, 24)
            };

            field.Enter += (s, e) =>
            {
                container.BorderColor = FieldBorderFocus;
                icon.ForeColor = CtaColor;
                container.Invalidate();
            };

            field.Leave += (s, e) =>
            {
                container.BorderColor = FieldBorder;
                icon.ForeColor = FieldLabel;
                container.Invalidate();
            };

            container.Controls.Add(icon);
            container.Controls.Add(field);
            container.Click += (s, e) => field.Focus();

            textBox = field;
            return container;
        }

        private RoundedPanel BuildPasswordFieldContainer(int width)
        {
            var container = new RoundedPanel
            {
                Size = new Size(width, FieldHeight),
                BackColor = Color.White,
                BorderColor = FieldBorder,
                CornerRadius = 12
            };

            var icon = new Label
            {
                Text = "\U0001F512",
                Font = new Font("Segoe UI Emoji", 12),
                ForeColor = FieldLabel,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(28, 28),
                Location = new Point(10, (FieldHeight - 28) / 2)
            };

            txtPassword = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true,
                Location = new Point(46, (FieldHeight - 24) / 2),
                Size = new Size(width - 90, 24)
            };

            lblEyeToggle = new Label
            {
                Text = "\U0001F441",
                Font = new Font("Segoe UI Emoji", 12),
                ForeColor = FieldLabel,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(28, 28),
                Location = new Point(width - 38, (FieldHeight - 28) / 2),
                Cursor = Cursors.Hand
            };

            lblEyeToggle.Click += LblEyeToggle_Click;

            txtPassword.Enter += (s, e) =>
            {
                container.BorderColor = FieldBorderFocus;
                icon.ForeColor = CtaColor;
                container.Invalidate();
            };

            txtPassword.Leave += (s, e) =>
            {
                container.BorderColor = FieldBorder;
                icon.ForeColor = FieldLabel;
                container.Invalidate();
            };

            container.Controls.Add(icon);
            container.Controls.Add(txtPassword);
            container.Controls.Add(lblEyeToggle);
            container.Click += (s, e) => txtPassword.Focus();

            return container;
        }

        private void LblEyeToggle_Click(object? sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
            lblEyeToggle.ForeColor = txtPassword.UseSystemPasswordChar ? FieldLabel : CtaColor;
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblMessage.ForeColor = Color.Firebrick;
                lblMessage.Text = "Please enter your username and password.";
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.BackColor = CtaColorDisabled;
            btnLogin.Text = "SIGNING IN...";
            lblMessage.ForeColor = Color.DimGray;
            lblMessage.Text = "Connecting to the server...";

            try
            {
                var result = await _apiService.LoginAsync(username, password);

                if (result == null)
                {
                    lblMessage.ForeColor = Color.Firebrick;
                    lblMessage.Text = "Invalid username or password.";
                    return;
                }

                _apiService.SetToken(result.Token);

                var role = result.Roles?.FirstOrDefault()?.Trim();

                if (string.IsNullOrWhiteSpace(role))
                {
                    MessageBox.Show(
                        "Your account does not have a system role assigned.",
                        "Access Denied",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var fullName = string.IsNullOrWhiteSpace(result.FullName)
                    ? username
                    : result.FullName;

                Form? dashboard = role.ToLowerInvariant() switch
                {
                    "superadmin" => new SuperAdminDashboardForm(fullName),
                    "admin" => new AdminDashboardForm(fullName),
                    "manager" => new ManagerDashboardForm(fullName),
                    "staff" => new StaffDashboardForm(fullName),
                    _ => null
                };

                if (dashboard == null)
                {
                    MessageBox.Show(
                        $"The role '{role}' is not recognized by PBCRM2.",
                        "Access Denied",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                Hide();

                dashboard.FormClosed += (s, args) => Close();
                dashboard.Show();
            }
            catch (Exception ex)
            {
                lblMessage.ForeColor = Color.Firebrick;
                lblMessage.Text = "Unable to connect to the API.";

                MessageBox.Show(
                    ex.Message,
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.BackColor = CtaColor;
                btnLogin.Text = "SIGN IN    \u2192";
            }
        }
    }
}
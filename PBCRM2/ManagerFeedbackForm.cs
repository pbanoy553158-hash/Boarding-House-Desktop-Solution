using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PBCRM2
{
    public class ManagerFeedbackForm : Form
    {
        private readonly ApiService _apiService;

        private DataGridView dgvFeedback = null!;
        private Button btnView = null!;
        private Button btnRefresh = null!;

        private ComboBox cmbTenant = null!;
        private Button btnSendFeedback = null!;
        private Label lblSendStatus = null!;
        private Label lblSelectedTenant = null!;

        private Label lblCount = null!;

        // =========================================================
        // COLORS
        // =========================================================

        private static readonly Color BrandBg =
            Color.FromArgb(32, 24, 18);

        private static readonly Color BrandBgTop =
            Color.FromArgb(72, 51, 32);

        private static readonly Color BrandAccent =
            Color.FromArgb(224, 194, 140);

        private static readonly Color CtaColor =
            Color.FromArgb(170, 130, 80);

        private static readonly Color PanelBg =
            Color.FromArgb(250, 247, 242);

        private static readonly Color BorderColor =
            Color.FromArgb(225, 215, 200);

        private static readonly Color TextDark =
            Color.FromArgb(55, 48, 42);

        private static readonly Color TextMuted =
            Color.FromArgb(120, 110, 100);

        private static readonly Color SoftCardBg =
            Color.FromArgb(252, 250, 247);

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ManagerFeedbackForm(
            ApiService apiService)
        {
            _apiService = apiService;

            Text = "Feedback & Satisfaction";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Dock = DockStyle.Fill;
            BackColor = PanelBg;
            AutoScroll = false;

            BuildInterface();

            Shown += async (_, _) =>
            {
                await LoadFeedbackAsync();
                await LoadTenantsAsync();
            };
        }

        // =========================================================
        // BUILD INTERFACE
        // =========================================================

        private void BuildInterface()
        {
            var main = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg
            };

            // =====================================================
            // HEADER
            // =====================================================

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 112,
                BackColor = BrandBg
            };

            var title = new Label
            {
                Text = "FEEDBACK & SATISFACTION",
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    21F,
                    FontStyle.Bold),
                AutoSize = true,
                Location = new Point(28, 20)
            };

            var subtitle = new Label
            {
                Text =
                    "Review tenant feedback and send feedback requests.",
                ForeColor = BrandAccent,
                Font = new Font(
                    "Segoe UI",
                    9.5F),
                AutoSize = true,
                Location = new Point(31, 62)
            };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            // =====================================================
            // HEADER GAP
            // =====================================================

            var headerGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 18,
                BackColor = PanelBg
            };

            // =====================================================
            // SEND FEEDBACK CARD
            // =====================================================

            var sendCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 176,
                BackColor = Color.White,
                Padding = new Padding(
                    22,
                    15,
                    22,
                    15)
            };

            sendCard.Paint += (_, e) =>
            {
                using var pen =
                    new Pen(BorderColor);

                e.Graphics.DrawRectangle(
                    pen,
                    0,
                    0,
                    sendCard.Width - 1,
                    sendCard.Height - 1);
            };

            // -----------------------------------------------------
            // CARD TITLE
            // -----------------------------------------------------

            var lblSendTitle = new Label
            {
                Text = "SEND FEEDBACK REQUEST",
                Font = new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold),
                ForeColor = BrandBg,
                AutoSize = true,
                Location = new Point(22, 12)
            };

            // -----------------------------------------------------
            // CARD DESCRIPTION
            // -----------------------------------------------------

            var lblSendDescription = new Label
            {
                Text =
                    "Choose a tenant below to send a feedback link and QR code by email.",
                Font = new Font(
                    "Segoe UI",
                    8.5F),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(22, 37)
            };

            // -----------------------------------------------------
            // TENANT LABEL
            // -----------------------------------------------------

            var lblTenant = new Label
            {
                Text = "Select Tenant",
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(22, 67)
            };

            // -----------------------------------------------------
            // TENANT COMBOBOX
            // -----------------------------------------------------

            cmbTenant = new ComboBox
            {
                DropDownStyle =
                    ComboBoxStyle.DropDownList,

                Font = new Font(
                    "Segoe UI",
                    10.5F),

                Location =
                    new Point(22, 88),

                Height = 38,

                Width = 520,

                BackColor = Color.White,

                ForeColor = TextDark,

                IntegralHeight = false,

                DropDownHeight = 320,

                DropDownWidth = 520,

                FlatStyle = FlatStyle.Flat
            };

            cmbTenant.SelectedIndexChanged +=
                (_, _) =>
                {
                    UpdateSelectedTenant();
                };

            // -----------------------------------------------------
            // SEND BUTTON
            // -----------------------------------------------------

            btnSendFeedback =
                CreateButton(
                    "Send Feedback",
                    145);

            btnSendFeedback.Height = 38;

            btnSendFeedback.Location =
                new Point(
                    560,
                    88);

            btnSendFeedback.Click += async (_, _) =>
            {
                await SendFeedbackRequestAsync();
            };

            // -----------------------------------------------------
            // SELECTED TENANT INFO CARD
            // -----------------------------------------------------

            var selectedTenantPanel = new Panel
            {
                BackColor = SoftCardBg,
                Location = new Point(
                    725,
                    67),
                Width = 390,
                Height = 59
            };

            selectedTenantPanel.Paint += (_, e) =>
            {
                using var pen =
                    new Pen(BorderColor);

                e.Graphics.DrawRectangle(
                    pen,
                    0,
                    0,
                    selectedTenantPanel.Width - 1,
                    selectedTenantPanel.Height - 1);
            };

            var lblSelectedTitle = new Label
            {
                Text = "SELECTED TENANT",
                Font = new Font(
                    "Segoe UI",
                    7.5F,
                    FontStyle.Bold),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(
                    12,
                    7)
            };

            lblSelectedTenant = new Label
            {
                Text = "No tenant selected",
                Font = new Font(
                    "Segoe UI",
                    8.5F),
                ForeColor = TextDark,
                AutoSize = false,
                Location = new Point(
                    12,
                    25),
                Width = 365,
                Height = 25,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            selectedTenantPanel.Controls.Add(
                lblSelectedTitle);

            selectedTenantPanel.Controls.Add(
                lblSelectedTenant);

            // -----------------------------------------------------
            // STATUS
            // -----------------------------------------------------

            lblSendStatus = new Label
            {
                Text =
                    "Select a tenant to send a feedback request.",
                Font = new Font(
                    "Segoe UI",
                    8F),
                ForeColor = TextMuted,
                AutoSize = false,
                Location = new Point(
                    22,
                    137),
                Width = 1090,
                Height = 24,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            sendCard.Controls.Add(
                lblSendTitle);

            sendCard.Controls.Add(
                lblSendDescription);

            sendCard.Controls.Add(
                lblTenant);

            sendCard.Controls.Add(
                cmbTenant);

            sendCard.Controls.Add(
                btnSendFeedback);

            sendCard.Controls.Add(
                selectedTenantPanel);

            sendCard.Controls.Add(
                lblSendStatus);

            // =====================================================
            // SPACE AFTER SEND CARD
            // =====================================================

            var cardGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 16,
                BackColor = PanelBg
            };

            // =====================================================
            // ACTION BAR
            // =====================================================

            var actionPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = PanelBg
            };

            btnView =
                CreateButton(
                    "View Feedback",
                    135);

            btnView.Location =
                new Point(20, 7);

            btnView.Enabled = false;

            btnView.Click += (_, _) =>
            {
                ShowSelectedFeedback();
            };

            btnRefresh =
                CreateButton(
                    "Refresh",
                    95);

            btnRefresh.Location =
                new Point(
                    165,
                    7);

            btnRefresh.Click += async (_, _) =>
            {
                await LoadFeedbackAsync();
                await LoadTenantsAsync();
            };

            actionPanel.Controls.Add(
                btnView);

            actionPanel.Controls.Add(
                btnRefresh);

            // =====================================================
            // TABLE SECTION
            // =====================================================

            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg,
                Padding = new Padding(
                    20,
                    0,
                    20,
                    20)
            };

            // -----------------------------------------------------
            // TABLE HEADER
            // -----------------------------------------------------

            var feedbackLabelPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = PanelBg
            };

            var lblSection = new Label
            {
                Text = "TENANT FEEDBACK",
                Dock = DockStyle.Left,
                Width = 220,
                Font = new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold),
                ForeColor = BrandBg,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            lblCount = new Label
            {
                Text = "0 feedback",
                Dock = DockStyle.Right,
                Width = 160,
                Font = new Font(
                    "Segoe UI",
                    8.5F),
                ForeColor = TextMuted,
                TextAlign =
                    ContentAlignment.MiddleRight
            };

            feedbackLabelPanel.Controls.Add(
                lblCount);

            feedbackLabelPanel.Controls.Add(
                lblSection);

            // -----------------------------------------------------
            // GRID
            // -----------------------------------------------------

            dgvFeedback =
                CreateGrid();

            dgvFeedback.Dock =
                DockStyle.Fill;

            dgvFeedback.SelectionChanged +=
                DgvFeedback_SelectionChanged;

            dgvFeedback.CellDoubleClick +=
                DgvFeedback_CellDoubleClick;

            content.Controls.Add(
                dgvFeedback);

            content.Controls.Add(
                feedbackLabelPanel);

            // =====================================================
            // MAIN CONTROLS
            // =====================================================

            main.Controls.Add(
                content);

            main.Controls.Add(
                actionPanel);

            main.Controls.Add(
                cardGap);

            main.Controls.Add(
                sendCard);

            main.Controls.Add(
                headerGap);

            main.Controls.Add(
                header);

            Controls.Add(main);
        }

        // =========================================================
        // LOAD FEEDBACK
        // =========================================================

        private async Task LoadFeedbackAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                btnView.Enabled = false;

                var feedback =
                    await _apiService
                        .GetAsync<List<FeedbackDto>>(
                            "api/Feedback");

                dgvFeedback.DataSource = null;

                if (feedback == null)
                {
                    lblCount.Text =
                        "0 feedback";

                    return;
                }

                dgvFeedback.DataSource =
                    feedback;

                lblCount.Text =
                    feedback.Count == 1
                        ? "1 feedback"
                        : $"{feedback.Count} feedback";

                ConfigureColumns();

                if (dgvFeedback.Rows.Count > 0)
                {
                    dgvFeedback.ClearSelection();
                    btnView.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load feedback.\n\n" +
                    $"Error:\n{ex.Message}\n\n" +
                    "Endpoint:\n" +
                    "https://localhost:7241/api/Feedback",
                    "Feedback",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        // =========================================================
        // LOAD TENANTS
        // =========================================================

        private async Task LoadTenantsAsync()
        {
            try
            {
                cmbTenant.Enabled = false;
                btnSendFeedback.Enabled = false;

                lblSelectedTenant.Text =
                    "No tenant selected";

                var tenants =
                    await _apiService
                        .GetAsync<List<TenantFeedbackDto>>(
                            "api/Tenants");

                cmbTenant.DataSource = null;

                if (tenants == null ||
                    tenants.Count == 0)
                {
                    lblSendStatus.Text =
                        "No tenants available.";

                    return;
                }

                cmbTenant.DisplayMember =
                    nameof(
                        TenantFeedbackDto.DisplayName);

                cmbTenant.ValueMember =
                    nameof(
                        TenantFeedbackDto.Id);

                cmbTenant.DataSource =
                    tenants;

                cmbTenant.SelectedIndex = -1;

                lblSendStatus.Text =
                    "Select a tenant to send a feedback request.";

                cmbTenant.Enabled = true;

                // Keep disabled until a tenant is selected.
                btnSendFeedback.Enabled = false;
            }
            catch (Exception ex)
            {
                lblSendStatus.Text =
                    "Unable to load tenants.";

                MessageBox.Show(
                    "Unable to load tenants.\n\n" +
                    $"Error:\n{ex.Message}\n\n" +
                    "Endpoint:\n" +
                    "https://localhost:7241/api/Tenants",
                    "Tenants",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // SELECTED TENANT
        // =========================================================

        private void UpdateSelectedTenant()
        {
            if (cmbTenant.SelectedItem
                is not TenantFeedbackDto tenant)
            {
                lblSelectedTenant.Text =
                    "No tenant selected";

                btnSendFeedback.Enabled = false;

                return;
            }

            if (string.IsNullOrWhiteSpace(
                tenant.Email))
            {
                lblSelectedTenant.Text =
                    $"{tenant.FullName}  •  No email";

                btnSendFeedback.Enabled = false;

                lblSendStatus.Text =
                    "This tenant does not have an email address.";

                return;
            }

            lblSelectedTenant.Text =
                $"{tenant.FullName}  •  {tenant.Email}";

            btnSendFeedback.Enabled = true;

            lblSendStatus.Text =
                "Ready to send the feedback request.";
        }

        // =========================================================
        // SEND FEEDBACK REQUEST
        // =========================================================

        private async Task SendFeedbackRequestAsync()
        {
            if (cmbTenant.SelectedItem
                is not TenantFeedbackDto tenant)
            {
                MessageBox.Show(
                    "Please select a tenant first.",
                    "Send Feedback",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (tenant.Id <= 0)
            {
                MessageBox.Show(
                    "The selected tenant has an invalid ID.",
                    "Send Feedback",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(
                tenant.Email))
            {
                MessageBox.Show(
                    "The selected tenant does not have an email address.\n\n" +
                    "Please update the tenant's email address first.",
                    "Send Feedback",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var confirm =
                MessageBox.Show(
                    $"Send a feedback request to:\n\n" +
                    $"{tenant.FullName}\n" +
                    $"{tenant.Email}\n\n" +
                    "The tenant will receive the feedback link " +
                    "and QR code by email.",
                    "Send Feedback Request",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                btnSendFeedback.Enabled = false;
                cmbTenant.Enabled = false;

                lblSendStatus.Text =
                    "Sending feedback request...";

                var result =
                    await _apiService
                        .PostAsync<FeedbackRequestResponse>(
                            $"api/FeedbackRequest/send/{tenant.Id}",
                            null,
                            false);

                // =================================================
                // FAILED
                // =================================================

                if (result == null)
                {
                    string errorMessage =
                        _apiService.LastErrorMessage;

                    if (string.IsNullOrWhiteSpace(
                        errorMessage))
                    {
                        errorMessage =
                            "The API did not provide an error message.";
                    }

                    lblSendStatus.Text =
                        "Failed to send feedback request.";

                    MessageBox.Show(
                        "The feedback request was not sent.\n\n" +
                        $"Reason:\n{errorMessage}",
                        "Feedback Request Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                // =================================================
                // SUCCESS
                // =================================================

                lblSendStatus.Text =
                    "Feedback request sent successfully.";

                string feedbackUrl =
                    result.FeedbackUrl;

                string expiresAt =
                    result.ExpiresAt
                        .ToLocalTime()
                        .ToString(
                            "MMM dd, yyyy hh:mm tt");

                MessageBox.Show(
                    "Feedback request sent successfully!\n\n" +
                    $"Tenant:\n" +
                    $"{result.TenantName}\n\n" +
                    $"Email:\n" +
                    $"{result.Email}\n\n" +
                    "The tenant has been sent:\n" +
                    "• Feedback link\n" +
                    "• QR code\n\n" +
                    $"Expires:\n" +
                    $"{expiresAt}",
                    "Feedback Request Sent",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // =================================================
                // OPEN LINK
                // =================================================

                if (!string.IsNullOrWhiteSpace(
                    feedbackUrl))
                {
                    var open =
                        MessageBox.Show(
                            "Would you like to open the feedback link now?",
                            "Feedback Link",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                    if (open == DialogResult.Yes)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(
                                new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName =
                                        feedbackUrl,
                                    UseShellExecute =
                                        true
                                });
                        }
                        catch
                        {
                            Clipboard.SetText(
                                feedbackUrl);

                            MessageBox.Show(
                                "The feedback link could not be opened automatically.\n\n" +
                                "The link has been copied to your clipboard.",
                                "Feedback Link",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                }

                await LoadFeedbackAsync();
                await LoadTenantsAsync();
            }
            catch (Exception ex)
            {
                lblSendStatus.Text =
                    "Failed to send feedback request.";

                MessageBox.Show(
                    "An unexpected error occurred while sending the feedback request.\n\n" +
                    $"Error:\n{ex.Message}",
                    "Feedback Request Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                cmbTenant.Enabled = true;

                // Only enable when a valid tenant is selected.
                UpdateSelectedTenant();
            }
        }

        // =========================================================
        // GRID SELECTION
        // =========================================================

        private void DgvFeedback_SelectionChanged(
            object? sender,
            EventArgs e)
        {
            btnView.Enabled =
                dgvFeedback.SelectedRows.Count > 0;
        }

        private void DgvFeedback_CellDoubleClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            ShowSelectedFeedback();
        }

        // =========================================================
        // CONFIGURE COLUMNS
        // =========================================================

        private void ConfigureColumns()
        {
            if (dgvFeedback.Columns.Count == 0)
            {
                return;
            }

            HideColumn("Id");
            HideColumn("TenantId");
            HideColumn("TenantEmail");

            RenameColumn(
                "TenantName",
                "Tenant");

            RenameColumn(
                "Rating",
                "Rating");

            RenameColumn(
                "Comment",
                "Feedback");

            RenameColumn(
                "SubmittedAt",
                "Date Submitted");

            if (dgvFeedback.Columns["TenantName"] != null)
            {
                dgvFeedback.Columns[
                    "TenantName"].Width = 190;

                dgvFeedback.Columns[
                    "TenantName"].AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.None;
            }

            if (dgvFeedback.Columns["Rating"] != null)
            {
                dgvFeedback.Columns[
                    "Rating"].Width = 110;

                dgvFeedback.Columns[
                    "Rating"].AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.None;
            }

            if (dgvFeedback.Columns["SubmittedAt"] != null)
            {
                dgvFeedback.Columns[
                    "SubmittedAt"].Width = 150;

                dgvFeedback.Columns[
                    "SubmittedAt"].AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.None;
            }

            if (dgvFeedback.Columns["Comment"] != null)
            {
                dgvFeedback.Columns[
                    "Comment"].AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void HideColumn(string name)
        {
            if (dgvFeedback.Columns.Contains(name))
            {
                dgvFeedback.Columns[name].Visible =
                    false;
            }
        }

        private void RenameColumn(
            string name,
            string header)
        {
            if (dgvFeedback.Columns.Contains(name))
            {
                dgvFeedback.Columns[name].HeaderText =
                    header;
            }
        }

        // =========================================================
        // VIEW SELECTED FEEDBACK
        // =========================================================

        private void ShowSelectedFeedback()
        {
            if (dgvFeedback.SelectedRows.Count == 0)
            {
                return;
            }

            var row =
                dgvFeedback.SelectedRows[0];

            if (row.DataBoundItem
                is not FeedbackDto feedback)
            {
                return;
            }

            string tenant =
                string.IsNullOrWhiteSpace(
                    feedback.TenantName)
                    ? "Unknown Tenant"
                    : feedback.TenantName;

            string email =
                string.IsNullOrWhiteSpace(
                    feedback.TenantEmail)
                    ? "No email provided"
                    : feedback.TenantEmail;

            string comment =
                string.IsNullOrWhiteSpace(
                    feedback.Comment)
                    ? "No comment provided."
                    : feedback.Comment;

            string stars =
                GetStars(
                    feedback.Rating);

            string submitted =
                feedback.SubmittedAt
                    .ToLocalTime()
                    .ToString(
                        "MMM dd, yyyy hh:mm tt");

            MessageBox.Show(
                $"Tenant\n" +
                $"{tenant}\n\n" +

                $"Email\n" +
                $"{email}\n\n" +

                $"Rating\n" +
                $"{stars}  ({feedback.Rating}/5)\n\n" +

                $"Feedback\n" +
                $"{comment}\n\n" +

                $"Date Submitted\n" +
                $"{submitted}",
                "Feedback Details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =========================================================
        // STARS
        // =========================================================

        private string GetStars(
            int rating)
        {
            rating =
                Math.Max(
                    0,
                    Math.Min(
                        5,
                        rating));

            return new string(
                       '★',
                       rating)
                   +
                   new string(
                       '☆',
                       5 - rating);
        }

        // =========================================================
        // GRID
        // =========================================================

        private DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                BackgroundColor =
                    Color.White,

                BorderStyle =
                    BorderStyle.FixedSingle,

                AllowUserToAddRows =
                    false,

                AllowUserToDeleteRows =
                    false,

                AllowUserToResizeRows =
                    false,

                AllowUserToResizeColumns =
                    false,

                ReadOnly =
                    true,

                MultiSelect =
                    false,

                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,

                AutoGenerateColumns =
                    true,

                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,

                RowHeadersVisible =
                    false,

                EnableHeadersVisualStyles =
                    false,

                ColumnHeadersHeight =
                    36,

                ShowCellToolTips =
                    false
            };

            grid.RowTemplate.Height = 32;

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
                    BackColor =
                        BrandBg,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
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
                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F),

                    BackColor =
                        Color.White,

                    ForeColor =
                        TextDark,

                    SelectionBackColor =
                        Color.FromArgb(
                            232,
                            220,
                            199),

                    SelectionForeColor =
                        BrandBg
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

            grid.CellFormatting +=
                (_, e) =>
                {
                    if (e.RowIndex < 0)
                    {
                        return;
                    }

                    if (
                        e.ColumnIndex < 0 ||
                        e.ColumnIndex >=
                        grid.Columns.Count)
                    {
                        return;
                    }

                    string columnName =
                        grid.Columns[
                            e.ColumnIndex].Name;

                    if (
                        columnName == "Rating" &&
                        int.TryParse(
                            e.Value?.ToString(),
                            out int rating))
                    {
                        e.Value =
                            GetStars(rating);

                        e.FormattingApplied =
                            true;
                    }

                    if (
                        columnName == "SubmittedAt" &&
                        DateTime.TryParse(
                            e.Value?.ToString(),
                            out DateTime date))
                    {
                        e.Value =
                            date.ToLocalTime()
                                .ToString(
                                    "MMM dd, yyyy");

                        e.FormattingApplied =
                            true;
                    }

                    if (
                        columnName == "Comment" &&
                        string.IsNullOrWhiteSpace(
                            e.Value?.ToString()))
                    {
                        e.Value =
                            "No comment";

                        e.FormattingApplied =
                            true;
                    }
                };

            return grid;
        }

        // =========================================================
        // BUTTON
        // =========================================================

        private Button CreateButton(
            string text,
            int width)
        {
            var button = new Button
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

                Font =
                    new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold),

                Cursor =
                    Cursors.Hand,

                UseVisualStyleBackColor =
                    false,

                TabStop =
                    false
            };

            button.FlatAppearance.BorderSize =
                0;

            button.MouseEnter +=
                (_, _) =>
                {
                    if (button.Enabled)
                    {
                        button.BackColor =
                            Color.FromArgb(
                                145,
                                105,
                                62);
                    }
                };

            button.MouseLeave +=
                (_, _) =>
                {
                    if (button.Enabled)
                    {
                        button.BackColor =
                            CtaColor;
                    }
                };

            return button;
        }
    }

    // =============================================================
    // FEEDBACK DTO
    // =============================================================

    public class FeedbackDto
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public string TenantName { get; set; } =
            "";

        public string TenantEmail { get; set; } =
            "";

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime SubmittedAt { get; set; }
    }

    // =============================================================
    // TENANT DTO
    // =============================================================

    public class TenantFeedbackDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } =
            "";

        public string Email { get; set; } =
            "";

        public int? BranchId { get; set; }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(
                    Email))
                {
                    return FullName;
                }

                return $"{FullName}  -  {Email}";
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    // =============================================================
    // FEEDBACK REQUEST RESPONSE
    // =============================================================

    public class FeedbackRequestResponse
    {
        public string Message { get; set; } =
            "";

        public int TenantId { get; set; }

        public string TenantName { get; set; } =
            "";

        public string Email { get; set; } =
            "";

        public int CompanyId { get; set; }

        public string FeedbackUrl { get; set; } =
            "";

        public DateTime ExpiresAt { get; set; }
    }
}

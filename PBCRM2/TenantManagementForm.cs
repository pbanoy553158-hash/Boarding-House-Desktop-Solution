using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using PBCRM2.WinForms.Services;

namespace PBCRM2
{
    public class TenantManagementForm : Form
    {
        private readonly ApiService _apiService;

        private DataGridView dgvTenants = null!;
        private Button btnAdd = null!;
        private Label lblPageTitle = null!;
        private Label lblPageSubtitle = null!;
        private Panel toolbar = null!;
        private Panel content = null!;
        private Panel countPanel = null!;
        private TextBox txtSearch = null!;
        private ComboBox cboFilterStatus = null!;
        private Label lblCountValue = null!;

        private List<TenantDto> _tenants = new();
        private List<TenantDto> _filteredTenants = new();

        private int _hoveredRowIndex = -1;
        private int _hoveredActionPart = -1;

        // =========================================================
        // COLORS
        // =========================================================

        private static readonly Color ContentBg =
            Color.FromArgb(247, 244, 239);

        private static readonly Color CardBg =
            Color.White;

        private static readonly Color TextDark =
            Color.FromArgb(32, 24, 18);

        private static readonly Color TextMuted =
            Color.FromArgb(105, 95, 85);

        private static readonly Color BorderColor =
            Color.FromArgb(230, 225, 218);

        private static readonly Color BrandBg =
            Color.FromArgb(32, 24, 18);

        private static readonly Color BrandAccent =
            Color.FromArgb(224, 194, 140);

        private static readonly Color CtaColor =
            Color.FromArgb(170, 130, 80);

        private static readonly Color RecordHoverBg =
            Color.FromArgb(232, 220, 199);

        private static readonly Color ActiveBg =
            Color.FromArgb(232, 248, 240);

        private static readonly Color ActiveText =
            Color.FromArgb(16, 135, 91);

        private static readonly Color MovedBg =
            Color.FromArgb(245, 245, 245);

        private static readonly Color MovedText =
            Color.FromArgb(105, 105, 105);

        private static readonly Color SectionBg =
            Color.FromArgb(247, 244, 239);

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public TenantManagementForm(ApiService apiService)
        {
            _apiService = apiService;

            BuildForm();
        }

        // =========================================================
        // MAIN FORM
        // =========================================================

        private void BuildForm()
        {
            Text = "Tenant Management";
            BackColor = ContentBg;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            Padding = Padding.Empty;

            BuildHeader();
            BuildToolbar();
            BuildContent();

            Panel headerGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = ContentBg
            };

            Panel toolbarGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 15,
                BackColor = ContentBg
            };

            Controls.Add(content);
            Controls.Add(toolbarGap);
            Controls.Add(toolbar);
            Controls.Add(headerGap);
            Controls.Add(CreateHeaderPanel());

            Load += async (s, e) =>
                await LoadTenantsAsync();
        }

        // =========================================================
        // HEADER
        // =========================================================

        private void BuildHeader()
        {
            lblPageTitle = new Label
            {
                Text = "TENANT MANAGEMENT",
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    22F,
                    FontStyle.Bold),
                AutoSize = true
            };

            lblPageSubtitle = new Label
            {
                Text =
                    "Manage tenant registrations, records, and occupancy status.",
                ForeColor = BrandAccent,
                Font = new Font(
                    "Segoe UI",
                    10F),
                AutoSize = true
            };
        }

        private Panel CreateHeaderPanel()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = BrandBg
            };

            lblPageTitle.Location =
                new Point(25, 22);

            lblPageSubtitle.Location =
                new Point(28, 67);

            header.Controls.Add(lblPageTitle);
            header.Controls.Add(lblPageSubtitle);

            return header;
        }

        // =========================================================
        // TOOLBAR
        // =========================================================

        private void BuildToolbar()
        {
            toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = ContentBg
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "Search tenant...",
                Font = new Font(
                    "Segoe UI",
                    9.5F),
                Size = new Size(210, 32)
            };

            txtSearch.TextChanged +=
                (s, e) => ApplyFilterAndSearch();

            cboFilterStatus = new ComboBox
            {
                Size = new Size(140, 32),
                Font = new Font(
                    "Segoe UI",
                    9.5F),
                DropDownStyle =
                    ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };

            cboFilterStatus.Items.AddRange(
                new object[]
                {
                    "All Statuses",
                    "Active",
                    "Moved Out",
                    "Archived"
                });

            cboFilterStatus.SelectedIndex = 0;

            cboFilterStatus.SelectedIndexChanged +=
                (s, e) => ApplyFilterAndSearch();

            btnAdd = new Button
            {
                Text = "Add Tenant",
                Size = new Size(115, 32),
                BackColor = CtaColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            btnAdd.FlatAppearance.BorderSize = 0;

            btnAdd.Click += async (s, e) =>
            {
                using var form =
                    new InlineTenantEditorForm(
                        _apiService);

                if (form.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    await LoadTenantsAsync();
                }
            };

            countPanel = new Panel
            {
                Size = new Size(130, 32),
                BackColor = Color.Transparent
            };

            lblCountValue = new Label
            {
                Text = "0 tenants",
                Font = new Font(
                    "Segoe UI",
                    9F),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(0, 6)
            };

            countPanel.Controls.Add(lblCountValue);

            toolbar.Controls.Add(txtSearch);
            toolbar.Controls.Add(cboFilterStatus);
            toolbar.Controls.Add(btnAdd);
            toolbar.Controls.Add(countPanel);

            PositionToolbarControls();
        }

        private void PositionToolbarControls()
        {
            txtSearch.Location =
                new Point(20, 10);

            cboFilterStatus.Location =
                new Point(245, 10);

            btnAdd.Location =
                new Point(400, 10);

            countPanel.Location =
                new Point(530, 14);
        }

        // =========================================================
        // CONTENT
        // =========================================================

        private void BuildContent()
        {
            content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentBg,
                Padding = new Padding(
                    20,
                    0,
                    20,
                    20)
            };

            Label lblRecords = new Label
            {
                Text = "TENANT RECORDS",
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = TextDark,
                Font = new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold),
                TextAlign =
                    ContentAlignment.MiddleLeft,
                Padding = new Padding(
                    2,
                    0,
                    0,
                    0)
            };

            Panel tableCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg
            };

            BuildGrid(tableCard);

            content.Controls.Add(tableCard);
            content.Controls.Add(lblRecords);
        }

        // =========================================================
        // FILTER
        // =========================================================

        private void ApplyFilterAndSearch()
        {
            if (dgvTenants == null)
                return;

            string keyword =
                txtSearch.Text
                    .Trim()
                    .ToLowerInvariant();

            string selectedStatus =
                cboFilterStatus.SelectedItem?.ToString()
                ?? "All Statuses";

            _filteredTenants =
                _tenants
                    .Where(t =>
                    {
                        bool statusOk =
                            selectedStatus ==
                                "All Statuses"
                            || t.Status.Equals(
                                selectedStatus,
                                StringComparison.OrdinalIgnoreCase);

                        if (!statusOk)
                            return false;

                        if (string.IsNullOrWhiteSpace(keyword))
                            return true;

                        return
                            (t.FullName ?? "")
                                .ToLowerInvariant()
                                .Contains(keyword)

                            || (t.Email ?? "")
                                .ToLowerInvariant()
                                .Contains(keyword)

                            || (t.ContactNumber ?? "")
                                .ToLowerInvariant()
                                .Contains(keyword)

                            || (t.CurrentAddress ?? "")
                                .ToLowerInvariant()
                                .Contains(keyword)

                            || (t.EmergencyContactName ?? "")
                                .ToLowerInvariant()
                                .Contains(keyword)

                            || (t.EmergencyContactNumber ?? "")
                                .ToLowerInvariant()
                                .Contains(keyword);
                    })
                    .ToList();

            dgvTenants.DataSource = null;
            dgvTenants.DataSource = _filteredTenants;

            lblCountValue.Text =
                $"{_filteredTenants.Count} tenant"
                + (_filteredTenants.Count == 1
                    ? ""
                    : "s");
        }

        // =========================================================
        // GRID
        // =========================================================

        private void BuildGrid(Panel parent)
        {
            dgvTenants = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CardBg,
                BorderStyle =
                    BorderStyle.FixedSingle,
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

                // IMPORTANT:
                // Allows email and other long values to use
                // multiple lines when necessary.
                AutoSizeRowsMode =
                    DataGridViewAutoSizeRowsMode.AllCells,

                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,

                ColumnHeadersHeight = 36,
                EnableHeadersVisualStyles = false,
                GridColor = BorderColor,
                CellBorderStyle =
                    DataGridViewCellBorderStyle.SingleHorizontal,
                ScrollBars = ScrollBars.Vertical,
                TabStop = false
            };

            // Slightly taller rows so wrapped email addresses
            // have enough room.
            dgvTenants.RowTemplate.Height = 62;
            dgvTenants.RowTemplate.MinimumHeight = 62;

            typeof(DataGridView)
                .GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(
                    dgvTenants,
                    true,
                    null);

            dgvTenants.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = BrandBg,
                    ForeColor = Color.White,
                    Font = new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold),
                    Alignment =
                        DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(
                        7,
                        2,
                        7,
                        2),
                    SelectionBackColor = BrandBg,
                    SelectionForeColor = Color.White,
                    WrapMode =
                        DataGridViewTriState.False
                };

            dgvTenants.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = TextDark,
                    Font = new Font(
                        "Segoe UI",
                        8.5F),
                    SelectionBackColor =
                        RecordHoverBg,
                    SelectionForeColor =
                        TextDark,
                    Padding = new Padding(6),
                    Alignment =
                        DataGridViewContentAlignment.MiddleLeft,

                    // IMPORTANT:
                    // General cells can wrap.
                    WrapMode =
                        DataGridViewTriState.True
                };

            dgvTenants.AlternatingRowsDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor =
                        Color.FromArgb(
                            247,
                            244,
                            239),
                    ForeColor = TextDark,
                    SelectionBackColor =
                        RecordHoverBg,
                    SelectionForeColor =
                        TextDark,

                    // IMPORTANT:
                    // Alternating rows can also wrap.
                    WrapMode =
                        DataGridViewTriState.True,

                    Padding = new Padding(6)
                };

            AddTextColumn(
                "FullName",
                "Name",
                1.15F,
                130);

            AddTextColumn(
                "ContactNumber",
                "Contact Number",
                0.95F,
                120);

            // =====================================================
            // EMAIL COLUMN
            // =====================================================
            // More width is given to Email because email addresses
            // are commonly long.
            // =====================================================

            AddTextColumn(
                "Email",
                "Email Address",
                1.65F,
                190);

            AddTextColumn(
                "CurrentAddress",
                "Current Address",
                1.45F,
                170);

            AddEmergencyColumn();

            AddTextColumn(
                "MoveInDate",
                "Move-In Date",
                0.85F,
                105);

            AddTextColumn(
                "ActualMoveOutDate",
                "Move-Out Date",
                0.85F,
                105);

            AddStatusColumn();
            AddActionColumn();

            dgvTenants.Columns["MoveInDate"]!
                .DefaultCellStyle.Format =
                "MMM dd, yyyy";

            dgvTenants.Columns["ActualMoveOutDate"]!
                .DefaultCellStyle.Format =
                "MMM dd, yyyy";

            dgvTenants.CellMouseClick +=
                DgvTenants_CellMouseClick;

            dgvTenants.CellClick +=
                DgvTenants_CellClick;

            dgvTenants.CellMouseMove +=
                DgvTenants_CellMouseMove;

            dgvTenants.CellMouseLeave +=
                DgvTenants_CellMouseLeave;

            dgvTenants.CellFormatting +=
                DgvTenants_CellFormatting;

            dgvTenants.CellPainting +=
                DgvTenants_CellPainting;

            dgvTenants.RowPrePaint +=
                DgvTenants_RowPrePaint;

            parent.Controls.Add(dgvTenants);
        }

        // =========================================================
        // GRID EVENTS
        // =========================================================

        private void DgvTenants_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            if (dgvTenants.Rows[e.RowIndex]
                .DataBoundItem is not TenantDto tenant)
                return;

            if (dgvTenants.Columns[e.ColumnIndex]
                .Name == "Action")
                return;

            using var detailsForm =
                new InlineTenantDetailsForm(tenant);

            detailsForm.ShowDialog(this);
        }

        private async void DgvTenants_CellMouseClick(
            object? sender,
            DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left ||
                e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            if (dgvTenants.Rows[e.RowIndex]
                .DataBoundItem is not TenantDto tenant)
                return;

            if (dgvTenants.Columns[e.ColumnIndex]
                .Name != "Action")
                return;

            Rectangle rect =
                dgvTenants.GetCellDisplayRectangle(
                    e.ColumnIndex,
                    e.RowIndex,
                    false);

            int third =
                Math.Max(1, rect.Width / 3);

            if (e.X < third)
            {
                await EditTenantAsync(tenant);
            }
            else if (e.X < third * 2)
            {
                await MoveOutTenantAsync(tenant);
            }
            else
            {
                await DeleteTenantAsync(tenant);
            }
        }

        // =========================================================
        // GRID COLUMNS
        // =========================================================

        private void AddTextColumn(
            string property,
            string header,
            float fillWeight,
            int minimumWidth)
        {
            var column =
                new DataGridViewTextBoxColumn
                {
                    Name = property,
                    DataPropertyName = property,
                    HeaderText = header,
                    FillWeight = fillWeight,
                    MinimumWidth = minimumWidth,
                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill,
                    SortMode =
                        DataGridViewColumnSortMode.Automatic,
                    ReadOnly = true
                };

            column.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    // IMPORTANT:
                    // Text is allowed to continue to another line.
                    WrapMode =
                        DataGridViewTriState.True,

                    Alignment =
                        DataGridViewContentAlignment.MiddleLeft,

                    Padding = new Padding(6)
                };

            dgvTenants.Columns.Add(column);
        }

        private void AddEmergencyColumn()
        {
            var column =
                new DataGridViewTextBoxColumn
                {
                    Name = "EmergencyContact",
                    HeaderText = "Emergency Contact",
                    FillWeight = 1.20F,
                    MinimumWidth = 150,
                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill,
                    ReadOnly = true,
                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                };

            column.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    WrapMode =
                        DataGridViewTriState.True,
                    Alignment =
                        DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(6)
                };

            dgvTenants.Columns.Add(column);
        }

        private void AddStatusColumn()
        {
            var column =
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    DataPropertyName = "Status",
                    HeaderText = "Status",
                    FillWeight = 0.75F,
                    MinimumWidth = 85,
                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill,
                    ReadOnly = true,
                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                };

            column.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    Alignment =
                        DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font(
                        "Segoe UI",
                        7.5F,
                        FontStyle.Bold),
                    Padding = new Padding(3),
                    WrapMode =
                        DataGridViewTriState.False
                };

            dgvTenants.Columns.Add(column);
        }

        private void AddActionColumn()
        {
            var column =
                new DataGridViewTextBoxColumn
                {
                    Name = "Action",
                    HeaderText = "Action",
                    FillWeight = 0.75F,
                    MinimumWidth = 90,
                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill,
                    ReadOnly = true,
                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                };

            column.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    Alignment =
                        DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font(
                        "Segoe UI Symbol",
                        9F,
                        FontStyle.Bold),
                    ForeColor = TextDark,
                    SelectionBackColor =
                        RecordHoverBg,
                    SelectionForeColor =
                        TextDark,
                    WrapMode =
                        DataGridViewTriState.False
                };

            dgvTenants.Columns.Add(column);
        }

        // =========================================================
        // GRID HOVER
        // =========================================================

        private void DgvTenants_CellMouseMove(
            object? sender,
            DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 &&
                e.ColumnIndex >= 0 &&
                dgvTenants.Columns[e.ColumnIndex]
                    .Name == "Action")
            {
                Rectangle rect =
                    dgvTenants.GetCellDisplayRectangle(
                        e.ColumnIndex,
                        e.RowIndex,
                        false);

                int third =
                    Math.Max(1, rect.Width / 3);

                int part =
                    e.X < third
                        ? 0
                        : e.X < third * 2
                            ? 1
                            : 2;

                if (_hoveredRowIndex != e.RowIndex ||
                    _hoveredActionPart != part)
                {
                    int old = _hoveredRowIndex;

                    _hoveredRowIndex = e.RowIndex;
                    _hoveredActionPart = part;

                    if (old >= 0)
                        dgvTenants.InvalidateRow(old);

                    dgvTenants.InvalidateRow(
                        _hoveredRowIndex);
                }
            }
            else if (_hoveredRowIndex >= 0)
            {
                int old = _hoveredRowIndex;

                _hoveredRowIndex = -1;
                _hoveredActionPart = -1;

                dgvTenants.InvalidateRow(old);
            }
        }

        private void DgvTenants_CellMouseLeave(
            object? sender,
            EventArgs e)
        {
            if (_hoveredRowIndex < 0)
                return;

            int old = _hoveredRowIndex;

            _hoveredRowIndex = -1;
            _hoveredActionPart = -1;

            dgvTenants.InvalidateRow(old);
        }

        // =========================================================
        // GRID FORMATTING
        // =========================================================

        private void DgvTenants_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            string column =
                dgvTenants.Columns[e.ColumnIndex].Name;

            // =====================================================
            // EMAIL
            // =====================================================
            // IMPORTANT:
            // Email is now allowed to wrap into multiple lines.
            // It is no longer forced into one line.
            // =====================================================

            if (column == "Email" &&
                dgvTenants.Rows[e.RowIndex]
                    .DataBoundItem is TenantDto emailTenant)
            {
                e.Value =
                    string.IsNullOrWhiteSpace(
                        emailTenant.Email)
                        ? "—"
                        : emailTenant.Email.Trim();

                // FIX:
                // Previously this was False.
                // True allows long emails to be fully displayed
                // across multiple lines.
                e.CellStyle.WrapMode =
                    DataGridViewTriState.True;

                e.CellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleLeft;

                e.CellStyle.Font =
                    new Font(
                        "Segoe UI",
                        8.2F);

                e.CellStyle.Padding =
                    new Padding(6, 5, 6, 5);
            }

            if (column == "CurrentAddress")
            {
                e.Value =
                    FormatAddress(
                        e.Value?.ToString());

                e.CellStyle.WrapMode =
                    DataGridViewTriState.True;

                e.CellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleLeft;
            }

            if (column == "ActualMoveOutDate" &&
                (e.Value == null ||
                 e.Value == DBNull.Value ||
                 string.IsNullOrWhiteSpace(
                     e.Value.ToString())))
            {
                e.Value = "—";
                e.CellStyle.ForeColor = TextMuted;
            }

            if (column == "Status")
            {
                string status =
                    e.Value?.ToString() ?? "";

                bool isActive =
                    string.Equals(
                        status,
                        "Active",
                        StringComparison.OrdinalIgnoreCase);

                bool isArchived =
                    string.Equals(
                        status,
                        "Archived",
                        StringComparison.OrdinalIgnoreCase);

                e.CellStyle.BackColor =
                    isActive
                        ? ActiveBg
                        : MovedBg;

                e.CellStyle.ForeColor =
                    isActive
                        ? ActiveText
                        : isArchived
                            ? Color.FromArgb(185, 28, 28)
                            : Color.FromArgb(220, 38, 38);

                e.CellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }

            if (column == "Action")
            {
                e.Value = "✎   ↪   ×";

                e.CellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private static string FormatAddress(
            string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return "No address provided";

            string[] parts =
                address
                    .Split(
                        '|',
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .ToArray();

            return parts.Length == 0
                ? "No address provided"
                : string.Join(", ", parts);
        }

        // =========================================================
        // CUSTOM CELL PAINTING
        // =========================================================

        private void DgvTenants_CellPainting(
            object? sender,
            DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            string column =
                dgvTenants.Columns[e.ColumnIndex].Name;

            if (column == "Action")
            {
                e.PaintBackground(
                    e.ClipBounds,
                    true);

                using Pen pen =
                    new Pen(BorderColor);

                e.Graphics.DrawLine(
                    pen,
                    e.CellBounds.Left,
                    e.CellBounds.Bottom - 1,
                    e.CellBounds.Right,
                    e.CellBounds.Bottom - 1);

                int third =
                    Math.Max(
                        1,
                        e.CellBounds.Width / 3);

                string[] symbols =
                {
                    "✎",
                    "↪",
                    "×"
                };

                for (int i = 0; i < 3; i++)
                {
                    Rectangle partRect =
                        new Rectangle(
                            e.CellBounds.X +
                            i * third,
                            e.CellBounds.Y,
                            third,
                            e.CellBounds.Height);

                    bool hovered =
                        e.RowIndex ==
                            _hoveredRowIndex &&
                        _hoveredActionPart == i;

                    if (hovered)
                    {
                        using Brush hover =
                            new SolidBrush(
                                RecordHoverBg);

                        e.Graphics.FillRectangle(
                            hover,
                            partRect);
                    }

                    Color textColor =
                        hovered
                            ? i == 2
                                ? Color.FromArgb(
                                    220,
                                    38,
                                    38)
                                : ActiveText
                            : TextDark;

                    using Font font =
                        new Font(
                            "Segoe UI Symbol",
                            11F,
                            FontStyle.Bold);

                    TextRenderer.DrawText(
                        e.Graphics,
                        symbols[i],
                        font,
                        partRect,
                        textColor,
                        TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter);
                }

                e.Handled = true;
                return;
            }

            if (column != "EmergencyContact" ||
                dgvTenants.Rows[e.RowIndex]
                    .DataBoundItem is not TenantDto tenant)
                return;

            e.PaintBackground(
                e.ClipBounds,
                true);

            using Pen borderPen =
                new Pen(BorderColor);

            e.Graphics.DrawLine(
                borderPen,
                e.CellBounds.Left,
                e.CellBounds.Bottom - 1,
                e.CellBounds.Right,
                e.CellBounds.Bottom - 1);

            string name =
                string.IsNullOrWhiteSpace(
                    tenant.EmergencyContactName)
                    ? "No emergency contact"
                    : tenant.EmergencyContactName;

            string number =
                tenant.EmergencyContactNumber ?? "";

            string relation =
                tenant.EmergencyRelationship ?? "";

            int x =
                e.CellBounds.X + 10;

            int width =
                Math.Max(
                    20,
                    e.CellBounds.Width - 20);

            int y =
                e.CellBounds.Y + 6;

            using Font nameFont =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            using Font numberFont =
                new Font(
                    "Segoe UI",
                    8F);

            using Font relationFont =
                new Font(
                    "Segoe UI",
                    7.5F);

            TextRenderer.DrawText(
                e.Graphics,
                name,
                nameFont,
                new Rectangle(
                    x,
                    y,
                    width,
                    17),
                TextDark,
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);

            if (!string.IsNullOrWhiteSpace(number))
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    number,
                    numberFont,
                    new Rectangle(
                        x,
                        y + 17,
                        width,
                        15),
                    TextMuted,
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPrefix);
            }

            if (!string.IsNullOrWhiteSpace(relation))
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    relation,
                    relationFont,
                    new Rectangle(
                        x,
                        y + 31,
                        width,
                        14),
                    TextMuted,
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPrefix);
            }

            e.Handled = true;
        }

        private void DgvTenants_RowPrePaint(
            object? sender,
            DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row =
                dgvTenants.Rows[e.RowIndex];

            row.DefaultCellStyle.SelectionBackColor =
                RecordHoverBg;

            row.DefaultCellStyle.SelectionForeColor =
                TextDark;
        }

        // =========================================================
        // API
        // =========================================================

        private async Task LoadTenantsAsync()
        {
            try
            {
                List<TenantDto>? result =
                    await _apiService
                        .GetAsync<List<TenantDto>>(
                            "api/Tenants");

                _tenants =
                    result ?? new List<TenantDto>();

                ApplyFilterAndSearch();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "Unable to connect to the PBCRM2 API.\n\n" +
                    "Please make sure the API is running.\n\n" +
                    ex.Message,
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "The request took too long and was cancelled.\n\n" +
                    "Please check that the API server is running and try again.",
                    "Connection Timeout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load tenant records.\n\n{ex.Message}",
                    "Tenant Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task EditTenantAsync(
            TenantDto tenant)
        {
            using var form =
                new InlineTenantEditorForm(
                    _apiService,
                    tenant);

            if (form.ShowDialog(this) ==
                DialogResult.OK)
            {
                await LoadTenantsAsync();
            }
        }

        private async Task MoveOutTenantAsync(
            TenantDto tenant)
        {
            if (tenant.IsArchived)
            {
                MessageBox.Show(
                    "This tenant record is archived.",
                    "Tenant Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (tenant.ActualMoveOutDate.HasValue)
            {
                MessageBox.Show(
                    "This tenant has already been marked as moved out.",
                    "Tenant Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (MessageBox.Show(
                    $"Are you sure you want to mark {tenant.FullName} as moved out?",
                    "Move Out Tenant",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)
                != DialogResult.Yes)
            {
                return;
            }

            try
            {
                bool success =
                    await _apiService.PostAsync(
                        $"api/Tenants/{tenant.Id}/move-out",
                        new MoveOutRequest
                        {
                            MoveOutDate = DateTime.Today
                        });

                if (!success)
                {
                    MessageBox.Show(
                        "The tenant could not be marked as moved out. " +
                        "The database was not updated.",
                        "Move Out Tenant",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                // Update the object immediately so the table cannot
                // temporarily continue showing Active.
                tenant.ActualMoveOutDate = DateTime.Today;

                ApplyFilterAndSearch();
                dgvTenants.Invalidate();

                MessageBox.Show(
                    "Tenant status updated to Moved Out.",
                    "Tenant Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Reload from the API so the table reflects the
                // value actually saved in the database.
                await LoadTenantsAsync();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "Unable to connect to the PBCRM2 API.\n\n" +
                    ex.Message,
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "The request took too long and was cancelled.\n\n" +
                    "Please check that the API server is running.",
                    "Connection Timeout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to update the tenant status.\n\n{ex.Message}",
                    "Move Out Tenant",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task DeleteTenantAsync(
            TenantDto tenant)
        {
            if (tenant.IsArchived)
            {
                MessageBox.Show(
                    "This tenant record is already archived.",
                    "Archive Tenant",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (MessageBox.Show(
                    $"Archive the tenant record for {tenant.FullName}?\n\n" +
                    "The record will NOT be permanently deleted.\n" +
                    "It will remain available for historical records.",
                    "Archive Tenant",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning)
                != DialogResult.Yes)
            {
                return;
            }

            try
            {
                bool success =
                    await _apiService.PostAsync(
                        $"api/Tenants/{tenant.Id}/archive",
                        new { });

                if (!success)
                {
                    MessageBox.Show(
                        "The tenant record could not be archived. " +
                        "Please make sure the API archive endpoint is available.",
                        "Archive Tenant",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                tenant.IsArchived = true;

                // Keep the record visible in the table, but its status
                // is now Archived instead of Active.
                ApplyFilterAndSearch();
                dgvTenants.Invalidate();

                MessageBox.Show(
                    "Tenant record archived successfully.\n\n" +
                    "The record was preserved and was not permanently deleted.",
                    "Archive Tenant",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "Unable to connect to the PBCRM2 API.\n\n" +
                    ex.Message,
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "The request took too long and was cancelled.\n\n" +
                    "Please check that the API server is running.",
                    "Connection Timeout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to archive the tenant record.\n\n{ex.Message}",
                    "Archive Tenant",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // TENANT DETAILS FORM
        // =========================================================

        private class InlineTenantDetailsForm : Form
        {
            private readonly TenantDto _tenant;

            private static readonly Color Bg =
                Color.FromArgb(250, 247, 242);

            public InlineTenantDetailsForm(
                TenantDto tenant)
            {
                _tenant = tenant;

                Text = "Tenant Record";

                StartPosition =
                    FormStartPosition.CenterParent;

                ClientSize =
                    new Size(920, 900);

                MinimumSize =
                    MaximumSize =
                        new Size(920, 900);

                FormBorderStyle =
                    FormBorderStyle.FixedDialog;

                MaximizeBox =
                    MinimizeBox =
                    ShowInTaskbar = false;

                BackColor = Bg;

                Build();
            }

            private void Build()
            {
                Panel header = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 96,
                    BackColor = BrandBg
                };

                Label title = new Label
                {
                    Text = "TENANT RECORD",
                    Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(28, 15)
                };

                Label subtitle = new Label
                {
                    Text = "Complete tenant information",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = BrandAccent,
                    AutoSize = true,
                    Location = new Point(30, 56)
                };

                header.Controls.Add(title);
                header.Controls.Add(subtitle);

                Panel body = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Bg,
                    Padding = new Padding(20, 12, 20, 10)
                };

                Panel card = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                const int left = 20;
                const int right = 405;
                const int half = 375;
                const int full = 760;

                AddSectionHeader(card, "PERSONAL INFORMATION", left, 14, full);
                AddInfoCard(card, "Full Name", ValueOrDash(_tenant.FullName), left, 50, half, true);
                AddInfoCard(card, "Date of Birth", _tenant.DateOfBirth == default ? "—" : _tenant.DateOfBirth.ToString("MMMM dd, yyyy"), right, 50, half, true);
                AddInfoCard(card, "Sex", ValueOrDash(_tenant.Sex), left, 102, half, true);
                AddStatusCard(card, _tenant.Status, right, 102, half);

                AddSectionHeader(card, "CONTACT INFORMATION", left, 154, full);
                AddInfoCard(card, "Contact Number", ValueOrDash(_tenant.ContactNumber), left, 190, half, true);
                AddInfoCard(card, "Email Address", ValueOrDash(_tenant.Email), right, 190, half, true);

                AddSectionHeader(card, "CURRENT ADDRESS", left, 244, full);
                string[] addressParts = ParseAddress(_tenant.CurrentAddress);
                AddInfoCard(card, "House / Unit No.", addressParts[0], left, 280, half, true);
                AddInfoCard(card, "Street", addressParts[1], right, 280, half, true);
                AddInfoCard(card, "Barangay", addressParts[2], left, 332, half, true);
                AddInfoCard(card, "City / Municipality", addressParts[3], right, 332, half, true);
                AddInfoCard(card, "Province", addressParts[4], left, 384, full, true);

                // Extra breathing room before Emergency Contact.
                AddSectionHeader(card, "EMERGENCY CONTACT", left, 448, full);
                AddInfoCard(card, "Full Name", ValueOrDash(_tenant.EmergencyContactName), left, 484, half, true);
                AddInfoCard(card, "Relationship", ValueOrDash(_tenant.EmergencyRelationship), right, 484, half, true);
                AddInfoCard(card, "Contact Number", ValueOrDash(_tenant.EmergencyContactNumber), left, 536, half, true);

                AddSectionHeader(card, "OCCUPANCY", left, 590, full);
                AddInfoCard(card, "Move-In Date", _tenant.MoveInDate == default ? "—" : _tenant.MoveInDate.ToString("MMMM dd, yyyy"), left, 626, half, true);
                AddInfoCard(card, "Move-Out Date", _tenant.ActualMoveOutDate.HasValue ? _tenant.ActualMoveOutDate.Value.ToString("MMMM dd, yyyy") : "Not moved out", right, 626, half, true);

                body.Controls.Add(card);

                Panel actions = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 58,
                    BackColor = Bg
                };

                Button btnClose = new Button
                {
                    Text = "Close",
                    Size = new Size(105, 36),
                    BackColor = CtaColor,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Location = new Point(ClientSize.Width - btnClose.Width - 20, 10);
                btnClose.Click += (s, e) =>
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                };

                actions.Controls.Add(btnClose);
                actions.Resize += (s, e) =>
                    btnClose.Left = Math.Max(10, actions.ClientSize.Width - btnClose.Width - 20);

                Controls.Add(body);
                Controls.Add(actions);
                Controls.Add(header);
            }


            private static string ValueOrDash(
                string? value)
            {
                return string.IsNullOrWhiteSpace(value)
                    ? "—"
                    : value.Trim();
            }

            private static string[] ParseAddress(
                string? address)
            {
                string[] result =
                {
                    "—",
                    "—",
                    "—",
                    "—",
                    "—"
                };

                if (string.IsNullOrWhiteSpace(address))
                    return result;

                string[] parts;

                if (address.Contains("|"))
                {
                    parts =
                        address
                            .Split(
                                '|',
                                StringSplitOptions.None)
                            .Select(x => x.Trim())
                            .ToArray();
                }
                else
                {
                    parts =
                        address
                            .Split(
                                ',',
                                StringSplitOptions.None)
                            .Select(x => x.Trim())
                            .ToArray();
                }

                for (int i = 0;
                     i < Math.Min(5, parts.Length);
                     i++)
                {
                    if (!string.IsNullOrWhiteSpace(parts[i]))
                        result[i] = parts[i];
                }

                return result;
            }

            private static void AddSectionHeader(
                Panel parent,
                string title,
                int x,
                int y,
                int width)
            {
                Panel section = new Panel
                {
                    Location =
                        new Point(x, y),
                    Size =
                        new Size(width, 28),
                    BackColor = SectionBg
                };

                Label label = new Label
                {
                    Text = title,
                    Font = new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold),
                    ForeColor = TextDark,
                    AutoSize = true,
                    Location =
                        new Point(11, 6)
                };

                section.Controls.Add(label);
                parent.Controls.Add(section);
            }

            // =====================================================
            // INFO CARD
            // =====================================================

            private static void AddInfoCard(
                Panel parent,
                string labelText,
                string value,
                int x,
                int y,
                int width,
                bool allowWrap = false)
            {
                Label label = new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = TextMuted,
                    AutoSize = true,
                    Location = new Point(x, y)
                };

                Label valueLabel = new Label
                {
                    Text = ValueOrDash(value),
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = TextDark,
                    AutoSize = false,
                    Location = new Point(x, y + 17),
                    Size = new Size(width, 34),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(0, 1, 5, 1),
                    AutoEllipsis = false,
                    UseMnemonic = false
                };

                parent.Controls.Add(label);
                parent.Controls.Add(valueLabel);
            }

            private static void AddStatusCard(
                Panel parent,
                string status,
                int x,
                int y,
                int width)
            {
                Label label = new Label
                {
                    Text = "Status",
                    Font = new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold),
                    ForeColor = TextMuted,
                    AutoSize = true,
                    Location =
                        new Point(x, y)
                };

                bool active =
                    string.Equals(
                        status,
                        "Active",
                        StringComparison.OrdinalIgnoreCase);

                Label value = new Label
                {
                    Text = ValueOrDash(status),
                    Font = new Font(
                        "Segoe UI",
                        9.5F,
                        FontStyle.Bold),
                    ForeColor =
                        active
                            ? ActiveText
                            : MovedText,
                    AutoSize = false,
                    Location =
                        new Point(
                            x,
                            y + 17),
                    Size =
                        new Size(
                            width,
                            26),
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

                parent.Controls.Add(label);
                parent.Controls.Add(value);
            }
        }

        // =========================================================
        // EDITOR FORM
        // =========================================================

        private class InlineTenantEditorForm : Form
        {
            private readonly ApiService _apiService;
            private readonly TenantDto? _tenant;

            private TextBox txtFullName = null!;
            private TextBox txtEmail = null!;
            private TextBox txtContactNumber = null!;

            private DateTimePicker dtpDateOfBirth = null!;
            private DateTimePicker dtpMoveInDate = null!;

            private ComboBox cboSex = null!;
            private ComboBox cboBarangay = null!;
            private ComboBox cboCity = null!;
            private ComboBox cboProvince = null!;

            private TextBox txtHouseUnit = null!;
            private TextBox txtStreet = null!;

            private TextBox txtEmergencyName = null!;
            private TextBox txtEmergencyRelationship = null!;
            private TextBox txtEmergencyNumber = null!;

            private Button btnSave = null!;
            private Button btnCancel = null!;

            private bool _loadingAddress;

            private const string PhonePrefix = "+63";

            private const int DefaultBranchId = 1;

            private static readonly string[] Provinces =
            {
                "Davao Oriental",
                "Davao de Oro",
                "Davao del Norte",
                "Davao del Sur",
                "Davao Occidental",
                "South Cotabato",
                "Sultan Kudarat",
                "Cotabato",
                "Sarangani"
            };

            private static readonly string[] Cities =
            {
                "Davao City",
                "Mati City",
                "Tagum City",
                "Panabo City",
                "Digos City",
                "General Santos City",
                "Koronadal City",
                "Kidapawan City",
                "Samal City"
            };

            private static readonly string[] DavaoCityBarangays =
            {
                "Acacia",
                "Agdao",
                "Bago Aplaya",
                "Bago Gallera",
                "Buana",
                "Buhangin",
                "Bunawan",
                "Cabantian",
                "Calinan",
                "Catalunan Grande",
                "Catalunan Pequeño",
                "Communal",
                "Davao City Proper",
                "Indangan",
                "Ilang",
                "Ma-a",
                "Matina Aplaya",
                "Matina Crossing",
                "Matina Pangi",
                "Mintal",
                "Pampanga",
                "Panacan",
                "Sasa",
                "Talomo",
                "Tibungco",
                "Tigatto",
                "Toril",
                "Tugbok"
            };

            public InlineTenantEditorForm(
                ApiService apiService,
                TenantDto? tenant = null)
            {
                _apiService = apiService;
                _tenant = tenant;

                Text =
                    _tenant == null
                        ? "Add Tenant"
                        : "Edit Tenant";

                StartPosition =
                    FormStartPosition.CenterParent;

                ClientSize =
                    new Size(920, 950);

                MinimumSize =
                    MaximumSize =
                        new Size(920, 950);

                FormBorderStyle =
                    FormBorderStyle.FixedDialog;

                MaximizeBox =
                    MinimizeBox =
                    ShowInTaskbar = false;

                BackColor =
                    Color.FromArgb(
                        250,
                        247,
                        242);

                BuildEditor();
            }

            // =====================================================
            // EDITOR BUILD
            // =====================================================

            private void BuildEditor()
            {
                Panel header = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 96,
                    BackColor = BrandBg
                };

                Label title = new Label
                {
                    Text = _tenant == null ? "CREATE TENANT" : "EDIT TENANT",
                    Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(28, 15)
                };

                Label subtitle = new Label
                {
                    Text = _tenant == null ? "Add a new tenant record." : "Update the tenant record.",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = BrandAccent,
                    AutoSize = true,
                    Location = new Point(30, 56)
                };

                header.Controls.Add(title);
                header.Controls.Add(subtitle);

                Panel body = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(250, 247, 242),
                    AutoScroll = false,
                    Padding = new Padding(20, 12, 20, 10)
                };

                Panel formCard = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                const int left = 20;
                const int right = 425;
                const int half = 395;
                const int full = 820;

                AddSectionHeader(formCard, "PERSONAL INFORMATION", left, 14, full);
                txtFullName = AddTextField(formCard, "Full Name *", left, 50, full);
                dtpDateOfBirth = AddDateField(formCard, "Date of Birth *", left, 111, half);
                cboSex = AddComboField(formCard, "Sex *", right, 111, half, new[] { "Male", "Female", "Other" });

                AddSectionHeader(formCard, "CONTACT INFORMATION", left, 176, full);
                txtEmail = AddTextField(formCard, "Email Address *", left, 212, half);
                txtContactNumber = AddPhoneField(formCard, "Contact Number *", right, 212, half);

                AddSectionHeader(formCard, "CURRENT ADDRESS", left, 277, full);
                txtHouseUnit = AddTextField(formCard, "House / Unit No. *", left, 313, half);
                txtStreet = AddTextField(formCard, "Street *", right, 313, half);
                cboBarangay = AddEditableComboField(formCard, "Barangay *", left, 374, half, Array.Empty<string>());
                cboCity = AddEditableComboField(formCard, "City / Municipality *", right, 374, half, Cities);
                cboProvince = AddEditableComboField(formCard, "Province *", left, 435, full, Provinces);

                cboCity.SelectedIndexChanged += (s, e) =>
                {
                    if (!_loadingAddress)
                        UpdateBarangayChoices(true);
                };

                cboCity.Leave += (s, e) =>
                {
                    if (!_loadingAddress)
                        UpdateBarangayChoices(false);
                };

                AddSectionHeader(formCard, "OCCUPANCY", left, 500, full);
                dtpMoveInDate = AddDateField(formCard, "Move-In Date *", left, 536, half);
                AddInfoNote(formCard, "The move-in date cannot be later than today.", right, 536, half);

                // More vertical space before the emergency section.
                AddSectionHeader(formCard, "EMERGENCY CONTACT", left, 612, full);
                txtEmergencyName = AddTextField(formCard, "Full Name *", left, 648, half);
                txtEmergencyRelationship = AddTextField(formCard, "Relationship *", right, 648, half);
                txtEmergencyNumber = AddPhoneField(formCard, "Contact Number *", left, 709, half);

                body.Controls.Add(formCard);

                Panel actions = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 64,
                    BackColor = Color.FromArgb(250, 247, 242)
                };

                Panel actionLine = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 1,
                    BackColor = BorderColor
                };
                actions.Controls.Add(actionLine);

                btnCancel = new Button
                {
                    Text = "Cancel",
                    Size = new Size(105, 36),
                    BackColor = Color.White,
                    ForeColor = TextDark,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };
                btnCancel.FlatAppearance.BorderColor = BorderColor;
                btnCancel.FlatAppearance.BorderSize = 1;

                btnSave = new Button
                {
                    Text = _tenant == null ? "Save Tenant" : "Save Changes",
                    Size = new Size(125, 36),
                    BackColor = CtaColor,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };
                btnSave.FlatAppearance.BorderSize = 0;

                btnCancel.Click += (s, e) =>
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                };

                btnSave.Click += async (s, e) => await SaveTenantAsync();

                actions.Controls.Add(btnCancel);
                actions.Controls.Add(btnSave);
                actions.Resize += (s, e) => PositionActionButtons(actions);

                Controls.Add(body);
                Controls.Add(actions);
                Controls.Add(header);

                AcceptButton = btnSave;
                CancelButton = btnCancel;

                PositionActionButtons(actions);
                UpdateBarangayChoices(false);

                if (_tenant != null)
                    LoadTenantData();
                else
                    dtpMoveInDate.Value = DateTime.Today;
            }


            // =====================================================
            // FIELD BUILDERS
            // =====================================================

            private void AddInfoNote(
                Panel parent,
                string text,
                int x,
                int y,
                int width)
            {
                Label note = new Label
                {
                    Text = text,
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = TextMuted,
                    AutoSize = false,
                    Location = new Point(x, y + 18),
                    Size = new Size(width, 30),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                parent.Controls.Add(note);
            }

            private void AddSectionHeader(
                Panel parent,
                string title,
                int x,
                int y,
                int width)
            {
                Panel section = new Panel
                {
                    Location =
                        new Point(x, y),
                    Size =
                        new Size(width, 28),
                    BackColor = SectionBg
                };

                Label accent = new Label
                {
                    Location =
                        new Point(0, 0),
                    Size =
                        new Size(4, 28),
                    BackColor = CtaColor
                };

                Label label = new Label
                {
                    Text = title,
                    Font = new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold),
                    ForeColor = TextDark,
                    AutoSize = true,
                    Location =
                        new Point(14, 6)
                };

                section.Controls.Add(accent);
                section.Controls.Add(label);

                parent.Controls.Add(section);
            }

            private TextBox AddTextField(
                Panel parent,
                string labelText,
                int x,
                int y,
                int width)
            {
                Label label = new Label
                {
                    Text = labelText,
                    Font = new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold),
                    ForeColor = TextDark,
                    AutoSize = true,
                    Location =
                        new Point(x, y)
                };

                TextBox textBox = new TextBox
                {
                    Location =
                        new Point(
                            x,
                            y + 18),
                    Width = width,
                    Height = 30,
                    Font = new Font(
                        "Segoe UI",
                        9F),
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    BackColor = Color.White,
                    ForeColor = TextDark
                };

                textBox.GotFocus +=
                    (s, e) =>
                    {
                        textBox.BackColor =
                            Color.FromArgb(
                                255,
                                252,
                                246);
                    };

                textBox.LostFocus +=
                    (s, e) =>
                    {
                        textBox.BackColor =
                            Color.White;
                    };

                parent.Controls.Add(label);
                parent.Controls.Add(textBox);

                return textBox;
            }

            private DateTimePicker AddDateField(
                Panel parent,
                string labelText,
                int x,
                int y,
                int width)
            {
                Label label = new Label
                {
                    Text = labelText,
                    Font = new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold),
                    ForeColor = TextDark,
                    AutoSize = true,
                    Location =
                        new Point(x, y)
                };

                DateTimePicker picker =
                    new DateTimePicker
                    {
                        Location =
                            new Point(
                                x,
                                y + 18),
                        Width = width,
                        Height = 30,
                        Format =
                            DateTimePickerFormat.Short,
                        Font = new Font(
                            "Segoe UI",
                            9F),
                        CalendarMonthBackground =
                            Color.White,

                        MaxDate = DateTime.Today
                    };

                parent.Controls.Add(label);
                parent.Controls.Add(picker);

                return picker;
            }

            private ComboBox AddComboField(
                Panel parent,
                string labelText,
                int x,
                int y,
                int width,
                string[] items)
            {
                Label label = new Label
                {
                    Text = labelText,
                    Font = new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold),
                    ForeColor = TextDark,
                    AutoSize = true,
                    Location =
                        new Point(x, y)
                };

                ComboBox combo =
                    new ComboBox
                    {
                        Location =
                            new Point(
                                x,
                                y + 18),
                        Width = width,
                        Height = 30,
                        DropDownStyle =
                            ComboBoxStyle.DropDownList,
                        Font = new Font(
                            "Segoe UI",
                            9F),
                        BackColor = Color.White,
                        ForeColor = TextDark,
                        FlatStyle = FlatStyle.Standard
                    };

                combo.Items.AddRange(items);

                parent.Controls.Add(label);
                parent.Controls.Add(combo);

                return combo;
            }

            private ComboBox AddEditableComboField(
                Panel parent,
                string labelText,
                int x,
                int y,
                int width,
                string[] items)
            {
                Label label = new Label
                {
                    Text = labelText,
                    Font = new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold),
                    ForeColor = TextDark,
                    AutoSize = true,
                    Location =
                        new Point(x, y)
                };

                ComboBox combo =
                    new ComboBox
                    {
                        Location =
                            new Point(
                                x,
                                y + 18),
                        Width = width,
                        Height = 30,
                        DropDownStyle =
                            ComboBoxStyle.DropDown,
                        Font = new Font(
                            "Segoe UI",
                            9F),
                        BackColor = Color.White,
                        ForeColor = TextDark,
                        FlatStyle = FlatStyle.Standard,
                        AutoCompleteMode =
                            AutoCompleteMode.SuggestAppend,
                        AutoCompleteSource =
                            AutoCompleteSource.ListItems
                    };

                combo.Items.AddRange(items);

                parent.Controls.Add(label);
                parent.Controls.Add(combo);

                return combo;
            }

            private TextBox AddPhoneField(
                Panel parent,
                string labelText,
                int x,
                int y,
                int width)
            {
                Label label = new Label
                {
                    Text = labelText,
                    Font = new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold),
                    ForeColor = TextDark,
                    AutoSize = true,
                    Location =
                        new Point(x, y)
                };

                Panel phonePanel = new Panel
                {
                    Location =
                        new Point(
                            x,
                            y + 18),
                    Width = width,
                    Height = 30,
                    BackColor = Color.White,
                    BorderStyle =
                        BorderStyle.FixedSingle
                };

                Label prefix = new Label
                {
                    Text = PhonePrefix,
                    Font = new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold),
                    ForeColor = TextDark,
                    AutoSize = false,
                    TextAlign =
                        ContentAlignment.MiddleCenter,
                    Location =
                        new Point(0, 0),
                    Size =
                        new Size(48, 28),
                    BackColor =
                        Color.FromArgb(
                            249,
                            246,
                            241)
                };

                TextBox numberBox = new TextBox
                {
                    Location =
                        new Point(52, 2),
                    Width = width - 58,
                    Height = 25,
                    Font = new Font(
                        "Segoe UI",
                        9F),
                    BorderStyle =
                        BorderStyle.None,
                    MaxLength = 10,
                    BackColor = Color.White,
                    ForeColor = TextDark
                };

                numberBox.KeyPress +=
                    (s, e) =>
                    {
                        if (!char.IsControl(e.KeyChar) &&
                            !char.IsDigit(e.KeyChar))
                        {
                            e.Handled = true;
                        }
                    };

                phonePanel.Controls.Add(prefix);
                phonePanel.Controls.Add(numberBox);

                parent.Controls.Add(label);
                parent.Controls.Add(phonePanel);

                return numberBox;
            }

            // =====================================================
            // ADDRESS
            // =====================================================

            private void UpdateBarangayChoices(
                bool clearCurrentSelection)
            {
                if (cboBarangay == null ||
                    cboCity == null)
                    return;

                string current =
                    cboBarangay.Text.Trim();

                string city =
                    cboCity.Text.Trim();

                cboBarangay.BeginUpdate();

                try
                {
                    cboBarangay.Items.Clear();

                    if (string.Equals(
                            city,
                            "Davao City",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        cboBarangay.Items.AddRange(
                            DavaoCityBarangays);
                    }

                    if (clearCurrentSelection)
                    {
                        cboBarangay.Text = "";
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(current))
                    {
                        int index =
                            cboBarangay.Items.IndexOf(
                                current);

                        cboBarangay.Text =
                            index >= 0
                                ? cboBarangay.Items[index]!
                                    .ToString()
                                : current;
                    }
                    else
                    {
                        cboBarangay.Text = "";
                    }
                }
                finally
                {
                    cboBarangay.EndUpdate();
                }
            }

            // =====================================================
            // BUTTON POSITION
            // =====================================================

            private void PositionActionButtons(
                Panel actions)
            {
                if (actions.ClientSize.Width <= 0)
                    return;

                btnSave.Location =
                    new Point(
                        actions.ClientSize.Width -
                        btnSave.Width - 20,
                        18);

                btnCancel.Location =
                    new Point(
                        btnSave.Left -
                        btnCancel.Width - 10,
                        18);
            }

            // =====================================================
            // LOAD TENANT
            // =====================================================

            private void LoadTenantData()
            {
                if (_tenant == null)
                    return;

                txtFullName.Text =
                    _tenant.FullName;

                if (_tenant.DateOfBirth != default &&
                    _tenant.DateOfBirth <= DateTime.Today)
                {
                    dtpDateOfBirth.Value =
                        _tenant.DateOfBirth;
                }

                if (!string.IsNullOrWhiteSpace(
                        _tenant.Sex))
                {
                    int index =
                        cboSex.Items.IndexOf(
                            _tenant.Sex);

                    if (index >= 0)
                        cboSex.SelectedIndex =
                            index;
                }

                // =================================================
                // LOAD EXACT SAVED EMAIL
                // =================================================

                txtEmail.Text =
                    (_tenant.Email ?? "").Trim();

                SetPhoneValue(
                    txtContactNumber,
                    _tenant.ContactNumber);

                SetPhoneValue(
                    txtEmergencyNumber,
                    _tenant.EmergencyContactNumber);

                LoadAddress(
                    _tenant.CurrentAddress);

                txtEmergencyName.Text =
                    _tenant.EmergencyContactName;

                txtEmergencyRelationship.Text =
                    _tenant.EmergencyRelationship;

                if (_tenant.MoveInDate != default)
                {
                    dtpMoveInDate.Value =
                        _tenant.MoveInDate <= DateTime.Today
                            ? _tenant.MoveInDate
                            : DateTime.Today;
                }
            }

            // =====================================================
            // LOAD ADDRESS
            // =====================================================

            private void LoadAddress(
                string? address)
            {
                txtHouseUnit.Clear();
                txtStreet.Clear();

                cboBarangay.Text = "";
                cboCity.Text = "";
                cboProvince.Text = "";

                if (string.IsNullOrWhiteSpace(address))
                    return;

                _loadingAddress = true;

                try
                {
                    string[] parts;

                    if (address.Contains("|"))
                    {
                        parts =
                            address
                                .Trim()
                                .Split(
                                    '|',
                                    StringSplitOptions.None)
                                .Select(x => x.Trim())
                                .ToArray();
                    }
                    else
                    {
                        parts =
                            address
                                .Trim()
                                .Split(
                                    ',',
                                    StringSplitOptions.None)
                                .Select(x => x.Trim())
                                .ToArray();
                    }

                    if (parts.Length >= 5)
                    {
                        txtHouseUnit.Text =
                            parts[0];

                        txtStreet.Text =
                            parts[1];

                        cboCity.Text =
                            parts[3];

                        UpdateBarangayChoices(false);

                        cboBarangay.Text =
                            parts[2];

                        cboProvince.Text =
                            parts[4];

                        return;
                    }

                    if (parts.Length == 4)
                    {
                        txtHouseUnit.Text =
                            parts[0];

                        txtStreet.Text =
                            parts[1];

                        cboBarangay.Text =
                            parts[2];

                        cboCity.Text =
                            parts[3];

                        return;
                    }

                    txtStreet.Text =
                        address.Trim();
                }
                finally
                {
                    _loadingAddress = false;
                }
            }

            // =====================================================
            // PHONE
            // =====================================================

            private void SetPhoneValue(
                TextBox textBox,
                string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    textBox.Text = "";
                    return;
                }

                string clean =
                    value
                        .Trim()
                        .Replace(" ", "")
                        .Replace("-", "");

                if (clean.StartsWith(
                        PhonePrefix))
                {
                    clean =
                        clean.Substring(
                            PhonePrefix.Length);
                }
                else if (clean.StartsWith("0"))
                {
                    clean =
                        clean.Substring(1);
                }

                textBox.Text = clean;
            }

            private string BuildPhoneValue(
                TextBox textBox)
            {
                string number =
                    textBox.Text
                        .Trim()
                        .Replace(" ", "")
                        .Replace("-", "");

                if (string.IsNullOrWhiteSpace(number))
                    return "";

                if (number.StartsWith(
                        PhonePrefix))
                {
                    return number;
                }

                if (number.StartsWith("0"))
                {
                    number =
                        number.Substring(1);
                }

                return PhonePrefix + number;
            }

            // =====================================================
            // ADDRESS VALUE
            // =====================================================

            private string BuildAddress()
            {
                return string.Join(
                    " | ",
                    txtHouseUnit.Text.Trim(),
                    txtStreet.Text.Trim(),
                    cboBarangay.Text.Trim(),
                    cboCity.Text.Trim(),
                    cboProvince.Text.Trim());
            }

            // =====================================================
            // VALIDATION HELPERS
            // =====================================================

            private bool ShowValidationWarning(
                string message,
                Control control)
            {
                MessageBox.Show(
                    message,
                    "Validation Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                control.Focus();

                return false;
            }

            // =====================================================
            // PHILIPPINE PHONE VALIDATION
            // =====================================================

            private static bool IsValidPhilippineMobileNumber(
                string number)
            {
                if (string.IsNullOrWhiteSpace(number))
                    return false;

                string clean =
                    number
                        .Trim()
                        .Replace(" ", "")
                        .Replace("-", "");

                return Regex.IsMatch(
                    clean,
                    @"^9\d{9}$");
            }

            // =====================================================
            // EMAIL VALIDATION
            // =====================================================

            private static bool IsValidEmail(
                string email)
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                string clean =
                    email.Trim();

                try
                {
                    MailAddress address =
                        new MailAddress(clean);

                    return
                        address.Address.Equals(
                            clean,
                            StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }

            // =====================================================
            // ALL TENANT FIELD VALIDATION
            // =====================================================

            private bool ValidateTenantFields()
            {
                if (string.IsNullOrWhiteSpace(
                        txtFullName.Text))
                {
                    return ShowValidationWarning(
                        "Please enter the tenant's full name.",
                        txtFullName);
                }

                if (dtpDateOfBirth.Value.Date >
                    DateTime.Today)
                {
                    return ShowValidationWarning(
                        "Date of birth cannot be a future date.",
                        dtpDateOfBirth);
                }

                if (cboSex.SelectedIndex < 0 ||
                    string.IsNullOrWhiteSpace(
                        cboSex.Text))
                {
                    return ShowValidationWarning(
                        "Please select the tenant's sex.",
                        cboSex);
                }

                string email =
                    txtEmail.Text.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    return ShowValidationWarning(
                        "Please enter the tenant's email address.",
                        txtEmail);
                }

                if (!IsValidEmail(email))
                {
                    return ShowValidationWarning(
                        "Please enter a valid email address.\n\n" +
                        "Example: tenant@example.com",
                        txtEmail);
                }

                if (string.IsNullOrWhiteSpace(
                        txtContactNumber.Text))
                {
                    return ShowValidationWarning(
                        "Please enter the tenant's contact number.",
                        txtContactNumber);
                }

                if (!IsValidPhilippineMobileNumber(
                        txtContactNumber.Text))
                {
                    return ShowValidationWarning(
                        "Please enter a valid Philippine mobile number.\n\n" +
                        "Since +63 is already provided, enter exactly 10 digits starting with 9.\n\n" +
                        "Example: 9171234567",
                        txtContactNumber);
                }

                if (string.IsNullOrWhiteSpace(
                        txtHouseUnit.Text))
                {
                    return ShowValidationWarning(
                        "Please enter the house or unit number.",
                        txtHouseUnit);
                }

                if (string.IsNullOrWhiteSpace(
                        txtStreet.Text))
                {
                    return ShowValidationWarning(
                        "Please enter the street.",
                        txtStreet);
                }

                if (string.IsNullOrWhiteSpace(
                        cboBarangay.Text))
                {
                    return ShowValidationWarning(
                        "Please enter or select the barangay.",
                        cboBarangay);
                }

                if (string.IsNullOrWhiteSpace(
                        cboCity.Text))
                {
                    return ShowValidationWarning(
                        "Please enter or select the city or municipality.",
                        cboCity);
                }

                if (string.IsNullOrWhiteSpace(
                        cboProvince.Text))
                {
                    return ShowValidationWarning(
                        "Please enter or select the province.",
                        cboProvince);
                }

                if (dtpMoveInDate.Value.Date >
                    DateTime.Today)
                {
                    return ShowValidationWarning(
                        "Move-in date cannot be a future date.",
                        dtpMoveInDate);
                }

                if (string.IsNullOrWhiteSpace(
                        txtEmergencyName.Text))
                {
                    return ShowValidationWarning(
                        "Please enter the emergency contact's full name.",
                        txtEmergencyName);
                }

                if (string.IsNullOrWhiteSpace(
                        txtEmergencyRelationship.Text))
                {
                    return ShowValidationWarning(
                        "Please enter the emergency contact's relationship to the tenant.",
                        txtEmergencyRelationship);
                }

                if (string.IsNullOrWhiteSpace(
                        txtEmergencyNumber.Text))
                {
                    return ShowValidationWarning(
                        "Please enter the emergency contact's phone number.",
                        txtEmergencyNumber);
                }

                if (!IsValidPhilippineMobileNumber(
                        txtEmergencyNumber.Text))
                {
                    return ShowValidationWarning(
                        "Please enter a valid Philippine mobile number for the emergency contact.\n\n" +
                        "Since +63 is already provided, enter exactly 10 digits starting with 9.\n\n" +
                        "Example: 9171234567",
                        txtEmergencyNumber);
                }

                return true;
            }

            // =====================================================
            // SAVE
            // =====================================================

            private async Task SaveTenantAsync()
            {
                if (!ValidateTenantFields())
                    return;

                string email =
                    txtEmail.Text.Trim();

                string contactNumber =
                    BuildPhoneValue(
                        txtContactNumber);

                string emergencyNumber =
                    BuildPhoneValue(
                        txtEmergencyNumber);

                string currentAddress =
                    BuildAddress();

                TenantRequestDto request =
                    new TenantRequestDto
                    {
                        FullName =
                            txtFullName.Text.Trim(),

                        DateOfBirth =
                            dtpDateOfBirth.Value.Date,

                        Sex =
                            cboSex.Text.Trim(),

                        ContactNumber =
                            contactNumber,

                        // Exact trimmed email sent to API.
                        Email =
                            email,

                        CurrentAddress =
                            currentAddress,

                        EmergencyContactName =
                            txtEmergencyName.Text.Trim(),

                        EmergencyRelationship =
                            txtEmergencyRelationship.Text.Trim(),

                        EmergencyContactNumber =
                            emergencyNumber,

                        BranchId =
                            DefaultBranchId,

                        MoveInDate =
                            dtpMoveInDate.Value.Date
                    };

                btnSave.Enabled = false;
                btnCancel.Enabled = false;

                try
                {
                    bool success;

                    if (_tenant == null)
                    {
                        TenantDto? created =
                            await _apiService
                                .PostAsync<TenantDto>(
                                    "api/Tenants",
                                    request);

                        success =
                            created != null;
                    }
                    else
                    {
                        TenantDto? updated =
                            await _apiService
                                .PutAsync<TenantDto>(
                                    $"api/Tenants/{_tenant.Id}",
                                    request);

                        success =
                            updated != null;
                    }

                    if (!success)
                    {
                        MessageBox.Show(
                            _tenant == null
                                ? "The tenant record could not be saved.\n\n" +
                                  "Please check the information and try again."
                                : "The tenant record could not be updated.\n\n" +
                                  "Please check the information and try again.",
                            "Tenant Management",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }

                    MessageBox.Show(
                        _tenant == null
                            ? "Tenant added successfully."
                            : "Tenant updated successfully.",
                        "Tenant Management",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    DialogResult =
                        DialogResult.OK;

                    Close();
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show(
                        "Unable to connect to the API server.\n\n" +
                        "Please make sure the PBCRM2 API is running.\n\n" +
                        ex.Message,
                        "Connection Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (TaskCanceledException)
                {
                    MessageBox.Show(
                        "The request took too long and was cancelled.\n\n" +
                        "Please check that the API server is running and try again.",
                        "Connection Timeout",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "An unexpected error occurred while saving the tenant record.\n\n" +
                        ex.Message,
                        "Tenant Management",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    if (!IsDisposed)
                    {
                        btnSave.Enabled = true;
                        btnCancel.Enabled = true;
                    }
                }
            }
        }
    }

    // =============================================================
    // DTOs
    // =============================================================

    public class TenantDto
    {
        public int Id { get; set; }

        public string FullName { get; set; }
            = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Sex { get; set; }
            = string.Empty;

        public string ContactNumber { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string CurrentAddress { get; set; }
            = string.Empty;

        public string EmergencyContactName { get; set; }
            = string.Empty;

        public string EmergencyRelationship { get; set; }
            = string.Empty;

        public string EmergencyContactNumber { get; set; }
            = string.Empty;

        public int? BranchId { get; set; }

        public DateTime MoveInDate { get; set; }

        // This matches the API/database field used when a tenant is moved out.
        public DateTime? ActualMoveOutDate { get; set; }

        // Archived records are never permanently deleted.
        public bool IsArchived { get; set; }

        public string Status =>
            IsArchived
                ? "Archived"
                : ActualMoveOutDate.HasValue
                    ? "Moved Out"
                    : "Active";
    }

    public class TenantRequestDto
    {
        public string FullName { get; set; }
            = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Sex { get; set; }
            = string.Empty;

        public string ContactNumber { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string CurrentAddress { get; set; }
            = string.Empty;

        public string EmergencyContactName { get; set; }
            = string.Empty;

        public string EmergencyRelationship { get; set; }
            = string.Empty;

        public string EmergencyContactNumber { get; set; }
            = string.Empty;

        public int? BranchId { get; set; }

        public DateTime MoveInDate { get; set; }
            = DateTime.Today;
    }

    public class MoveOutRequest
    {
        public DateTime? MoveOutDate { get; set; }
    }
}
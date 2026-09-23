using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PBCRM2.WinForms.Forms
{
    public class AdminBranchManagementForm : Form
    {
        // ============================================================
        // COLORS
        // ============================================================

        private static readonly Color BrandBg =
            Color.FromArgb(32, 24, 18);

        private static readonly Color BrandAccent =
            Color.FromArgb(224, 194, 140);

        private static readonly Color CtaColor =
            Color.FromArgb(170, 130, 80);

        private static readonly Color PanelBg =
            Color.FromArgb(250, 247, 242);

        private static readonly Color CardBg =
            Color.White;

        private static readonly Color CardBorder =
            Color.FromArgb(225, 215, 200);

        private static readonly Color TextDark =
            Color.FromArgb(55, 48, 42);

        private static readonly Color TextMuted =
            Color.FromArgb(120, 110, 100);

        private static readonly Color GridAlternate =
            Color.FromArgb(247, 244, 239);

        private static readonly Color GridSelection =
            Color.FromArgb(232, 220, 199);

        private static readonly Color GridBorder =
            Color.FromArgb(225, 215, 200);

        private static readonly Color ActiveGreen =
            Color.FromArgb(62, 130, 82);

        private static readonly Color InactiveRed =
            Color.FromArgb(170, 82, 72);

        // ============================================================
        // SERVICE
        // ============================================================

        private readonly ApiService _apiService;

        // ============================================================
        // CONTROLS
        // ============================================================

        private Button btnAdd = null!;
        private Button btnEdit = null!;
        private Button btnAssignManager = null!;
        private Button btnToggleStatus = null!;
        private Button btnRefresh = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbStatus = null!;
        private Button btnClearSearch = null!;

        private Panel pnlSummary = null!;

        private Label lblTotalBranches = null!;
        private Label lblActiveBranches = null!;
        private Label lblTotalBeds = null!;
        private Label lblOccupiedBeds = null!;
        private Label lblAvailableBeds = null!;

        private Label lblGridInfo = null!;
        private DataGridView dgvBranches = null!;

        // ============================================================
        // DATA
        // ============================================================

        private List<AdminBranchDto> _branches =
            new List<AdminBranchDto>();

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public AdminBranchManagementForm(
            ApiService apiService)
        {
            _apiService =
                apiService
                ?? throw new ArgumentNullException(
                    nameof(apiService));

            Text =
                "Branch Management";

            StartPosition =
                FormStartPosition.CenterScreen;

            Size =
                new Size(
                    1250,
                    780);

            MinimumSize =
                new Size(
                    1050,
                    680);

            BackColor =
                PanelBg;

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            DoubleBuffered =
                true;

            FormBorderStyle =
                FormBorderStyle.None;

            Padding =
                Padding.Empty;

            BuildInterface();

            Shown += async (_, _) =>
                await LoadBranchesAsync();
        }

        // ============================================================
        // BUILD INTERFACE
        // ============================================================

        private void BuildInterface()
        {
            Panel main =
                new Panel
                {
                    Dock =
                        DockStyle.Fill,

                    BackColor =
                        PanelBg
                };

            // ========================================================
            // HEADER
            // ========================================================

            Panel header =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        120,

                    BackColor =
                        BrandBg
                };

            Label title =
                new Label
                {
                    Text =
                        "BRANCH MANAGEMENT",

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            22F,
                            FontStyle.Bold),

                    AutoSize =
                        true,

                    Location =
                        new Point(
                            25,
                            22)
                };

            Label subtitle =
                new Label
                {
                    Text =
                        "Manage boarding house branches, managers, contact details, and branch status.",

                    ForeColor =
                        BrandAccent,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F),

                    AutoSize =
                        true,

                    Location =
                        new Point(
                            28,
                            67)
                };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            Panel headerGap =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        18,

                    BackColor =
                        PanelBg
                };

            // ========================================================
            // ACTION BAR
            // ========================================================

            Panel actionPanel =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        55,

                    BackColor =
                        PanelBg
                };

            btnAdd =
                CreateButton(
                    "+  Add Branch",
                    130);

            btnAdd.Location =
                new Point(
                    20,
                    10);

            btnAdd.Click += async (_, _) =>
                await AddBranchAsync();

            btnEdit =
                CreateButton(
                    "Edit Branch",
                    115);

            btnEdit.Location =
                new Point(
                    160,
                    10);

            btnEdit.Enabled =
                false;

            btnEdit.Click += async (_, _) =>
            {
                BranchGridRow? selected =
                    GetSelectedBranch();

                if (selected != null)
                {
                    await EditBranchAsync(
                        selected.Id);
                }
            };

            btnAssignManager =
                CreateButton(
                    "Assign Manager",
                    135);

            btnAssignManager.Location =
                new Point(
                    285,
                    10);

            btnAssignManager.Enabled =
                false;

            btnAssignManager.Click += async (_, _) =>
            {
                BranchGridRow? selected =
                    GetSelectedBranch();

                if (selected != null)
                {
                    await AssignManagerAsync(
                        selected.Id);
                }
            };

            btnToggleStatus =
                CreateButton(
                    "Toggle Status",
                    125);

            btnToggleStatus.Location =
                new Point(
                    430,
                    10);

            btnToggleStatus.Enabled =
                false;

            btnToggleStatus.Click += async (_, _) =>
            {
                BranchGridRow? selected =
                    GetSelectedBranch();

                if (selected != null)
                {
                    await ToggleBranchStatusAsync(
                        selected.Id);
                }
            };

            btnRefresh =
                CreateButton(
                    "Refresh",
                    95);

            btnRefresh.Location =
                new Point(
                    565,
                    10);

            btnRefresh.Click += async (_, _) =>
                await LoadBranchesAsync();

            actionPanel.Controls.Add(btnAdd);
            actionPanel.Controls.Add(btnEdit);
            actionPanel.Controls.Add(btnAssignManager);
            actionPanel.Controls.Add(btnToggleStatus);
            actionPanel.Controls.Add(btnRefresh);

            Panel actionGap =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        12,

                    BackColor =
                        PanelBg
                };

            // ========================================================
            // FILTER BAR
            // ========================================================

            Panel filterPanel =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        55,

                    BackColor =
                        PanelBg
                };

            Label searchLabel =
                new Label
                {
                    Text =
                        "Search:",

                    AutoSize =
                        true,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),

                    ForeColor =
                        BrandBg,

                    Location =
                        new Point(
                            20,
                            18)
                };

            txtSearch =
                new TextBox
                {
                    Location =
                        new Point(
                            78,
                            12),

                    Width =
                        300,

                    Height =
                        30,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F),

                    BorderStyle =
                        BorderStyle.FixedSingle,

                    PlaceholderText =
                        "Search branch, address, contact, or manager..."
                };

            txtSearch.TextChanged +=
                (_, _) => ApplyFilters();

            Label statusLabel =
                new Label
                {
                    Text =
                        "Status:",

                    AutoSize =
                        true,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),

                    ForeColor =
                        BrandBg,

                    Location =
                        new Point(
                            400,
                            18)
                };

            cmbStatus =
                new ComboBox
                {
                    Location =
                        new Point(
                            455,
                            12),

                    Width =
                        160,

                    Height =
                        30,

                    DropDownStyle =
                        ComboBoxStyle.DropDownList,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F)
                };

            cmbStatus.Items.AddRange(
                new object[]
                {
                    "All Status",
                    "Active",
                    "Inactive"
                });

            cmbStatus.SelectedIndex =
                0;

            cmbStatus.SelectedIndexChanged +=
                (_, _) => ApplyFilters();

            btnClearSearch =
                CreateButton(
                    "Clear",
                    80);

            btnClearSearch.Location =
                new Point(
                    630,
                    12);

            btnClearSearch.Click += (_, _) =>
            {
                txtSearch.Clear();

                cmbStatus.SelectedIndex =
                    0;
            };

            filterPanel.Controls.Add(searchLabel);
            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(statusLabel);
            filterPanel.Controls.Add(cmbStatus);
            filterPanel.Controls.Add(btnClearSearch);

            Panel filterGap =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        12,

                    BackColor =
                        PanelBg
                };

            // ========================================================
            // SUMMARY
            // ========================================================

            pnlSummary =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        100,

                    BackColor =
                        PanelBg,

                    Padding =
                        new Padding(
                            20,
                            0,
                            20,
                            0)
                };

            CreateSummaryCard(
                pnlSummary,
                0,
                "TOTAL BRANCHES",
                out lblTotalBranches);

            CreateSummaryCard(
                pnlSummary,
                1,
                "ACTIVE",
                out lblActiveBranches);

            CreateSummaryCard(
                pnlSummary,
                2,
                "TOTAL BEDS",
                out lblTotalBeds);

            CreateSummaryCard(
                pnlSummary,
                3,
                "OCCUPIED",
                out lblOccupiedBeds);

            CreateSummaryCard(
                pnlSummary,
                4,
                "AVAILABLE",
                out lblAvailableBeds);

            Panel summaryGap =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        12,

                    BackColor =
                        PanelBg
                };

            // ========================================================
            // CONTENT
            // ========================================================

            Panel content =
                new Panel
                {
                    Dock =
                        DockStyle.Fill,

                    BackColor =
                        PanelBg,

                    Padding =
                        new Padding(
                            20,
                            0,
                            20,
                            20)
                };

            lblGridInfo =
                new Label
                {
                    Text =
                        "BRANCH RECORDS",

                    Dock =
                        DockStyle.Top,

                    Height =
                        28,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F,
                            FontStyle.Bold),

                    ForeColor =
                        BrandBg,

                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            dgvBranches =
                CreateGrid();

            dgvBranches.Dock =
                DockStyle.Fill;

            dgvBranches.SelectionChanged +=
                (_, _) =>
                {
                    bool hasSelection =
                        dgvBranches.SelectedRows.Count >
                        0;

                    btnEdit.Enabled =
                        hasSelection;

                    btnAssignManager.Enabled =
                        hasSelection;

                    btnToggleStatus.Enabled =
                        hasSelection;
                };

            dgvBranches.CellDoubleClick +=
                async (_, e) =>
                {
                    if (e.RowIndex < 0)
                        return;

                    BranchGridRow? selected =
                        GetSelectedBranch();

                    if (selected != null)
                    {
                        await EditBranchAsync(
                            selected.Id);
                    }
                };

            content.Controls.Add(
                dgvBranches);

            content.Controls.Add(
                lblGridInfo);

            // ========================================================
            // DOCK ORDER
            // ========================================================

            main.Controls.Add(content);
            main.Controls.Add(summaryGap);
            main.Controls.Add(pnlSummary);
            main.Controls.Add(filterGap);
            main.Controls.Add(filterPanel);
            main.Controls.Add(actionGap);
            main.Controls.Add(actionPanel);
            main.Controls.Add(headerGap);
            main.Controls.Add(header);

            Controls.Add(main);

            Resize +=
                (_, _) => LayoutSummaryCards();

            LayoutSummaryCards();
        }

        // ============================================================
        // SUMMARY CARD
        // ============================================================

        private void CreateSummaryCard(
            Panel parent,
            int index,
            string title,
            out Label valueLabel)
        {
            Panel card =
                new Panel
                {
                    Size =
                        new Size(
                            200,
                            88),

                    BackColor =
                        CardBg,

                    Location =
                        new Point(
                            20 +
                            index * 220,
                            6)
                };

            card.Paint += (_, e) =>
            {
                using Pen pen =
                    new Pen(
                        CardBorder);

                e.Graphics.DrawRectangle(
                    pen,
                    0,
                    0,
                    Math.Max(
                        0,
                        card.Width - 1),
                    Math.Max(
                        0,
                        card.Height - 1));

                using SolidBrush accent =
                    new SolidBrush(
                        CtaColor);

                e.Graphics.FillRectangle(
                    accent,
                    0,
                    0,
                    3,
                    card.Height);
            };

            Label lblTitle =
                new Label
                {
                    Text =
                        title,

                    AutoSize =
                        false,

                    Size =
                        new Size(
                            170,
                            20),

                    Location =
                        new Point(
                            14,
                            10),

                    Font =
                        new Font(
                            "Segoe UI",
                            7.5F,
                            FontStyle.Bold),

                    ForeColor =
                        TextMuted,

                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            valueLabel =
                new Label
                {
                    Text =
                        "0",

                    AutoSize =
                        false,

                    Size =
                        new Size(
                            170,
                            42),

                    Location =
                        new Point(
                            14,
                            34),

                    Font =
                        new Font(
                            "Segoe UI",
                            22F,
                            FontStyle.Bold),

                    ForeColor =
                        TextDark,

                    TextAlign =
                        ContentAlignment.MiddleRight
                };

            card.Controls.Add(lblTitle);
            card.Controls.Add(valueLabel);

            parent.Controls.Add(card);
        }

        // ============================================================
        // SUMMARY LAYOUT
        // ============================================================

        private void LayoutSummaryCards()
        {
            if (pnlSummary == null ||
                pnlSummary.Controls.Count == 0)
            {
                return;
            }

            int available =
                pnlSummary.ClientSize.Width -
                40;

            if (available <= 0)
                return;

            const int gap = 12;

            const int cardCount = 5;

            int cardWidth =
                Math.Max(
                    150,
                    (
                        available -
                        gap *
                        (cardCount - 1)
                    ) /
                    cardCount);

            const int cardHeight = 88;

            int x = 20;

            foreach (Control control
                in pnlSummary.Controls)
            {
                if (control is not Panel card)
                    continue;

                card.Location =
                    new Point(
                        x,
                        6);

                card.Size =
                    new Size(
                        cardWidth,
                        cardHeight);

                x +=
                    cardWidth +
                    gap;

                foreach (Control child
                    in card.Controls)
                {
                    if (child is not Label label)
                        continue;

                    label.Width =
                        Math.Max(
                            110,
                            cardWidth - 28);

                    if (label.Font.Size >= 18F)
                    {
                        label.TextAlign =
                            ContentAlignment.MiddleRight;
                    }
                }

                card.Invalidate();
            }

            pnlSummary.Height =
                cardHeight + 12;
        }

        // ============================================================
        // GRID
        // ============================================================

        private DataGridView CreateGrid()
        {
            DataGridView grid =
                new DataGridView
                {
                    BackgroundColor =
                        Color.White,

                    BorderStyle =
                        BorderStyle.FixedSingle,

                    CellBorderStyle =
                        DataGridViewCellBorderStyle
                            .SingleHorizontal,

                    GridColor =
                        GridBorder,

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
                        DataGridViewSelectionMode
                            .FullRowSelect,

                    AutoGenerateColumns =
                        false,

                    AutoSizeColumnsMode =
                        DataGridViewAutoSizeColumnsMode
                            .Fill,

                    RowHeadersVisible =
                        false,

                    EnableHeadersVisualStyles =
                        false,

                    ColumnHeadersHeight =
                        40,

                    ShowCellToolTips =
                        true,

                    AutoSizeRowsMode =
                        DataGridViewAutoSizeRowsMode.None
                };

            grid.RowTemplate.Height =
                42;

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
                        DataGridViewContentAlignment
                            .MiddleLeft,

                    Padding =
                        new Padding(
                            12,
                            0,
                            12,
                            0),

                    WrapMode =
                        DataGridViewTriState.False
                };

            grid.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    Font =
                        new Font(
                            "Segoe UI",
                            9F),

                    BackColor =
                        Color.White,

                    ForeColor =
                        TextDark,

                    SelectionBackColor =
                        GridSelection,

                    SelectionForeColor =
                        TextDark,

                    Alignment =
                        DataGridViewContentAlignment
                            .MiddleLeft,

                    Padding =
                        new Padding(
                            12,
                            4,
                            12,
                            4),

                    WrapMode =
                        DataGridViewTriState.False
                };

            grid.AlternatingRowsDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor =
                        GridAlternate,

                    ForeColor =
                        TextDark,

                    SelectionBackColor =
                        GridSelection,

                    SelectionForeColor =
                        TextDark,

                    Alignment =
                        DataGridViewContentAlignment
                            .MiddleLeft,

                    Padding =
                        new Padding(
                            12,
                            4,
                            12,
                            4)
                };

            // --------------------------------------------------------
            // BRANCH NAME
            // --------------------------------------------------------

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name =
                        "BranchName",

                    HeaderText =
                        "BRANCH NAME",

                    DataPropertyName =
                        "BranchName",

                    FillWeight =
                        18,

                    MinimumWidth =
                        140,

                    SortMode =
                        DataGridViewColumnSortMode
                            .NotSortable
                });

            // --------------------------------------------------------
            // ADDRESS
            // --------------------------------------------------------

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name =
                        "Address",

                    HeaderText =
                        "ADDRESS",

                    DataPropertyName =
                        "Address",

                    FillWeight =
                        24,

                    MinimumWidth =
                        180,

                    SortMode =
                        DataGridViewColumnSortMode
                            .NotSortable
                });

            // --------------------------------------------------------
            // CONTACT
            // --------------------------------------------------------

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name =
                        "ContactNumber",

                    HeaderText =
                        "CONTACT",

                    DataPropertyName =
                        "ContactNumber",

                    FillWeight =
                        12,

                    MinimumWidth =
                        110,

                    SortMode =
                        DataGridViewColumnSortMode
                            .NotSortable
                });

            // --------------------------------------------------------
            // MANAGER
            // --------------------------------------------------------

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name =
                        "ManagerName",

                    HeaderText =
                        "BRANCH MANAGER",

                    DataPropertyName =
                        "ManagerName",

                    FillWeight =
                        14,

                    MinimumWidth =
                        130,

                    SortMode =
                        DataGridViewColumnSortMode
                            .NotSortable
                });

            // --------------------------------------------------------
            // BEDS
            // --------------------------------------------------------

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name =
                        "TotalBeds",

                    HeaderText =
                        "BEDS",

                    DataPropertyName =
                        "TotalBeds",

                    FillWeight =
                        7,

                    MinimumWidth =
                        60,

                    SortMode =
                        DataGridViewColumnSortMode
                            .NotSortable,

                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Alignment =
                                DataGridViewContentAlignment
                                    .MiddleCenter
                        }
                });

            // --------------------------------------------------------
            // OCCUPIED
            // --------------------------------------------------------

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name =
                        "OccupiedBeds",

                    HeaderText =
                        "OCCUPIED",

                    DataPropertyName =
                        "OccupiedBeds",

                    FillWeight =
                        8,

                    MinimumWidth =
                        70,

                    SortMode =
                        DataGridViewColumnSortMode
                            .NotSortable,

                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Alignment =
                                DataGridViewContentAlignment
                                    .MiddleCenter
                        }
                });

            // --------------------------------------------------------
            // AVAILABLE
            // --------------------------------------------------------

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name =
                        "AvailableBeds",

                    HeaderText =
                        "AVAILABLE",

                    DataPropertyName =
                        "AvailableBeds",

                    FillWeight =
                        9,

                    MinimumWidth =
                        75,

                    SortMode =
                        DataGridViewColumnSortMode
                            .NotSortable,

                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Alignment =
                                DataGridViewContentAlignment
                                    .MiddleCenter
                        }
                });

            // --------------------------------------------------------
            // STATUS
            // --------------------------------------------------------

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name =
                        "Status",

                    HeaderText =
                        "STATUS",

                    DataPropertyName =
                        "Status",

                    FillWeight =
                        10,

                    MinimumWidth =
                        85,

                    SortMode =
                        DataGridViewColumnSortMode
                            .NotSortable,

                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Alignment =
                                DataGridViewContentAlignment
                                    .MiddleCenter,

                            Font =
                                new Font(
                                    "Segoe UI",
                                    9F,
                                    FontStyle.Bold)
                        }
                });

            grid.CellFormatting +=
                DgvBranches_CellFormatting;

            return grid;
        }

        // ============================================================
        // GRID FORMATTING
        // ============================================================

        private void DgvBranches_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
            {
                return;
            }

            string columnName =
                dgvBranches
                    .Columns[e.ColumnIndex]
                    .Name;

            if (columnName != "Status")
                return;

            string status =
                e.Value?
                    .ToString()?
                    .Trim()
                ?? string.Empty;

            e.CellStyle.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            e.CellStyle.Alignment =
                DataGridViewContentAlignment
                    .MiddleCenter;

            if (string.Equals(
                status,
                "Active",
                StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.ForeColor =
                    ActiveGreen;

                e.CellStyle.SelectionForeColor =
                    ActiveGreen;
            }
            else
            {
                e.CellStyle.ForeColor =
                    InactiveRed;

                e.CellStyle.SelectionForeColor =
                    InactiveRed;
            }
        }

        // ============================================================
        // BUTTON
        // ============================================================

        private Button CreateButton(
            string text,
            int width)
        {
            Button button =
                new Button
                {
                    Text =
                        text,

                    Width =
                        width,

                    Height =
                        32,

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
                        false
                };

            button.FlatAppearance.BorderSize =
                0;

            button.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(
                    187,
                    145,
                    92);

            button.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(
                    150,
                    112,
                    68);

            return button;
        }

        // ============================================================
        // LOAD BRANCHES
        // ============================================================

        private async Task LoadBranchesAsync()
        {
            try
            {
                SetLoadingState(
                    true);

                List<AdminBranchDto>? result =
                    await _apiService
                        .GetAllAdminBranchesAsync();

                _branches =
                    result ??
                    new List<AdminBranchDto>();

                ApplyFilters();

                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load branch records.\n\n{ex.Message}",
                    "Branch Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(
                    false);
            }
        }

        // ============================================================
        // LOADING STATE
        // ============================================================

        private void SetLoadingState(
            bool loading)
        {
            if (btnRefresh != null)
            {
                btnRefresh.Enabled =
                    !loading;

                btnRefresh.Text =
                    loading
                        ? "Loading..."
                        : "Refresh";
            }

            if (btnAdd != null)
                btnAdd.Enabled =
                    !loading;

            bool hasSelection =
                !loading &&
                dgvBranches != null &&
                dgvBranches.SelectedRows.Count > 0;

            if (btnEdit != null)
                btnEdit.Enabled =
                    hasSelection;

            if (btnAssignManager != null)
                btnAssignManager.Enabled =
                    hasSelection;

            if (btnToggleStatus != null)
                btnToggleStatus.Enabled =
                    hasSelection;
        }

        // ============================================================
        // FILTERS
        // ============================================================

        private void ApplyFilters()
        {
            if (dgvBranches == null)
                return;

            string search =
                txtSearch?.Text?.Trim()
                ?? string.Empty;

            string status =
                cmbStatus?.SelectedItem?
                    .ToString()
                ?? "All Status";

            IEnumerable<AdminBranchDto> filtered =
                _branches;

            if (!string.IsNullOrWhiteSpace(
                search))
            {
                filtered =
                    filtered.Where(x =>
                        (
                            x.BranchName ??
                            string.Empty)
                        .Contains(
                            search,
                            StringComparison
                                .OrdinalIgnoreCase)

                        ||

                        (
                            x.Address ??
                            string.Empty)
                        .Contains(
                            search,
                            StringComparison
                                .OrdinalIgnoreCase)

                        ||

                        (
                            x.ContactNumber ??
                            string.Empty)
                        .Contains(
                            search,
                            StringComparison
                                .OrdinalIgnoreCase)

                        ||

                        (
                            x.ManagerName ??
                            string.Empty)
                        .Contains(
                            search,
                            StringComparison
                                .OrdinalIgnoreCase));
            }

            if (status == "Active")
            {
                filtered =
                    filtered.Where(
                        x => x.IsActive);
            }
            else if (status == "Inactive")
            {
                filtered =
                    filtered.Where(
                        x => !x.IsActive);
            }

            List<BranchGridRow> displayList =
                filtered
                    .Select(
                        x =>
                            new BranchGridRow
                            {
                                Id =
                                    x.Id,

                                BranchName =
                                    string.IsNullOrWhiteSpace(
                                        x.BranchName)
                                        ? "Unnamed Branch"
                                        : x.BranchName,

                                Address =
                                    string.IsNullOrWhiteSpace(
                                        x.Address)
                                        ? "No address provided"
                                        : x.Address,

                                ContactNumber =
                                    string.IsNullOrWhiteSpace(
                                        x.ContactNumber)
                                        ? "—"
                                        : x.ContactNumber,

                                ManagerName =
                                    string.IsNullOrWhiteSpace(
                                        x.ManagerName)
                                        ? "Unassigned"
                                        : x.ManagerName,

                                TotalBeds =
                                    0,

                                OccupiedBeds =
                                    0,

                                AvailableBeds =
                                    0,

                                Status =
                                    x.IsActive
                                        ? "Active"
                                        : "Inactive"
                            })
                    .ToList();

            dgvBranches.DataSource =
                null;

            dgvBranches.DataSource =
                displayList;

            int count =
                displayList.Count;

            lblGridInfo.Text =
                count == 1
                    ? "BRANCH RECORDS  •  1 RECORD"
                    : $"BRANCH RECORDS  •  {count} RECORDS";

            dgvBranches.ClearSelection();

            btnEdit.Enabled =
                false;

            btnAssignManager.Enabled =
                false;

            btnToggleStatus.Enabled =
                false;
        }

        // ============================================================
        // SUMMARY
        // ============================================================

        private void UpdateSummary()
        {
            if (lblTotalBranches == null)
                return;

            int total =
                _branches.Count;

            int active =
                _branches.Count(
                    x => x.IsActive);

            int totalBeds =
                0;

            int occupiedBeds =
                0;

            int availableBeds =
                0;

            lblTotalBranches.Text =
                total.ToString();

            lblActiveBranches.Text =
                active.ToString();

            lblTotalBeds.Text =
                totalBeds.ToString();

            lblOccupiedBeds.Text =
                occupiedBeds.ToString();

            lblAvailableBeds.Text =
                availableBeds.ToString();
        }

        // ============================================================
        // SELECTION
        // ============================================================

        private BranchGridRow? GetSelectedBranch()
        {
            if (dgvBranches.SelectedRows.Count == 0)
                return null;

            return dgvBranches
                .SelectedRows[0]
                .DataBoundItem
                as BranchGridRow;
        }

        // ============================================================
        // GET MANAGERS
        // ============================================================

        private async Task<List<ManagerDto>>
            GetManagersAsync()
        {
            List<ManagerDto>? managers =
                await _apiService
                    .GetBranchManagersAsync();

            if (managers == null)
            {
                MessageBox.Show(
                    "The API did not return a Manager list.\n\n" +
                    $"HTTP Status: {_apiService.LastStatusCode}\n\n" +
                    $"API Message:\n{_apiService.LastErrorMessage}",
                    "Manager Loading",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return new List<ManagerDto>();
            }

            List<ManagerDto> orderedManagers =
                managers
                    .Where(
                        x => x != null)
                    .OrderBy(
                        x => x.DisplayName,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            // ========================================================
            // DIAGNOSTIC
            // ========================================================
            //
            // If the API returns zero Managers, show this clearly.
            // This tells us the problem is NOT the ComboBox.
            // ========================================================

            if (orderedManagers.Count == 0)
            {
                MessageBox.Show(
                    "No Manager accounts were returned by the API.\n\n" +
                    "For a Manager to appear in the branch assignment list, " +
                    "the account must:\n\n" +
                    "• Have the Manager role\n" +
                    "• Belong to the same company\n" +
                    "• Have BranchId = NULL if it is unassigned\n\n" +
                    $"HTTP Status: {_apiService.LastStatusCode}\n\n" +
                    $"API Message:\n{_apiService.LastErrorMessage}",
                    "Manager Loading",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return orderedManagers;
        }

        // ============================================================
        // GET AVAILABLE MANAGERS
        // ============================================================

        private List<ManagerDto>
            GetAvailableManagers(
                List<ManagerDto> managers,
                int branchId,
                bool includeCurrentBranchManager)
        {
            IEnumerable<ManagerDto> available =
                managers.Where(
                    manager =>
                        manager.BranchId == null);

            if (includeCurrentBranchManager)
            {
                ManagerDto? currentManager =
                    managers.FirstOrDefault(
                        manager =>
                            manager.BranchId ==
                            branchId);

                if (currentManager != null)
                {
                    available =
                        available.Append(
                            currentManager);
                }
            }

            return available
                .GroupBy(
                    x => x.Id,
                    StringComparer.OrdinalIgnoreCase)
                .Select(
                    group =>
                        group.First())
                .OrderBy(
                    x => x.DisplayName,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // ============================================================
        // ADD BRANCH
        // ============================================================

        private async Task AddBranchAsync()
        {
            try
            {
                SetLoadingState(
                    true);

                List<ManagerDto> managers =
                    await GetManagersAsync();

                List<ManagerDto> availableManagers =
                    managers
                        .Where(
                            x => x.BranchId == null)
                        .OrderBy(
                            x => x.DisplayName,
                            StringComparer.OrdinalIgnoreCase)
                        .ToList();

                using BranchEditorForm dialog =
                    new BranchEditorForm(
                        availableManagers);

                if (dialog.ShowDialog(this)
                    != DialogResult.OK)
                {
                    return;
                }

                BranchSaveRequest request =
                    new BranchSaveRequest
                    {
                        BranchName =
                            dialog.BranchName,

                        Address =
                            dialog.BranchAddress,

                        ContactNumber =
                            dialog.BranchContactNumber,

                        CreateNewManager =
                            dialog.CreateNewManager,

                        ManagerFullName =
                            dialog.ManagerFullName,

                        ManagerUsername =
                            dialog.ManagerUsername,

                        ManagerEmail =
                            dialog.ManagerEmail,

                        ManagerPassword =
                            dialog.ManagerPassword,

                        ManagerConfirmPassword =
                            dialog.ManagerConfirmPassword,

                        ManagerId =
                            dialog.CreateNewManager
                                ? null
                                : dialog.SelectedManagerId
                    };

                BranchCreateResponse? result =
                    await _apiService
                        .CreateBranchAsync(
                            request);

                if (result == null)
                {
                    MessageBox.Show(
                        "The branch could not be created.\n\n" +
                        $"HTTP Status: {_apiService.LastStatusCode}\n\n" +
                        $"API Message:\n{_apiService.LastErrorMessage}",
                        "Add Branch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                string managerName =
                    string.IsNullOrWhiteSpace(
                        result.ManagerName)
                        ? "Unassigned"
                        : result.ManagerName;

                MessageBox.Show(
                    $"The branch was created successfully.\n\n" +
                    $"Branch: {result.BranchName}\n" +
                    $"Manager: {managerName}",
                    "Branch Created",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await LoadBranchesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to create the branch.\n\n{ex.Message}",
                    "Add Branch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(
                    false);
            }
        }

        // ============================================================
        // EDIT BRANCH
        // ============================================================

        private async Task EditBranchAsync(
            int branchId)
        {
            try
            {
                AdminBranchDto? branch =
                    _branches.FirstOrDefault(
                        x => x.Id == branchId);

                if (branch == null)
                {
                    MessageBox.Show(
                        "The selected branch could not be found.",
                        "Edit Branch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                SetLoadingState(
                    true);

                List<ManagerDto> managers =
                    await GetManagersAsync();

                List<ManagerDto> availableManagers =
                    GetAvailableManagers(
                        managers,
                        branchId,
                        true);

                using BranchEditorForm dialog =
                    new BranchEditorForm(
                        branch,
                        availableManagers);

                if (dialog.ShowDialog(this)
                    != DialogResult.OK)
                {
                    return;
                }

                BranchSaveRequest request =
                    new BranchSaveRequest
                    {
                        BranchName =
                            dialog.BranchName,

                        Address =
                            dialog.BranchAddress,

                        ContactNumber =
                            dialog.BranchContactNumber,

                        CreateNewManager =
                            false,

                        ManagerId =
                            dialog.SelectedManagerId
                    };

                BranchUpdateResponse? result =
                    await _apiService
                        .UpdateBranchAsync(
                            branchId,
                            request);

                if (result == null)
                {
                    MessageBox.Show(
                        "The branch could not be updated.\n\n" +
                        $"HTTP Status: {_apiService.LastStatusCode}\n\n" +
                        $"API Message:\n{_apiService.LastErrorMessage}",
                        "Edit Branch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                MessageBox.Show(
                    "The branch was updated successfully.",
                    "Branch Updated",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await LoadBranchesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to update the branch.\n\n{ex.Message}",
                    "Edit Branch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(
                    false);
            }
        }

        // ============================================================
        // ASSIGN MANAGER
        // ============================================================

        private async Task AssignManagerAsync(
            int branchId)
        {
            try
            {
                AdminBranchDto? branch =
                    _branches.FirstOrDefault(
                        x => x.Id == branchId);

                if (branch == null)
                {
                    MessageBox.Show(
                        "The selected branch could not be found.",
                        "Manager Assignment",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                SetLoadingState(
                    true);

                List<ManagerDto> managers =
                    await GetManagersAsync();

                List<ManagerDto> availableManagers =
                    GetAvailableManagers(
                        managers,
                        branchId,
                        true);

                // ====================================================
                // IMPORTANT
                // ====================================================
                //
                // We intentionally allow the current Manager,
                // unassigned Managers, AND "Unassigned".
                //
                // BranchEditorForm itself contains the "Unassigned"
                // option.
                // ====================================================

                if (availableManagers.Count == 0)
                {
                    MessageBox.Show(
                        "There are no Manager accounts available for assignment.\n\n" +
                        "Make sure the Manager account:\n\n" +
                        "• Has the Manager role\n" +
                        "• Belongs to the same company\n" +
                        "• Is not assigned to another branch\n\n" +
                        "If the Manager was just created, refresh the branch management screen.",
                        "Manager Assignment",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                using BranchEditorForm dialog =
                    new BranchEditorForm(
                        branch,
                        availableManagers);

                if (dialog.ShowDialog(this)
                    != DialogResult.OK)
                {
                    return;
                }

                BranchSaveRequest request =
                    new BranchSaveRequest
                    {
                        BranchName =
                            dialog.BranchName,

                        Address =
                            dialog.BranchAddress,

                        ContactNumber =
                            dialog.BranchContactNumber,

                        CreateNewManager =
                            false,

                        ManagerId =
                            dialog.SelectedManagerId
                    };

                BranchUpdateResponse? result =
                    await _apiService
                        .UpdateBranchAsync(
                            branchId,
                            request);

                if (result == null)
                {
                    MessageBox.Show(
                        "The manager assignment could not be saved.\n\n" +
                        $"HTTP Status: {_apiService.LastStatusCode}\n\n" +
                        $"API Message:\n{_apiService.LastErrorMessage}",
                        "Manager Assignment",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                string message;

                if (string.IsNullOrWhiteSpace(
                    dialog.SelectedManagerId))
                {
                    message =
                        "The branch manager assignment was cleared.";
                }
                else
                {
                    message =
                        "The manager was assigned successfully.";
                }

                MessageBox.Show(
                    message,
                    "Manager Assignment",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await LoadBranchesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to update the manager assignment.\n\n{ex.Message}",
                    "Manager Assignment",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(
                    false);
            }
        }

        // ============================================================
        // TOGGLE STATUS
        // ============================================================

        private async Task ToggleBranchStatusAsync(
            int branchId)
        {
            try
            {
                AdminBranchDto? branch =
                    _branches.FirstOrDefault(
                        x => x.Id == branchId);

                if (branch == null)
                    return;

                if (branch.IsActive)
                {
                    DialogResult confirmation =
                        MessageBox.Show(
                            $"Are you sure you want to deactivate '{branch.BranchName}'?",
                            "Deactivate Branch",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                    if (confirmation !=
                        DialogResult.Yes)
                    {
                        return;
                    }

                    bool success =
                        await _apiService
                            .DeactivateBranchAsync(
                                branchId);

                    if (!success)
                    {
                        MessageBox.Show(
                            "The branch could not be deactivated.\n\n" +
                            $"HTTP Status: {_apiService.LastStatusCode}\n\n" +
                            $"API Message:\n{_apiService.LastErrorMessage}",
                            "Branch Status",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }

                    MessageBox.Show(
                        "The branch was deactivated successfully.",
                        "Branch Status",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    DialogResult confirmation =
                        MessageBox.Show(
                            $"Activate '{branch.BranchName}'?",
                            "Activate Branch",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                    if (confirmation !=
                        DialogResult.Yes)
                    {
                        return;
                    }

                    BranchStatusResponse? result =
                        await _apiService
                            .ActivateBranchAsync(
                                branchId);

                    if (result == null)
                    {
                        MessageBox.Show(
                            "The branch could not be activated.\n\n" +
                            $"HTTP Status: {_apiService.LastStatusCode}\n\n" +
                            $"API Message:\n{_apiService.LastErrorMessage}",
                            "Branch Status",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }

                    MessageBox.Show(
                        "The branch was activated successfully.",
                        "Branch Status",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                await LoadBranchesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to change the branch status.\n\n{ex.Message}",
                    "Branch Status",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ============================================================
        // GRID ROW MODEL
        // ============================================================

        private class BranchGridRow
        {
            public int Id { get; set; }

            public string BranchName { get; set; } =
                string.Empty;

            public string Address { get; set; } =
                string.Empty;

            public string ContactNumber { get; set; } =
                string.Empty;

            public string ManagerName { get; set; } =
                "Unassigned";

            public int TotalBeds { get; set; }

            public int OccupiedBeds { get; set; }

            public int AvailableBeds { get; set; }

            public string Status { get; set; } =
                string.Empty;
        }
    }
}
using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PBCRM2
{
    public class RoomsBedsManagementForm : Form
    {
        private readonly ApiService _apiService;

        private DataGridView dgvRooms = null!;
        private DataGridView dgvBeds = null!;

        private Button btnAddRoom = null!;
        private Button btnAddBed = null!;
        private Button btnPendingRequests = null!;
        private Button btnRefresh = null!;

        private Label lblSelectedRoom = null!;

        private int? selectedRoomId;

        private ToolTip actionToolTip = null!;

        private int hoveredRoomAction = -1;
        private int hoveredBedAction = -1;

        // =========================================================
        // ROOM RULES
        // =========================================================

        private const string SharedRoomType = "Shared";
        private const string PrivateRoomType = "Private";

        private const int SharedRoomCapacity = 4;
        private const int PrivateRoomCapacity = 1;

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

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public RoomsBedsManagementForm(ApiService apiService)
        {
            _apiService = apiService;

            actionToolTip = new ToolTip
            {
                InitialDelay = 300,
                ReshowDelay = 100,
                AutoPopDelay = 3000,
                ShowAlways = true
            };

            Text = "Rooms & Beds";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1250, 780);
            MinimumSize = new Size(1050, 680);
            BackColor = PanelBg;

            BuildInterface();

            Shown += async (_, _) => await LoadRooms();
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
                Height = 120,
                BackColor = BrandBg
            };

            var title = new Label
            {
                Text = "ROOMS & BEDS",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 22)
            };

            var subtitle = new Label
            {
                Text = "Manage rooms, beds, and tenant bed assignments",
                ForeColor = BrandAccent,
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(28, 67)
            };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            // =====================================================
            // HEADER GAP
            // =====================================================

            var headerGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = PanelBg
            };

            // =====================================================
            // ACTION TOOLBAR
            // =====================================================

            var actionPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = PanelBg
            };

            btnAddRoom = CreateButton("Add Room", 105);
            btnAddRoom.Location = new Point(20, 10);
            btnAddRoom.Click += async (_, _) =>
                await ShowAddRoomDialogAsync();

            btnAddBed = CreateButton("Add Bed", 100);
            btnAddBed.Location = new Point(135, 10);
            btnAddBed.Click += async (_, _) =>
                await ShowAddBedDialogAsync();

            btnPendingRequests = CreateButton("Pending Requests", 145);
            btnPendingRequests.Location = new Point(245, 10);
            btnPendingRequests.Click += async (_, _) =>
                await ShowPendingRequests();

            btnRefresh = CreateButton("Refresh", 95);
            btnRefresh.Location = new Point(400, 10);
            btnRefresh.Click += async (_, _) =>
                await RefreshRooms();

            actionPanel.Controls.Add(btnAddRoom);
            actionPanel.Controls.Add(btnAddBed);
            actionPanel.Controls.Add(btnPendingRequests);
            actionPanel.Controls.Add(btnRefresh);

            // =====================================================
            // TOOLBAR GAP
            // =====================================================

            var toolbarGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 20,
                BackColor = PanelBg
            };

            // =====================================================
            // CONTENT
            // =====================================================

            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg,
                Padding = new Padding(20, 0, 20, 20)
            };

            // =====================================================
            // ROOMS LABEL
            // =====================================================

            var roomLabel = new Label
            {
                Text = "ROOMS",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = BrandBg,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // =====================================================
            // ROOMS GRID
            // =====================================================

            dgvRooms = CreateGrid();
            dgvRooms.Dock = DockStyle.Top;
            dgvRooms.Height = 190;

            dgvRooms.SelectionChanged += async (_, _) =>
            {
                await RoomSelectionChanged();
            };

            dgvRooms.CellMouseClick += DgvRooms_CellMouseClick;
            dgvRooms.CellPainting += DgvRooms_CellPainting;
            dgvRooms.CellToolTipTextNeeded +=
                DgvRooms_CellToolTipTextNeeded;
            dgvRooms.MouseMove += DgvRooms_MouseMove;
            dgvRooms.MouseLeave += DgvRooms_MouseLeave;

            // =====================================================
            // BEDS GAP
            // =====================================================

            var bedsGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 25,
                BackColor = PanelBg
            };

            // =====================================================
            // BEDS LABEL
            // =====================================================

            lblSelectedRoom = new Label
            {
                Text = "BEDS",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = BrandBg,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // =====================================================
            // BEDS GRID
            // =====================================================

            dgvBeds = CreateGrid();
            dgvBeds.Dock = DockStyle.Fill;

            dgvBeds.CellMouseClick += DgvBeds_CellMouseClick;
            dgvBeds.CellPainting += DgvBeds_CellPainting;
            dgvBeds.CellToolTipTextNeeded +=
                DgvBeds_CellToolTipTextNeeded;
            dgvBeds.MouseMove += DgvBeds_MouseMove;
            dgvBeds.MouseLeave += DgvBeds_MouseLeave;

            // =====================================================
            // ADD CONTENT CONTROLS
            // =====================================================

            content.Controls.Add(dgvBeds);
            content.Controls.Add(lblSelectedRoom);
            content.Controls.Add(bedsGap);
            content.Controls.Add(dgvRooms);
            content.Controls.Add(roomLabel);

            // =====================================================
            // ADD MAIN CONTROLS
            // =====================================================

            main.Controls.Add(content);
            main.Controls.Add(toolbarGap);
            main.Controls.Add(actionPanel);
            main.Controls.Add(headerGap);
            main.Controls.Add(header);

            Controls.Add(main);
        }

        // =========================================================
        // CREATE GRID
        // =========================================================

        private DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 34,
                ShowCellToolTips = false
            };

            grid.RowTemplate.Height = 30;

            typeof(DataGridView)
                .GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(grid, true, null);

            grid.ColumnHeadersDefaultCellStyle =
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
                    WrapMode = DataGridViewTriState.False
                };

            grid.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 8.5F),
                    BackColor = Color.White,
                    ForeColor = BrandBg,
                    SelectionBackColor =
                        Color.FromArgb(232, 220, 199),
                    SelectionForeColor = BrandBg
                };

            grid.AlternatingRowsDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(247, 244, 239)
                };

            return grid;
        }

        // =========================================================
        // CREATE BUTTON
        // =========================================================

        private Button CreateButton(string text, int width)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = CtaColor,
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;

            return button;
        }

        // =========================================================
        // LOAD ROOMS
        // =========================================================

        private async Task LoadRooms()
        {
            try
            {
                int? previousRoomId = selectedRoomId;

                var rooms =
                    await _apiService.GetAsync<List<RoomDto>>(
                        "api/Rooms");

                if (rooms == null)
                    return;

                dgvRooms.DataSource = null;
                dgvRooms.DataSource = rooms;

                ConfigureRoomColumns();
                AddRoomActionColumn();

                if (previousRoomId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvRooms.Rows)
                    {
                        if (row.DataBoundItem is RoomDto room &&
                            room.Id == previousRoomId.Value)
                        {
                            selectedRoomId = room.Id;

                            dgvRooms.ClearSelection();
                            row.Selected = true;

                            DataGridViewColumn? visibleColumn = null;

                            foreach (
                                DataGridViewColumn column
                                in dgvRooms.Columns)
                            {
                                if (column.Visible)
                                {
                                    visibleColumn = column;
                                    break;
                                }
                            }

                            if (visibleColumn != null)
                            {
                                dgvRooms.CurrentCell =
                                    row.Cells[
                                        visibleColumn.Index];
                            }

                            lblSelectedRoom.Text =
                                $"BEDS — ROOM {room.RoomNumber}";

                            await LoadBeds(room.Id);

                            return;
                        }
                    }
                }

                selectedRoomId = null;
                dgvBeds.DataSource = null;
                lblSelectedRoom.Text = "BEDS";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load rooms.\n\n{ex.Message}",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // REFRESH
        // =========================================================

        private async Task RefreshRooms()
        {
            await LoadRooms();
        }

        // =========================================================
        // ROOM SELECTION
        // =========================================================

        private async Task RoomSelectionChanged()
        {
            if (dgvRooms.CurrentRow?.DataBoundItem
                is not RoomDto room)
            {
                return;
            }

            if (selectedRoomId.HasValue &&
                selectedRoomId.Value == room.Id)
            {
                return;
            }

            selectedRoomId = room.Id;

            lblSelectedRoom.Text =
                $"BEDS — ROOM {room.RoomNumber}";

            await LoadBeds(room.Id);
        }

        // =========================================================
        // LOAD BEDS
        // =========================================================

        private async Task LoadBeds(int roomId)
        {
            try
            {
                var beds =
                    await _apiService.GetAsync<List<BedDto>>(
                        $"api/Rooms/{roomId}/beds");

                if (beds == null)
                    return;

                if (selectedRoomId.HasValue &&
                    selectedRoomId.Value != roomId)
                {
                    return;
                }

                dgvBeds.DataSource = null;
                dgvBeds.DataSource = beds;

                ConfigureBedColumns();
                AddBedActionColumn();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load beds.\n\n{ex.Message}",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // ROOM COLUMNS
        // =========================================================

        private void ConfigureRoomColumns()
        {
            HideColumn(dgvRooms, "Id");
            HideColumn(dgvRooms, "BranchId");
            HideColumn(dgvRooms, "BranchName");
            HideColumn(dgvRooms, "IsActive");

            RenameColumn(dgvRooms, "RoomNumber", "Room");
            RenameColumn(dgvRooms, "RoomType", "Type");
            RenameColumn(dgvRooms, "Capacity", "Capacity");
            RenameColumn(dgvRooms, "Status", "Status");
            RenameColumn(dgvRooms, "BedCount", "Beds");
            RenameColumn(dgvRooms, "OccupiedBeds", "Occupied");
            RenameColumn(dgvRooms, "AvailableBeds", "Available");
            RenameColumn(
                dgvRooms,
                "MaintenanceBeds",
                "Maintenance");
            RenameColumn(
                dgvRooms,
                "PendingAssignments",
                "Pending");
        }

        // =========================================================
        // BED COLUMNS
        // =========================================================

        private void ConfigureBedColumns()
        {
            HideColumn(dgvBeds, "Id");
            HideColumn(dgvBeds, "RoomId");
            HideColumn(dgvBeds, "TenantId");
            HideColumn(dgvBeds, "PendingAssignment");

            RenameColumn(dgvBeds, "BedNumber", "Bed");
            RenameColumn(dgvBeds, "Status", "Status");
            RenameColumn(dgvBeds, "TenantName", "Tenant");
        }

        // =========================================================
        // ADD ROOM ACTION COLUMN
        // =========================================================

        private void AddRoomActionColumn()
        {
            if (dgvRooms.Columns.Contains("RoomActions"))
            {
                MoveRoomActionColumnToRight();
                return;
            }

            var actionColumn =
                new DataGridViewTextBoxColumn
                {
                    Name = "RoomActions",
                    HeaderText = "Actions",
                    Width = 85,
                    MinimumWidth = 85,
                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.None,
                    ReadOnly = true,
                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                };

            actionColumn.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    Alignment =
                        DataGridViewContentAlignment.MiddleCenter,
                    Font =
                        new Font("Segoe UI Symbol", 11F),
                    ForeColor = BrandBg,
                    BackColor = Color.White,
                    SelectionBackColor =
                        Color.FromArgb(232, 220, 199),
                    SelectionForeColor = BrandBg
                };

            dgvRooms.Columns.Add(actionColumn);
            MoveRoomActionColumnToRight();
        }

        // =========================================================
        // ADD BED ACTION COLUMN
        // =========================================================

        private void AddBedActionColumn()
        {
            if (dgvBeds.Columns.Contains("BedActions"))
            {
                MoveBedActionColumnToRight();
                return;
            }

            var actionColumn =
                new DataGridViewTextBoxColumn
                {
                    Name = "BedActions",
                    HeaderText = "Actions",
                    Width = 110,
                    MinimumWidth = 110,
                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.None,
                    ReadOnly = true,
                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                };

            actionColumn.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    Alignment =
                        DataGridViewContentAlignment.MiddleCenter,
                    Font =
                        new Font("Segoe UI Symbol", 11F),
                    ForeColor = BrandBg,
                    BackColor = Color.White,
                    SelectionBackColor =
                        Color.FromArgb(232, 220, 199),
                    SelectionForeColor = BrandBg
                };

            dgvBeds.Columns.Add(actionColumn);
            MoveBedActionColumnToRight();
        }

        // =========================================================
        // MOVE ROOM ACTION COLUMN TO RIGHT
        // =========================================================

        private void MoveRoomActionColumnToRight()
        {
            if (dgvRooms.Columns.Contains("RoomActions"))
            {
                dgvRooms.Columns["RoomActions"].DisplayIndex =
                    dgvRooms.Columns.Count - 1;
            }
        }

        // =========================================================
        // MOVE BED ACTION COLUMN TO RIGHT
        // =========================================================

        private void MoveBedActionColumnToRight()
        {
            if (dgvBeds.Columns.Contains("BedActions"))
            {
                dgvBeds.Columns["BedActions"].DisplayIndex =
                    dgvBeds.Columns.Count - 1;
            }
        }

        // =========================================================
        // ROOM ACTION PAINT
        // =========================================================

        private void DgvRooms_CellPainting(
            object? sender,
            DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            if (dgvRooms.Columns[e.ColumnIndex].Name !=
                "RoomActions")
                return;

            e.PaintBackground(e.CellBounds, true);
            e.Paint(
                e.CellBounds,
                DataGridViewPaintParts.Border);

            using var font =
                new Font(
                    "Segoe UI Symbol",
                    11F,
                    FontStyle.Regular);

            using var brush =
                new SolidBrush(BrandBg);

            string text = "✎     ×";

            var textSize =
                e.Graphics.MeasureString(text, font);

            float x =
                e.CellBounds.X +
                (e.CellBounds.Width - textSize.Width) / 2F;

            float y =
                e.CellBounds.Y +
                (e.CellBounds.Height - textSize.Height) / 2F;

            e.Graphics.DrawString(
                text,
                font,
                brush,
                x,
                y);

            e.Handled = true;
        }

        // =========================================================
        // BED ACTION PAINT
        // =========================================================

        private void DgvBeds_CellPainting(
            object? sender,
            DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            if (dgvBeds.Columns[e.ColumnIndex].Name !=
                "BedActions")
                return;

            e.PaintBackground(e.CellBounds, true);
            e.Paint(
                e.CellBounds,
                DataGridViewPaintParts.Border);

            using var font =
                new Font(
                    "Segoe UI Symbol",
                    11F,
                    FontStyle.Regular);

            using var brush =
                new SolidBrush(BrandBg);

            string text = "✎    ↻    ×";

            var textSize =
                e.Graphics.MeasureString(text, font);

            float x =
                e.CellBounds.X +
                (e.CellBounds.Width - textSize.Width) / 2F;

            float y =
                e.CellBounds.Y +
                (e.CellBounds.Height - textSize.Height) / 2F;

            e.Graphics.DrawString(
                text,
                font,
                brush,
                x,
                y);

            e.Handled = true;
        }

        // =========================================================
        // ROOM MOUSE MOVE
        // =========================================================

        private void DgvRooms_MouseMove(
            object? sender,
            MouseEventArgs e)
        {
            var hit = dgvRooms.HitTest(e.X, e.Y);

            if (hit.RowIndex < 0 ||
                hit.ColumnIndex < 0)
            {
                hoveredRoomAction = -1;
                HideActionToolTip();
                return;
            }

            if (dgvRooms.Columns[hit.ColumnIndex].Name !=
                "RoomActions")
            {
                hoveredRoomAction = -1;
                HideActionToolTip();
                return;
            }

            Rectangle cell =
                dgvRooms.GetCellDisplayRectangle(
                    hit.ColumnIndex,
                    hit.RowIndex,
                    false);

            int action =
                GetRoomActionFromPosition(
                    e.X - cell.X,
                    cell.Width);

            if (action == hoveredRoomAction)
                return;

            hoveredRoomAction = action;

            if (action == 0)
            {
                ShowActionToolTip(
                    dgvRooms,
                    cell,
                    "Edit Room");
            }
            else if (action == 1)
            {
                ShowActionToolTip(
                    dgvRooms,
                    cell,
                    "Remove Room");
            }
            else
            {
                HideActionToolTip();
            }
        }

        // =========================================================
        // ROOM MOUSE LEAVE
        // =========================================================

        private void DgvRooms_MouseLeave(
            object? sender,
            EventArgs e)
        {
            hoveredRoomAction = -1;
            HideActionToolTip();
        }

        // =========================================================
        // GET ROOM ACTION
        // =========================================================

        private int GetRoomActionFromPosition(
            int x,
            int width)
        {
            int middle = width / 2;

            if (x < middle)
                return 0;

            return 1;
        }

        // =========================================================
        // BED MOUSE MOVE
        // =========================================================

        private void DgvBeds_MouseMove(
            object? sender,
            MouseEventArgs e)
        {
            var hit = dgvBeds.HitTest(e.X, e.Y);

            if (hit.RowIndex < 0 ||
                hit.ColumnIndex < 0)
            {
                hoveredBedAction = -1;
                HideActionToolTip();
                return;
            }

            if (dgvBeds.Columns[hit.ColumnIndex].Name !=
                "BedActions")
            {
                hoveredBedAction = -1;
                HideActionToolTip();
                return;
            }

            Rectangle cell =
                dgvBeds.GetCellDisplayRectangle(
                    hit.ColumnIndex,
                    hit.RowIndex,
                    false);

            int action =
                GetBedActionFromPosition(
                    e.X - cell.X,
                    cell.Width);

            if (action == hoveredBedAction)
                return;

            hoveredBedAction = action;

            if (action == 0)
            {
                ShowActionToolTip(
                    dgvBeds,
                    cell,
                    "Edit Bed");
            }
            else if (action == 1)
            {
                ShowActionToolTip(
                    dgvBeds,
                    cell,
                    "Release Bed");
            }
            else if (action == 2)
            {
                ShowActionToolTip(
                    dgvBeds,
                    cell,
                    "Remove Bed");
            }
            else
            {
                HideActionToolTip();
            }
        }

        // =========================================================
        // BED MOUSE LEAVE
        // =========================================================

        private void DgvBeds_MouseLeave(
            object? sender,
            EventArgs e)
        {
            hoveredBedAction = -1;
            HideActionToolTip();
        }

        // =========================================================
        // GET BED ACTION
        // =========================================================

        private int GetBedActionFromPosition(
            int x,
            int width)
        {
            int third = width / 3;

            if (x < third)
                return 0;

            if (x < third * 2)
                return 1;

            return 2;
        }

        // =========================================================
        // SHOW ACTION TOOLTIP
        // =========================================================

        private void ShowActionToolTip(
            DataGridView grid,
            Rectangle cell,
            string text)
        {
            if (actionToolTip == null)
                return;

            actionToolTip.Hide(dgvRooms);
            actionToolTip.Hide(dgvBeds);

            Point location =
                new Point(
                    cell.X,
                    cell.Bottom + 2);

            actionToolTip.Show(
                text,
                grid,
                location,
                1500);
        }

        // =========================================================
        // HIDE ACTION TOOLTIP
        // =========================================================

        private void HideActionToolTip()
        {
            if (actionToolTip == null)
                return;

            actionToolTip.Hide(dgvRooms);
            actionToolTip.Hide(dgvBeds);
        }

        // =========================================================
        // CELL TOOLTIP - ROOM
        // =========================================================

        private void DgvRooms_CellToolTipTextNeeded(
            object? sender,
            DataGridViewCellToolTipTextNeededEventArgs e)
        {
            // Individual action tooltips are handled through MouseMove.
        }

        // =========================================================
        // CELL TOOLTIP - BED
        // =========================================================

        private void DgvBeds_CellToolTipTextNeeded(
            object? sender,
            DataGridViewCellToolTipTextNeededEventArgs e)
        {
            // Individual action tooltips are handled through MouseMove.
        }

        // =========================================================
        // ROOM ACTION CLICK
        // =========================================================

        private async void DgvRooms_CellMouseClick(
            object? sender,
            DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            if (dgvRooms.Columns[e.ColumnIndex].Name !=
                "RoomActions")
                return;

            if (dgvRooms.Rows[e.RowIndex].DataBoundItem
                is not RoomDto room)
                return;

            Rectangle cell =
                dgvRooms.GetCellDisplayRectangle(
                    e.ColumnIndex,
                    e.RowIndex,
                    false);

            int action =
                GetRoomActionFromPosition(
                    e.X,
                    cell.Width);

            if (action == 0)
            {
                await EditRoom(room);
            }
            else
            {
                await RemoveRoom(room);
            }
        }

        // =========================================================
        // BED ACTION CLICK
        // =========================================================

        private async void DgvBeds_CellMouseClick(
            object? sender,
            DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            if (dgvBeds.Columns[e.ColumnIndex].Name !=
                "BedActions")
                return;

            if (dgvBeds.Rows[e.RowIndex].DataBoundItem
                is not BedDto bed)
                return;

            Rectangle cell =
                dgvBeds.GetCellDisplayRectangle(
                    e.ColumnIndex,
                    e.RowIndex,
                    false);

            int action =
                GetBedActionFromPosition(
                    e.X,
                    cell.Width);

            if (action == 0)
            {
                await EditBed(bed);
            }
            else if (action == 1)
            {
                await ReleaseBed(bed);
            }
            else
            {
                await RemoveBed(bed);
            }
        }

        // =========================================================
        // EDIT ROOM
        // =========================================================

        private async Task EditRoom(RoomDto room)
        {
            using var dialog = new Form
            {
                Text = "Edit Room",
                Size = new Size(420, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = PanelBg
            };

            var txtRoom = new TextBox
            {
                Location = new Point(145, 35),
                Width = 220,
                Text = room.RoomNumber
            };

            var cmbType = new ComboBox
            {
                Location = new Point(145, 80),
                Width = 220,
                DropDownStyle =
                    ComboBoxStyle.DropDownList
            };

            cmbType.Items.Add(SharedRoomType);
            cmbType.Items.Add(PrivateRoomType);

            if (room.RoomType.Equals(
                    PrivateRoomType,
                    StringComparison.OrdinalIgnoreCase))
            {
                cmbType.SelectedItem = PrivateRoomType;
            }
            else
            {
                cmbType.SelectedItem = SharedRoomType;
            }

            // =====================================================
            // CAPACITY DISPLAY
            // =====================================================

            var lblCapacityValue = new Label
            {
                Location = new Point(145, 125),
                Width = 220,
                Height = 28,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold),
                ForeColor = BrandBg,
                TextAlign = ContentAlignment.MiddleLeft
            };

            UpdateCapacityLabel(
                lblCapacityValue,
                cmbType.SelectedItem?.ToString());

            dialog.Controls.Add(
                CreateDialogLabel("Room Number", 35));

            dialog.Controls.Add(
                CreateDialogLabel("Room Type", 80));

            dialog.Controls.Add(
                CreateDialogLabel("Capacity", 125));

            dialog.Controls.Add(txtRoom);
            dialog.Controls.Add(cmbType);
            dialog.Controls.Add(lblCapacityValue);

            // =====================================================
            // ROOM TYPE CHANGED
            // =====================================================

            cmbType.SelectedIndexChanged += (_, _) =>
            {
                UpdateCapacityLabel(
                    lblCapacityValue,
                    cmbType.SelectedItem?.ToString());
            };

            // =====================================================
            // SAVE
            // =====================================================

            var save = CreateButton("Save", 100);
            save.Location = new Point(145, 180);

            save.Click += async (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtRoom.Text))
                {
                    MessageBox.Show(
                        "Room number is required.",
                        "PBCRM2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtRoom.Focus();
                    return;
                }

                string roomType =
                    cmbType.SelectedItem?.ToString()
                    ?? SharedRoomType;

                if (!IsValidRoomType(roomType))
                {
                    MessageBox.Show(
                        "Please select either Shared or Private.",
                        "PBCRM2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                int capacity =
                    GetRoomCapacity(roomType);

                // =================================================
                // EXISTING BED VALIDATION
                // =================================================

                if (room.BedCount > capacity)
                {
                    MessageBox.Show(
                        $"This room currently has {room.BedCount} bed(s).\n\n" +
                        $"{roomType} rooms can only have {capacity} bed(s).\n\n" +
                        "Remove the extra beds before changing the room type.",
                        "Cannot Change Room Type",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (room.OccupiedBeds > capacity)
                {
                    MessageBox.Show(
                        "The room has more occupied beds than the selected room type allows.",
                        "Cannot Change Room Type",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                try
                {
                    await _apiService.PutAsync<object>(
                        $"api/Rooms/{room.Id}",
                        new
                        {
                            roomNumber = txtRoom.Text.Trim(),
                            roomType = roomType,
                            capacity = capacity,
                            isActive = room.IsActive
                        });

                    MessageBox.Show(
                        "Room updated successfully.",
                        "PBCRM2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Unable to update the room.\n\n{ex.Message}",
                        "PBCRM2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            };

            dialog.Controls.Add(save);
            dialog.AcceptButton = save;

            dialog.ShowDialog(this);

            if (dialog.DialogResult == DialogResult.OK)
            {
                selectedRoomId = room.Id;
                await LoadRooms();
            }
        }

        // =========================================================
        // REMOVE ROOM
        // =========================================================

        private async Task RemoveRoom(RoomDto room)
        {
            if (room.BedCount > 0)
            {
                MessageBox.Show(
                    "This room still has beds.\n\n" +
                    "Remove the beds before deleting the room.",
                    "Remove Room",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var confirm =
                MessageBox.Show(
                    $"Remove Room {room.RoomNumber}?\n\n" +
                    "This action cannot be undone.",
                    "Remove Room",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                await _apiService.DeleteAsync(
                    $"api/Rooms/{room.Id}");

                MessageBox.Show(
                    "Room removed successfully.",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                selectedRoomId = null;
                dgvBeds.DataSource = null;
                lblSelectedRoom.Text = "BEDS";

                await LoadRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to remove the room.\n\n{ex.Message}",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // EDIT BED
        // =========================================================

        private async Task EditBed(BedDto bed)
        {
            string? bedNumber =
                Prompt(
                    "Edit Bed",
                    "Bed Number:",
                    bed.BedNumber);

            if (string.IsNullOrWhiteSpace(bedNumber))
                return;

            try
            {
                await _apiService.PutAsync<object>(
                    $"api/Rooms/beds/{bed.Id}",
                    new
                    {
                        bedNumber = bedNumber.Trim()
                    });

                MessageBox.Show(
                    "Bed updated successfully.",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                selectedRoomId = bed.RoomId;

                await LoadRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to update the bed.\n\n{ex.Message}",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // REMOVE BED
        // =========================================================

        private async Task RemoveBed(BedDto bed)
        {
            if (bed.TenantId.HasValue)
            {
                MessageBox.Show(
                    "This bed cannot be removed while a tenant " +
                    "is assigned to it.\n\nRelease the bed first.",
                    "Remove Bed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var confirm =
                MessageBox.Show(
                    $"Remove Bed {bed.BedNumber}?",
                    "Remove Bed",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                await _apiService.DeleteAsync(
                    $"api/Rooms/beds/{bed.Id}");

                MessageBox.Show(
                    "Bed removed successfully.",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                selectedRoomId = bed.RoomId;

                await LoadRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to remove the bed.\n\n{ex.Message}",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // RELEASE BED
        // =========================================================

        private async Task ReleaseBed(BedDto bed)
        {
            if (!bed.TenantId.HasValue)
            {
                MessageBox.Show(
                    "This bed is not currently assigned.",
                    "Release Bed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string tenantName =
                string.IsNullOrWhiteSpace(bed.TenantName)
                    ? "this tenant"
                    : bed.TenantName;

            var confirm =
                MessageBox.Show(
                    $"Release Bed {bed.BedNumber} from {tenantName}?",
                    "Release Bed",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                await _apiService.PostAsync<object>(
                    $"api/Rooms/beds/{bed.Id}/release",
                    new { });

                MessageBox.Show(
                    "Bed released successfully.",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                selectedRoomId = bed.RoomId;

                await LoadRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to release the bed.\n\n{ex.Message}",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // ADD ROOM
        // =========================================================

        private async Task ShowAddRoomDialogAsync()
        {
            using var dialog = new Form
            {
                Text = "Add Room",
                Size = new Size(420, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = PanelBg
            };

            var txtRoom = new TextBox
            {
                Location = new Point(145, 35),
                Width = 220
            };

            var cmbType = new ComboBox
            {
                Location = new Point(145, 80),
                Width = 220,
                DropDownStyle =
                    ComboBoxStyle.DropDownList
            };

            cmbType.Items.Add(SharedRoomType);
            cmbType.Items.Add(PrivateRoomType);

            cmbType.SelectedItem = SharedRoomType;

            // =====================================================
            // CAPACITY DISPLAY
            // =====================================================

            var lblCapacityValue = new Label
            {
                Location = new Point(145, 125),
                Width = 220,
                Height = 28,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold),
                ForeColor = BrandBg,
                TextAlign = ContentAlignment.MiddleLeft
            };

            UpdateCapacityLabel(
                lblCapacityValue,
                SharedRoomType);

            dialog.Controls.Add(
                CreateDialogLabel("Room Number", 35));

            dialog.Controls.Add(
                CreateDialogLabel("Room Type", 80));

            dialog.Controls.Add(
                CreateDialogLabel("Capacity", 125));

            dialog.Controls.Add(txtRoom);
            dialog.Controls.Add(cmbType);
            dialog.Controls.Add(lblCapacityValue);

            // =====================================================
            // ROOM TYPE CHANGED
            // =====================================================

            cmbType.SelectedIndexChanged += (_, _) =>
            {
                UpdateCapacityLabel(
                    lblCapacityValue,
                    cmbType.SelectedItem?.ToString());
            };

            // =====================================================
            // SAVE
            // =====================================================

            var save = CreateButton("Save", 100);
            save.Location = new Point(145, 180);

            save.Click += async (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtRoom.Text))
                {
                    MessageBox.Show(
                        "Room number is required.",
                        "PBCRM2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtRoom.Focus();
                    return;
                }

                string roomType =
                    cmbType.SelectedItem?.ToString()
                    ?? SharedRoomType;

                if (!IsValidRoomType(roomType))
                {
                    MessageBox.Show(
                        "Please select either Shared or Private.",
                        "PBCRM2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                int capacity =
                    GetRoomCapacity(roomType);

                try
                {
                    await _apiService.PostAsync<object>(
                        "api/Rooms",
                        new
                        {
                            roomNumber = txtRoom.Text.Trim(),
                            roomType = roomType,
                            capacity = capacity,
                            isActive = true
                        });

                    MessageBox.Show(
                        "Room created successfully.",
                        "PBCRM2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Unable to create the room.\n\n{ex.Message}",
                        "PBCRM2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            };

            dialog.Controls.Add(save);
            dialog.AcceptButton = save;

            dialog.ShowDialog(this);

            if (dialog.DialogResult == DialogResult.OK)
            {
                await LoadRooms();
            }
        }

        // =========================================================
        // ADD BED
        // =========================================================

        private async Task ShowAddBedDialogAsync()
        {
            if (!selectedRoomId.HasValue)
            {
                MessageBox.Show(
                    "Select a room first.",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string? bedNumber =
                Prompt(
                    "Add Bed",
                    "Bed Number:");

            if (string.IsNullOrWhiteSpace(bedNumber))
                return;

            int roomId = selectedRoomId.Value;

            try
            {
                await _apiService.PostAsync<object>(
                    $"api/Rooms/{roomId}/beds",
                    new
                    {
                        bedNumber = bedNumber.Trim()
                    });

                MessageBox.Show(
                    "Bed added successfully.",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                selectedRoomId = roomId;

                await LoadRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to add the bed.\n\n{ex.Message}",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // PENDING REQUESTS
        // =========================================================

        private async Task ShowPendingRequests()
        {
            try
            {
                var requests =
                    await _apiService.GetAsync<
                        List<AssignmentRequestDto>>(
                        "api/Rooms/assignment-requests");

                if (requests == null)
                    return;

                if (requests.Count == 0)
                {
                    MessageBox.Show(
                        "There are no pending bed assignment requests.",
                        "Pending Requests",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                using var dialog = new Form
                {
                    Text =
                        "Pending Bed Assignment Requests",
                    Size = new Size(900, 500),
                    StartPosition =
                        FormStartPosition.CenterParent,
                    BackColor = PanelBg,
                    FormBorderStyle =
                        FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                // =================================================
                // PENDING REQUEST GRID
                // =================================================

                var grid = new DataGridView
                {
                    Location = new Point(10, 10),
                    Size = new Size(860, 340),
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Bottom |
                        AnchorStyles.Left |
                        AnchorStyles.Right,
                    BackgroundColor = Color.White,
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
                    EnableHeadersVisualStyles = false,
                    ColumnHeadersHeight = 34,
                    RowTemplate = { Height = 30 },
                    ShowCellToolTips = false
                };

                // =================================================
                // GRID STYLE
                // =================================================

                grid.ColumnHeadersDefaultCellStyle =
                    new DataGridViewCellStyle
                    {
                        BackColor = BrandBg,
                        ForeColor = Color.White,
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
                        BackColor = Color.White,
                        ForeColor = BrandBg,
                        SelectionBackColor =
                            Color.FromArgb(232, 220, 199),
                        SelectionForeColor = BrandBg
                    };

                grid.AlternatingRowsDefaultCellStyle =
                    new DataGridViewCellStyle
                    {
                        BackColor =
                            Color.FromArgb(247, 244, 239)
                    };

                // =================================================
                // TENANT COLUMN
                // =================================================

                var tenantColumn =
                    new DataGridViewTextBoxColumn
                    {
                        Name = "Tenant",
                        HeaderText = "Tenant",
                        AutoSizeMode =
                            DataGridViewAutoSizeColumnMode.Fill,
                        FillWeight = 30,
                        ReadOnly = true,
                        SortMode =
                            DataGridViewColumnSortMode.NotSortable
                    };

                // =================================================
                // ROOM COLUMN
                // =================================================

                var roomColumn =
                    new DataGridViewTextBoxColumn
                    {
                        Name = "RoomNumber",
                        HeaderText = "Room No.",
                        AutoSizeMode =
                            DataGridViewAutoSizeColumnMode.Fill,
                        FillWeight = 15,
                        ReadOnly = true,
                        SortMode =
                            DataGridViewColumnSortMode.NotSortable
                    };

                // =================================================
                // BED COLUMN
                // =================================================

                var bedColumn =
                    new DataGridViewTextBoxColumn
                    {
                        Name = "BedNumber",
                        HeaderText = "Bed No.",
                        AutoSizeMode =
                            DataGridViewAutoSizeColumnMode.Fill,
                        FillWeight = 15,
                        ReadOnly = true,
                        SortMode =
                            DataGridViewColumnSortMode.NotSortable
                    };

                // =================================================
                // STATUS COLUMN
                // =================================================

                var statusColumn =
                    new DataGridViewTextBoxColumn
                    {
                        Name = "Status",
                        HeaderText = "Status",
                        AutoSizeMode =
                            DataGridViewAutoSizeColumnMode.Fill,
                        FillWeight = 15,
                        ReadOnly = true,
                        SortMode =
                            DataGridViewColumnSortMode.NotSortable
                    };

                // =================================================
                // REQUEST DATE COLUMN
                // =================================================

                var requestDateColumn =
                    new DataGridViewTextBoxColumn
                    {
                        Name = "RequestDate",
                        HeaderText = "Request Date",
                        AutoSizeMode =
                            DataGridViewAutoSizeColumnMode.Fill,
                        FillWeight = 25,
                        ReadOnly = true,
                        SortMode =
                            DataGridViewColumnSortMode.NotSortable,
                        DefaultCellStyle =
                            new DataGridViewCellStyle
                            {
                                Format =
                                    "MMM dd, yyyy hh:mm tt"
                            }
                    };

                // =================================================
                // ADD ONLY FIVE DISPLAY COLUMNS
                // =================================================

                grid.Columns.Add(tenantColumn);
                grid.Columns.Add(roomColumn);
                grid.Columns.Add(bedColumn);
                grid.Columns.Add(statusColumn);
                grid.Columns.Add(requestDateColumn);

                // =================================================
                // ADD REQUEST DATA
                // =================================================

                foreach (
                    AssignmentRequestDto request
                    in requests)
                {
                    grid.Rows.Add(
                        request.TenantName,
                        request.RoomNumber,
                        request.BedNumber,
                        request.Status,
                        request.RequestedAt);
                }

                dialog.Controls.Add(grid);

                // =================================================
                // APPROVE BUTTON
                // =================================================

                var approve =
                    CreateButton("Approve", 110);

                approve.Location =
                    new Point(20, 365);

                approve.Click += async (_, _) =>
                {
                    int rowIndex =
                        grid.CurrentRow?.Index ?? -1;

                    if (rowIndex < 0 ||
                        rowIndex >= requests.Count)
                    {
                        MessageBox.Show(
                            "Select a pending request first.",
                            "PBCRM2",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }

                    AssignmentRequestDto item =
                        requests[rowIndex];

                    try
                    {
                        await _apiService.PostAsync<object>(
                            $"api/Rooms/assignment-requests/{item.Id}/approve",
                            new { });

                        MessageBox.Show(
                            "Assignment approved successfully.",
                            "PBCRM2",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        dialog.Close();

                        selectedRoomId = item.RoomId;

                        await LoadRooms();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Unable to approve assignment.\n\n{ex.Message}",
                            "PBCRM2",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                };

                // =================================================
                // REJECT BUTTON
                // =================================================

                var reject =
                    CreateButton("Reject", 110);

                reject.Location =
                    new Point(145, 365);

                reject.Click += async (_, _) =>
                {
                    int rowIndex =
                        grid.CurrentRow?.Index ?? -1;

                    if (rowIndex < 0 ||
                        rowIndex >= requests.Count)
                    {
                        MessageBox.Show(
                            "Select a pending request first.",
                            "PBCRM2",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }

                    AssignmentRequestDto item =
                        requests[rowIndex];

                    string? reason =
                        Prompt(
                            "Reject Assignment",
                            "Reason:");

                    if (string.IsNullOrWhiteSpace(reason))
                    {
                        MessageBox.Show(
                            "A rejection reason is required.",
                            "PBCRM2",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }

                    try
                    {
                        await _apiService.PostAsync<object>(
                            $"api/Rooms/assignment-requests/{item.Id}/reject",
                            new
                            {
                                rejectionReason =
                                    reason.Trim()
                            });

                        MessageBox.Show(
                            "Assignment request rejected.",
                            "PBCRM2",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        dialog.Close();

                        selectedRoomId = item.RoomId;

                        await LoadRooms();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Unable to reject assignment.\n\n{ex.Message}",
                            "PBCRM2",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                };

                dialog.Controls.Add(approve);
                dialog.Controls.Add(reject);

                dialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load pending requests.\n\n{ex.Message}",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // ROOM CAPACITY HELPERS
        // =========================================================

        private static bool IsValidRoomType(string? roomType)
        {
            return string.Equals(
                       roomType,
                       SharedRoomType,
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   string.Equals(
                       roomType,
                       PrivateRoomType,
                       StringComparison.OrdinalIgnoreCase);
        }

        private static int GetRoomCapacity(string? roomType)
        {
            if (string.Equals(
                    roomType,
                    PrivateRoomType,
                    StringComparison.OrdinalIgnoreCase))
            {
                return PrivateRoomCapacity;
            }

            return SharedRoomCapacity;
        }

        private static void UpdateCapacityLabel(
            Label label,
            string? roomType)
        {
            int capacity =
                GetRoomCapacity(roomType);

            string bedText =
                capacity == 1
                    ? "1 bed"
                    : $"{capacity} beds";

            label.Text =
                $"{capacity} {bedText switch
                {
                    "1 bed" => "",
                    _ => ""
                }}".Trim();

            // Keep the displayed value simple and natural.
            label.Text =
                capacity == 1
                    ? "1 bed"
                    : $"{capacity} beds";
        }

        // =========================================================
        // PROMPT
        // =========================================================

        private string? Prompt(
            string title,
            string labelText,
            string initialValue = "")
        {
            using var dialog = new Form
            {
                Text = title,
                Size = new Size(400, 190),
                StartPosition =
                    FormStartPosition.CenterParent,
                FormBorderStyle =
                    FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = PanelBg
            };

            var label = new Label
            {
                Text = labelText,
                Location = new Point(20, 25),
                AutoSize = true,
                Font =
                    new Font(
                        "Segoe UI",
                        9,
                        FontStyle.Bold),
                ForeColor = BrandBg
            };

            var textbox = new TextBox
            {
                Location = new Point(20, 55),
                Width = 340,
                Text = initialValue
            };

            var ok = CreateButton("OK", 90);
            ok.Location = new Point(20, 95);
            ok.DialogResult = DialogResult.OK;

            var cancel = CreateButton("Cancel", 90);
            cancel.Location = new Point(120, 95);
            cancel.DialogResult = DialogResult.Cancel;

            dialog.Controls.Add(label);
            dialog.Controls.Add(textbox);
            dialog.Controls.Add(ok);
            dialog.Controls.Add(cancel);

            dialog.AcceptButton = ok;
            dialog.CancelButton = cancel;

            dialog.Shown += (_, _) =>
            {
                textbox.Focus();
                textbox.SelectAll();
            };

            return dialog.ShowDialog(this) ==
                   DialogResult.OK
                ? textbox.Text
                : null;
        }

        // =========================================================
        // DIALOG LABEL
        // =========================================================

        private Label CreateDialogLabel(
            string text,
            int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(25, y + 3),
                Width = 110,
                Font =
                    new Font(
                        "Segoe UI",
                        9,
                        FontStyle.Bold),
                ForeColor = BrandBg
            };
        }

        // =========================================================
        // HIDE COLUMN
        // =========================================================

        private static void HideColumn(
            DataGridView grid,
            string name)
        {
            if (grid.Columns.Contains(name))
            {
                grid.Columns[name].Visible = false;
            }
        }

        // =========================================================
        // RENAME COLUMN
        // =========================================================

        private static void RenameColumn(
            DataGridView grid,
            string name,
            string header)
        {
            if (grid.Columns.Contains(name))
            {
                grid.Columns[name].HeaderText = header;
            }
        }
    }

    // =============================================================
    // ROOM DTO
    // =============================================================

    public class RoomDto
    {
        public int Id { get; set; }

        public int BranchId { get; set; }

        public string BranchName { get; set; } =
            string.Empty;

        public string RoomNumber { get; set; } =
            string.Empty;

        public string RoomType { get; set; } =
            string.Empty;

        public int Capacity { get; set; }

        public string Status { get; set; } =
            string.Empty;

        public bool IsActive { get; set; }

        public int BedCount { get; set; }

        public int OccupiedBeds { get; set; }

        public int AvailableBeds { get; set; }

        public int MaintenanceBeds { get; set; }

        public int PendingAssignments { get; set; }
    }

    // =============================================================
    // BED DTO
    // =============================================================

    public class BedDto
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public string BedNumber { get; set; } =
            string.Empty;

        public string Status { get; set; } =
            string.Empty;

        public int? TenantId { get; set; }

        public string? TenantName { get; set; }

        public JsonElement? PendingAssignment { get; set; }
    }

    // =============================================================
    // ASSIGNMENT REQUEST DTO
    // =============================================================

    public class AssignmentRequestDto
    {
        // =========================================================
        // INTERNAL VALUES
        // These are hidden from the table but are still needed
        // when approving or rejecting a request.
        // =========================================================

        public int Id { get; set; }

        public int BedId { get; set; }

        public int TenantId { get; set; }

        public int RoomId { get; set; }

        public string? RequestedByUserId { get; set; }

        // =========================================================
        // DISPLAY VALUES
        // =========================================================

        public string TenantName { get; set; } =
            string.Empty;

        public string RoomNumber { get; set; } =
            string.Empty;

        public string BedNumber { get; set; } =
            string.Empty;

        public string Status { get; set; } =
            string.Empty;

        public DateTime RequestedAt { get; set; }
    }
}
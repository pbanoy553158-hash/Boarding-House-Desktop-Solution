using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PBCRM2
{
    public class StaffDashboardRoomsBedsForm : Form
    {
        private readonly ApiService _apiService;

        private DataGridView dgvRooms = null!;
        private DataGridView dgvBeds = null!;

        private Button btnRefresh = null!;
        private Button btnRequestAssignment = null!;
        private TextBox txtSearch = null!;

        private Button btnFilterAll = null!;
        private Button btnFilterAvailable = null!;
        private Button btnFilterOccupied = null!;
        private Button btnFilterMaintenance = null!;

        private Label lblSelectedRoom = null!;
        private Label lblRoomCount = null!;

        private List<StaffRoomDto> allRooms = new();

        private int? selectedRoomId;

        private bool isLoadingRooms;
        private bool isLoadingBeds;

        // Prevents DataGridView selection recursion
        private bool isSelectingRoom;

        private string currentFilter = "All";

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

        private static readonly Color CardBorder =
            Color.FromArgb(230, 224, 215);

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public StaffDashboardRoomsBedsForm(
            ApiService apiService)
        {
            _apiService = apiService;

            FormBorderStyle =
                FormBorderStyle.None;

            Dock = DockStyle.Fill;

            BackColor = PanelBg;

            BuildInterface();

            Shown += async (_, _) =>
            {
                await LoadRooms();
            };
        }

        // =========================================================
        // BUILD INTERFACE
        // =========================================================

        private void BuildInterface()
        {
            Panel main = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg
            };

            // =====================================================
            // HEADER
            // =====================================================

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = BrandBg
            };

            Label title = new Label
            {
                Text = "ROOMS & BEDS",
                ForeColor = Color.White,
                Font =
                    new Font(
                        "Segoe UI",
                        22F,
                        FontStyle.Bold
                    ),
                AutoSize = true,
                Location =
                    new Point(
                        25,
                        22
                    )
            };

            Label subtitle = new Label
            {
                Text =
                    "View rooms and beds and submit requests",
                ForeColor = BrandAccent,
                Font =
                    new Font(
                        "Segoe UI",
                        10F
                    ),
                AutoSize = true,
                Location =
                    new Point(
                        28,
                        67
                    )
            };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            // =====================================================
            // HEADER GAP
            // =====================================================

            Panel headerGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = PanelBg
            };

            // =====================================================
            // ACTION TOOLBAR
            // =====================================================

            Panel actionPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = PanelBg
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "Search room...",
                Font =
                    new Font(
                        "Segoe UI",
                        9F
                    ),
                Location =
                    new Point(
                        20,
                        12
                    ),
                Size =
                    new Size(
                        200,
                        28
                    ),
                BorderStyle =
                    BorderStyle.FixedSingle
            };

            txtSearch.TextChanged += (_, _) =>
            {
                ApplyRoomFilter();
            };

            // =====================================================
            // FILTER BUTTONS
            // =====================================================

            btnFilterAll =
                CreateFilterChip(
                    "All",
                    230
                );

            btnFilterAvailable =
                CreateFilterChip(
                    "Available",
                    300
                );

            btnFilterOccupied =
                CreateFilterChip(
                    "Occupied",
                    395
                );

            btnFilterMaintenance =
                CreateFilterChip(
                    "Maintenance",
                    490
                );

            SetActiveFilter(
                btnFilterAll
            );

            btnFilterAll.Click += (_, _) =>
            {
                currentFilter = "All";

                SetActiveFilter(
                    btnFilterAll
                );

                ApplyRoomFilter();
            };

            btnFilterAvailable.Click += (_, _) =>
            {
                currentFilter = "Available";

                SetActiveFilter(
                    btnFilterAvailable
                );

                ApplyRoomFilter();
            };

            btnFilterOccupied.Click += (_, _) =>
            {
                currentFilter = "Occupied";

                SetActiveFilter(
                    btnFilterOccupied
                );

                ApplyRoomFilter();
            };

            btnFilterMaintenance.Click += (_, _) =>
            {
                currentFilter = "Maintenance";

                SetActiveFilter(
                    btnFilterMaintenance
                );

                ApplyRoomFilter();
            };

            // =====================================================
            // REFRESH
            // =====================================================

            btnRefresh =
                CreateButton(
                    "Refresh",
                    95
                );

            btnRefresh.Location =
                new Point(
                    620,
                    10
                );

            btnRefresh.Click += async (_, _) =>
            {
                await LoadRooms();
            };

            // =====================================================
            // REQUEST ASSIGNMENT
            // =====================================================

            btnRequestAssignment =
                CreateButton(
                    "Request Assignment",
                    155
                );

            btnRequestAssignment.Location =
                new Point(
                    725,
                    10
                );

            btnRequestAssignment.Click += async (_, _) =>
            {
                await RequestAssignment();
            };

            // =====================================================
            // ROOM COUNT
            // =====================================================

            lblRoomCount = new Label
            {
                Text = "0 rooms",
                ForeColor =
                    Color.FromArgb(
                        105,
                        95,
                        85
                    ),
                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold
                    ),
                AutoSize = true,
                Location =
                    new Point(
                        895,
                        17
                    )
            };

            actionPanel.Controls.Add(txtSearch);
            actionPanel.Controls.Add(btnFilterAll);
            actionPanel.Controls.Add(btnFilterAvailable);
            actionPanel.Controls.Add(btnFilterOccupied);
            actionPanel.Controls.Add(btnFilterMaintenance);
            actionPanel.Controls.Add(btnRefresh);
            actionPanel.Controls.Add(btnRequestAssignment);
            actionPanel.Controls.Add(lblRoomCount);

            // =====================================================
            // TOOLBAR GAP
            // =====================================================

            Panel toolbarGap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 20,
                BackColor = PanelBg
            };

            // =====================================================
            // CONTENT
            // =====================================================

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PanelBg,
                Padding =
                    new Padding(
                        20,
                        0,
                        20,
                        20
                    )
            };

            // =====================================================
            // ROOMS LABEL
            // =====================================================

            Label roomLabel = new Label
            {
                Text = "ROOMS",
                Dock = DockStyle.Top,
                Height = 30,
                Font =
                    new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Bold
                    ),
                ForeColor = BrandBg,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            // =====================================================
            // ROOMS TABLE
            // =====================================================

            dgvRooms = CreateGrid();

            dgvRooms.Dock =
                DockStyle.Top;

            dgvRooms.Height = 240;

            // IMPORTANT:
            // Do NOT use SelectionChanged here.
            // It can recursively trigger SelectRoomById()
            // and cause StackOverflowException.
            dgvRooms.CellClick +=
                async (_, e) =>
                {
                    await RoomCellClicked(e);
                };

            // =====================================================
            // BEDS GAP
            // =====================================================

            Panel bedsGap = new Panel
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
                Font =
                    new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Bold
                    ),
                ForeColor = BrandBg,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            // =====================================================
            // BEDS TABLE
            // =====================================================

            dgvBeds = CreateGrid();

            dgvBeds.Dock =
                DockStyle.Fill;

            // =====================================================
            // CONTENT CONTROLS
            //
            // Dock order is reversed.
            // =====================================================

            content.Controls.Add(dgvBeds);
            content.Controls.Add(lblSelectedRoom);
            content.Controls.Add(bedsGap);
            content.Controls.Add(dgvRooms);
            content.Controls.Add(roomLabel);

            // =====================================================
            // MAIN CONTROLS
            // =====================================================

            main.Controls.Add(content);
            main.Controls.Add(toolbarGap);
            main.Controls.Add(actionPanel);
            main.Controls.Add(headerGap);
            main.Controls.Add(header);

            Controls.Add(main);
        }

        // =========================================================
        // CREATE FILTER CHIP
        // =========================================================

        private Button CreateFilterChip(
            string text,
            int x)
        {
            Button btn = new Button
            {
                Text = text,
                Location =
                    new Point(
                        x,
                        10
                    ),
                Height = 32,
                AutoSize = true,
                FlatStyle =
                    FlatStyle.Flat,
                Font =
                    new Font(
                        "Segoe UI",
                        8.5F
                    ),
                Cursor = Cursors.Hand,
                Padding =
                    new Padding(
                        12,
                        0,
                        12,
                        0
                    ),
                BackColor = Color.White,
                ForeColor = BrandBg
            };

            btn.FlatAppearance.BorderColor =
                CardBorder;

            btn.FlatAppearance.BorderSize = 1;

            return btn;
        }

        // =========================================================
        // ACTIVE FILTER
        // =========================================================

        private void SetActiveFilter(
            Button active)
        {
            foreach (
                Button btn
                in new[]
                {
                    btnFilterAll,
                    btnFilterAvailable,
                    btnFilterOccupied,
                    btnFilterMaintenance
                })
            {
                btn.BackColor = Color.White;
                btn.ForeColor = BrandBg;

                btn.FlatAppearance.BorderColor =
                    CardBorder;
            }

            active.BackColor =
                BrandBg;

            active.ForeColor =
                Color.White;

            active.FlatAppearance.BorderColor =
                BrandBg;
        }

        // =========================================================
        // CREATE GRID
        // =========================================================

        private DataGridView CreateGrid()
        {
            DataGridView grid =
                new DataGridView
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
                        34,

                    ShowCellToolTips =
                        false
                };

            grid.RowTemplate.Height =
                30;

            typeof(DataGridView)
                .GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic
                )
                ?.SetValue(
                    grid,
                    true,
                    null
                );

            grid.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = BrandBg,
                    ForeColor = Color.White,
                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F,
                            FontStyle.Bold
                        ),
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
                            8.5F
                        ),

                    BackColor =
                        Color.White,

                    ForeColor =
                        BrandBg,

                    SelectionBackColor =
                        Color.FromArgb(
                            232,
                            220,
                            199
                        ),

                    SelectionForeColor =
                        BrandBg,

                    Alignment =
                        DataGridViewContentAlignment.MiddleLeft
                };

            grid.AlternatingRowsDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor =
                        Color.FromArgb(
                            247,
                            244,
                            239
                        )
                };

            return grid;
        }

        // =========================================================
        // CREATE BUTTON
        // =========================================================

        private Button CreateButton(
            string text,
            int width)
        {
            Button button =
                new Button
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
                            FontStyle.Bold
                        ),
                    Cursor =
                        Cursors.Hand,
                    UseVisualStyleBackColor =
                        false
                };

            button.FlatAppearance.BorderSize =
                0;

            return button;
        }

        // =========================================================
        // LOAD ROOMS
        // =========================================================

        private async Task LoadRooms()
        {
            if (isLoadingRooms)
                return;

            try
            {
                isLoadingRooms = true;

                btnRefresh.Enabled = false;

                int? previousRoomId =
                    selectedRoomId;

                List<StaffRoomDto>? rooms =
                    await _apiService
                        .GetAsync<List<StaffRoomDto>>(
                            "api/Rooms"
                        );

                if (rooms == null)
                {
                    MessageBox.Show(
                        "No room data was returned from the server.",
                        "Rooms & Beds",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                allRooms = rooms;

                ApplyRoomFilter();

                // =================================================
                // RESTORE PREVIOUS ROOM
                // =================================================

                if (previousRoomId.HasValue)
                {
                    DataGridViewRow? row =
                        FindRoomRow(
                            previousRoomId.Value
                        );

                    if (row != null)
                    {
                        await SelectRoomById(
                            previousRoomId.Value
                        );

                        return;
                    }
                }

                // =================================================
                // CLEAR SELECTION
                // =================================================

                ClearSelectedRoom();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"Unable to connect to the server.\n\n{ex.Message}",
                    "Rooms & Beds",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "The request timed out while loading rooms.",
                    "Rooms & Beds",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load rooms.\n\n{ex.Message}",
                    "Rooms & Beds",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                isLoadingRooms = false;

                btnRefresh.Enabled = true;
            }
        }

        // =========================================================
        // FILTER ROOMS
        // =========================================================

        private void ApplyRoomFilter()
        {
            if (dgvRooms == null)
                return;

            string search =
                txtSearch?.Text.Trim()
                ?? string.Empty;

            IEnumerable<StaffRoomDto> filtered =
                allRooms;

            // =====================================================
            // SEARCH
            // =====================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered =
                    filtered.Where(
                        room =>
                            room.RoomNumber.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase
                            )
                            ||
                            room.RoomType.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase
                            )
                            ||
                            room.Status.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase
                            )
                    );
            }

            // =====================================================
            // STATUS FILTER
            // =====================================================

            if (currentFilter != "All")
            {
                filtered =
                    filtered.Where(
                        room =>
                            string.Equals(
                                room.Status,
                                currentFilter,
                                StringComparison.OrdinalIgnoreCase
                            )
                    );
            }

            List<StaffRoomDto> result =
                filtered
                    .OrderBy(
                        room => room.RoomNumber
                    )
                    .ToList();

            dgvRooms.SuspendLayout();

            try
            {
                dgvRooms.DataSource = null;

                dgvRooms.DataSource =
                    result;

                ConfigureRoomColumns();
            }
            finally
            {
                dgvRooms.ResumeLayout();
            }

            lblRoomCount.Text =
                $"{result.Count} room" +
                (
                    result.Count == 1
                        ? ""
                        : "s"
                );

            // =====================================================
            // KEEP CURRENT ROOM IF STILL VISIBLE
            // =====================================================

            if (selectedRoomId.HasValue)
            {
                bool stillExists =
                    result.Any(
                        room =>
                            room.Id ==
                            selectedRoomId.Value
                    );

                if (!stillExists)
                {
                    ClearSelectedRoom();
                }
                else
                {
                    SelectRoomRow(
                        selectedRoomId.Value
                    );
                }
            }
            else
            {
                dgvRooms.ClearSelection();
            }
        }

        // =========================================================
        // CONFIGURE ROOM COLUMNS
        // =========================================================

        private void ConfigureRoomColumns()
        {
            // =====================================================
            // HIDDEN SYSTEM COLUMNS
            // =====================================================

            HideColumn(
                dgvRooms,
                "Id"
            );

            HideColumn(
                dgvRooms,
                "BranchId"
            );

            HideColumn(
                dgvRooms,
                "BranchName"
            );

            HideColumn(
                dgvRooms,
                "IsActive"
            );

            // =====================================================
            // ROOM
            // =====================================================

            RenameColumn(
                dgvRooms,
                "RoomNumber",
                "Room"
            );

            // =====================================================
            // TYPE
            // =====================================================

            RenameColumn(
                dgvRooms,
                "RoomType",
                "Type"
            );

            // =====================================================
            // CAPACITY
            // =====================================================

            RenameColumn(
                dgvRooms,
                "Capacity",
                "Capacity"
            );

            // =====================================================
            // BEDS
            // =====================================================

            RenameColumn(
                dgvRooms,
                "BedCount",
                "Beds"
            );

            RenameColumn(
                dgvRooms,
                "OccupiedBeds",
                "Occupied"
            );

            RenameColumn(
                dgvRooms,
                "AvailableBeds",
                "Available"
            );

            RenameColumn(
                dgvRooms,
                "MaintenanceBeds",
                "Maintenance"
            );

            RenameColumn(
                dgvRooms,
                "PendingAssignments",
                "Pending"
            );

            // =====================================================
            // STATUS
            // =====================================================

            RenameColumn(
                dgvRooms,
                "Status",
                "Status"
            );

            // =====================================================
            // COLUMN WIDTHS
            // =====================================================

            SetColumnFill(
                dgvRooms,
                "RoomNumber",
                1.2F
            );

            SetColumnFill(
                dgvRooms,
                "RoomType",
                1.1F
            );

            SetColumnFill(
                dgvRooms,
                "Capacity",
                0.8F
            );

            SetColumnFill(
                dgvRooms,
                "BedCount",
                0.8F
            );

            SetColumnFill(
                dgvRooms,
                "OccupiedBeds",
                0.9F
            );

            SetColumnFill(
                dgvRooms,
                "AvailableBeds",
                0.9F
            );

            SetColumnFill(
                dgvRooms,
                "MaintenanceBeds",
                1.1F
            );

            SetColumnFill(
                dgvRooms,
                "PendingAssignments",
                0.9F
            );

            SetColumnFill(
                dgvRooms,
                "Status",
                1.0F
            );
        }

        // =========================================================
        // ROOM CELL CLICK
        // =========================================================

        private async Task RoomCellClicked(
            DataGridViewCellEventArgs e)
        {
            if (isLoadingRooms)
                return;

            if (isSelectingRoom)
                return;

            if (e.RowIndex < 0)
                return;

            if (e.RowIndex >= dgvRooms.Rows.Count)
                return;

            DataGridViewRow row =
                dgvRooms.Rows[e.RowIndex];

            if (
                row.DataBoundItem
                is not StaffRoomDto room
            )
            {
                return;
            }

            await SelectRoomById(
                room.Id
            );
        }

        // =========================================================
        // SELECT ROOM BY ID
        // =========================================================

        private async Task SelectRoomById(
            int roomId)
        {
            if (isSelectingRoom)
                return;

            StaffRoomDto? room =
                allRooms.FirstOrDefault(
                    r => r.Id == roomId
                );

            if (room == null)
                return;

            try
            {
                isSelectingRoom = true;

                selectedRoomId =
                    roomId;

                // =================================================
                // UPDATE LABEL
                // =================================================

                lblSelectedRoom.Text =
                    $"BEDS — ROOM {room.RoomNumber}";

                // =================================================
                // SELECT THE ROOM ROW
                // =================================================

                SelectRoomRow(
                    roomId
                );
            }
            finally
            {
                isSelectingRoom = false;
            }

            // =====================================================
            // LOAD BEDS ONLY ONCE
            // =====================================================

            await LoadBeds(
                room.Id
            );
        }

        // =========================================================
        // SELECT ROOM ROW
        // =========================================================

        private void SelectRoomRow(
            int roomId)
        {
            foreach (
                DataGridViewRow row
                in dgvRooms.Rows)
            {
                if (
                    row.DataBoundItem
                    is StaffRoomDto room &&
                    room.Id == roomId
                )
                {
                    if (!row.Selected)
                    {
                        row.Selected = true;
                    }

                    if (
                        dgvRooms.Columns.Contains(
                            "RoomNumber"
                        )
                    )
                    {
                        dgvRooms.CurrentCell =
                            row.Cells[
                                "RoomNumber"
                            ];
                    }

                    return;
                }
            }
        }

        // =========================================================
        // FIND ROOM ROW
        // =========================================================

        private DataGridViewRow? FindRoomRow(
            int roomId)
        {
            foreach (
                DataGridViewRow row
                in dgvRooms.Rows)
            {
                if (
                    row.DataBoundItem
                    is StaffRoomDto room &&
                    room.Id == roomId
                )
                {
                    return row;
                }
            }

            return null;
        }

        // =========================================================
        // CLEAR SELECTED ROOM
        // =========================================================

        private void ClearSelectedRoom()
        {
            selectedRoomId = null;

            dgvRooms.ClearSelection();

            dgvBeds.DataSource =
                null;

            lblSelectedRoom.Text =
                "BEDS";
        }

        // =========================================================
        // LOAD BEDS
        // =========================================================

        private async Task LoadBeds(
            int roomId)
        {
            if (isLoadingBeds)
                return;

            try
            {
                isLoadingBeds = true;

                List<StaffBedDto>? beds =
                    await _apiService
                        .GetAsync<List<StaffBedDto>>(
                            $"api/Rooms/{roomId}/beds"
                        );

                if (beds == null)
                {
                    dgvBeds.DataSource =
                        null;

                    return;
                }

                // =================================================
                // IMPORTANT:
                // Ignore old request if another room was selected.
                // =================================================

                if (selectedRoomId != roomId)
                    return;

                dgvBeds.SuspendLayout();

                try
                {
                    dgvBeds.DataSource =
                        null;

                    dgvBeds.DataSource =
                        beds;

                    ConfigureBedColumns();
                }
                finally
                {
                    dgvBeds.ResumeLayout();
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"Unable to connect to the server while loading beds.\n\n{ex.Message}",
                    "Rooms & Beds",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "The request timed out while loading beds.",
                    "Rooms & Beds",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load beds.\n\n{ex.Message}",
                    "Rooms & Beds",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                isLoadingBeds = false;
            }
        }

        // =========================================================
        // BED COLUMNS
        // =========================================================

        private void ConfigureBedColumns()
        {
            HideColumn(
                dgvBeds,
                "Id"
            );

            HideColumn(
                dgvBeds,
                "RoomId"
            );

            HideColumn(
                dgvBeds,
                "TenantId"
            );

            HideColumn(
                dgvBeds,
                "PendingAssignment"
            );

            RenameColumn(
                dgvBeds,
                "BedNumber",
                "Bed"
            );

            RenameColumn(
                dgvBeds,
                "Status",
                "Status"
            );

            RenameColumn(
                dgvBeds,
                "TenantName",
                "Tenant"
            );

            SetColumnFill(
                dgvBeds,
                "BedNumber",
                1F
            );

            SetColumnFill(
                dgvBeds,
                "Status",
                1F
            );

            SetColumnFill(
                dgvBeds,
                "TenantName",
                2F
            );
        }

        // =========================================================
        // REQUEST ASSIGNMENT
        // =========================================================

        private async Task RequestAssignment()
        {
            try
            {
                using Form dialog =
                    new Form
                    {
                        Text =
                            "Request Bed Assignment",

                        Size =
                            new Size(
                                500,
                                390
                            ),

                        StartPosition =
                            FormStartPosition.CenterParent,

                        FormBorderStyle =
                            FormBorderStyle.FixedDialog,

                        MaximizeBox = false,

                        MinimizeBox = false,

                        BackColor =
                            PanelBg
                    };

                Label title =
                    new Label
                    {
                        Text =
                            "BED ASSIGNMENT REQUEST",

                        Location =
                            new Point(
                                25,
                                20
                            ),

                        AutoSize = true,

                        Font =
                            new Font(
                                "Segoe UI",
                                14F,
                                FontStyle.Bold
                            ),

                        ForeColor =
                            BrandBg
                    };

                Label subtitle =
                    new Label
                    {
                        Text =
                            "Select a registered room, available bed, and active tenant.",

                        Location =
                            new Point(
                                27,
                                52
                            ),

                        AutoSize = true,

                        Font =
                            new Font(
                                "Segoe UI",
                                8.5F
                            ),

                        ForeColor =
                            Color.FromArgb(
                                105,
                                95,
                                85
                            )
                    };

                Label lblRoom =
                    CreateDialogLabel(
                        "Room",
                        92
                    );

                ComboBox cmbRoom =
                    new ComboBox
                    {
                        Location =
                            new Point(
                                145,
                                90
                            ),

                        Width = 300,

                        DropDownStyle =
                            ComboBoxStyle.DropDownList,

                        Font =
                            new Font(
                                "Segoe UI",
                                9F
                            ),

                        DisplayMember =
                            nameof(
                                StaffRoomDto.RoomNumber
                            ),

                        ValueMember =
                            nameof(
                                StaffRoomDto.Id
                            )
                    };

                Label lblBed =
                    CreateDialogLabel(
                        "Bed",
                        145
                    );

                ComboBox cmbBed =
                    new ComboBox
                    {
                        Location =
                            new Point(
                                145,
                                143
                            ),

                        Width = 300,

                        DropDownStyle =
                            ComboBoxStyle.DropDownList,

                        Font =
                            new Font(
                                "Segoe UI",
                                9F
                            ),

                        DisplayMember =
                            nameof(
                                StaffBedDto.BedNumber
                            ),

                        ValueMember =
                            nameof(
                                StaffBedDto.Id
                            ),

                        Enabled = false
                    };

                Label lblTenant =
                    CreateDialogLabel(
                        "Tenant",
                        198
                    );

                ComboBox cmbTenant =
                    new ComboBox
                    {
                        Location =
                            new Point(
                                145,
                                196
                            ),

                        Width = 300,

                        DropDownStyle =
                            ComboBoxStyle.DropDownList,

                        Font =
                            new Font(
                                "Segoe UI",
                                9F
                            ),

                        DisplayMember =
                            nameof(
                                StaffTenantDto.FullName
                            ),

                        ValueMember =
                            nameof(
                                StaffTenantDto.Id
                            )
                    };

                Label lblStatus =
                    new Label
                    {
                        Text =
                            "Select a room first. Available beds will appear automatically.",

                        Location =
                            new Point(
                                145,
                                242
                            ),

                        Width = 300,

                        Height = 40,

                        Font =
                            new Font(
                                "Segoe UI",
                                8F
                            ),

                        ForeColor =
                            Color.FromArgb(
                                105,
                                95,
                                85
                            )
                    };

                Button btnSubmit =
                    CreateButton(
                        "Send Request",
                        120
                    );

                btnSubmit.Location =
                    new Point(
                        145,
                        295
                    );

                Button btnCancel =
                    CreateButton(
                        "Cancel",
                        90
                    );

                btnCancel.Location =
                    new Point(
                        275,
                        295
                    );

                btnCancel.DialogResult =
                    DialogResult.Cancel;

                // =================================================
                // ACTIVE ROOMS
                // =================================================

                List<StaffRoomDto> requestRooms =
                    allRooms
                        .Where(
                            room =>
                                room.IsActive
                        )
                        .OrderBy(
                            room =>
                                room.RoomNumber
                        )
                        .ToList();

                if (requestRooms.Count == 0)
                {
                    MessageBox.Show(
                        "There are no registered active rooms available.",
                        "Request Assignment",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    return;
                }

                cmbRoom.DataSource =
                    requestRooms;

                // =================================================
                // TENANTS
                // =================================================

                List<StaffTenantDto>? tenants =
                    await LoadRegisteredTenants();

                if (tenants == null)
                    return;

                List<StaffTenantDto> activeTenants =
                    tenants
                        .Where(
                            tenant =>
                                tenant.IsActiveTenant
                        )
                        .OrderBy(
                            tenant =>
                                tenant.FullName
                        )
                        .ToList();

                if (activeTenants.Count == 0)
                {
                    MessageBox.Show(
                        "There are no active registered tenants available for assignment.",
                        "Request Assignment",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    return;
                }

                cmbTenant.DataSource =
                    activeTenants;

                // =================================================
                // LOAD AVAILABLE BEDS
                // =================================================

                async Task LoadAvailableBedsForSelectedRoom()
                {
                    cmbBed.DataSource =
                        null;

                    cmbBed.Items.Clear();

                    cmbBed.Enabled =
                        false;

                    lblStatus.Text =
                        "Loading beds...";

                    if (
                        cmbRoom.SelectedItem
                        is not StaffRoomDto selectedRoom
                    )
                    {
                        lblStatus.Text =
                            "Please select a room first.";

                        return;
                    }

                    try
                    {
                        List<StaffBedDto>? beds =
                            await _apiService
                                .GetAsync<List<StaffBedDto>>(
                                    $"api/Rooms/{selectedRoom.Id}/beds"
                                );

                        if (beds == null)
                        {
                            lblStatus.Text =
                                "No beds were returned.";

                            return;
                        }

                        List<StaffBedDto> availableBeds =
                            beds
                                .Where(
                                    bed =>
                                        !bed.TenantId.HasValue
                                )
                                .Where(
                                    bed =>
                                        !IsMaintenanceStatus(
                                            bed.Status
                                        )
                                )
                                .Where(
                                    bed =>
                                        !HasPendingAssignment(
                                            bed
                                        )
                                )
                                .OrderBy(
                                    bed =>
                                        bed.BedNumber
                                )
                                .ToList();

                        if (availableBeds.Count == 0)
                        {
                            cmbBed.DataSource =
                                null;

                            cmbBed.Enabled =
                                false;

                            lblStatus.Text =
                                $"No available beds in Room {selectedRoom.RoomNumber}.";

                            return;
                        }

                        cmbBed.DisplayMember =
                            nameof(
                                StaffBedDto.BedNumber
                            );

                        cmbBed.ValueMember =
                            nameof(
                                StaffBedDto.Id
                            );

                        cmbBed.DataSource =
                            availableBeds;

                        cmbBed.Enabled =
                            true;

                        cmbBed.SelectedIndex =
                            0;

                        lblStatus.Text =
                            $"{availableBeds.Count} available bed" +
                            (
                                availableBeds.Count == 1
                                    ? ""
                                    : "s"
                            ) +
                            $" in Room {selectedRoom.RoomNumber}.";
                    }
                    catch (Exception ex)
                    {
                        cmbBed.DataSource =
                            null;

                        cmbBed.Enabled =
                            false;

                        lblStatus.Text =
                            "Unable to load beds.";

                        MessageBox.Show(
                            $"Unable to load beds for Room {selectedRoom.RoomNumber}.\n\n{ex.Message}",
                            "Request Assignment",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }

                cmbRoom.SelectedIndexChanged +=
                    async (_, _) =>
                    {
                        await LoadAvailableBedsForSelectedRoom();
                    };

                if (cmbRoom.Items.Count > 0)
                {
                    cmbRoom.SelectedIndex =
                        0;

                    await LoadAvailableBedsForSelectedRoom();
                }

                // =================================================
                // SUBMIT
                // =================================================

                btnSubmit.Click += async (_, _) =>
                {
                    if (
                        cmbRoom.SelectedItem
                        is not StaffRoomDto selectedRoom
                    )
                    {
                        MessageBox.Show(
                            "Please select a room.",
                            "Request Assignment",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        return;
                    }

                    if (
                        cmbBed.SelectedItem
                        is not StaffBedDto selectedBed
                    )
                    {
                        MessageBox.Show(
                            "Please select an available bed.",
                            "Request Assignment",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        return;
                    }

                    if (
                        cmbTenant.SelectedItem
                        is not StaffTenantDto selectedTenant
                    )
                    {
                        MessageBox.Show(
                            "Please select a registered tenant.",
                            "Request Assignment",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        return;
                    }

                    if (
                        selectedBed.TenantId.HasValue
                    )
                    {
                        MessageBox.Show(
                            "The selected bed is already occupied. Please select another bed.",
                            "Request Assignment",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        await LoadAvailableBedsForSelectedRoom();

                        return;
                    }

                    if (
                        IsMaintenanceStatus(
                            selectedBed.Status
                        )
                    )
                    {
                        MessageBox.Show(
                            "The selected bed is under maintenance. Please select another bed.",
                            "Request Assignment",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        await LoadAvailableBedsForSelectedRoom();

                        return;
                    }

                    if (
                        HasPendingAssignment(
                            selectedBed
                        )
                    )
                    {
                        MessageBox.Show(
                            "The selected bed already has a pending assignment request.",
                            "Request Assignment",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        await LoadAvailableBedsForSelectedRoom();

                        return;
                    }

                    DialogResult confirm =
                        MessageBox.Show(
                            $"Send this bed assignment request?\n\n" +
                            $"Tenant: {selectedTenant.FullName}\n" +
                            $"Room: {selectedRoom.RoomNumber}\n" +
                            $"Bed: {selectedBed.BedNumber}\n\n" +
                            "The Manager must approve the request before the tenant is assigned.",
                            "Request Assignment",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                    if (
                        confirm !=
                        DialogResult.Yes
                    )
                    {
                        return;
                    }

                    try
                    {
                        btnSubmit.Enabled =
                            false;

                        bool success =
                            await _apiService.PostAsync(
                                $"api/Rooms/beds/{selectedBed.Id}/request-assignment",
                                new
                                {
                                    tenantId =
                                        selectedTenant.Id
                                }
                            );

                        if (!success)
                        {
                            btnSubmit.Enabled =
                                true;

                            return;
                        }

                        MessageBox.Show(
                            "Request sent successfully.\n\n" +
                            $"Tenant: {selectedTenant.FullName}\n" +
                            $"Room: {selectedRoom.RoomNumber}\n" +
                            $"Bed: {selectedBed.BedNumber}\n\n" +
                            "The request is now waiting for Manager approval.",
                            "Request Assignment",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        selectedRoomId =
                            selectedRoom.Id;

                        dialog.DialogResult =
                            DialogResult.OK;

                        dialog.Close();

                        await LoadRooms();
                    }
                    catch (HttpRequestException ex)
                    {
                        btnSubmit.Enabled =
                            true;

                        MessageBox.Show(
                            $"Unable to connect to the server.\n\n{ex.Message}",
                            "Request Assignment",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                    catch (TaskCanceledException)
                    {
                        btnSubmit.Enabled =
                            true;

                        MessageBox.Show(
                            "The assignment request timed out.",
                            "Request Assignment",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                    catch (Exception ex)
                    {
                        btnSubmit.Enabled =
                            true;

                        MessageBox.Show(
                            $"Unable to submit the assignment request.\n\n{ex.Message}",
                            "Request Assignment",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                };

                dialog.Controls.Add(title);
                dialog.Controls.Add(subtitle);
                dialog.Controls.Add(lblRoom);
                dialog.Controls.Add(cmbRoom);
                dialog.Controls.Add(lblBed);
                dialog.Controls.Add(cmbBed);
                dialog.Controls.Add(lblTenant);
                dialog.Controls.Add(cmbTenant);
                dialog.Controls.Add(lblStatus);
                dialog.Controls.Add(btnSubmit);
                dialog.Controls.Add(btnCancel);

                dialog.CancelButton =
                    btnCancel;

                dialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open the assignment request form.\n\n{ex.Message}",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // =========================================================
        // CHECK MAINTENANCE STATUS
        // =========================================================

        private static bool IsMaintenanceStatus(
            string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            return
                string.Equals(
                    status,
                    "Maintenance",
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                string.Equals(
                    status,
                    "Under Maintenance",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        // =========================================================
        // CHECK PENDING ASSIGNMENT
        // =========================================================

        private static bool HasPendingAssignment(
            StaffBedDto bed)
        {
            if (!bed.PendingAssignment.HasValue)
                return false;

            JsonElement value =
                bed.PendingAssignment.Value;

            if (
                value.ValueKind ==
                JsonValueKind.Null
                ||
                value.ValueKind ==
                JsonValueKind.Undefined
            )
            {
                return false;
            }

            return true;
        }

        // =========================================================
        // LOAD REGISTERED TENANTS
        // =========================================================

        private async Task<List<StaffTenantDto>?>
            LoadRegisteredTenants()
        {
            try
            {
                List<StaffTenantDto>? tenants =
                    await _apiService
                        .GetAsync<List<StaffTenantDto>>(
                            "api/Tenants"
                        );

                if (tenants == null)
                {
                    return new List<StaffTenantDto>();
                }

                return tenants;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"Unable to connect to the server while loading tenants.\n\n{ex.Message}",
                    "Request Assignment",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return null;
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "The request timed out while loading tenants.",
                    "Request Assignment",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load registered tenants.\n\n{ex.Message}",
                    "Request Assignment",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return null;
            }
        }

        // =========================================================
        // HIDE COLUMN
        // =========================================================

        private static void HideColumn(
            DataGridView grid,
            string name)
        {
            if (
                grid.Columns.Contains(
                    name
                )
            )
            {
                grid.Columns[name]
                    .Visible = false;
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
            if (
                grid.Columns.Contains(
                    name
                )
            )
            {
                grid.Columns[name]
                    .HeaderText = header;
            }
        }

        // =========================================================
        // COLUMN FILL
        // =========================================================

        private static void SetColumnFill(
            DataGridView grid,
            string name,
            float weight)
        {
            if (
                !grid.Columns.Contains(
                    name
                )
            )
            {
                return;
            }

            DataGridViewColumn column =
                grid.Columns[name];

            column.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;

            column.FillWeight =
                weight;
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

                Location =
                    new Point(
                        25,
                        y + 3
                    ),

                Width = 110,

                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold
                    ),

                ForeColor =
                    BrandBg
            };
        }
    }

    // =============================================================
    // ROOM DTO
    // =============================================================

    public class StaffRoomDto
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

    public class StaffBedDto
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
    // TENANT DTO
    // =============================================================

    public class StaffTenantDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } =
            string.Empty;

        public int BranchId { get; set; }

        public string Status { get; set; } =
            string.Empty;

        public bool IsActiveTenant =>
            string.Equals(
                Status,
                "Active",
                StringComparison.OrdinalIgnoreCase
            );

        public override string ToString() =>
            FullName;
    }
}
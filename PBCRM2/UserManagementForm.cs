using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PBCRM2
{
    public class UserManagementForm : Form
    {
        private readonly ApiService _apiService;

        private DataGridView dgvUsers = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbRoleFilter = null!;
        private ComboBox cmbStatusFilter = null!;

        private Button btnRefresh = null!;
        private Button btnAddUser = null!;
        private Button btnEditUser = null!;
        private Button btnResetPassword = null!;
        private Button btnToggleStatus = null!;

        private Label lblRecordCount = null!;

        private List<UserDto> _users = new();

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

        public UserManagementForm(ApiService apiService)
        {
            _apiService =
                apiService
                ?? throw new ArgumentNullException(
                    nameof(apiService));

            Text =
                "User & Role Management";

            StartPosition =
                FormStartPosition.CenterScreen;

            Size =
                new Size(1250, 780);

            MinimumSize =
                new Size(1050, 680);

            BackColor =
                PanelBg;

            BuildInterface();

            Shown += async (_, _) =>
                await LoadUsersAsync();
        }

        // =========================================================
        // MAIN INTERFACE
        // =========================================================

        private void BuildInterface()
        {
            Panel main =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = PanelBg
                };

            // =====================================================
            // HEADER
            // =====================================================

            Panel header =
                new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 120,
                    BackColor = BrandBg
                };

            Label title =
                new Label
                {
                    Text =
                        "USER & ROLE MANAGEMENT",

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
                        new Point(25, 22)
                };

            Label subtitle =
                new Label
                {
                    Text =
                        "Manage system users, roles, assignments and account access.",

                    ForeColor =
                        BrandAccent,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F),

                    AutoSize =
                        true,

                    Location =
                        new Point(28, 67)
                };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            // =====================================================
            // HEADER GAP
            // =====================================================

            Panel headerGap =
                new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 30,
                    BackColor = PanelBg
                };

            // =====================================================
            // TOOLBAR
            // =====================================================

            Panel toolbar =
                new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 55,
                    BackColor = PanelBg,
                    AutoScroll = true
                };

            // SEARCH
            txtSearch =
                new TextBox
                {
                    Location =
                        new Point(20, 10),

                    Width =
                        220,

                    Height =
                        32,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F),

                    BorderStyle =
                        BorderStyle.FixedSingle,

                    PlaceholderText =
                        "Search users..."
                };

            txtSearch.TextChanged +=
                (_, _) => ApplyFilters();

            // ROLE FILTER
            cmbRoleFilter =
                new ComboBox
                {
                    Location =
                        new Point(250, 10),

                    Width =
                        135,

                    Height =
                        32,

                    DropDownStyle =
                        ComboBoxStyle.DropDownList,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F)
                };

            cmbRoleFilter.Items.Add("All Roles");
            cmbRoleFilter.Items.Add("SuperAdmin");
            cmbRoleFilter.Items.Add("Admin");
            cmbRoleFilter.Items.Add("Manager");
            cmbRoleFilter.Items.Add("Staff");

            cmbRoleFilter.SelectedIndex = 0;

            cmbRoleFilter.SelectedIndexChanged +=
                (_, _) => ApplyFilters();

            // STATUS FILTER
            cmbStatusFilter =
                new ComboBox
                {
                    Location =
                        new Point(400, 10),

                    Width =
                        125,

                    Height =
                        32,

                    DropDownStyle =
                        ComboBoxStyle.DropDownList,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F)
                };

            cmbStatusFilter.Items.Add("All Status");
            cmbStatusFilter.Items.Add("Active");
            cmbStatusFilter.Items.Add("Deactivated");

            cmbStatusFilter.SelectedIndex = 0;

            cmbStatusFilter.SelectedIndexChanged +=
                (_, _) => ApplyFilters();

            // REFRESH
            btnRefresh =
                CreateButton(
                    "Refresh",
                    90);

            btnRefresh.Location =
                new Point(540, 10);

            btnRefresh.Click += async (_, _) =>
                await LoadUsersAsync();

            // ADD USER
            btnAddUser =
                CreateButton(
                    "Add User",
                    105);

            btnAddUser.Location =
                new Point(640, 10);

            btnAddUser.Click += async (_, _) =>
                await AddUserAsync();

            // EDIT USER
            btnEditUser =
                CreateButton(
                    "Edit User",
                    100);

            btnEditUser.Location =
                new Point(755, 10);

            btnEditUser.Click += async (_, _) =>
                await EditSelectedUserAsync();

            // RESET PASSWORD
            btnResetPassword =
                CreateButton(
                    "Reset Password",
                    120);

            btnResetPassword.Location =
                new Point(865, 10);

            btnResetPassword.Click += async (_, _) =>
                await ResetSelectedPasswordAsync();

            // TOGGLE STATUS
            btnToggleStatus =
                CreateButton(
                    "Deactivate",
                    105);

            btnToggleStatus.Location =
                new Point(995, 10);

            btnToggleStatus.Click += async (_, _) =>
                await ToggleSelectedUserAsync();

            // RECORD COUNT
            lblRecordCount =
                new Label
                {
                    Text =
                        "0 RECORDS",

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
                        new Point(1115, 18)
                };

            toolbar.Controls.Add(txtSearch);
            toolbar.Controls.Add(cmbRoleFilter);
            toolbar.Controls.Add(cmbStatusFilter);
            toolbar.Controls.Add(btnRefresh);
            toolbar.Controls.Add(btnAddUser);
            toolbar.Controls.Add(btnEditUser);
            toolbar.Controls.Add(btnResetPassword);
            toolbar.Controls.Add(btnToggleStatus);
            toolbar.Controls.Add(lblRecordCount);

            // =====================================================
            // TOOLBAR GAP
            // =====================================================

            Panel toolbarGap =
                new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 20,
                    BackColor = PanelBg
                };

            // =====================================================
            // CONTENT
            // =====================================================

            Panel content =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = PanelBg,
                    Padding =
                        new Padding(
                            20,
                            0,
                            20,
                            20)
                };

            Label userLabel =
                new Label
                {
                    Text =
                        "SYSTEM USERS",

                    Dock =
                        DockStyle.Top,

                    Height =
                        30,

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

            dgvUsers =
                CreateGrid();

            dgvUsers.Dock =
                DockStyle.Fill;

            dgvUsers.SelectionChanged +=
                (_, _) => UpdateSelectedUserButtons();

            dgvUsers.CellDoubleClick +=
                async (_, e) =>
                {
                    if (e.RowIndex >= 0)
                    {
                        await EditSelectedUserAsync();
                    }
                };

            content.Controls.Add(dgvUsers);
            content.Controls.Add(userLabel);

            // =====================================================
            // ADD TO MAIN
            // =====================================================

            main.Controls.Add(content);
            main.Controls.Add(toolbarGap);
            main.Controls.Add(toolbar);
            main.Controls.Add(headerGap);
            main.Controls.Add(header);

            Controls.Add(main);

            UpdateSelectedUserButtons();
        }

        // =========================================================
        // GRID
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
                        false,

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
                        BrandBg,

                    SelectionBackColor =
                        Color.FromArgb(
                            232,
                            220,
                            199),

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
                            239)
                };

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Username",
                    HeaderText = "Username",
                    DataPropertyName = "Username",
                    FillWeight = 14,
                    MinimumWidth = 100
                });

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "FullName",
                    HeaderText = "Full Name",
                    DataPropertyName = "FullName",
                    FillWeight = 18,
                    MinimumWidth = 130
                });

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Email",
                    HeaderText = "Email",
                    DataPropertyName = "Email",
                    FillWeight = 20,
                    MinimumWidth = 150
                });

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Role",
                    HeaderText = "Role",
                    DataPropertyName = "Role",
                    FillWeight = 13,
                    MinimumWidth = 90
                });

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Company",
                    HeaderText = "Company",
                    DataPropertyName = "CompanyName",
                    FillWeight = 16,
                    MinimumWidth = 120
                });

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Branch",
                    HeaderText = "Branch",
                    DataPropertyName = "BranchName",
                    FillWeight = 12,
                    MinimumWidth = 100
                });

            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Status",
                    DataPropertyName = "Status",
                    FillWeight = 10,
                    MinimumWidth = 90
                });

            return grid;
        }

        // =========================================================
        // BUTTON
        // =========================================================

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
                        false,

                    TabStop =
                        false
                };

            button.FlatAppearance.BorderSize =
                0;

            button.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(
                    185,
                    145,
                    95);

            button.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(
                    150,
                    110,
                    65);

            return button;
        }

        // =========================================================
        // LOAD USERS
        // =========================================================

        private async Task LoadUsersAsync()
        {
            try
            {
                btnRefresh.Enabled = false;

                List<UserDto>? result =
                    await _apiService.GetAsync<List<UserDto>>(
                        "api/Users");

                _users =
                    result ??
                    new List<UserDto>();

                List<CompanyDto>? companies =
                    await _apiService.GetCompaniesAsync();

                Dictionary<int, string> companyLookup =
                    companies?
                        .ToDictionary(
                            c => c.Id,
                            c => c.CompanyName)
                    ??
                    new Dictionary<int, string>();

                Dictionary<int, List<BranchDto>> branchCache =
                    new();

                foreach (UserDto user in _users)
                {
                    if (user.CompanyId.HasValue &&
                        companyLookup.TryGetValue(
                            user.CompanyId.Value,
                            out string? companyName))
                    {
                        user.CompanyName =
                            companyName;
                    }
                    else
                    {
                        user.CompanyName =
                            string.Empty;
                    }

                    if (user.CompanyId.HasValue &&
                        user.BranchId.HasValue)
                    {
                        int companyId =
                            user.CompanyId.Value;

                        if (!branchCache.TryGetValue(
                                companyId,
                                out List<BranchDto>? branches))
                        {
                            branches =
                                await _apiService
                                    .GetBranchesAsync(
                                        companyId)
                                ??
                                new List<BranchDto>();

                            branchCache[companyId] =
                                branches;
                        }

                        BranchDto? branch =
                            branches.FirstOrDefault(
                                b =>
                                    b.Id ==
                                    user.BranchId.Value);

                        user.BranchName =
                            branch?.BranchName
                            ??
                            string.Empty;
                    }
                    else
                    {
                        user.BranchName =
                            string.Empty;
                    }

                    user.Role =
                        user.Roles?
                            .FirstOrDefault()
                        ??
                        string.Empty;

                    user.Status =
                        user.IsLockedOut
                            ? "Deactivated"
                            : "Active";
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load system users.\n\n{ex.Message}",
                    "User & Role Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;

                UpdateSelectedUserButtons();
            }
        }

        // =========================================================
        // FILTER
        // =========================================================

        private void ApplyFilters()
        {
            if (dgvUsers == null)
                return;

            string search =
                txtSearch?.Text
                    .Trim()
                    .ToLowerInvariant()
                ??
                string.Empty;

            string role =
                cmbRoleFilter?
                    .SelectedItem?
                    .ToString()
                ??
                "All Roles";

            string status =
                cmbStatusFilter?
                    .SelectedItem?
                    .ToString()
                ??
                "All Status";

            IEnumerable<UserDto> filtered =
                _users;

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered =
                    filtered.Where(
                        user =>
                            (user.Username ?? string.Empty)
                                .ToLowerInvariant()
                                .Contains(search)

                            ||

                            (user.FullName ?? string.Empty)
                                .ToLowerInvariant()
                                .Contains(search)

                            ||

                            (user.Email ?? string.Empty)
                                .ToLowerInvariant()
                                .Contains(search)

                            ||

                            (user.Role ?? string.Empty)
                                .ToLowerInvariant()
                                .Contains(search)

                            ||

                            (user.CompanyName ?? string.Empty)
                                .ToLowerInvariant()
                                .Contains(search)

                            ||

                            (user.BranchName ?? string.Empty)
                                .ToLowerInvariant()
                                .Contains(search));
            }

            if (!string.Equals(
                    role,
                    "All Roles",
                    StringComparison.OrdinalIgnoreCase))
            {
                filtered =
                    filtered.Where(
                        user =>
                            string.Equals(
                                user.Role,
                                role,
                                StringComparison.OrdinalIgnoreCase));
            }

            if (!string.Equals(
                    status,
                    "All Status",
                    StringComparison.OrdinalIgnoreCase))
            {
                filtered =
                    filtered.Where(
                        user =>
                            string.Equals(
                                user.Status,
                                status,
                                StringComparison.OrdinalIgnoreCase));
            }

            List<UserDto> display =
                filtered.ToList();

            dgvUsers.DataSource = null;
            dgvUsers.DataSource = display;

            int count =
                display.Count;

            lblRecordCount.Text =
                count == 1
                    ? "1 RECORD"
                    : $"{count} RECORDS";

            UpdateSelectedUserButtons();
        }

        // =========================================================
        // SELECTED USER
        // =========================================================

        private UserDto? GetSelectedUser()
        {
            if (dgvUsers.CurrentRow == null)
                return null;

            return dgvUsers
                .CurrentRow
                .DataBoundItem as UserDto;
        }

        // =========================================================
        // UPDATE BUTTONS
        // =========================================================

        private void UpdateSelectedUserButtons()
        {
            if (btnEditUser == null)
                return;

            UserDto? user =
                GetSelectedUser();

            if (user == null)
            {
                btnEditUser.Enabled = false;
                btnResetPassword.Enabled = false;
                btnToggleStatus.Enabled = false;

                return;
            }

            bool isSuperAdminAccount =
                string.Equals(
                    user.Username,
                    "superadmin",
                    StringComparison.OrdinalIgnoreCase);

            btnEditUser.Enabled =
                !isSuperAdminAccount;

            btnResetPassword.Enabled =
                true;

            btnToggleStatus.Enabled =
                !isSuperAdminAccount;

            btnToggleStatus.Text =
                user.IsLockedOut
                    ? "Activate"
                    : "Deactivate";
        }

        // =========================================================
        // ADD USER
        // =========================================================

        private async Task AddUserAsync()
        {
            UserInput? input =
                await ShowUserDialogAsync(null);

            if (input == null)
                return;

            try
            {
                CreateUserRequest request =
                    new CreateUserRequest
                    {
                        Username =
                            input.Username,

                        Password =
                            input.Password,

                        FullName =
                            input.FullName,

                        Email =
                            input.Email,

                        CompanyId =
                            input.CompanyId,

                        BranchId =
                            input.BranchId,

                        Role =
                            input.Role
                    };

                CreateUserResponse? result =
                    await _apiService.PostAsync<CreateUserResponse>(
                        "api/Users",
                        request);

                if (result != null)
                {
                    MessageBox.Show(
                        "User created successfully.",
                        "User & Role Management",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to create user.\n\n{ex.Message}",
                    "User & Role Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // EDIT USER
        // =========================================================

        private async Task EditSelectedUserAsync()
        {
            UserDto? selected =
                GetSelectedUser();

            if (selected == null)
            {
                MessageBox.Show(
                    "Please select a user first.",
                    "Edit User",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (string.Equals(
                    selected.Username,
                    "superadmin",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "The main Super Admin account cannot be edited from User & Role Management.",
                    "Edit User",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            UserInput initial =
                new UserInput
                {
                    Username =
                        selected.Username,

                    FullName =
                        selected.FullName,

                    Email =
                        selected.Email,

                    Role =
                        selected.Role,

                    CompanyId =
                        selected.CompanyId,

                    BranchId =
                        selected.BranchId,

                    Password =
                        string.Empty
                };

            UserInput? input =
                await ShowUserDialogAsync(initial);

            if (input == null)
                return;

            try
            {
                UpdateUserRequest request =
                    new UpdateUserRequest
                    {
                        FullName =
                            input.FullName,

                        Email =
                            input.Email,

                        CompanyId =
                            input.CompanyId,

                        BranchId =
                            input.BranchId,

                        Role =
                            input.Role
                    };

                bool result =
                    await _apiService.PutAsync(
                        $"api/Users/{selected.Id}",
                        request);

                if (result)
                {
                    MessageBox.Show(
                        "User updated successfully.",
                        "User & Role Management",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to update user.\n\n{ex.Message}",
                    "User & Role Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // RESET PASSWORD
        // =========================================================

        private async Task ResetSelectedPasswordAsync()
        {
            UserDto? user =
                GetSelectedUser();

            if (user == null)
            {
                MessageBox.Show(
                    "Please select a user first.",
                    "Reset Password",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using Form form =
                new Form
                {
                    Text =
                        "Reset Password",

                    StartPosition =
                        FormStartPosition.CenterParent,

                    FormBorderStyle =
                        FormBorderStyle.FixedDialog,

                    MaximizeBox =
                        false,

                    MinimizeBox =
                        false,

                    ClientSize =
                        new Size(
                            430,
                            220),

                    BackColor =
                        PanelBg
                };

            Label lblUser =
                new Label
                {
                    Text =
                        $"Reset password for: {user.Username}",

                    AutoSize =
                        true,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F,
                            FontStyle.Bold),

                    ForeColor =
                        BrandBg,

                    Location =
                        new Point(
                            30,
                            25)
                };

            Label lblPassword =
                CreateInputLabel(
                    "New Password",
                    30,
                    70);

            TextBox txtPassword =
                new TextBox
                {
                    Location =
                        new Point(
                            30,
                            95),

                    Width =
                        370,

                    Height =
                        30,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F),

                    UseSystemPasswordChar =
                        true
                };

            Button btnCancel =
                CreateButton(
                    "Cancel",
                    120);

            btnCancel.Location =
                new Point(
                    150,
                    150);

            btnCancel.DialogResult =
                DialogResult.Cancel;

            Button btnSave =
                CreateButton(
                    "Reset Password",
                    140);

            btnSave.Location =
                new Point(
                    280,
                    150);

            string? newPassword = null;

            btnSave.Click +=
                (_, _) =>
                {
                    if (string.IsNullOrWhiteSpace(
                            txtPassword.Text))
                    {
                        MessageBox.Show(
                            "Please enter a new password.",
                            "Reset Password",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        txtPassword.Focus();

                        return;
                    }

                    if (txtPassword.Text.Length < 6)
                    {
                        MessageBox.Show(
                            "Password must contain at least 6 characters.",
                            "Reset Password",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        txtPassword.Focus();

                        return;
                    }

                    newPassword =
                        txtPassword.Text;

                    form.DialogResult =
                        DialogResult.OK;
                };

            form.Controls.Add(lblUser);
            form.Controls.Add(lblPassword);
            form.Controls.Add(txtPassword);
            form.Controls.Add(btnCancel);
            form.Controls.Add(btnSave);

            form.AcceptButton =
                btnSave;

            form.CancelButton =
                btnCancel;

            if (form.ShowDialog(this)
                != DialogResult.OK
                ||
                string.IsNullOrWhiteSpace(
                    newPassword))
            {
                return;
            }

            try
            {
                ResetPasswordRequest request =
                    new ResetPasswordRequest
                    {
                        NewPassword =
                            newPassword
                    };

                bool result =
                    await _apiService.PostAsync(
                        $"api/Users/{user.Id}/reset-password",
                        request);

                if (result)
                {
                    MessageBox.Show(
                        "Password reset successfully.",
                        "User & Role Management",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to reset password.\n\n{ex.Message}",
                    "Reset Password",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // ACTIVATE / DEACTIVATE
        // =========================================================

        private async Task ToggleSelectedUserAsync()
        {
            UserDto? user =
                GetSelectedUser();

            if (user == null)
            {
                MessageBox.Show(
                    "Please select a user first.",
                    "User Status",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (string.Equals(
                    user.Username,
                    "superadmin",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "The Super Admin account cannot be deactivated.",
                    "User Status",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string action =
                user.IsLockedOut
                    ? "activate"
                    : "deactivate";

            DialogResult confirmation =
                MessageBox.Show(
                    $"Are you sure you want to {action} '{user.Username}'?",
                    "Confirm Action",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirmation !=
                DialogResult.Yes)
            {
                return;
            }

            try
            {
                string endpoint =
                    user.IsLockedOut
                        ? $"api/Users/{user.Id}/activate"
                        : $"api/Users/{user.Id}/deactivate";

                bool result =
                    await _apiService.PostAsync(
                        endpoint,
                        new { });

                if (result)
                {
                    MessageBox.Show(
                        user.IsLockedOut
                            ? "User activated successfully."
                            : "User deactivated successfully.",
                        "User & Role Management",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to change user status.\n\n{ex.Message}",
                    "User Status",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // USER DIALOG
        // =========================================================

        private async Task<UserInput?> ShowUserDialogAsync(
            UserInput? existing)
        {
            bool isEdit =
                existing != null;

            Form form =
                new Form
                {
                    Text =
                        isEdit
                            ? "Edit User"
                            : "Add User",

                    StartPosition =
                        FormStartPosition.CenterParent,

                    FormBorderStyle =
                        FormBorderStyle.FixedDialog,

                    MaximizeBox =
                        false,

                    MinimizeBox =
                        false,

                    ClientSize =
                        new Size(
                            520,
                            isEdit
                                ? 455
                                : 525),

                    BackColor =
                        PanelBg
                };

            // =====================================================
            // USERNAME
            // =====================================================

            Label lblUsername =
                CreateInputLabel(
                    "Username",
                    30,
                    25);

            TextBox txtUsername =
                new TextBox
                {
                    Location =
                        new Point(
                            30,
                            50),

                    Width =
                        460,

                    Height =
                        30,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F),

                    Text =
                        existing?.Username
                        ??
                        string.Empty,

                    ReadOnly =
                        isEdit
                };

            // =====================================================
            // PASSWORD
            // =====================================================

            Label lblPassword =
                CreateInputLabel(
                    "Password",
                    30,
                    90);

            TextBox txtPassword =
                new TextBox
                {
                    Location =
                        new Point(
                            30,
                            115),

                    Width =
                        460,

                    Height =
                        30,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F),

                    UseSystemPasswordChar =
                        true
                };

            if (isEdit)
            {
                txtPassword.Visible =
                    false;

                lblPassword.Visible =
                    false;
            }

            // =====================================================
            // FULL NAME
            // =====================================================

            int fullNameY =
                isEdit
                    ? 90
                    : 155;

            Label lblFullName =
                CreateInputLabel(
                    "Full Name",
                    30,
                    fullNameY);

            TextBox txtFullName =
                new TextBox
                {
                    Location =
                        new Point(
                            30,
                            fullNameY + 25),

                    Width =
                        460,

                    Height =
                        30,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F),

                    Text =
                        existing?.FullName
                        ??
                        string.Empty
                };

            // =====================================================
            // EMAIL
            // =====================================================

            int emailY =
                fullNameY + 65;

            Label lblEmail =
                CreateInputLabel(
                    "Email",
                    30,
                    emailY);

            TextBox txtEmail =
                new TextBox
                {
                    Location =
                        new Point(
                            30,
                            emailY + 25),

                    Width =
                        460,

                    Height =
                        30,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F),

                    Text =
                        existing?.Email
                        ??
                        string.Empty
                };

            // =====================================================
            // ROLE
            // =====================================================

            int roleY =
                emailY + 65;

            Label lblRole =
                CreateInputLabel(
                    "Role",
                    30,
                    roleY);

            ComboBox cmbRole =
                new ComboBox
                {
                    Location =
                        new Point(
                            30,
                            roleY + 25),

                    Width =
                        460,

                    Height =
                        30,

                    DropDownStyle =
                        ComboBoxStyle.DropDownList,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F)
                };

            cmbRole.Items.Add(
                UserRoles.SuperAdmin);

            cmbRole.Items.Add(
                UserRoles.Admin);

            cmbRole.Items.Add(
                UserRoles.Manager);

            cmbRole.Items.Add(
                UserRoles.Staff);

            int selectedRole =
                cmbRole.Items.IndexOf(
                    existing?.Role
                    ??
                    UserRoles.Staff);

            cmbRole.SelectedIndex =
                selectedRole >= 0
                    ? selectedRole
                    : 3;

            // =====================================================
            // COMPANY
            // =====================================================

            int companyY =
                roleY + 65;

            Label lblCompany =
                CreateInputLabel(
                    "Company",
                    30,
                    companyY);

            ComboBox cmbCompany =
                new ComboBox
                {
                    Location =
                        new Point(
                            30,
                            companyY + 25),

                    Width =
                        220,

                    Height =
                        30,

                    DropDownStyle =
                        ComboBoxStyle.DropDownList,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F)
                };

            // =====================================================
            // BRANCH
            // =====================================================

            Label lblBranch =
                CreateInputLabel(
                    "Branch",
                    270,
                    companyY);

            ComboBox cmbBranch =
                new ComboBox
                {
                    Location =
                        new Point(
                            270,
                            companyY + 25),

                    Width =
                        220,

                    Height =
                        30,

                    DropDownStyle =
                        ComboBoxStyle.DropDownList,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F),

                    Enabled =
                        false
                };

            // =====================================================
            // BUTTONS
            // =====================================================

            int buttonY =
                companyY + 75;

            Button btnCancel =
                CreateButton(
                    "Cancel",
                    120);

            btnCancel.Location =
                new Point(
                    245,
                    buttonY);

            btnCancel.DialogResult =
                DialogResult.Cancel;

            Button btnSave =
                CreateButton(
                    isEdit
                        ? "Save Changes"
                        : "Create User",
                    145);

            btnSave.Location =
                new Point(
                    370,
                    buttonY);

            // =====================================================
            // LOCAL RESULT
            // =====================================================

            UserInput? result =
                null;

            bool loadingBranches =
                false;

            bool initialized =
                false;

            // =====================================================
            // LOAD COMPANIES
            // =====================================================

            List<CompanyDto> companies =
                await _apiService
                    .GetCompaniesAsync()
                ??
                new List<CompanyDto>();

            cmbCompany.Items.Clear();

            cmbCompany.Items.Add(
                new CompanyDto
                {
                    Id = 0,
                    CompanyName =
                        "Select Company"
                });

            foreach (CompanyDto company in companies)
            {
                cmbCompany.Items.Add(
                    company);
            }

            cmbCompany.SelectedIndex =
                0;

            // =====================================================
            // SET BRANCH PLACEHOLDER
            // =====================================================

            void SetBranchPlaceholder(
                string text)
            {
                cmbBranch.Items.Clear();

                cmbBranch.Items.Add(
                    new BranchDto
                    {
                        Id = 0,
                        BranchName =
                            text
                    });

                cmbBranch.SelectedIndex =
                    0;
            }

            // =====================================================
            // LOAD BRANCHES
            // =====================================================

            async Task LoadBranchesAsync(
                int companyId,
                int? branchToSelect = null)
            {
                if (loadingBranches)
                    return;

                loadingBranches =
                    true;

                try
                {
                    cmbBranch.Enabled =
                        false;

                    btnSave.Enabled =
                        false;

                    SetBranchPlaceholder(
                        "Loading branches...");

                    List<BranchDto> branches =
                        await _apiService
                            .GetBranchesAsync(
                                companyId)
                        ??
                        new List<BranchDto>();

                    cmbBranch.Items.Clear();

                    if (branches.Count == 0)
                    {
                        cmbBranch.Items.Add(
                            new BranchDto
                            {
                                Id = 0,
                                BranchName =
                                    "No Branch Available"
                            });

                        cmbBranch.SelectedIndex =
                            0;

                        cmbBranch.Enabled =
                            false;

                        return;
                    }

                    cmbBranch.Items.Add(
                        new BranchDto
                        {
                            Id = 0,
                            BranchName =
                                "Select Branch"
                        });

                    foreach (BranchDto branch in branches)
                    {
                        cmbBranch.Items.Add(
                            branch);
                    }

                    cmbBranch.SelectedIndex =
                        0;

                    if (branchToSelect.HasValue)
                    {
                        for (int i = 1;
                             i < cmbBranch.Items.Count;
                             i++)
                        {
                            if (cmbBranch.Items[i]
                                is BranchDto branch
                                &&
                                branch.Id ==
                                branchToSelect.Value)
                            {
                                cmbBranch.SelectedIndex =
                                    i;

                                break;
                            }
                        }
                    }

                    cmbBranch.Enabled =
                        true;
                }
                catch (Exception ex)
                {
                    SetBranchPlaceholder(
                        "Unable to load branches");

                    cmbBranch.Enabled =
                        false;

                    MessageBox.Show(
                        "Unable to load branches.\n\n" +
                        ex.Message,
                        "Branch Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                finally
                {
                    loadingBranches =
                        false;

                    btnSave.Enabled =
                        true;
                }
            }

            // =====================================================
            // UPDATE ASSIGNMENT FIELDS
            // =====================================================

            async Task UpdateAssignmentFieldsAsync(
                int? branchToSelect = null)
            {
                string role =
                    cmbRole.SelectedItem?
                        .ToString()
                    ??
                    UserRoles.Staff;

                bool isSuperAdmin =
                    string.Equals(
                        role,
                        UserRoles.SuperAdmin,
                        StringComparison.OrdinalIgnoreCase);

                bool isAdmin =
                    string.Equals(
                        role,
                        UserRoles.Admin,
                        StringComparison.OrdinalIgnoreCase);

                bool needsBranch =
                    string.Equals(
                        role,
                        UserRoles.Manager,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                        role,
                        UserRoles.Staff,
                        StringComparison.OrdinalIgnoreCase);

                // -------------------------------------------------
                // SUPER ADMIN
                // -------------------------------------------------

                if (isSuperAdmin)
                {
                    cmbCompany.SelectedIndex =
                        0;

                    cmbCompany.Enabled =
                        false;

                    SetBranchPlaceholder(
                        "Not Applicable");

                    cmbBranch.Enabled =
                        false;

                    return;
                }

                // -------------------------------------------------
                // ADMIN
                // -------------------------------------------------

                if (isAdmin)
                {
                    cmbCompany.Enabled =
                        true;

                    SetBranchPlaceholder(
                        "Not Applicable");

                    cmbBranch.Enabled =
                        false;

                    return;
                }

                // -------------------------------------------------
                // MANAGER / STAFF
                // -------------------------------------------------

                cmbCompany.Enabled =
                    true;

                if (cmbCompany.SelectedItem
                    is not CompanyDto selectedCompany
                    ||
                    selectedCompany.Id <= 0)
                {
                    SetBranchPlaceholder(
                        "Select Branch");

                    cmbBranch.Enabled =
                        false;

                    return;
                }

                await LoadBranchesAsync(
                    selectedCompany.Id,
                    branchToSelect);
            }

            // =====================================================
            // ROLE CHANGED
            // =====================================================

            cmbRole.SelectedIndexChanged +=
                async (_, _) =>
                {
                    if (!initialized)
                        return;

                    await UpdateAssignmentFieldsAsync();
                };

            // =====================================================
            // COMPANY CHANGED
            // =====================================================

            cmbCompany.SelectedIndexChanged +=
                async (_, _) =>
                {
                    if (!initialized)
                        return;

                    string role =
                        cmbRole.SelectedItem?
                            .ToString()
                        ??
                        UserRoles.Staff;

                    bool needsBranch =
                        role ==
                            UserRoles.Manager
                        ||
                        role ==
                            UserRoles.Staff;

                    if (!needsBranch)
                        return;

                    if (cmbCompany.SelectedItem
                        is CompanyDto selectedCompany
                        &&
                        selectedCompany.Id > 0)
                    {
                        await LoadBranchesAsync(
                            selectedCompany.Id);
                    }
                    else
                    {
                        SetBranchPlaceholder(
                            "Select Branch");

                        cmbBranch.Enabled =
                            false;
                    }
                };

            // =====================================================
            // ADD CONTROLS
            // =====================================================

            form.Controls.Add(lblUsername);
            form.Controls.Add(txtUsername);

            form.Controls.Add(lblPassword);
            form.Controls.Add(txtPassword);

            form.Controls.Add(lblFullName);
            form.Controls.Add(txtFullName);

            form.Controls.Add(lblEmail);
            form.Controls.Add(txtEmail);

            form.Controls.Add(lblRole);
            form.Controls.Add(cmbRole);

            form.Controls.Add(lblCompany);
            form.Controls.Add(cmbCompany);

            form.Controls.Add(lblBranch);
            form.Controls.Add(cmbBranch);

            form.Controls.Add(btnCancel);
            form.Controls.Add(btnSave);

            form.AcceptButton =
                btnSave;

            form.CancelButton =
                btnCancel;

            // =====================================================
            // SAVE
            // =====================================================

            btnSave.Click +=
                (_, _) =>
                {
                    // ---------------------------------------------
                    // USERNAME
                    // ---------------------------------------------

                    string username =
                        txtUsername.Text.Trim();

                    if (!isEdit &&
                        string.IsNullOrWhiteSpace(
                            username))
                    {
                        MessageBox.Show(
                            "Username is required.",
                            "User",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        txtUsername.Focus();

                        return;
                    }

                    // ---------------------------------------------
                    // PASSWORD
                    // ---------------------------------------------

                    if (!isEdit &&
                        string.IsNullOrWhiteSpace(
                            txtPassword.Text))
                    {
                        MessageBox.Show(
                            "Password is required.",
                            "User",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        txtPassword.Focus();

                        return;
                    }

                    if (!isEdit &&
                        txtPassword.Text.Length < 6)
                    {
                        MessageBox.Show(
                            "Password must contain at least 6 characters.",
                            "User",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        txtPassword.Focus();

                        return;
                    }

                    // ---------------------------------------------
                    // FULL NAME
                    // ---------------------------------------------

                    string fullName =
                        txtFullName.Text.Trim();

                    if (string.IsNullOrWhiteSpace(
                            fullName))
                    {
                        MessageBox.Show(
                            "Full name is required.",
                            "User",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        txtFullName.Focus();

                        return;
                    }

                    // ---------------------------------------------
                    // ROLE
                    // ---------------------------------------------

                    string role =
                        cmbRole.SelectedItem?
                            .ToString()
                        ??
                        string.Empty;

                    if (string.IsNullOrWhiteSpace(
                            role))
                    {
                        MessageBox.Show(
                            "Please select a role.",
                            "User",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }

                    // ---------------------------------------------
                    // COMPANY
                    // ---------------------------------------------

                    int? companyId =
                        null;

                    if (cmbCompany.SelectedItem
                        is CompanyDto selectedCompany
                        &&
                        selectedCompany.Id > 0)
                    {
                        companyId =
                            selectedCompany.Id;
                    }

                    // ---------------------------------------------
                    // BRANCH
                    // ---------------------------------------------

                    int? branchId =
                        null;

                    if (cmbBranch.SelectedItem
                        is BranchDto selectedBranch
                        &&
                        selectedBranch.Id > 0)
                    {
                        branchId =
                            selectedBranch.Id;
                    }

                    // ---------------------------------------------
                    // SUPER ADMIN
                    // ---------------------------------------------

                    if (role ==
                        UserRoles.SuperAdmin)
                    {
                        companyId =
                            null;

                        branchId =
                            null;
                    }

                    // ---------------------------------------------
                    // ADMIN
                    // ---------------------------------------------

                    if (role ==
                        UserRoles.Admin)
                    {
                        if (!companyId.HasValue)
                        {
                            MessageBox.Show(
                                "Please select a company for the Admin.",
                                "User",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            return;
                        }

                        branchId =
                            null;
                    }

                    // ---------------------------------------------
                    // MANAGER / STAFF
                    // ---------------------------------------------

                    if (role ==
                            UserRoles.Manager
                        ||
                        role ==
                            UserRoles.Staff)
                    {
                        if (!companyId.HasValue)
                        {
                            MessageBox.Show(
                                $"Please select a company for the {role}.",
                                "User",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            return;
                        }

                        if (!branchId.HasValue)
                        {
                            MessageBox.Show(
                                $"Please select a branch for the {role}.",
                                "User",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            return;
                        }
                    }

                    // ---------------------------------------------
                    // CREATE RESULT
                    // ---------------------------------------------

                    result =
                        new UserInput
                        {
                            Username =
                                username,

                            Password =
                                txtPassword.Text,

                            FullName =
                                fullName,

                            Email =
                                txtEmail.Text.Trim(),

                            Role =
                                role,

                            CompanyId =
                                companyId,

                            BranchId =
                                branchId
                        };

                    form.DialogResult =
                        DialogResult.OK;
                };

            // =====================================================
            // INITIAL STATE
            // =====================================================

            form.Shown +=
                async (_, _) =>
                {
                    initialized =
                        false;

                    try
                    {
                        // -----------------------------------------
                        // EDIT: SELECT COMPANY FIRST
                        // -----------------------------------------

                        if (existing?.CompanyId.HasValue == true)
                        {
                            for (int i = 0;
                                 i < cmbCompany.Items.Count;
                                 i++)
                            {
                                if (cmbCompany.Items[i]
                                    is CompanyDto company
                                    &&
                                    company.Id ==
                                    existing.CompanyId.Value)
                                {
                                    cmbCompany.SelectedIndex =
                                        i;

                                    break;
                                }
                            }
                        }

                        // -----------------------------------------
                        // LOAD ROLE / COMPANY / BRANCH
                        // -----------------------------------------

                        await UpdateAssignmentFieldsAsync(
                            existing?.BranchId);

                        // -----------------------------------------
                        // EDIT BRANCH
                        // -----------------------------------------

                        if (existing?.BranchId.HasValue == true
                            &&
                            cmbBranch.Items.Count > 0)
                        {
                            for (int i = 0;
                                 i < cmbBranch.Items.Count;
                                 i++)
                            {
                                if (cmbBranch.Items[i]
                                    is BranchDto branch
                                    &&
                                    branch.Id ==
                                    existing.BranchId.Value)
                                {
                                    cmbBranch.SelectedIndex =
                                        i;

                                    break;
                                }
                            }
                        }
                    }
                    finally
                    {
                        initialized =
                            true;
                    }
                };

            // =====================================================
            // SHOW
            // =====================================================

            using (form)
            {
                DialogResult dialogResult =
                    form.ShowDialog(this);

                if (dialogResult !=
                    DialogResult.OK)
                {
                    return null;
                }

                return result;
            }
        }

        // =========================================================
        // INPUT LABEL
        // =========================================================

        private Label CreateInputLabel(
            string text,
            int x,
            int y)
        {
            return new Label
            {
                Text =
                    text,

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
                        x,
                        y)
            };
        }

        // =========================================================
        // DTOs
        // =========================================================

        public class UserDto
        {
            public string Id { get; set; } =
                string.Empty;

            public string Username { get; set; } =
                string.Empty;

            public string Email { get; set; } =
                string.Empty;

            public string FullName { get; set; } =
                string.Empty;

            public int? CompanyId { get; set; }

            public int? BranchId { get; set; }

            public List<string> Roles { get; set; } =
                new();

            public bool IsLockedOut { get; set; }

            public string Role { get; set; } =
                string.Empty;

            public string CompanyName { get; set; } =
                string.Empty;

            public string BranchName { get; set; } =
                string.Empty;

            public string Status { get; set; } =
                "Active";
        }

        public class CreateUserRequest
        {
            public string Username { get; set; } =
                string.Empty;

            public string Password { get; set; } =
                string.Empty;

            public string FullName { get; set; } =
                string.Empty;

            public string Email { get; set; } =
                string.Empty;

            public int? CompanyId { get; set; }

            public int? BranchId { get; set; }

            public string Role { get; set; } =
                UserRoles.Staff;
        }

        public class UpdateUserRequest
        {
            public string FullName { get; set; } =
                string.Empty;

            public string Email { get; set; } =
                string.Empty;

            public int? CompanyId { get; set; }

            public int? BranchId { get; set; }

            public string Role { get; set; } =
                UserRoles.Staff;
        }

        public class ResetPasswordRequest
        {
            public string NewPassword { get; set; } =
                string.Empty;
        }

        public class CreateUserResponse
        {
            public string Message { get; set; } =
                string.Empty;

            public UserDto? User { get; set; }
        }

        public class UserInput
        {
            public string Username { get; set; } =
                string.Empty;

            public string Password { get; set; } =
                string.Empty;

            public string FullName { get; set; } =
                string.Empty;

            public string Email { get; set; } =
                string.Empty;

            public string Role { get; set; } =
                UserRoles.Staff;

            public int? CompanyId { get; set; }

            public int? BranchId { get; set; }
        }

        // =========================================================
        // USER ROLES
        // =========================================================

        private static class UserRoles
        {
            public const string SuperAdmin =
                "SuperAdmin";

            public const string Admin =
                "Admin";

            public const string Manager =
                "Manager";

            public const string Staff =
                "Staff";
        }
    }
}
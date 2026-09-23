using PBCRM2.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Windows.Forms;

namespace PBCRM2.WinForms.Forms
{
    public class BranchEditorForm : Form
    {
        // ============================================================
        // COLORS
        // ============================================================

        private static readonly Color PanelBg =
            Color.FromArgb(250, 247, 242);

        private static readonly Color CardBg =
            Color.White;

        private static readonly Color CardBorder =
            Color.FromArgb(226, 219, 210);

        private static readonly Color BrandAccent =
            Color.FromArgb(224, 194, 140);

        private static readonly Color CtaColor =
            Color.FromArgb(170, 130, 80);

        private static readonly Color TextDark =
            Color.FromArgb(55, 48, 42);

        private static readonly Color TextMuted =
            Color.FromArgb(120, 110, 100);

        private static readonly Color SoftBg =
            Color.FromArgb(253, 251, 248);

        // ============================================================
        // DATA
        // ============================================================

        private readonly AdminBranchDto? _branch;

        private readonly List<ManagerDto> _managers;

        private readonly bool _isEdit;

        // ============================================================
        // MAIN LAYOUT
        // ============================================================

        private Panel mainPanel = null!;

        private Panel contentPanel = null!;

        private Panel buttonPanel = null!;

        // ============================================================
        // BRANCH CONTROLS
        // ============================================================

        private TextBox txtBranchName = null!;

        private TextBox txtStreet = null!;

        private TextBox txtBarangay = null!;

        private TextBox txtCity = null!;

        private TextBox txtProvince = null!;

        private TextBox txtPostalCode = null!;

        private TextBox txtContact = null!;

        // ============================================================
        // MANAGER CONTROLS
        // ============================================================

        private RadioButton rdoCreateNewManager = null!;

        private RadioButton rdoExistingManager = null!;

        private Panel pnlNewManager = null!;

        private Panel pnlExistingManager = null!;

        private TextBox txtManagerFullName = null!;

        private TextBox txtManagerUsername = null!;

        private TextBox txtManagerEmail = null!;

        private TextBox txtManagerPassword = null!;

        private TextBox txtManagerConfirmPassword = null!;

        private ComboBox cmbManager = null!;

        // ============================================================
        // SECTIONS
        // ============================================================

        private Panel branchSection = null!;

        private Panel managerSection = null!;

        // ============================================================
        // BUTTONS
        // ============================================================

        private Button btnSave = null!;

        private Button btnCancel = null!;

        // ============================================================
        // PUBLIC RESULT PROPERTIES
        // ============================================================

        public string BranchName { get; private set; } =
            string.Empty;

        public string BranchAddress { get; private set; } =
            string.Empty;

        public string BranchContactNumber { get; private set; } =
            string.Empty;

        public string? SelectedManagerId { get; private set; }

        // ============================================================
        // NEW MANAGER RESULT
        // ============================================================

        public bool CreateNewManager { get; private set; }

        public string ManagerFullName { get; private set; } =
            string.Empty;

        public string ManagerUsername { get; private set; } =
            string.Empty;

        public string ManagerEmail { get; private set; } =
            string.Empty;

        public string ManagerPassword { get; private set; } =
            string.Empty;

        public string ManagerConfirmPassword { get; private set; } =
            string.Empty;

        // ============================================================
        // CONSTRUCTOR - ADD
        // ============================================================

        public BranchEditorForm(
            List<ManagerDto> managers)
        {
            _branch = null;

            _managers =
                managers ??
                new List<ManagerDto>();

            _isEdit = false;

            InitializeForm();

            BuildInterface();

            LoadManagerOptions();

            SetAddMode();
        }

        // ============================================================
        // CONSTRUCTOR - EDIT
        // ============================================================

        public BranchEditorForm(
            AdminBranchDto branch,
            List<ManagerDto> managers)
        {
            _branch =
                branch ??
                throw new ArgumentNullException(
                    nameof(branch));

            _managers =
                managers ??
                new List<ManagerDto>();

            _isEdit = true;

            InitializeForm();

            BuildInterface();

            LoadManagerOptions();

            LoadExistingBranch();

            SetEditMode();
        }

        // ============================================================
        // FORM INITIALIZATION
        // ============================================================

        private void InitializeForm()
        {
            Text =
                _isEdit
                    ? "Edit Branch"
                    : "Add New Branch";

            StartPosition =
                FormStartPosition.CenterParent;

            Width = 660;

            Height =
                _isEdit
                    ? 650
                    : 820;

            MinimumSize =
                new Size(
                    620,
                    _isEdit
                        ? 620
                        : 760);

            MaximumSize =
                new Size(
                    900,
                    950);

            BackColor =
                PanelBg;

            ForeColor =
                TextDark;

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox =
                false;

            MinimizeBox =
                false;

            ShowInTaskbar =
                false;

            AutoScaleMode =
                AutoScaleMode.Dpi;

            Padding =
                new Padding(0);
        }

        // ============================================================
        // BUILD INTERFACE
        // ============================================================

        private void BuildInterface()
        {
            Controls.Clear();

            // --------------------------------------------------------
            // MAIN PANEL
            // --------------------------------------------------------

            mainPanel =
                new Panel
                {
                    Dock =
                        DockStyle.Fill,

                    BackColor =
                        PanelBg,

                    Padding =
                        new Padding(
                            24,
                            20,
                            24,
                            0)
                };

            Controls.Add(mainPanel);

            // --------------------------------------------------------
            // BUTTON PANEL
            // --------------------------------------------------------

            BuildBottomButtons();

            // --------------------------------------------------------
            // CONTENT PANEL
            // --------------------------------------------------------

            contentPanel =
                new Panel
                {
                    Dock =
                        DockStyle.Fill,

                    BackColor =
                        PanelBg,

                    AutoScroll =
                        true,

                    Padding =
                        new Padding(
                            0,
                            0,
                            8,
                            20)
                };

            mainPanel.Controls.Add(
                contentPanel);

            // --------------------------------------------------------
            // SECTIONS
            // --------------------------------------------------------

            BuildBranchSection();

            BuildManagerSection();

            UpdateContentLayout();
        }

        // ============================================================
        // BRANCH SECTION
        // ============================================================

        private void BuildBranchSection()
        {
            branchSection =
                CreateSectionPanel(
                    "Branch Information",
                    "Enter the branch identity, address, and contact details.");

            branchSection.Location =
                new Point(
                    0,
                    0);

            branchSection.Width =
                Math.Max(
                    560,
                    contentPanel.ClientSize.Width - 8);

            branchSection.Height =
                360;

            branchSection.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            contentPanel.Controls.Add(
                branchSection);

            BuildBranchFields();
        }

        // ============================================================
        // BRANCH FIELDS
        // ============================================================

        private void BuildBranchFields()
        {
            int left = 18;

            int width =
                branchSection.Width - 36;

            // --------------------------------------------------------
            // BRANCH NAME
            // --------------------------------------------------------

            AddFieldLabel(
                branchSection,
                "Branch Name",
                left,
                62);

            txtBranchName =
                CreateTextBox();

            txtBranchName.Location =
                new Point(
                    left,
                    82);

            txtBranchName.Width =
                width;

            branchSection.Controls.Add(
                txtBranchName);

            // --------------------------------------------------------
            // STREET / PUROK
            // --------------------------------------------------------

            AddFieldLabel(
                branchSection,
                "Street / Purok",
                left,
                122);

            txtStreet =
                CreateTextBox();

            txtStreet.Location =
                new Point(
                    left,
                    142);

            txtStreet.Width =
                width;

            branchSection.Controls.Add(
                txtStreet);

            // --------------------------------------------------------
            // BARANGAY
            // --------------------------------------------------------

            AddFieldLabel(
                branchSection,
                "Barangay",
                left,
                182);

            txtBarangay =
                CreateTextBox();

            txtBarangay.Location =
                new Point(
                    left,
                    202);

            txtBarangay.Width =
                width;

            branchSection.Controls.Add(
                txtBarangay);

            // --------------------------------------------------------
            // CITY
            // --------------------------------------------------------

            int halfWidth =
                (width - 12) / 2;

            AddFieldLabel(
                branchSection,
                "City / Municipality",
                left,
                242);

            txtCity =
                CreateTextBox();

            txtCity.Location =
                new Point(
                    left,
                    262);

            txtCity.Width =
                halfWidth;

            branchSection.Controls.Add(
                txtCity);

            // --------------------------------------------------------
            // PROVINCE
            // --------------------------------------------------------

            int right =
                left +
                halfWidth +
                12;

            AddFieldLabel(
                branchSection,
                "Province",
                right,
                242);

            txtProvince =
                CreateTextBox();

            txtProvince.Location =
                new Point(
                    right,
                    262);

            txtProvince.Width =
                halfWidth;

            branchSection.Controls.Add(
                txtProvince);

            // --------------------------------------------------------
            // POSTAL CODE
            // --------------------------------------------------------

            AddFieldLabel(
                branchSection,
                "Postal Code",
                left,
                302);

            txtPostalCode =
                CreateTextBox();

            txtPostalCode.Location =
                new Point(
                    left,
                    322);

            txtPostalCode.Width =
                130;

            txtPostalCode.MaxLength =
                4;

            txtPostalCode.KeyPress +=
                PostalCode_KeyPress;

            branchSection.Controls.Add(
                txtPostalCode);

            // --------------------------------------------------------
            // CONTACT NUMBER
            // --------------------------------------------------------

            int phoneLeft =
                left + 150;

            AddFieldLabel(
                branchSection,
                "Contact Number",
                phoneLeft,
                302);

            Panel phonePanel =
                new Panel
                {
                    Location =
                        new Point(
                            phoneLeft,
                            322),

                    Width =
                        width - 150,

                    Height =
                        32,

                    BackColor =
                        Color.White
                };

            phonePanel.Paint +=
                (_, e) =>
                {
                    using Pen pen =
                        new Pen(
                            CardBorder);

                    e.Graphics.DrawRectangle(
                        pen,
                        0,
                        0,
                        phonePanel.Width - 1,
                        phonePanel.Height - 1);
                };

            branchSection.Controls.Add(
                phonePanel);

            Label lblPrefix =
                new Label
                {
                    Text =
                        "+63",

                    ForeColor =
                        TextDark,

                    Font =
                        new Font(
                            "Segoe UI",
                            9.5F,
                            FontStyle.Bold),

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Location =
                        new Point(
                            0,
                            0),

                    Size =
                        new Size(
                            48,
                            32)
                };

            phonePanel.Controls.Add(
                lblPrefix);

            txtContact =
                new TextBox
                {
                    BorderStyle =
                        BorderStyle.None,

                    Font =
                        new Font(
                            "Segoe UI",
                            9.5F),

                    ForeColor =
                        TextDark,

                    Location =
                        new Point(
                            52,
                            6),

                    Width =
                        Math.Max(
                            100,
                            phonePanel.Width - 60),

                    MaxLength =
                        10
                };

            txtContact.KeyPress +=
                Contact_KeyPress;

            phonePanel.Controls.Add(
                txtContact);
        }

        // ============================================================
        // MANAGER SECTION
        // ============================================================

        private void BuildManagerSection()
        {
            managerSection =
                CreateSectionPanel(
                    "Branch Manager",
                    _isEdit
                        ? "Assign an existing Manager account to this branch."
                        : "Create a dedicated Manager account or assign an existing unassigned Manager.");

            managerSection.Location =
                new Point(
                    0,
                    branchSection.Bottom + 16);

            managerSection.Width =
                Math.Max(
                    560,
                    contentPanel.ClientSize.Width - 8);

            managerSection.Height =
                _isEdit
                    ? 170
                    : 395;

            managerSection.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            contentPanel.Controls.Add(
                managerSection);

            // --------------------------------------------------------
            // RADIO BUTTONS
            // --------------------------------------------------------

            rdoCreateNewManager =
                new RadioButton
                {
                    Text =
                        "Create New Manager",

                    AutoSize =
                        true,

                    Location =
                        new Point(
                            20,
                            61),

                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),

                    ForeColor =
                        TextDark
                };

            rdoExistingManager =
                new RadioButton
                {
                    Text =
                        "Use Existing Manager",

                    AutoSize =
                        true,

                    Location =
                        new Point(
                            210,
                            61),

                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),

                    ForeColor =
                        TextDark
                };

            rdoCreateNewManager.CheckedChanged +=
                (_, _) =>
                {
                    UpdateManagerMode();
                };

            rdoExistingManager.CheckedChanged +=
                (_, _) =>
                {
                    UpdateManagerMode();
                };

            managerSection.Controls.Add(
                rdoCreateNewManager);

            managerSection.Controls.Add(
                rdoExistingManager);

            // --------------------------------------------------------
            // NEW MANAGER PANEL
            // --------------------------------------------------------

            pnlNewManager =
                new Panel
                {
                    Location =
                        new Point(
                            18,
                            94),

                    Width =
                        managerSection.Width - 36,

                    Height =
                        280,

                    BackColor =
                        SoftBg,

                    BorderStyle =
                        BorderStyle.FixedSingle,

                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Left |
                        AnchorStyles.Right
                };

            managerSection.Controls.Add(
                pnlNewManager);

            BuildNewManagerFields();

            // --------------------------------------------------------
            // EXISTING MANAGER PANEL
            // --------------------------------------------------------

            pnlExistingManager =
                new Panel
                {
                    Location =
                        new Point(
                            18,
                            94),

                    Width =
                        managerSection.Width - 36,

                    Height =
                        82,

                    BackColor =
                        SoftBg,

                    BorderStyle =
                        BorderStyle.FixedSingle,

                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Left |
                        AnchorStyles.Right
                };

            managerSection.Controls.Add(
                pnlExistingManager);

            BuildExistingManagerField();
        }

        // ============================================================
        // NEW MANAGER FIELDS
        // ============================================================

        private void BuildNewManagerFields()
        {
            int left = 16;

            int width =
                pnlNewManager.Width - 32;

            // --------------------------------------------------------
            // FULL NAME
            // --------------------------------------------------------

            AddFieldLabel(
                pnlNewManager,
                "Manager Full Name",
                left,
                12);

            txtManagerFullName =
                CreateTextBox();

            txtManagerFullName.Location =
                new Point(
                    left,
                    32);

            txtManagerFullName.Width =
                width;

            pnlNewManager.Controls.Add(
                txtManagerFullName);

            // --------------------------------------------------------
            // USERNAME / EMAIL
            // --------------------------------------------------------

            int halfWidth =
                (width - 12) / 2;

            AddFieldLabel(
                pnlNewManager,
                "Username",
                left,
                72);

            txtManagerUsername =
                CreateTextBox();

            txtManagerUsername.Location =
                new Point(
                    left,
                    92);

            txtManagerUsername.Width =
                halfWidth;

            pnlNewManager.Controls.Add(
                txtManagerUsername);

            int right =
                left +
                halfWidth +
                12;

            AddFieldLabel(
                pnlNewManager,
                "Email Address",
                right,
                72);

            txtManagerEmail =
                CreateTextBox();

            txtManagerEmail.Location =
                new Point(
                    right,
                    92);

            txtManagerEmail.Width =
                halfWidth;

            pnlNewManager.Controls.Add(
                txtManagerEmail);

            // --------------------------------------------------------
            // PASSWORD
            // --------------------------------------------------------

            AddFieldLabel(
                pnlNewManager,
                "Password",
                left,
                132);

            txtManagerPassword =
                CreatePasswordBox();

            txtManagerPassword.Location =
                new Point(
                    left,
                    152);

            txtManagerPassword.Width =
                halfWidth;

            pnlNewManager.Controls.Add(
                txtManagerPassword);

            // --------------------------------------------------------
            // CONFIRM PASSWORD
            // --------------------------------------------------------

            AddFieldLabel(
                pnlNewManager,
                "Confirm Password",
                right,
                132);

            txtManagerConfirmPassword =
                CreatePasswordBox();

            txtManagerConfirmPassword.Location =
                new Point(
                    right,
                    152);

            txtManagerConfirmPassword.Width =
                halfWidth;

            pnlNewManager.Controls.Add(
                txtManagerConfirmPassword);

            // --------------------------------------------------------
            // PASSWORD REQUIREMENT NOTE
            // --------------------------------------------------------

            Label passwordNote =
                new Label
                {
                    AutoSize =
                        false,

                    Text =
                        "Password must be at least 8 characters and include uppercase, lowercase, number, and special character.",

                    ForeColor =
                        TextMuted,

                    Font =
                        new Font(
                            "Segoe UI",
                            8F),

                    Location =
                        new Point(
                            left,
                            192),

                    Size =
                        new Size(
                            width - 10,
                            32)
                };

            pnlNewManager.Controls.Add(
                passwordNote);

            // --------------------------------------------------------
            // INFORMATION NOTE
            // --------------------------------------------------------

            Label note =
                new Label
                {
                    AutoSize =
                        false,

                    Text =
                        "A new Manager account will be created and automatically assigned to this branch.",

                    ForeColor =
                        TextMuted,

                    Font =
                        new Font(
                            "Segoe UI",
                            8F),

                    Location =
                        new Point(
                            left,
                            225),

                    Size =
                        new Size(
                            width - 10,
                            42)
                };

            pnlNewManager.Controls.Add(
                note);
        }

        // ============================================================
        // EXISTING MANAGER
        // ============================================================

        private void BuildExistingManagerField()
        {
            int left = 16;

            int width =
                pnlExistingManager.Width - 32;

            AddFieldLabel(
                pnlExistingManager,
                "Manager Account",
                left,
                10);

            cmbManager =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,

                    Location =
                        new Point(
                            left,
                            30),

                    Width =
                        width,

                    Height =
                        32,

                    Font =
                        new Font(
                            "Segoe UI",
                            9.5F),

                    BackColor =
                        Color.White,

                    ForeColor =
                        TextDark
                };

            pnlExistingManager.Controls.Add(
                cmbManager);
        }

        // ============================================================
        // BOTTOM BUTTONS
        // ============================================================

        private void BuildBottomButtons()
        {
            buttonPanel =
                new Panel
                {
                    Dock =
                        DockStyle.Bottom,

                    Height =
                        70,

                    BackColor =
                        PanelBg
                };

            mainPanel.Controls.Add(
                buttonPanel);

            // --------------------------------------------------------
            // TOP SEPARATOR
            // --------------------------------------------------------

            Panel separator =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        1,

                    BackColor =
                        CardBorder
                };

            buttonPanel.Controls.Add(
                separator);

            // --------------------------------------------------------
            // CANCEL
            // --------------------------------------------------------

            btnCancel =
                new Button
                {
                    Text =
                        "Cancel",

                    Width =
                        105,

                    Height =
                        38,

                    FlatStyle =
                        FlatStyle.Flat,

                    BackColor =
                        Color.White,

                    ForeColor =
                        TextDark,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),

                    Cursor =
                        Cursors.Hand
                };

            btnCancel.FlatAppearance.BorderSize =
                1;

            btnCancel.FlatAppearance.BorderColor =
                CardBorder;

            buttonPanel.Controls.Add(
                btnCancel);

            // --------------------------------------------------------
            // SAVE
            // --------------------------------------------------------

            btnSave =
                new Button
                {
                    Text =
                        _isEdit
                            ? "Save Changes"
                            : "Create Branch",

                    Width =
                        125,

                    Height =
                        38,

                    FlatStyle =
                        FlatStyle.Flat,

                    BackColor =
                        CtaColor,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),

                    Cursor =
                        Cursors.Hand
                };

            btnSave.FlatAppearance.BorderSize =
                0;

            btnSave.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(
                    187,
                    145,
                    92);

            btnSave.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(
                    150,
                    112,
                    68);

            buttonPanel.Controls.Add(
                btnSave);

            // --------------------------------------------------------
            // RESIZE ALIGNMENT
            // --------------------------------------------------------

            buttonPanel.Resize +=
                (_, _) =>
                {
                    AlignBottomButtons();
                };

            AlignBottomButtons();

            // --------------------------------------------------------
            // EVENTS
            // --------------------------------------------------------

            btnCancel.Click +=
                (_, _) =>
                {
                    DialogResult =
                        DialogResult.Cancel;

                    Close();
                };

            btnSave.Click +=
                (_, _) =>
                {
                    SaveForm();
                };
        }

        // ============================================================
        // ALIGN BOTTOM BUTTONS
        // ============================================================

        private void AlignBottomButtons()
        {
            if (buttonPanel == null ||
                btnCancel == null ||
                btnSave == null)
            {
                return;
            }

            int gap = 10;

            btnSave.Left =
                buttonPanel.ClientSize.Width -
                btnSave.Width -
                2;

            btnSave.Top =
                18;

            btnCancel.Left =
                btnSave.Left -
                gap -
                btnCancel.Width;

            btnCancel.Top =
                18;
        }

        // ============================================================
        // SECTION PANEL
        // ============================================================

        private Panel CreateSectionPanel(
            string title,
            string subtitle)
        {
            Panel section =
                new Panel
                {
                    BackColor =
                        CardBg,

                    BorderStyle =
                        BorderStyle.FixedSingle
                };

            Label lblTitle =
                new Label
                {
                    AutoSize =
                        false,

                    Text =
                        title,

                    ForeColor =
                        TextDark,

                    Font =
                        new Font(
                            "Segoe UI",
                            11F,
                            FontStyle.Bold),

                    Location =
                        new Point(
                            18,
                            12),

                    Size =
                        new Size(
                            520,
                            22)
                };

            section.Controls.Add(
                lblTitle);

            Label lblSubtitle =
                new Label
                {
                    AutoSize =
                        false,

                    Text =
                        subtitle,

                    ForeColor =
                        TextMuted,

                    Font =
                        new Font(
                            "Segoe UI",
                            8F),

                    Location =
                        new Point(
                            19,
                            36),

                    Size =
                        new Size(
                            560,
                            18)
                };

            section.Controls.Add(
                lblSubtitle);

            return section;
        }

        // ============================================================
        // FIELD LABEL
        // ============================================================

        private void AddFieldLabel(
            Control parent,
            string text,
            int x,
            int y)
        {
            Label label =
                new Label
                {
                    AutoSize =
                        false,

                    Text =
                        text,

                    ForeColor =
                        TextMuted,

                    Font =
                        new Font(
                            "Segoe UI",
                            7.5F,
                            FontStyle.Bold),

                    Location =
                        new Point(
                            x,
                            y),

                    Size =
                        new Size(
                            250,
                            18)
                };

            parent.Controls.Add(
                label);
        }

        // ============================================================
        // TEXTBOX
        // ============================================================

        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Height =
                    32,

                BorderStyle =
                    BorderStyle.FixedSingle,

                Font =
                    new Font(
                        "Segoe UI",
                        9.5F),

                BackColor =
                    Color.White,

                ForeColor =
                    TextDark
            };
        }

        // ============================================================
        // PASSWORD BOX
        // ============================================================

        private TextBox CreatePasswordBox()
        {
            TextBox box =
                CreateTextBox();

            box.UseSystemPasswordChar =
                true;

            return box;
        }

        // ============================================================
        // LOAD MANAGER OPTIONS
        // ============================================================

        private void LoadManagerOptions()
        {
            if (cmbManager == null)
            {
                return;
            }

            cmbManager.Items.Clear();

            // --------------------------------------------------------
            // UNASSIGNED OPTION
            // --------------------------------------------------------

            cmbManager.Items.Add(
                new ManagerSelectionItem
                {
                    Id = null,
                    Name = "Unassigned"
                });

            // --------------------------------------------------------
            // AVAILABLE MANAGERS
            //
            // A Manager can be selected when:
            //
            // 1. The Manager has no BranchId.
            // 2. The Manager is already assigned to this branch.
            //
            // A Manager assigned to another branch is not shown.
            // This prevents one Manager from being assigned to
            // multiple branches.
            // --------------------------------------------------------

            IEnumerable<ManagerDto> available =
                _managers
                    .Where(
                        manager =>
                            manager.BranchId == null ||
                            (
                                _branch != null &&
                                manager.BranchId ==
                                _branch.Id
                            ))
                    .OrderBy(
                        manager =>
                            manager.DisplayName);

            foreach (ManagerDto manager in available)
            {
                string displayName =
                    manager.DisplayName;

                // ----------------------------------------------------
                // Show email when available.
                // This makes it easier to distinguish Managers
                // with similar names.
                // ----------------------------------------------------

                if (!string.IsNullOrWhiteSpace(
                    manager.Email))
                {
                    displayName +=
                        "  •  " +
                        manager.Email;
                }

                cmbManager.Items.Add(
                    new ManagerSelectionItem
                    {
                        Id =
                            manager.Id,

                        Name =
                            displayName
                    });
            }

            // --------------------------------------------------------
            // DEFAULT
            // --------------------------------------------------------

            if (cmbManager.Items.Count > 0)
            {
                cmbManager.SelectedIndex = 0;
            }

            // --------------------------------------------------------
            // EDIT MODE
            //
            // Automatically select the Manager currently assigned
            // to this branch.
            // --------------------------------------------------------

            if (_branch != null)
            {
                ManagerDto? currentManager =
                    _managers.FirstOrDefault(
                        manager =>
                            manager.BranchId ==
                            _branch.Id);

                if (currentManager != null)
                {
                    for (int i = 0;
                         i < cmbManager.Items.Count;
                         i++)
                    {
                        if (cmbManager.Items[i]
                            is ManagerSelectionItem item &&
                            item.Id ==
                            currentManager.Id)
                        {
                            cmbManager.SelectedIndex =
                                i;

                            break;
                        }
                    }
                }
            }
        }

        // ============================================================
        // LOAD EXISTING BRANCH
        // ============================================================

        private void LoadExistingBranch()
        {
            if (_branch == null)
            {
                return;
            }

            txtBranchName.Text =
                _branch.BranchName ??
                string.Empty;

            ParseExistingAddress(
                _branch.Address);

            LoadExistingPhone(
                _branch.ContactNumber);
        }

        // ============================================================
        // ADD MODE
        // ============================================================

        private void SetAddMode()
        {
            rdoCreateNewManager.Visible =
                true;

            rdoExistingManager.Visible =
                true;

            // --------------------------------------------------------
            // DEFAULT TO EXISTING MANAGER
            //
            // This makes the normal branch workflow:
            //
            // Add Branch
            //      ↓
            // Use Existing Manager
            //      ↓
            // Select an unassigned Manager
            // --------------------------------------------------------

            rdoCreateNewManager.Checked =
                false;

            rdoExistingManager.Checked =
                true;

            UpdateManagerMode();
        }

        // ============================================================
        // EDIT MODE
        // ============================================================

        private void SetEditMode()
        {
            rdoCreateNewManager.Visible =
                false;

            rdoExistingManager.Visible =
                false;

            pnlNewManager.Visible =
                false;

            pnlExistingManager.Visible =
                true;

            pnlExistingManager.Location =
                new Point(
                    18,
                    78);

            managerSection.Height =
                155;

            UpdateContentLayout();
        }

        // ============================================================
        // MANAGER MODE
        // ============================================================

        private void UpdateManagerMode()
        {
            if (_isEdit)
            {
                pnlNewManager.Visible =
                    false;

                pnlExistingManager.Visible =
                    true;

                return;
            }

            bool createNew =
                rdoCreateNewManager.Checked;

            pnlNewManager.Visible =
                createNew;

            pnlExistingManager.Visible =
                !createNew;

            UpdateContentLayout();
        }

        // ============================================================
        // UPDATE CONTENT LAYOUT
        // ============================================================

        private void UpdateContentLayout()
        {
            if (branchSection == null ||
                managerSection == null ||
                contentPanel == null)
            {
                return;
            }

            managerSection.Location =
                new Point(
                    0,
                    branchSection.Bottom + 16);

            if (_isEdit)
            {
                managerSection.Height =
                    155;
            }
            else if (rdoCreateNewManager != null &&
                     rdoCreateNewManager.Checked)
            {
                managerSection.Height =
                    395;
            }
            else
            {
                managerSection.Height =
                    205;
            }

            int totalHeight =
                managerSection.Bottom + 20;

            contentPanel.AutoScrollMinSize =
                new Size(
                    0,
                    totalHeight);
        }

        // ============================================================
        // SAVE
        // ============================================================

        private void SaveForm()
        {
            if (!ValidateBranch())
            {
                return;
            }

            if (!ValidateContact())
            {
                return;
            }

            if (!_isEdit &&
                !ValidateManager())
            {
                return;
            }

            // ========================================================
            // BRANCH
            // ========================================================

            BranchName =
                txtBranchName.Text.Trim();

            BranchAddress =
                BuildAddress();

            BranchContactNumber =
                "+63" +
                txtContact.Text.Trim();

            // ========================================================
            // MANAGER
            // ========================================================

            if (!_isEdit &&
                rdoCreateNewManager.Checked)
            {
                CreateNewManager =
                    true;

                ManagerFullName =
                    txtManagerFullName.Text.Trim();

                ManagerUsername =
                    txtManagerUsername.Text.Trim();

                ManagerEmail =
                    txtManagerEmail.Text.Trim();

                ManagerPassword =
                    txtManagerPassword.Text;

                ManagerConfirmPassword =
                    txtManagerConfirmPassword.Text;

                SelectedManagerId =
                    null;
            }
            else
            {
                CreateNewManager =
                    false;

                ManagerFullName =
                    string.Empty;

                ManagerUsername =
                    string.Empty;

                ManagerEmail =
                    string.Empty;

                ManagerPassword =
                    string.Empty;

                ManagerConfirmPassword =
                    string.Empty;

                SelectedManagerId =
                    GetSelectedManagerId();
            }

            DialogResult =
                DialogResult.OK;

            Close();
        }

        // ============================================================
        // VALIDATE BRANCH
        // ============================================================

        private bool ValidateBranch()
        {
            if (string.IsNullOrWhiteSpace(
                txtBranchName.Text))
            {
                ShowValidation(
                    "Please enter the branch name.",
                    txtBranchName);

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                txtStreet.Text))
            {
                ShowValidation(
                    "Please enter the street or purok.",
                    txtStreet);

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                txtBarangay.Text))
            {
                ShowValidation(
                    "Please enter the barangay.",
                    txtBarangay);

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                txtCity.Text))
            {
                ShowValidation(
                    "Please enter the city or municipality.",
                    txtCity);

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                txtProvince.Text))
            {
                ShowValidation(
                    "Please enter the province.",
                    txtProvince);

                return false;
            }

            string postal =
                txtPostalCode.Text.Trim();

            if (postal.Length != 4 ||
                !postal.All(char.IsDigit))
            {
                ShowValidation(
                    "Please enter a valid 4-digit postal code.",
                    txtPostalCode);

                return false;
            }

            return true;
        }

        // ============================================================
        // VALIDATE CONTACT
        // ============================================================

        private bool ValidateContact()
        {
            string contact =
                txtContact.Text.Trim();

            if (contact.Length != 10)
            {
                ShowValidation(
                    "Please enter a 10-digit Philippine mobile number after +63.",
                    txtContact);

                return false;
            }

            if (!contact.All(
                char.IsDigit))
            {
                ShowValidation(
                    "The contact number must contain digits only.",
                    txtContact);

                return false;
            }

            if (!contact.StartsWith("9"))
            {
                ShowValidation(
                    "A Philippine mobile number after +63 should start with 9.",
                    txtContact);

                return false;
            }

            return true;
        }

        // ============================================================
        // VALIDATE MANAGER
        // ============================================================

        private bool ValidateManager()
        {
            // --------------------------------------------------------
            // EXISTING MANAGER
            // --------------------------------------------------------

            if (rdoExistingManager.Checked)
            {
                return true;
            }

            // --------------------------------------------------------
            // NEW MANAGER
            // --------------------------------------------------------

            string fullName =
                txtManagerFullName.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                fullName))
            {
                ShowValidation(
                    "Please enter the Manager's full name.",
                    txtManagerFullName);

                return false;
            }

            // --------------------------------------------------------
            // USERNAME
            // --------------------------------------------------------

            string username =
                txtManagerUsername.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                username))
            {
                ShowValidation(
                    "Please enter a Manager username.",
                    txtManagerUsername);

                return false;
            }

            if (username.Length < 3)
            {
                ShowValidation(
                    "The Manager username must contain at least 3 characters.",
                    txtManagerUsername);

                return false;
            }

            // --------------------------------------------------------
            // EMAIL
            // --------------------------------------------------------

            string email =
                txtManagerEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                email))
            {
                ShowValidation(
                    "Please enter the Manager's email address.",
                    txtManagerEmail);

                return false;
            }

            try
            {
                _ =
                    new MailAddress(
                        email);
            }
            catch
            {
                ShowValidation(
                    "Please enter a valid email address.",
                    txtManagerEmail);

                return false;
            }

            // --------------------------------------------------------
            // PASSWORD
            //
            // Must match Program.cs:
            //
            // RequireDigit = true
            // RequireLowercase = true
            // RequireUppercase = true
            // RequireNonAlphanumeric = true
            // RequiredLength = 8
            // --------------------------------------------------------

            string password =
                txtManagerPassword.Text;

            if (string.IsNullOrWhiteSpace(
                password))
            {
                ShowValidation(
                    "Please enter a password for the Manager account.",
                    txtManagerPassword);

                return false;
            }

            if (password.Length < 8)
            {
                ShowValidation(
                    "The Manager password must contain at least 8 characters.",
                    txtManagerPassword);

                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                ShowValidation(
                    "The Manager password must contain at least one uppercase letter (A-Z).",
                    txtManagerPassword);

                return false;
            }

            if (!password.Any(char.IsLower))
            {
                ShowValidation(
                    "The Manager password must contain at least one lowercase letter (a-z).",
                    txtManagerPassword);

                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                ShowValidation(
                    "The Manager password must contain at least one number (0-9).",
                    txtManagerPassword);

                return false;
            }

            if (!password.Any(
                character =>
                    !char.IsLetterOrDigit(character)))
            {
                ShowValidation(
                    "The Manager password must contain at least one special character, such as !, @, #, or $.",
                    txtManagerPassword);

                return false;
            }

            // --------------------------------------------------------
            // CONFIRM PASSWORD
            // --------------------------------------------------------

            string confirmPassword =
                txtManagerConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(
                confirmPassword))
            {
                ShowValidation(
                    "Please confirm the Manager password.",
                    txtManagerConfirmPassword);

                return false;
            }

            if (password != confirmPassword)
            {
                ShowValidation(
                    "The Manager passwords do not match.",
                    txtManagerConfirmPassword);

                return false;
            }

            return true;
        }

        // ============================================================
        // BUILD ADDRESS
        // ============================================================

        private string BuildAddress()
        {
            List<string> parts =
                new List<string>();

            if (!string.IsNullOrWhiteSpace(
                txtStreet.Text))
            {
                parts.Add(
                    txtStreet.Text.Trim());
            }

            if (!string.IsNullOrWhiteSpace(
                txtBarangay.Text))
            {
                parts.Add(
                    "Brgy. " +
                    txtBarangay.Text.Trim());
            }

            if (!string.IsNullOrWhiteSpace(
                txtCity.Text))
            {
                parts.Add(
                    txtCity.Text.Trim());
            }

            if (!string.IsNullOrWhiteSpace(
                txtProvince.Text))
            {
                parts.Add(
                    txtProvince.Text.Trim());
            }

            if (!string.IsNullOrWhiteSpace(
                txtPostalCode.Text))
            {
                parts.Add(
                    txtPostalCode.Text.Trim());
            }

            return string.Join(
                ", ",
                parts);
        }

        // ============================================================
        // PARSE EXISTING ADDRESS
        // ============================================================

        private void ParseExistingAddress(
            string? address)
        {
            if (string.IsNullOrWhiteSpace(
                address))
            {
                return;
            }

            string[] parts =
                address
                    .Split(
                        ',',
                        StringSplitOptions
                            .RemoveEmptyEntries)
                    .Select(
                        x =>
                            x.Trim())
                    .ToArray();

            if (parts.Length > 0)
            {
                txtStreet.Text =
                    parts[0];
            }

            if (parts.Length > 1)
            {
                txtBarangay.Text =
                    parts[1]
                        .Replace(
                            "Brgy.",
                            "",
                            StringComparison
                                .OrdinalIgnoreCase)
                        .Trim();
            }

            if (parts.Length > 2)
            {
                txtCity.Text =
                    parts[2];
            }

            if (parts.Length > 3)
            {
                txtProvince.Text =
                    parts[3];
            }

            if (parts.Length > 4)
            {
                string postal =
                    parts[4];

                if (postal.Length == 4 &&
                    postal.All(
                        char.IsDigit))
                {
                    txtPostalCode.Text =
                        postal;
                }
            }
        }

        // ============================================================
        // LOAD EXISTING PHONE
        // ============================================================

        private void LoadExistingPhone(
            string? phone)
        {
            if (string.IsNullOrWhiteSpace(
                phone))
            {
                return;
            }

            string value =
                phone.Trim();

            if (value.StartsWith(
                "+63"))
            {
                value =
                    value.Substring(3);
            }
            else if (value.StartsWith(
                "63"))
            {
                value =
                    value.Substring(2);
            }
            else if (value.StartsWith(
                "0"))
            {
                value =
                    value.Substring(1);
            }

            txtContact.Text =
                value;
        }

        // ============================================================
        // SELECTED MANAGER
        // ============================================================

        private string? GetSelectedManagerId()
        {
            if (cmbManager.SelectedItem
                is ManagerSelectionItem item)
            {
                return item.Id;
            }

            return null;
        }

        // ============================================================
        // VALIDATION MESSAGE
        // ============================================================

        private void ShowValidation(
            string message,
            Control control)
        {
            MessageBox.Show(
                message,
                "Branch Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            control.Focus();

            if (control is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        // ============================================================
        // NUMERIC INPUT
        // ============================================================

        private void PostalCode_KeyPress(
            object? sender,
            KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) &&
                !char.IsDigit(e.KeyChar))
            {
                e.Handled =
                    true;
            }
        }

        private void Contact_KeyPress(
            object? sender,
            KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) &&
                !char.IsDigit(e.KeyChar))
            {
                e.Handled =
                    true;
            }
        }

        // ============================================================
        // MANAGER SELECTION MODEL
        // ============================================================

        private class ManagerSelectionItem
        {
            public string? Id { get; set; }

            public string Name { get; set; } =
                string.Empty;

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
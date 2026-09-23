using PBCRM2.WinForms.Services;
using PBCRM2.WinForms.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace PBCRM2
{
    // =========================================================
    // ADMIN DASHBOARD CHART TYPES
    // =========================================================

    public enum AdminDashboardChartType
    {
        Bar,
        Donut,
        Line
    }

    // =========================================================
    // ADMIN DASHBOARD CHART ITEM
    // =========================================================

    public class AdminDashboardChartItem
    {
        public string Label { get; set; } = string.Empty;

        public decimal Value { get; set; }

        public Color ItemColor { get; set; } = Color.Gray;

        public AdminDashboardChartItem(
            string label,
            decimal value,
            Color color)
        {
            this.Label = label ?? string.Empty;
            this.Value = value;
            this.ItemColor = color;
        }
    }

    // =========================================================
    // ADMIN DASHBOARD CHART PANEL
    // =========================================================

    public class AdminDashboardChartPanel : Panel
    {
        private readonly List<AdminDashboardChartItem> items;

        private string title;
        private string subtitle;

        private AdminDashboardChartType chartType;

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.title = value ?? string.Empty;
                this.Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public string Subtitle
        {
            get
            {
                return this.subtitle;
            }

            set
            {
                this.subtitle = value ?? string.Empty;
                this.Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public AdminDashboardChartType ChartType
        {
            get
            {
                return this.chartType;
            }

            set
            {
                this.chartType = value;
                this.Invalidate();
            }
        }

        public AdminDashboardChartPanel()
        {
            this.items =
                new List<AdminDashboardChartItem>();

            this.title =
                string.Empty;

            this.subtitle =
                string.Empty;

            this.chartType =
                AdminDashboardChartType.Bar;

            this.DoubleBuffered =
                true;

            this.ResizeRedraw =
                true;

            this.BackColor =
                Color.White;

            this.Margin =
                new Padding(0);
        }

        public void SetItems(
            IEnumerable<AdminDashboardChartItem> newItems)
        {
            this.items.Clear();

            if (newItems != null)
            {
                this.items.AddRange(newItems);
            }

            this.Invalidate();
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics =
                e.Graphics;

            graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            graphics.TextRenderingHint =
                System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            graphics.Clear(
                this.BackColor);

            using (Pen borderPen =
                new Pen(
                    Color.FromArgb(
                        225,
                        215,
                        200)))
            {
                graphics.DrawRectangle(
                    borderPen,
                    0,
                    0,
                    Math.Max(
                        0,
                        this.Width - 1),
                    Math.Max(
                        0,
                        this.Height - 1));
            }

            this.DrawHeader(
                graphics);

            if (this.items.Count == 0)
            {
                this.DrawEmptyMessage(
                    graphics);

                return;
            }

            switch (this.chartType)
            {
                case AdminDashboardChartType.Donut:
                    this.DrawDonut(graphics);
                    break;

                case AdminDashboardChartType.Line:
                    this.DrawLine(graphics);
                    break;

                default:
                    this.DrawBar(graphics);
                    break;
            }
        }

        // =========================================================
        // HEADER
        // =========================================================

        private void DrawHeader(
            Graphics graphics)
        {
            using Font titleFont =
                new Font(
                    "Segoe UI",
                    13,
                    FontStyle.Bold);

            using Font subtitleFont =
                new Font(
                    "Segoe UI",
                    8.5f,
                    FontStyle.Regular);

            using SolidBrush titleBrush =
                new SolidBrush(
                    Color.FromArgb(
                        55,
                        39,
                        20));

            using SolidBrush subtitleBrush =
                new SolidBrush(
                    Color.FromArgb(
                        120,
                        120,
                        120));

            graphics.DrawString(
                this.title,
                titleFont,
                titleBrush,
                20,
                16);

            graphics.DrawString(
                this.subtitle,
                subtitleFont,
                subtitleBrush,
                21,
                43);
        }

        // =========================================================
        // BAR CHART
        // =========================================================

        private void DrawBar(
            Graphics graphics)
        {
            int left = 48;
            int right = 22;
            int top = 70;
            int bottom = 48;

            int width =
                this.ClientSize.Width -
                left -
                right;

            int height =
                this.ClientSize.Height -
                top -
                bottom;

            if (width <= 50 ||
                height <= 50)
            {
                return;
            }

            decimal maxValue =
                this.items
                    .Select(x => x.Value)
                    .DefaultIfEmpty(0)
                    .Max();

            if (maxValue <= 0)
            {
                maxValue = 1;
            }

            using Pen axisPen =
                new Pen(
                    Color.FromArgb(
                        225,
                        215,
                        200),
                    1);

            using Pen gridPen =
                new Pen(
                    Color.FromArgb(
                        238,
                        232,
                        225),
                    1);

            using Font labelFont =
                new Font(
                    "Segoe UI",
                    8f);

            using Font valueFont =
                new Font(
                    "Segoe UI",
                    8.5f,
                    FontStyle.Bold);

            using SolidBrush labelBrush =
                new SolidBrush(
                    Color.FromArgb(
                        100,
                        100,
                        100));

            using SolidBrush valueBrush =
                new SolidBrush(
                    Color.FromArgb(
                        55,
                        39,
                        20));

            for (int g = 0; g <= 4; g++)
            {
                float gy =
                    top +
                    height *
                    (1f - g / 4f);

                graphics.DrawLine(
                    gridPen,
                    left,
                    gy,
                    left + width,
                    gy);
            }

            graphics.DrawLine(
                axisPen,
                left,
                top + height,
                left + width,
                top + height);

            int count =
                this.items.Count;

            float slotWidth =
                (float)width /
                Math.Max(count, 1);

            float barWidth =
                Math.Min(
                    95,
                    slotWidth * 0.70f);

            for (int i = 0;
                 i < count;
                 i++)
            {
                AdminDashboardChartItem item =
                    this.items[i];

                float barHeight =
                    (float)(
                        item.Value /
                        maxValue *
                        height);

                if (barHeight < 0)
                {
                    barHeight = 0;
                }

                if (item.Value > 0 &&
                    barHeight < 5)
                {
                    barHeight = 5;
                }

                float x =
                    left +
                    slotWidth * i +
                    (slotWidth - barWidth) / 2;

                float y =
                    top +
                    height -
                    barHeight;

                using SolidBrush barBrush =
                    new SolidBrush(
                        item.ItemColor);

                graphics.FillRectangle(
                    barBrush,
                    x,
                    y,
                    barWidth,
                    barHeight);

                string valueText =
                    item.Value.ToString("0");

                SizeF valueSize =
                    graphics.MeasureString(
                        valueText,
                        valueFont);

                graphics.DrawString(
                    valueText,
                    valueFont,
                    valueBrush,
                    x +
                    (barWidth -
                     valueSize.Width) / 2,
                    Math.Max(
                        top - 2,
                        y -
                        valueSize.Height -
                        3));

                string label =
                    item.Label;

                SizeF labelSize =
                    graphics.MeasureString(
                        label,
                        labelFont);

                graphics.DrawString(
                    label,
                    labelFont,
                    labelBrush,
                    x +
                    (barWidth -
                     labelSize.Width) / 2,
                    top +
                    height +
                    8);
            }
        }

        // =========================================================
        // DONUT CHART
        // =========================================================

        private void DrawDonut(
            Graphics graphics)
        {
            int headerReserve = 72;
            int bottomPadding = 16;
            int sidePadding = 18;

            int availableHeight =
                Math.Max(
                    120,
                    this.ClientSize.Height -
                    headerReserve -
                    bottomPadding);

            int availableWidth =
                Math.Max(
                    180,
                    this.ClientSize.Width -
                    sidePadding * 2);

            int legendWidth =
                Math.Min(
                    130,
                    Math.Max(
                        100,
                        availableWidth / 3));

            int gapBetween = 24;

            int maxDonut =
                Math.Min(
                    availableHeight,
                    availableWidth -
                    legendWidth -
                    gapBetween);

            int diameter =
                Math.Min(
                    maxDonut,
                    240);

            diameter =
                Math.Max(
                    diameter,
                    110);

            int blockWidth =
                diameter +
                gapBetween +
                legendWidth;

            int chartX =
                sidePadding +
                Math.Max(
                    0,
                    (availableWidth -
                     blockWidth) / 2);

            int chartY =
                headerReserve +
                Math.Max(
                    0,
                    (availableHeight -
                     diameter) / 2);

            decimal total =
                this.items
                    .Select(x => x.Value)
                    .Where(x => x > 0)
                    .Sum();

            if (total <= 0)
            {
                using Pen emptyPen =
                    new Pen(
                        Color.FromArgb(
                            225,
                            215,
                            200),
                        22);

                graphics.DrawEllipse(
                    emptyPen,
                    chartX + 11,
                    chartY + 11,
                    diameter - 22,
                    diameter - 22);

                using Font emptyFont =
                    new Font(
                        "Segoe UI",
                        8.5f,
                        FontStyle.Bold);

                using SolidBrush emptyBrush =
                    new SolidBrush(
                        Color.FromArgb(
                            130,
                            130,
                            130));

                string text =
                    "No data";

                SizeF size =
                    graphics.MeasureString(
                        text,
                        emptyFont);

                graphics.DrawString(
                    text,
                    emptyFont,
                    emptyBrush,
                    chartX +
                    (diameter -
                     size.Width) / 2,
                    chartY +
                    (diameter -
                     size.Height) / 2);

                return;
            }

            float startAngle = -90f;

            foreach (
                AdminDashboardChartItem item
                in this.items)
            {
                if (item.Value <= 0)
                {
                    continue;
                }

                float sweepAngle =
                    (float)(
                        item.Value /
                        total *
                        360m);

                using SolidBrush brush =
                    new SolidBrush(
                        item.ItemColor);

                graphics.FillPie(
                    brush,
                    chartX,
                    chartY,
                    diameter,
                    diameter,
                    startAngle,
                    sweepAngle);

                startAngle +=
                    sweepAngle;
            }

            int holeSize =
                (int)(
                    diameter *
                    0.56);

            int holeX =
                chartX +
                (diameter -
                 holeSize) / 2;

            int holeY =
                chartY +
                (diameter -
                 holeSize) / 2;

            using SolidBrush holeBrush =
                new SolidBrush(
                    Color.White);

            graphics.FillEllipse(
                holeBrush,
                holeX,
                holeY,
                holeSize,
                holeSize);

            decimal totalValue =
                this.items
                    .Select(x => x.Value)
                    .Sum();

            float centerFontSize =
                diameter >= 180
                    ? 16f
                    : 13f;

            using Font centerFont =
                new Font(
                    "Segoe UI",
                    centerFontSize,
                    FontStyle.Bold);

            using SolidBrush centerBrush =
                new SolidBrush(
                    Color.FromArgb(
                        55,
                        39,
                        20));

            string centerText =
                totalValue.ToString("0");

            SizeF centerSize =
                graphics.MeasureString(
                    centerText,
                    centerFont);

            graphics.DrawString(
                centerText,
                centerFont,
                centerBrush,
                chartX +
                (diameter -
                 centerSize.Width) / 2,
                chartY +
                (diameter -
                 centerSize.Height) / 2);

            using Font legendFont =
                new Font(
                    "Segoe UI",
                    9f);

            using SolidBrush legendTextBrush =
                new SolidBrush(
                    Color.FromArgb(
                        80,
                        80,
                        80));

            int legendX =
                chartX +
                diameter +
                gapBetween;

            int legendItemHeight = 36;

            int legendBlockHeight =
                this.items.Count *
                legendItemHeight;

            int legendY =
                chartY +
                Math.Max(
                    0,
                    (diameter -
                     legendBlockHeight) / 2);

            foreach (
                AdminDashboardChartItem item
                in this.items)
            {
                using SolidBrush colorBrush =
                    new SolidBrush(
                        item.ItemColor);

                graphics.FillRectangle(
                    colorBrush,
                    legendX,
                    legendY + 3,
                    12,
                    12);

                string percentage =
                    total > 0
                        ? (
                            item.Value /
                            total *
                            100m
                          ).ToString("0") +
                          "%"
                        : "0%";

                string legend =
                    item.Label +
                    "  " +
                    percentage;

                graphics.DrawString(
                    legend,
                    legendFont,
                    legendTextBrush,
                    legendX + 20,
                    legendY);

                legendY +=
                    legendItemHeight;
            }
        }

        // =========================================================
        // LINE CHART
        // =========================================================

        private void DrawLine(
            Graphics graphics)
        {
            int left = 50;
            int right = 22;
            int top = 72;
            int bottom = 48;

            int width =
                this.ClientSize.Width -
                left -
                right;

            int height =
                this.ClientSize.Height -
                top -
                bottom;

            if (width <= 60 ||
                height <= 60)
            {
                return;
            }

            decimal maxValue =
                this.items
                    .Select(x => x.Value)
                    .DefaultIfEmpty(0)
                    .Max();

            if (maxValue <= 0)
            {
                maxValue = 1;
            }

            using Pen gridPen =
                new Pen(
                    Color.FromArgb(
                        238,
                        232,
                        225),
                    1);

            using Pen linePen =
                new Pen(
                    Color.FromArgb(
                        170,
                        130,
                        80),
                    3);

            using SolidBrush pointBrush =
                new SolidBrush(
                    Color.FromArgb(
                        170,
                        130,
                        80));

            using SolidBrush valueBrush =
                new SolidBrush(
                    Color.FromArgb(
                        55,
                        39,
                        20));

            using SolidBrush labelBrush =
                new SolidBrush(
                    Color.FromArgb(
                        105,
                        105,
                        105));

            using Font labelFont =
                new Font(
                    "Segoe UI",
                    7.5f);

            using Font valueFont =
                new Font(
                    "Segoe UI",
                    7.5f,
                    FontStyle.Bold);

            for (int i = 0;
                 i <= 4;
                 i++)
            {
                float y =
                    top +
                    height *
                    i /
                    4f;

                graphics.DrawLine(
                    gridPen,
                    left,
                    y,
                    left + width,
                    y);
            }

            if (this.items.Count == 1)
            {
                this.DrawSingleLinePoint(
                    graphics,
                    left,
                    top,
                    width,
                    height,
                    maxValue,
                    this.items[0],
                    pointBrush,
                    valueBrush,
                    labelBrush,
                    labelFont,
                    valueFont);

                return;
            }

            PointF[] points =
                new PointF[
                    this.items.Count];

            for (int i = 0;
                 i < this.items.Count;
                 i++)
            {
                decimal value =
                    this.items[i].Value;

                float x =
                    left +
                    width *
                    i /
                    (float)(
                        this.items.Count - 1);

                float y =
                    top +
                    height -
                    (
                        (float)(
                            value /
                            maxValue) *
                        height
                    );

                points[i] =
                    new PointF(
                        x,
                        y);
            }

            if (points.Length >= 2)
            {
                graphics.DrawLines(
                    linePen,
                    points);
            }

            for (int i = 0;
                 i < points.Length;
                 i++)
            {
                PointF point =
                    points[i];

                graphics.FillEllipse(
                    pointBrush,
                    point.X - 4,
                    point.Y - 4,
                    8,
                    8);

                string valueText =
                    this.items[i]
                        .Value
                        .ToString("0");

                SizeF valueSize =
                    graphics.MeasureString(
                        valueText,
                        valueFont);

                graphics.DrawString(
                    valueText,
                    valueFont,
                    valueBrush,
                    point.X -
                    valueSize.Width / 2,
                    point.Y -
                    valueSize.Height -
                    7);

                string label =
                    this.items[i].Label;

                if (!string.IsNullOrWhiteSpace(
                    label))
                {
                    SizeF labelSize =
                        graphics.MeasureString(
                            label,
                            labelFont);

                    graphics.DrawString(
                        label,
                        labelFont,
                        labelBrush,
                        point.X -
                        labelSize.Width / 2,
                        top +
                        height +
                        8);
                }
            }
        }

        // =========================================================
        // SINGLE LINE POINT
        // =========================================================

        private void DrawSingleLinePoint(
            Graphics graphics,
            int left,
            int top,
            int width,
            int height,
            decimal maxValue,
            AdminDashboardChartItem item,
            SolidBrush pointBrush,
            SolidBrush valueBrush,
            SolidBrush labelBrush,
            Font labelFont,
            Font valueFont)
        {
            float x =
                left +
                width / 2f;

            float y =
                top +
                height -
                (
                    (float)(
                        item.Value /
                        maxValue) *
                    height
                );

            graphics.FillEllipse(
                pointBrush,
                x - 5,
                y - 5,
                10,
                10);

            string valueText =
                item.Value.ToString("0");

            SizeF valueSize =
                graphics.MeasureString(
                    valueText,
                    valueFont);

            graphics.DrawString(
                valueText,
                valueFont,
                valueBrush,
                x -
                valueSize.Width / 2,
                y -
                valueSize.Height -
                8);

            if (!string.IsNullOrWhiteSpace(
                item.Label))
            {
                SizeF labelSize =
                    graphics.MeasureString(
                        item.Label,
                        labelFont);

                graphics.DrawString(
                    item.Label,
                    labelFont,
                    labelBrush,
                    x -
                    labelSize.Width / 2,
                    top +
                    height +
                    8);
            }
        }

        // =========================================================
        // EMPTY MESSAGE
        // =========================================================

        private void DrawEmptyMessage(
            Graphics graphics)
        {
            using Font font =
                new Font(
                    "Segoe UI",
                    9.5f);

            using SolidBrush brush =
                new SolidBrush(
                    Color.FromArgb(
                        140,
                        140,
                        140));

            string message =
                "No dashboard data available.";

            SizeF size =
                graphics.MeasureString(
                    message,
                    font);

            float x =
                (this.ClientSize.Width -
                 size.Width) / 2f;

            float y =
                Math.Max(
                    80f,
                    (this.ClientSize.Height -
                     size.Height) / 2f);

            graphics.DrawString(
                message,
                font,
                brush,
                x,
                y);
        }
    }

    // =========================================================
    // ADMIN DASHBOARD FORM
    // =========================================================

    public class AdminDashboardForm : Form
    {
        private readonly string _fullName;

        private readonly ApiService _apiService;

        private readonly List<Button> _sidebarButtons =
            new List<Button>();

        private readonly List<Panel> _summaryCards =
            new List<Panel>();

        // =========================================================
        // COLORS
        // =========================================================

        private static readonly Color BrandBg =
            Color.FromArgb(32, 24, 18);

        private static readonly Color BrandAccent =
            Color.FromArgb(224, 194, 140);

        private static readonly Color CtaColor =
            Color.FromArgb(170, 130, 80);

        private static readonly Color ContentBg =
            Color.FromArgb(247, 244, 239);

        private static readonly Color CardBg =
            Color.White;

        private static readonly Color TextDark =
            Color.FromArgb(55, 39, 20);

        private static readonly Color TextMuted =
            Color.FromArgb(120, 120, 120);

        private static readonly Color SidebarHover =
            Color.FromArgb(58, 44, 31);

        private static readonly Color SidebarSelected =
            Color.FromArgb(80, 59, 40);

        // =========================================================
        // MAIN CONTROLS
        // =========================================================

        private Panel pnlSidebar = null!;

        private Panel pnlContent = null!;

        private Label lblPageTitle = null!;

        private Label lblWelcome = null!;

        private Label lblDate = null!;

        private Button btnRefreshDashboard = null!;

        private Panel pnlHeaderBar = null!;

        private Form? _currentModuleForm;

        // =========================================================
        // CHARTS
        // =========================================================

        private AdminDashboardChartPanel tenantStatusChart = null!;

        private AdminDashboardChartPanel branchOccupancyChart = null!;

        private AdminDashboardChartPanel revenueTrendChart = null!;

        private AdminDashboardChartPanel adminOverviewChart = null!;

        // =========================================================
        // DASHBOARD DATA
        // =========================================================

        private int totalTenants;

        private int activeTenants;

        private int pendingTenants;

        private int totalBranches;

        private int occupiedBeds;

        private int availableBeds;

        private decimal monthlyRevenue;

        private int pendingPayments;

        private int renewalCount;

        private int maintenanceCount;

        private int feedbackCount;

        private readonly List<decimal> monthlyRevenueValues =
            new List<decimal>();

        private readonly List<string> monthlyRevenueLabels =
            new List<string>();

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public AdminDashboardForm(
            ApiService apiService,
            string fullName)
        {
            this._apiService =
                apiService ??
                throw new ArgumentNullException(
                    nameof(apiService));

            this._fullName =
                string.IsNullOrWhiteSpace(fullName)
                    ? "Administrator"
                    : fullName;

            this.BuildDashboard();

            this.Shown +=
                this.AdminDashboardForm_Shown;
        }

        // =========================================================
        // DASHBOARD INITIALIZATION
        // =========================================================

        private void BuildDashboard()
        {
            this.Text =
                "PBCRM2 - Admin Dashboard";

            this.StartPosition =
                FormStartPosition.CenterScreen;

            this.Size =
                new Size(1250, 820);

            this.MinimumSize =
                new Size(1050, 720);

            this.BackColor =
                ContentBg;

            this.FormBorderStyle =
                FormBorderStyle.Sizable;

            this.MaximizeBox =
                true;

            this.MinimizeBox =
                true;

            this.DoubleBuffered =
                true;

            this.BuildSidebar();

            this.BuildContentPanel();

            this.Controls.Add(
                this.pnlContent);

            this.Controls.Add(
                this.pnlSidebar);

            this.Resize +=
                delegate
                {
                    this.PerformResponsiveLayout();
                };

            this.ShowDashboard();
        }

        // =========================================================
        // SIDEBAR
        // =========================================================

        private void BuildSidebar()
        {
            this.pnlSidebar =
                new Panel();

            this.pnlSidebar.Dock =
                DockStyle.Left;

            this.pnlSidebar.Width =
                250;

            this.pnlSidebar.BackColor =
                BrandBg;

            PictureBox picLogo =
                new PictureBox();

            picLogo.SizeMode =
                PictureBoxSizeMode.Zoom;

            picLogo.Size =
                new Size(190, 65);

            picLogo.Location =
                new Point(30, 18);

            picLogo.BackColor =
                Color.Transparent;

            try
            {
                string logoPath =
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Resources",
                        "logo.png");

                if (File.Exists(logoPath))
                {
                    using FileStream stream =
                        new FileStream(
                            logoPath,
                            FileMode.Open,
                            FileAccess.Read);

                    using Image image =
                        Image.FromStream(stream);

                    picLogo.Image =
                        new Bitmap(image);
                }
            }
            catch
            {
            }

            Label lblSubtitle =
                new Label();

            lblSubtitle.Text =
                "BOARDING HOUSE CRM";

            lblSubtitle.ForeColor =
                Color.FromArgb(
                    175,
                    155,
                    130);

            lblSubtitle.Font =
                new Font(
                    "Segoe UI",
                    8,
                    FontStyle.Bold);

            lblSubtitle.AutoSize =
                false;

            lblSubtitle.TextAlign =
                ContentAlignment.MiddleCenter;

            lblSubtitle.Size =
                new Size(250, 25);

            lblSubtitle.Location =
                new Point(0, 88);

            Panel divider =
                new Panel();

            divider.Size =
                new Size(190, 1);

            divider.Location =
                new Point(30, 120);

            divider.BackColor =
                Color.FromArgb(
                    85,
                    65,
                    45);

            this.pnlSidebar.Controls.Add(
                picLogo);

            this.pnlSidebar.Controls.Add(
                lblSubtitle);

            this.pnlSidebar.Controls.Add(
                divider);

            int y = 140;

            this.AddSidebarButton(
                "⌂",
                "Dashboard",
                y,
                true);

            y += 55;

            this.AddSidebarButton(
                "♙",
                "Tenant Overview",
                y);

            y += 55;

            this.AddSidebarButton(
                "▣",
                "Reports",
                y);

            y += 55;

            this.AddSidebarButton(
                "↻",
                "Renewal & Retention",
                y);

            y += 55;

            this.AddSidebarButton(
                "⌂",
                "Branch Management",
                y);

            y += 55;

            this.AddSidebarButton(
                "★",
                "Feedback & Satisfaction",
                y);

            this.BuildUserPanel();
        }

        // =========================================================
        // SIDEBAR BUTTON
        // =========================================================

        private void AddSidebarButton(
            string icon,
            string text,
            int y,
            bool selected = false)
        {
            Button button =
                new Button();

            button.Text =
                $"  {icon}    {text}";

            button.Location =
                new Point(15, y);

            button.Size =
                new Size(220, 45);

            button.FlatStyle =
                FlatStyle.Flat;

            button.TextAlign =
                ContentAlignment.MiddleLeft;

            button.Font =
                new Font(
                    "Segoe UI",
                    9.5f);

            button.Cursor =
                Cursors.Hand;

            button.BackColor =
                selected
                    ? SidebarSelected
                    : Color.Transparent;

            button.ForeColor =
                selected
                    ? BrandAccent
                    : Color.FromArgb(
                        215,
                        205,
                        192);

            button.TabStop =
                false;

            button.Tag =
                text;

            button.FlatAppearance.BorderSize =
                0;

            button.MouseEnter +=
                delegate
                {
                    if (!this.IsButtonSelected(button))
                    {
                        button.BackColor =
                            SidebarHover;

                        button.ForeColor =
                            BrandAccent;
                    }
                };

            button.MouseLeave +=
                delegate
                {
                    if (!this.IsButtonSelected(button))
                    {
                        button.BackColor =
                            Color.Transparent;

                        button.ForeColor =
                            Color.FromArgb(
                                215,
                                205,
                                192);
                    }
                };

            button.Click +=
                delegate
                {
                    this.SetActiveButton(button);

                    string moduleName =
                        button.Tag?.ToString()
                        ?? "Dashboard";

                    this.OpenModule(moduleName);
                };

            this._sidebarButtons.Add(button);

            this.pnlSidebar.Controls.Add(button);
        }

        private bool IsButtonSelected(
            Button button)
        {
            return button.BackColor ==
                SidebarSelected;
        }

        private void SetActiveButton(
            Button activeButton)
        {
            foreach (
                Button button
                in this._sidebarButtons)
            {
                if (button == activeButton)
                {
                    button.BackColor =
                        SidebarSelected;

                    button.ForeColor =
                        BrandAccent;
                }
                else
                {
                    button.BackColor =
                        Color.Transparent;

                    button.ForeColor =
                        Color.FromArgb(
                            215,
                            205,
                            192);
                }
            }
        }

        private void HighlightMenuButton(
            string moduleName)
        {
            foreach (
                Button button
                in this._sidebarButtons)
            {
                if (
                    string.Equals(
                        button.Tag?.ToString(),
                        moduleName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    this.SetActiveButton(button);
                    break;
                }
            }
        }

        // =========================================================
        // CONTENT PANEL
        // =========================================================

        private void BuildContentPanel()
        {
            this.pnlContent =
                new Panel();

            this.pnlContent.Dock =
                DockStyle.Fill;

            this.pnlContent.BackColor =
                ContentBg;

            this.pnlContent.AutoScroll =
                true;

            this.pnlContent.Padding =
                new Padding(0);
        }

        // =========================================================
        // CLEAR CURRENT MODULE
        // =========================================================

        private void ClearCurrentModule()
        {
            Form? oldForm =
                this._currentModuleForm;

            this._currentModuleForm =
                null;

            if (oldForm != null)
            {
                try
                {
                    if (!oldForm.IsDisposed)
                    {
                        oldForm.Close();
                    }
                }
                catch
                {
                }

                try
                {
                    if (!oldForm.IsDisposed)
                    {
                        oldForm.Dispose();
                    }
                }
                catch
                {
                }
            }

            if (this.pnlContent != null &&
                !this.pnlContent.IsDisposed)
            {
                this.pnlContent.Controls.Clear();

                this.pnlContent.AutoScroll =
                    true;
            }
        }

        // =========================================================
        // GENERIC EMBEDDED MODULE OPENER
        // =========================================================

        private void OpenEmbeddedModule(
            Form moduleForm,
            string moduleName)
        {
            if (moduleForm == null ||
                moduleForm.IsDisposed)
            {
                return;
            }

            try
            {
                this.ClearCurrentModule();

                this.HighlightMenuButton(
                    moduleName);

                moduleForm.TopLevel =
                    false;

                moduleForm.FormBorderStyle =
                    FormBorderStyle.None;

                moduleForm.ShowInTaskbar =
                    false;

                moduleForm.Dock =
                    DockStyle.Fill;

                moduleForm.Margin =
                    Padding.Empty;

                moduleForm.Padding =
                    Padding.Empty;

                moduleForm.StartPosition =
                    FormStartPosition.Manual;

                this._currentModuleForm =
                    moduleForm;

                this.pnlContent.SuspendLayout();

                this.pnlContent.AutoScroll =
                    false;

                this.pnlContent.Controls.Add(
                    moduleForm);

                moduleForm.Show();

                moduleForm.BringToFront();

                this.pnlContent.ResumeLayout(true);
            }
            catch
            {
                this._currentModuleForm =
                    null;

                try
                {
                    if (!moduleForm.IsDisposed)
                    {
                        moduleForm.Close();
                    }
                }
                catch
                {
                }

                try
                {
                    if (!moduleForm.IsDisposed)
                    {
                        moduleForm.Dispose();
                    }
                }
                catch
                {
                }

                this.pnlContent.Controls.Clear();

                this.pnlContent.AutoScroll =
                    true;

                throw;
            }
        }

        // =========================================================
        // SHOW DASHBOARD
        // =========================================================

        private void ShowDashboard()
        {
            this.ClearCurrentModule();

            this.HighlightMenuButton(
                "Dashboard");

            this.BuildDashboardContent();

            this.PerformResponsiveLayout();
        }

        // =========================================================
        // BUILD DASHBOARD CONTENT
        // =========================================================

        private void BuildDashboardContent()
        {
            this.pnlContent.SuspendLayout();

            this.pnlContent.Controls.Clear();

            this._summaryCards.Clear();

            // Header height increased to match sidebar branding area (~120px)
            // and give full space for title + welcome text without clipping
            this.pnlHeaderBar =
                new Panel();

            this.pnlHeaderBar.Dock =
                DockStyle.Top;

            this.pnlHeaderBar.Height =
                128;

            this.pnlHeaderBar.BackColor =
                BrandBg;

            this.lblPageTitle =
                new Label();

            this.lblPageTitle.Text =
                "ADMIN DASHBOARD";

            this.lblPageTitle.ForeColor =
                Color.White;

            this.lblPageTitle.Font =
                new Font(
                    "Segoe UI",
                    22f,
                    FontStyle.Bold);

            this.lblPageTitle.AutoSize =
                true;

            this.lblPageTitle.Location =
                new Point(28, 22);

            this.lblWelcome =
                new Label();

            this.lblWelcome.Text =
                $"Welcome back, {this._fullName}";

            this.lblWelcome.ForeColor =
                BrandAccent;

            this.lblWelcome.Font =
                new Font(
                    "Segoe UI",
                    10.5f);

            this.lblWelcome.AutoSize =
                true;

            this.lblWelcome.Location =
                new Point(30, 62);

            this.lblDate =
                new Label();

            this.lblDate.Text =
                DateTime.Now.ToString(
                    "dddd, MMMM dd, yyyy");

            this.lblDate.ForeColor =
                Color.FromArgb(
                    175,
                    155,
                    130);

            this.lblDate.Font =
                new Font(
                    "Segoe UI",
                    9f);

            this.lblDate.AutoSize =
                true;

            this.btnRefreshDashboard =
                new Button();

            this.btnRefreshDashboard.Text =
                "↻  Refresh";

            this.btnRefreshDashboard.Size =
                new Size(110, 34);

            this.btnRefreshDashboard.FlatStyle =
                FlatStyle.Flat;

            this.btnRefreshDashboard.BackColor =
                CtaColor;

            this.btnRefreshDashboard.ForeColor =
                Color.White;

            this.btnRefreshDashboard.Font =
                new Font(
                    "Segoe UI",
                    9f,
                    FontStyle.Bold);

            this.btnRefreshDashboard.Cursor =
                Cursors.Hand;

            this.btnRefreshDashboard.FlatAppearance.BorderSize =
                0;

            this.btnRefreshDashboard.Click +=
                async delegate
                {
                    await this.LoadDashboardDataAsync();
                };

            this.pnlHeaderBar.Controls.Add(
                this.lblPageTitle);

            this.pnlHeaderBar.Controls.Add(
                this.lblWelcome);

            this.pnlHeaderBar.Controls.Add(
                this.lblDate);

            this.pnlHeaderBar.Controls.Add(
                this.btnRefreshDashboard);

            this.pnlContent.Controls.Add(
                this.pnlHeaderBar);

            this.BuildSummaryCards();

            this.BuildCharts();

            this.pnlContent.ResumeLayout();
        }

        // =========================================================
        // SUMMARY CARDS
        // =========================================================

        private void BuildSummaryCards()
        {
            this._summaryCards.Add(
                this.CreateSummaryCardPanel(
                    "TENANTS",
                    "0",
                    "Active tenant records"));

            this._summaryCards.Add(
                this.CreateSummaryCardPanel(
                    "OCCUPANCY",
                    "0%",
                    "Overall bed occupancy"));

            this._summaryCards.Add(
                this.CreateSummaryCardPanel(
                    "PAYMENTS",
                    "₱0.00",
                    "Payments collected"));

            this._summaryCards.Add(
                this.CreateSummaryCardPanel(
                    "RENEWALS",
                    "0",
                    "Renewals requiring attention"));

            foreach (
                Panel card
                in this._summaryCards)
            {
                this.pnlContent.Controls.Add(card);
            }
        }

        private Panel CreateSummaryCardPanel(
            string title,
            string value,
            string description)
        {
            Panel card =
                new Panel();

            card.Size =
                new Size(240, 110);

            card.BackColor =
                CardBg;

            card.Paint +=
                delegate (
                    object? sender,
                    PaintEventArgs e)
                {
                    using Pen borderPen =
                        new Pen(
                            Color.FromArgb(
                                225,
                                215,
                                200));

                    e.Graphics.DrawRectangle(
                        borderPen,
                        0,
                        0,
                        Math.Max(
                            0,
                            card.Width - 1),
                        Math.Max(
                            0,
                            card.Height - 1));

                    using SolidBrush accent =
                        new SolidBrush(CtaColor);

                    e.Graphics.FillRectangle(
                        accent,
                        0,
                        0,
                        4,
                        card.Height);
                };

            Label lblTitle =
                new Label();

            lblTitle.Text =
                title;

            lblTitle.Font =
                new Font(
                    "Segoe UI",
                    8f,
                    FontStyle.Bold);

            lblTitle.ForeColor =
                TextMuted;

            lblTitle.AutoSize =
                false;

            lblTitle.TextAlign =
                ContentAlignment.MiddleLeft;

            lblTitle.Location =
                new Point(18, 12);

            lblTitle.Size =
                new Size(100, 18);

            Label lblValue =
                new Label();

            lblValue.Text =
                value;

            lblValue.Font =
                new Font(
                    "Segoe UI",
                    18f,
                    FontStyle.Bold);

            lblValue.ForeColor =
                CtaColor;

            lblValue.AutoSize =
                false;

            lblValue.TextAlign =
                ContentAlignment.MiddleRight;

            lblValue.Location =
                new Point(18, 32);

            lblValue.Size =
                new Size(200, 34);

            Label lblDescription =
                new Label();

            lblDescription.Text =
                description;

            lblDescription.Font =
                new Font(
                    "Segoe UI",
                    8f);

            lblDescription.ForeColor =
                TextMuted;

            lblDescription.AutoSize =
                false;

            lblDescription.TextAlign =
                ContentAlignment.MiddleLeft;

            lblDescription.Location =
                new Point(18, 72);

            lblDescription.Size =
                new Size(200, 22);

            card.Controls.Add(lblTitle);

            card.Controls.Add(lblValue);

            card.Controls.Add(lblDescription);

            card.Tag =
                lblValue;

            return card;
        }

        // =========================================================
        // CHARTS
        // =========================================================

        private void BuildCharts()
        {
            this.tenantStatusChart =
                new AdminDashboardChartPanel();

            this.tenantStatusChart.Title =
                "Tenant Status";

            this.tenantStatusChart.Subtitle =
                "Current tenant registration status";

            this.tenantStatusChart.ChartType =
                AdminDashboardChartType.Donut;

            this.tenantStatusChart.SetItems(
                new List<AdminDashboardChartItem>
                {
                    new AdminDashboardChartItem(
                        "Active",
                        0,
                        Color.FromArgb(
                            170,
                            130,
                            80)),

                    new AdminDashboardChartItem(
                        "Pending",
                        0,
                        Color.FromArgb(
                            224,
                            194,
                            140))
                });

            this.branchOccupancyChart =
                new AdminDashboardChartPanel();

            this.branchOccupancyChart.Title =
                "Branch Occupancy";

            this.branchOccupancyChart.Subtitle =
                "Overall boarding house capacity";

            this.branchOccupancyChart.ChartType =
                AdminDashboardChartType.Bar;

            this.branchOccupancyChart.SetItems(
                new List<AdminDashboardChartItem>
                {
                    new AdminDashboardChartItem(
                        "Occupied",
                        0,
                        Color.FromArgb(
                            170,
                            130,
                            80)),

                    new AdminDashboardChartItem(
                        "Available",
                        0,
                        Color.FromArgb(
                            224,
                            194,
                            140))
                });

            this.revenueTrendChart =
                new AdminDashboardChartPanel();

            this.revenueTrendChart.Title =
                "Revenue Trend";

            this.revenueTrendChart.Subtitle =
                "Payment collection over recent months";

            this.revenueTrendChart.ChartType =
                AdminDashboardChartType.Line;

            List<AdminDashboardChartItem> initialRevenue =
                new List<AdminDashboardChartItem>();

            DateTime now =
                DateTime.Now;

            for (int i = 5;
                 i >= 0;
                 i--)
            {
                initialRevenue.Add(
                    new AdminDashboardChartItem(
                        now.AddMonths(-i)
                            .ToString("MMM"),
                        0,
                        CtaColor));
            }

            this.revenueTrendChart.SetItems(
                initialRevenue);

            this.adminOverviewChart =
                new AdminDashboardChartPanel();

            this.adminOverviewChart.Title =
                "Admin Overview";

            this.adminOverviewChart.Subtitle =
                "Business areas requiring attention";

            this.adminOverviewChart.ChartType =
                AdminDashboardChartType.Bar;

            this.adminOverviewChart.SetItems(
                new List<AdminDashboardChartItem>
                {
                    new AdminDashboardChartItem(
                        "Registrations",
                        0,
                        Color.FromArgb(
                            170,
                            130,
                            80)),

                    new AdminDashboardChartItem(
                        "Payments",
                        0,
                        Color.FromArgb(
                            190,
                            150,
                            95)),

                    new AdminDashboardChartItem(
                        "Renewals",
                        0,
                        Color.FromArgb(
                            205,
                            170,
                            115)),

                    new AdminDashboardChartItem(
                        "Maintenance",
                        0,
                        Color.FromArgb(
                            215,
                            180,
                            125)),

                    new AdminDashboardChartItem(
                        "Feedback",
                        0,
                        Color.FromArgb(
                            150,
                            115,
                            75))
                });

            this.pnlContent.Controls.Add(
                this.tenantStatusChart);

            this.pnlContent.Controls.Add(
                this.branchOccupancyChart);

            this.pnlContent.Controls.Add(
                this.revenueTrendChart);

            this.pnlContent.Controls.Add(
                this.adminOverviewChart);
        }

        // =========================================================
        // INITIAL DISPLAY
        // =========================================================

        private async void AdminDashboardForm_Shown(
            object sender,
            EventArgs e)
        {
            this.PerformResponsiveLayout();

            await Task.Delay(50);

            this.PerformResponsiveLayout();

            await this.LoadDashboardDataAsync();

            this.PerformResponsiveLayout();

            if (!this.IsDisposed &&
                this.IsHandleCreated)
            {
                this.BeginInvoke(
                    new Action(
                        delegate
                        {
                            this.PerformResponsiveLayout();
                        }));
            }
        }

        // =========================================================
        // RESPONSIVE LAYOUT
        // =========================================================

        private void PerformResponsiveLayout()
        {
            if (this.pnlContent == null)
            {
                return;
            }

            if (this._currentModuleForm != null)
            {
                return;
            }

            int visibleWidth =
                this.pnlContent.ClientSize.Width;

            int visibleHeight =
                this.pnlContent.ClientSize.Height;

            if (visibleWidth <= 0)
            {
                return;
            }

            this.pnlContent.SuspendLayout();

            const int leftMargin = 18;
            const int rightMargin = 18;
            const int gap = 12;
            const int bottomPad = 14;

            int availableWidth =
                Math.Max(
                    640,
                    visibleWidth -
                    leftMargin -
                    rightMargin);

            // Position date and refresh button inside the taller header
            if (this.lblDate != null &&
                this.pnlHeaderBar != null)
            {
                this.lblDate.Location =
                    new Point(
                        Math.Max(
                            28,
                            this.pnlHeaderBar.ClientSize.Width -
                            this.lblDate.Width -
                            150),
                        24);
            }

            if (this.btnRefreshDashboard != null &&
                this.pnlHeaderBar != null)
            {
                this.btnRefreshDashboard.Location =
                    new Point(
                        Math.Max(
                            28,
                            this.pnlHeaderBar.ClientSize.Width -
                            this.btnRefreshDashboard.Width -
                            28),
                        58);
            }

            int headerH =
                this.pnlHeaderBar != null
                    ? this.pnlHeaderBar.Height
                    : 128;

            // Cards start closer under the header (less whitespace)
            int cardY =
                headerH + 14;

            int cardHeight =
                118;

            int cardWidth =
                Math.Max(
                    160,
                    (availableWidth -
                     gap * 3) / 4);

            for (int i = 0;
                 i < this._summaryCards.Count;
                 i++)
            {
                int x =
                    leftMargin +
                    i *
                    (cardWidth + gap);

                this._summaryCards[i].Location =
                    new Point(
                        x,
                        cardY);

                this._summaryCards[i].Size =
                    new Size(
                        cardWidth,
                        cardHeight);

                this.ResizeSummaryCardContents(
                    this._summaryCards[i],
                    cardWidth);
            }

            // Charts start right after cards with smaller gap
            int chartsY =
                cardY +
                cardHeight +
                14;

            int chartGap =
                gap;

            int chartWidth =
                Math.Max(
                    300,
                    (availableWidth -
                     chartGap) / 2);

            int remainingHeight =
                visibleHeight -
                chartsY -
                bottomPad;

            // Allow charts to grow and fill available space
            if (remainingHeight < 460)
            {
                remainingHeight = 460;
            }

            int chartHeight =
                (remainingHeight -
                 chartGap) / 2;

            // Prefer larger charts (up to ~320px) to reduce empty space
            if (chartHeight > 320)
            {
                chartHeight = 320;
            }

            if (chartHeight < 250)
            {
                chartHeight = 250;
            }

            if (this.tenantStatusChart != null &&
                this.branchOccupancyChart != null &&
                this.revenueTrendChart != null &&
                this.adminOverviewChart != null)
            {
                this.tenantStatusChart.Location =
                    new Point(
                        leftMargin,
                        chartsY);

                this.tenantStatusChart.Size =
                    new Size(
                        chartWidth,
                        chartHeight);

                this.branchOccupancyChart.Location =
                    new Point(
                        leftMargin +
                        chartWidth +
                        chartGap,
                        chartsY);

                this.branchOccupancyChart.Size =
                    new Size(
                        chartWidth,
                        chartHeight);

                this.revenueTrendChart.Location =
                    new Point(
                        leftMargin,
                        chartsY +
                        chartHeight +
                        chartGap);

                this.revenueTrendChart.Size =
                    new Size(
                        chartWidth,
                        chartHeight);

                this.adminOverviewChart.Location =
                    new Point(
                        leftMargin +
                        chartWidth +
                        chartGap,
                        chartsY +
                        chartHeight +
                        chartGap);

                this.adminOverviewChart.Size =
                    new Size(
                        chartWidth,
                        chartHeight);

                this.tenantStatusChart.Invalidate();

                this.branchOccupancyChart.Invalidate();

                this.revenueTrendChart.Invalidate();

                this.adminOverviewChart.Invalidate();
            }

            int bottom =
                chartsY +
                chartHeight * 2 +
                chartGap +
                bottomPad;

            this.pnlContent.AutoScrollMinSize =
                new Size(
                    0,
                    bottom);

            this.pnlContent.ResumeLayout();
        }

        private void ResizeSummaryCardContents(
            Panel card,
            int width)
        {
            int contentWidth =
                Math.Max(
                    120,
                    width - 36);

            Label? title = null;
            Label? value = null;
            Label? desc = null;

            foreach (
                Control control
                in card.Controls)
            {
                if (control is Label label)
                {
                    if (title == null)
                    {
                        title = label;
                    }
                    else if (value == null)
                    {
                        value = label;
                    }
                    else if (desc == null)
                    {
                        desc = label;
                    }
                }
            }

            if (title != null)
            {
                title.Location =
                    new Point(18, 12);

                title.Size =
                    new Size(
                        contentWidth,
                        18);

                title.TextAlign =
                    ContentAlignment.MiddleLeft;
            }

            if (value != null)
            {
                value.Location =
                    new Point(18, 36);

                value.Size =
                    new Size(
                        contentWidth,
                        40);

                value.TextAlign =
                    ContentAlignment.MiddleRight;

                value.Font =
                    new Font(
                        "Segoe UI",
                        20f,
                        FontStyle.Bold);
            }

            if (desc != null)
            {
                desc.Location =
                    new Point(18, 84);

                desc.Size =
                    new Size(
                        contentWidth,
                        22);

                desc.TextAlign =
                    ContentAlignment.MiddleLeft;
            }

            card.Invalidate();
        }

        // =========================================================
        // LOAD DASHBOARD DATA
        // =========================================================

        private async Task LoadDashboardDataAsync()
        {
            if (this.btnRefreshDashboard != null)
            {
                this.btnRefreshDashboard.Enabled =
                    false;

                this.btnRefreshDashboard.Text =
                    "Loading...";
            }

            try
            {
                await this.LoadTenantDataAsync();

                await this.LoadBranchAndBedDataAsync();

                await this.LoadPaymentDataAsync();

                await this.LoadRenewalDataAsync();

                await this.LoadMaintenanceDataAsync();

                await this.LoadFeedbackDataAsync();

                this.UpdateDashboardUI();
            }
            catch
            {
                this.UpdateDashboardUI();
            }
            finally
            {
                if (this.btnRefreshDashboard != null)
                {
                    this.btnRefreshDashboard.Enabled =
                        true;

                    this.btnRefreshDashboard.Text =
                        "↻  Refresh";
                }

                this.PerformResponsiveLayout();
            }
        }

        // =========================================================
        // TENANT DATA
        // =========================================================

        private async Task LoadTenantDataAsync()
        {
            try
            {
                List<JsonElement> tenants =
                    await this.GetListAsync<JsonElement>(
                        "api/Tenants");

                this.totalTenants =
                    tenants.Count;

                this.activeTenants =
                    tenants.Count(
                        delegate (JsonElement tenant)
                        {
                            return this.GetStringProperty(
                                tenant,
                                "status")
                                .Equals(
                                    "Active",
                                    StringComparison.OrdinalIgnoreCase);
                        });

                this.pendingTenants =
                    tenants.Count(
                        delegate (JsonElement tenant)
                        {
                            return this.GetStringProperty(
                                tenant,
                                "status")
                                .Equals(
                                    "Pending",
                                    StringComparison.OrdinalIgnoreCase);
                        });
            }
            catch
            {
                this.totalTenants = 0;
                this.activeTenants = 0;
                this.pendingTenants = 0;
            }
        }

        // =========================================================
        // BRANCH / BED DATA
        // =========================================================

        private async Task LoadBranchAndBedDataAsync()
        {
            this.occupiedBeds = 0;
            this.availableBeds = 0;
            this.totalBranches = 0;

            try
            {
                List<JsonElement> branches =
                    await this.GetListAsync<JsonElement>(
                        "api/Branches");

                this.totalBranches =
                    branches.Count;
            }
            catch
            {
                this.totalBranches = 0;
            }

            try
            {
                List<JsonElement> rooms =
                    await this.GetListAsync<JsonElement>(
                        "api/Rooms");

                foreach (
                    JsonElement room
                    in rooms)
                {
                    if (
                        room.ValueKind ==
                        JsonValueKind.Object)
                    {
                        bool foundBeds = false;

                        foreach (
                            JsonProperty prop
                            in room.EnumerateObject())
                        {
                            if (
                                prop.Name.Equals(
                                    "beds",
                                    StringComparison.OrdinalIgnoreCase) &&
                                prop.Value.ValueKind ==
                                JsonValueKind.Array)
                            {
                                foundBeds = true;

                                foreach (
                                    JsonElement bed
                                    in prop.Value.EnumerateArray())
                                {
                                    string status =
                                        this.GetStringProperty(
                                            bed,
                                            "status");

                                    int tenantId =
                                        this.GetIntProperty(
                                            bed,
                                            "tenantId");

                                    if (
                                        status.Equals(
                                            "Occupied",
                                            StringComparison.OrdinalIgnoreCase)
                                        ||
                                        tenantId > 0)
                                    {
                                        this.occupiedBeds++;
                                    }
                                    else
                                    {
                                        this.availableBeds++;
                                    }
                                }

                                break;
                            }
                        }

                        if (foundBeds)
                        {
                            continue;
                        }
                    }

                    int bedCount =
                        this.GetIntProperty(
                            room,
                            "bedCount");

                    if (bedCount <= 0)
                    {
                        bedCount =
                            this.GetIntProperty(
                                room,
                                "totalBeds");
                    }

                    int occupied =
                        this.GetIntProperty(
                            room,
                            "occupiedBeds");

                    if (occupied <= 0)
                    {
                        occupied =
                            this.GetIntProperty(
                                room,
                                "occupied");
                    }

                    if (bedCount > 0)
                    {
                        this.occupiedBeds +=
                            Math.Min(
                                occupied,
                                bedCount);

                        this.availableBeds +=
                            Math.Max(
                                0,
                                bedCount -
                                occupied);
                    }
                }
            }
            catch
            {
                this.occupiedBeds = 0;
                this.availableBeds = 0;
            }
        }

        // =========================================================
        // PAYMENT DATA
        // =========================================================

        private async Task LoadPaymentDataAsync()
        {
            this.monthlyRevenue = 0;
            this.pendingPayments = 0;

            this.monthlyRevenueValues.Clear();
            this.monthlyRevenueLabels.Clear();

            DateTime now =
                DateTime.Now;

            for (int i = 5;
                 i >= 0;
                 i--)
            {
                DateTime month =
                    now.AddMonths(-i);

                this.monthlyRevenueLabels.Add(
                    month.ToString("MMM"));

                this.monthlyRevenueValues.Add(
                    0);
            }

            try
            {
                List<JsonElement> billings =
                    await this.GetListAsync<JsonElement>(
                        "api/Billing");

                foreach (
                    JsonElement billing
                    in billings)
                {
                    string billingStatus =
                        this.GetStringProperty(
                            billing,
                            "status");

                    if (
                        billingStatus.Equals(
                            "Pending",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        billingStatus.Equals(
                            "Overdue",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        billingStatus.Equals(
                            "Partially Paid",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        this.pendingPayments++;
                    }

                    decimal amount =
                        this.GetDecimalProperty(
                            billing,
                            "totalPaid");

                    if (amount <= 0)
                    {
                        amount =
                            this.GetDecimalProperty(
                                billing,
                                "paidAmount");
                    }

                    if (amount <= 0)
                    {
                        continue;
                    }

                    DateTime billingDate =
                        this.GetDateProperty(
                            billing,
                            "dueDate");

                    if (
                        billingDate ==
                        DateTime.MinValue)
                    {
                        billingDate =
                            this.GetDateProperty(
                                billing,
                                "billingPeriodStart");
                    }

                    if (
                        billingDate ==
                        DateTime.MinValue)
                    {
                        billingDate =
                            this.GetDateProperty(
                                billing,
                                "paymentDate");
                    }

                    if (
                        billingDate ==
                        DateTime.MinValue)
                    {
                        billingDate =
                            DateTime.Today;
                    }

                    int monthDifference =
                        (
                            now.Year -
                            billingDate.Year
                        ) * 12 +
                        (
                            now.Month -
                            billingDate.Month
                        );

                    if (
                        monthDifference >= 0 &&
                        monthDifference <= 5)
                    {
                        int index =
                            5 -
                            monthDifference;

                        if (
                            index >= 0 &&
                            index <
                            this.monthlyRevenueValues.Count)
                        {
                            this.monthlyRevenueValues[index] +=
                                amount;
                        }
                    }

                    if (
                        billingDate.Month ==
                        now.Month &&
                        billingDate.Year ==
                        now.Year)
                    {
                        this.monthlyRevenue +=
                            amount;
                    }
                }
            }
            catch
            {
                this.monthlyRevenue = 0;
                this.pendingPayments = 0;
            }
        }

        // =========================================================
        // RENEWAL DATA
        // =========================================================

        private async Task LoadRenewalDataAsync()
        {
            try
            {
                List<JsonElement> renewals =
                    await this.GetListAsync<JsonElement>(
                        "api/Renewals");

                this.renewalCount =
                    renewals.Count(
                        delegate (JsonElement renewal)
                        {
                            string status =
                                this.GetStringProperty(
                                    renewal,
                                    "status");

                            return
                                !status.Equals(
                                    "Completed",
                                    StringComparison.OrdinalIgnoreCase);
                        });
            }
            catch
            {
                this.renewalCount = 0;
            }
        }

        // =========================================================
        // MAINTENANCE
        // =========================================================

        private async Task LoadMaintenanceDataAsync()
        {
            try
            {
                List<JsonElement> requests =
                    await this.GetListAsync<JsonElement>(
                        "api/MaintenanceRequests");

                this.maintenanceCount =
                    requests.Count(
                        delegate (JsonElement request)
                        {
                            return this.GetStringProperty(
                                request,
                                "status")
                                .Equals(
                                    "Pending",
                                    StringComparison.OrdinalIgnoreCase);
                        });
            }
            catch
            {
                this.maintenanceCount = 0;
            }
        }

        // =========================================================
        // FEEDBACK
        // =========================================================

        private async Task LoadFeedbackDataAsync()
        {
            try
            {
                List<JsonElement> feedbacks =
                    await this.GetListAsync<JsonElement>(
                        "api/Feedback");

                this.feedbackCount =
                    feedbacks.Count;
            }
            catch
            {
                this.feedbackCount = 0;
            }
        }

        // =========================================================
        // UPDATE DASHBOARD UI
        // =========================================================

        private void UpdateDashboardUI()
        {
            this.SetSummaryValue(
                0,
                this.activeTenants.ToString());

            decimal totalBeds =
                this.occupiedBeds +
                this.availableBeds;

            decimal occupancy =
                totalBeds > 0
                    ? (
                        (decimal)this.occupiedBeds /
                        totalBeds
                      ) * 100
                    : 0;

            this.SetSummaryValue(
                1,
                occupancy.ToString("0") +
                "%");

            this.SetSummaryValue(
                2,
                this.monthlyRevenue.ToString(
                    "₱#,##0.00"));

            this.SetSummaryValue(
                3,
                this.renewalCount.ToString());

            this.tenantStatusChart.SetItems(
                new List<AdminDashboardChartItem>
                {
                    new AdminDashboardChartItem(
                        "Active",
                        this.activeTenants,
                        Color.FromArgb(
                            170,
                            130,
                            80)),

                    new AdminDashboardChartItem(
                        "Pending",
                        this.pendingTenants,
                        Color.FromArgb(
                            224,
                            194,
                            140))
                });

            this.branchOccupancyChart.SetItems(
                new List<AdminDashboardChartItem>
                {
                    new AdminDashboardChartItem(
                        "Occupied",
                        this.occupiedBeds,
                        Color.FromArgb(
                            170,
                            130,
                            80)),

                    new AdminDashboardChartItem(
                        "Available",
                        this.availableBeds,
                        Color.FromArgb(
                            224,
                            194,
                            140))
                });

            List<AdminDashboardChartItem> revenueItems =
                new List<AdminDashboardChartItem>();

            for (
                int i = 0;
                i < this.monthlyRevenueLabels.Count;
                i++)
            {
                decimal value =
                    i <
                    this.monthlyRevenueValues.Count
                        ? this.monthlyRevenueValues[i]
                        : 0;

                revenueItems.Add(
                    new AdminDashboardChartItem(
                        this.monthlyRevenueLabels[i],
                        value,
                        CtaColor));
            }

            this.revenueTrendChart.SetItems(
                revenueItems);

            this.adminOverviewChart.SetItems(
                new List<AdminDashboardChartItem>
                {
                    new AdminDashboardChartItem(
                        "Registrations",
                        this.pendingTenants,
                        Color.FromArgb(
                            170,
                            130,
                            80)),

                    new AdminDashboardChartItem(
                        "Payments",
                        this.pendingPayments,
                        Color.FromArgb(
                            190,
                            150,
                            95)),

                    new AdminDashboardChartItem(
                        "Renewals",
                        this.renewalCount,
                        Color.FromArgb(
                            205,
                            170,
                            115)),

                    new AdminDashboardChartItem(
                        "Maintenance",
                        this.maintenanceCount,
                        Color.FromArgb(
                            215,
                            180,
                            125)),

                    new AdminDashboardChartItem(
                        "Feedback",
                        this.feedbackCount,
                        Color.FromArgb(
                            150,
                            115,
                            75))
                });

            this.PerformResponsiveLayout();
        }

        // =========================================================
        // SET SUMMARY VALUE
        // =========================================================

        private void SetSummaryValue(
            int index,
            string value)
        {
            if (
                index < 0 ||
                index >= this._summaryCards.Count)
            {
                return;
            }

            Panel card =
                this._summaryCards[index];

            if (card.Tag is Label label)
            {
                label.Text =
                    value;
            }
        }

        // =========================================================
        // USER PANEL
        // =========================================================

        private void BuildUserPanel()
        {
            Panel userPanel =
                new Panel();

            userPanel.Dock =
                DockStyle.Bottom;

            userPanel.Height =
                105;

            userPanel.BackColor =
                Color.FromArgb(
                    42,
                    31,
                    23);

            Label lblUser =
                new Label();

            lblUser.Text =
                this._fullName;

            lblUser.ForeColor =
                Color.White;

            lblUser.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            lblUser.AutoSize =
                false;

            lblUser.TextAlign =
                ContentAlignment.MiddleLeft;

            lblUser.Size =
                new Size(
                    200,
                    25);

            lblUser.Location =
                new Point(
                    25,
                    15);

            Label lblRole =
                new Label();

            lblRole.Text =
                "Administrator";

            lblRole.ForeColor =
                BrandAccent;

            lblRole.Font =
                new Font(
                    "Segoe UI",
                    8);

            lblRole.AutoSize =
                false;

            lblRole.TextAlign =
                ContentAlignment.MiddleLeft;

            lblRole.Size =
                new Size(
                    200,
                    20);

            lblRole.Location =
                new Point(
                    25,
                    40);

            Button btnLogout =
                new Button();

            btnLogout.Text =
                "LOG OUT";

            btnLogout.Location =
                new Point(
                    25,
                    67);

            btnLogout.Size =
                new Size(
                    90,
                    28);

            btnLogout.FlatStyle =
                FlatStyle.Flat;

            btnLogout.BackColor =
                Color.Transparent;

            btnLogout.ForeColor =
                Color.FromArgb(
                    210,
                    190,
                    165);

            btnLogout.Font =
                new Font(
                    "Segoe UI",
                    8,
                    FontStyle.Bold);

            btnLogout.Cursor =
                Cursors.Hand;

            btnLogout.FlatAppearance.BorderColor =
                Color.FromArgb(
                    100,
                    80,
                    60);

            btnLogout.FlatAppearance.BorderSize =
                1;

            btnLogout.Click +=
                delegate
                {
                    DialogResult result =
                        MessageBox.Show(
                            "Are you sure you want to log out?",
                            "Logout",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                    if (
                        result ==
                        DialogResult.Yes)
                    {
                        this._apiService.ClearToken();

                        this.DialogResult =
                            DialogResult.Retry;

                        this.Close();
                    }
                };

            userPanel.Controls.Add(
                lblUser);

            userPanel.Controls.Add(
                lblRole);

            userPanel.Controls.Add(
                btnLogout);

            this.pnlSidebar.Controls.Add(
                userPanel);
        }

        // =========================================================
        // MODULE NAVIGATION
        // =========================================================

        private void OpenModule(
            string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                return;
            }

            string selectedModule =
                moduleName.Trim();

            switch (selectedModule)
            {
                case "Dashboard":

                    this.ShowDashboard();

                    break;

                case "Tenant Overview":

                    this.OpenTenantOverview();

                    break;

                case "Reports":

                    this.OpenReports();

                    break;

                case "Renewal & Retention":

                    this.OpenRenewalRetention();

                    break;

                case "Branch Management":

                    this.OpenBranchManagement();

                    break;

                case "Feedback & Satisfaction":

                    this.OpenFeedback();

                    break;
            }
        }

        // =========================================================
        // TENANT OVERVIEW
        // =========================================================

        private void OpenTenantOverview()
        {
            try
            {
                TenantManagementForm form =
                    new TenantManagementForm(
                        this._apiService);

                this.OpenEmbeddedModule(
                    form,
                    "Tenant Overview");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open Tenant Overview.\n\n{ex.Message}",
                    "Tenant Overview",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // REPORTS
        // =========================================================

        private void OpenReports()
        {
            try
            {
                ReportsForm form =
                    new ReportsForm(
                        this._apiService);

                this.OpenEmbeddedModule(
                    form,
                    "Reports");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open Reports.\n\n{ex.Message}",
                    "Reports",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // RENEWAL & RETENTION
        // =========================================================

        private void OpenRenewalRetention()
        {
            try
            {
                AdminRenewalRetentionForm form =
                    new AdminRenewalRetentionForm(
                        this._apiService);

                this.OpenEmbeddedModule(
                    form,
                    "Renewal & Retention");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to open Renewal & Retention.\n\n" +
                    ex.Message,
                    "Renewal & Retention",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // BRANCH MANAGEMENT
        // =========================================================

        private void OpenBranchManagement()
        {
            try
            {
                AdminBranchManagementForm form =
                    new AdminBranchManagementForm(
                        this._apiService);

                this.OpenEmbeddedModule(
                    form,
                    "Branch Management");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open Branch Management.\n\n{ex.Message}",
                    "Branch Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // FEEDBACK
        // =========================================================

        private void OpenFeedback()
        {
            this.ShowComingSoon(
                "Feedback & Satisfaction");
        }

        // =========================================================
        // COMING SOON
        // =========================================================

        private void ShowComingSoon(
            string moduleName)
        {
            this.ClearCurrentModule();

            this.HighlightMenuButton(
                moduleName);

            Label title =
                new Label();

            title.Text =
                moduleName;

            title.ForeColor =
                TextDark;

            title.Font =
                new Font(
                    "Segoe UI",
                    24,
                    FontStyle.Bold);

            title.AutoSize =
                true;

            title.Location =
                new Point(
                    35,
                    30);

            Label subtitle =
                new Label();

            subtitle.Text =
                "Boarding House CRM module";

            subtitle.ForeColor =
                TextMuted;

            subtitle.Font =
                new Font(
                    "Segoe UI",
                    10.5f);

            subtitle.AutoSize =
                true;

            subtitle.Location =
                new Point(
                    38,
                    78);

            Panel panel =
                new Panel();

            panel.Location =
                new Point(
                    35,
                    135);

            panel.Size =
                new Size(
                    800,
                    220);

            panel.BackColor =
                CardBg;

            panel.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            Label message =
                new Label();

            message.Text =
                $"{moduleName}\n\nThis module will be connected next.";

            message.ForeColor =
                TextDark;

            message.Font =
                new Font(
                    "Segoe UI",
                    14);

            message.AutoSize =
                true;

            message.Location =
                new Point(
                    30,
                    35);

            Button btnBack =
                new Button();

            btnBack.Text =
                "Back to Dashboard";

            btnBack.Size =
                new Size(
                    150,
                    38);

            btnBack.Location =
                new Point(
                    30,
                    145);

            btnBack.FlatStyle =
                FlatStyle.Flat;

            btnBack.BackColor =
                CtaColor;

            btnBack.ForeColor =
                Color.White;

            btnBack.Font =
                new Font(
                    "Segoe UI",
                    8.5f,
                    FontStyle.Bold);

            btnBack.Cursor =
                Cursors.Hand;

            btnBack.FlatAppearance.BorderSize =
                0;

            btnBack.Click +=
                delegate
                {
                    this.ShowDashboard();
                };

            panel.Controls.Add(
                message);

            panel.Controls.Add(
                btnBack);

            this.pnlContent.Controls.Add(
                title);

            this.pnlContent.Controls.Add(
                subtitle);

            this.pnlContent.Controls.Add(
                panel);
        }

        // =========================================================
        // API HELPER
        // =========================================================

        private async Task<List<T>> GetListAsync<T>(
            string endpoint)
        {
            try
            {
                List<T> result =
                    await this._apiService
                        .GetSilentAsync<List<T>>(
                            endpoint);

                return result ??
                    new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        // =========================================================
        // JSON STRING HELPER
        // =========================================================

        private string GetStringProperty(
            JsonElement element,
            string propertyName)
        {
            if (
                element.ValueKind !=
                JsonValueKind.Object)
            {
                return string.Empty;
            }

            foreach (
                JsonProperty property
                in element.EnumerateObject())
            {
                if (
                    property.Name.Equals(
                        propertyName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (
                        property.Value.ValueKind ==
                        JsonValueKind.String)
                    {
                        return property.Value
                            .GetString()
                            ?? string.Empty;
                    }

                    return property.Value.ToString();
                }
            }

            return string.Empty;
        }

        // =========================================================
        // JSON INT HELPER
        // =========================================================

        private int GetIntProperty(
            JsonElement element,
            string propertyName)
        {
            if (
                element.ValueKind !=
                JsonValueKind.Object)
            {
                return 0;
            }

            foreach (
                JsonProperty property
                in element.EnumerateObject())
            {
                if (
                    property.Name.Equals(
                        propertyName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (
                        property.Value.TryGetInt32(
                            out int value))
                    {
                        return value;
                    }

                    if (
                        int.TryParse(
                            property.Value.ToString(),
                            out int parsed))
                    {
                        return parsed;
                    }
                }
            }

            return 0;
        }

        // =========================================================
        // JSON DECIMAL HELPER
        // =========================================================

        private decimal GetDecimalProperty(
            JsonElement element,
            string propertyName)
        {
            if (
                element.ValueKind !=
                JsonValueKind.Object)
            {
                return 0;
            }

            foreach (
                JsonProperty property
                in element.EnumerateObject())
            {
                if (
                    property.Name.Equals(
                        propertyName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (
                        property.Value.TryGetDecimal(
                            out decimal value))
                    {
                        return value;
                    }

                    if (
                        decimal.TryParse(
                            property.Value.ToString(),
                            out decimal parsed))
                    {
                        return parsed;
                    }
                }
            }

            return 0;
        }

        // =========================================================
        // JSON DATE HELPER
        // =========================================================

        private DateTime GetDateProperty(
            JsonElement element,
            string propertyName)
        {
            if (
                element.ValueKind !=
                JsonValueKind.Object)
            {
                return DateTime.MinValue;
            }

            foreach (
                JsonProperty property
                in element.EnumerateObject())
            {
                if (
                    property.Name.Equals(
                        propertyName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (
                        property.Value.ValueKind ==
                        JsonValueKind.String)
                    {
                        string? value =
                            property.Value.GetString();

                        if (
                            DateTime.TryParse(
                                value,
                                out DateTime date))
                        {
                            return date;
                        }
                    }
                }
            }

            return DateTime.MinValue;
        }

        // =========================================================
        // FORM CLEANUP
        // =========================================================

        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            try
            {
                Form? module =
                    this._currentModuleForm;

                this._currentModuleForm =
                    null;

                if (module != null)
                {
                    try
                    {
                        if (!module.IsDisposed)
                        {
                            module.Close();
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
                        if (!module.IsDisposed)
                        {
                            module.Dispose();
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            base.OnFormClosed(e);
        }
    }
}
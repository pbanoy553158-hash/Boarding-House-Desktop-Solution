using PBCRM2.WinForms.Services;
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
    // =============================================================
    // BORDERED PANEL
    // =============================================================

    public class ManagerBorderedPanel : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } =
            Color.FromArgb(225, 215, 200);

        public ManagerBorderedPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(BackColor);

            using var pen = new Pen(BorderColor);

            e.Graphics.DrawRectangle(
                pen,
                0,
                0,
                Math.Max(0, Width - 1),
                Math.Max(0, Height - 1));
        }
    }

    // =============================================================
    // CHART TYPE
    // =============================================================

    public enum ManagerDashboardChartType
    {
        Bar,
        Donut,
        Line
    }

    // =============================================================
    // CHART ITEM
    // =============================================================

    public class ManagerDashboardChartItem
    {
        public string Label { get; set; } =
            string.Empty;

        public double Value { get; set; }

        public Color Color { get; set; } =
            Color.FromArgb(170, 130, 80);
    }

    // =============================================================
    // CUSTOM DASHBOARD CHART PANEL
    // =============================================================

    public class ManagerDashboardChartPanel : Panel
    {
        private static readonly Color TextDark =
            Color.FromArgb(55, 39, 20);

        private static readonly Color TextMuted =
            Color.FromArgb(120, 120, 120);

        private static readonly Color BorderColor =
            Color.FromArgb(225, 215, 200);

        private static readonly Color GridColor =
            Color.FromArgb(235, 231, 225);

        private static readonly Color ChartAccent =
            Color.FromArgb(170, 130, 80);

        private readonly List<ManagerDashboardChartItem> _items =
            new();

        private string _title =
            string.Empty;

        private string _subtitle =
            string.Empty;

        private ManagerDashboardChartType _chartType;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get => _title;

            set
            {
                _title = value ?? string.Empty;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Subtitle
        {
            get => _subtitle;

            set
            {
                _subtitle = value ?? string.Empty;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ManagerDashboardChartType ChartType
        {
            get => _chartType;

            set
            {
                _chartType = value;
                Invalidate();
            }
        }

        public ManagerDashboardChartPanel()
        {
            BackColor = Color.White;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            DoubleBuffered = true;
        }

        public void SetData(
            IEnumerable<ManagerDashboardChartItem> items)
        {
            _items.Clear();

            if (items != null)
            {
                _items.AddRange(items);
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;

            graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            graphics.TextRenderingHint =
                System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            graphics.Clear(BackColor);

            DrawBorder(graphics);

            DrawHeader(graphics);

            switch (_chartType)
            {
                case ManagerDashboardChartType.Bar:
                    DrawBarChart(graphics);
                    break;

                case ManagerDashboardChartType.Donut:
                    DrawDonutChart(graphics);
                    break;

                case ManagerDashboardChartType.Line:
                    DrawLineChart(graphics);
                    break;
            }
        }

        // =========================================================
        // BORDER
        // =========================================================

        private void DrawBorder(Graphics graphics)
        {
            using var pen = new Pen(BorderColor);

            graphics.DrawRectangle(
                pen,
                0,
                0,
                Math.Max(0, Width - 1),
                Math.Max(0, Height - 1));
        }

        // =========================================================
        // HEADER
        // =========================================================

        private void DrawHeader(Graphics graphics)
        {
            using var titleFont =
                new Font(
                    "Segoe UI",
                    13,
                    FontStyle.Bold);

            using var subtitleFont =
                new Font(
                    "Segoe UI",
                    8.5f);

            using var titleBrush =
                new SolidBrush(TextDark);

            using var subtitleBrush =
                new SolidBrush(TextMuted);

            graphics.DrawString(
                _title,
                titleFont,
                titleBrush,
                new PointF(22, 17));

            graphics.DrawString(
                _subtitle,
                subtitleFont,
                subtitleBrush,
                new PointF(22, 42));
        }

        // =========================================================
        // BAR GRAPH
        // =========================================================

        private void DrawBarChart(Graphics graphics)
        {
            if (_items.Count == 0)
            {
                DrawNoData(graphics);
                return;
            }

            int left = 60;
            int right = 25;
            int top = 78;
            int bottom = 50;

            int chartWidth =
                Math.Max(
                    100,
                    Width - left - right);

            int chartHeight =
                Math.Max(
                    80,
                    Height - top - bottom);

            double maxValue =
                Math.Max(
                    1,
                    _items.Max(x => x.Value));

            using var gridPen =
                new Pen(GridColor);

            using var axisPen =
                new Pen(
                    Color.FromArgb(
                        210,
                        205,
                        198));

            using var axisFont =
                new Font(
                    "Segoe UI",
                    7.5f);

            using var axisBrush =
                new SolidBrush(TextMuted);

            for (int i = 0; i <= 4; i++)
            {
                float ratio =
                    i / 4f;

                float y =
                    top +
                    chartHeight -
                    chartHeight * ratio;

                graphics.DrawLine(
                    gridPen,
                    left,
                    y,
                    left + chartWidth,
                    y);

                double value =
                    maxValue * ratio;

                graphics.DrawString(
                    FormatNumber(value),
                    axisFont,
                    axisBrush,
                    12,
                    y - 8);
            }

            graphics.DrawLine(
                axisPen,
                left,
                top,
                left,
                top + chartHeight);

            graphics.DrawLine(
                axisPen,
                left,
                top + chartHeight,
                left + chartWidth,
                top + chartHeight);

            float slotWidth =
                chartWidth /
                (float)_items.Count;

            float barWidth =
                Math.Min(
                    65,
                    slotWidth * 0.52f);

            using var labelFont =
                new Font(
                    "Segoe UI",
                    7.5f);

            using var valueFont =
                new Font(
                    "Segoe UI",
                    8,
                    FontStyle.Bold);

            using var labelBrush =
                new SolidBrush(TextMuted);

            using var valueBrush =
                new SolidBrush(TextDark);

            for (int i = 0; i < _items.Count; i++)
            {
                ManagerDashboardChartItem item =
                    _items[i];

                float barHeight =
                    (float)(
                        item.Value /
                        maxValue *
                        chartHeight);

                float x =
                    left +
                    slotWidth * i +
                    (slotWidth - barWidth) / 2f;

                float y =
                    top +
                    chartHeight -
                    barHeight;

                using var barBrush =
                    new SolidBrush(item.Color);

                graphics.FillRectangle(
                    barBrush,
                    x,
                    y,
                    barWidth,
                    Math.Max(2, barHeight));

                string valueText =
                    FormatNumber(item.Value);

                SizeF valueSize =
                    graphics.MeasureString(
                        valueText,
                        valueFont);

                graphics.DrawString(
                    valueText,
                    valueFont,
                    valueBrush,
                    x +
                    (barWidth - valueSize.Width) / 2f,
                    Math.Max(top, y - 20));

                string label =
                    ShortenLabel(
                        item.Label,
                        12);

                SizeF labelSize =
                    graphics.MeasureString(
                        label,
                        labelFont);

                graphics.DrawString(
                    label,
                    labelFont,
                    labelBrush,
                    x +
                    (barWidth - labelSize.Width) / 2f,
                    top +
                    chartHeight +
                    10);
            }
        }

        // =========================================================
        // DONUT GRAPH
        // =========================================================

        private void DrawDonutChart(Graphics graphics)
        {
            double total =
                _items.Sum(x => x.Value);

            if (_items.Count == 0 || total <= 0)
            {
                DrawNoData(graphics);
                return;
            }

            int headerReserve = 72;
            int bottomPadding = 20;

            int availableHeight =
                Math.Max(
                    140,
                    Height -
                    headerReserve -
                    bottomPadding);

            int availableWidth =
                Math.Max(
                    200,
                    Width - 40);

            int legendWidth = 110;
            int gapBetween = 28;

            int maxDonutByWidth =
                availableWidth -
                legendWidth -
                gapBetween;

            int diameter =
                Math.Min(
                    availableHeight,
                    maxDonutByWidth);

            diameter =
                Math.Min(
                    diameter,
                    260);

            diameter =
                Math.Max(
                    diameter,
                    140);

            int blockWidth =
                diameter +
                gapBetween +
                legendWidth;

            int startX =
                Math.Max(
                    20,
                    (Width - blockWidth) / 2);

            int donutY =
                headerReserve +
                Math.Max(
                    0,
                    (availableHeight - diameter) / 2);

            Rectangle donutRect =
                new Rectangle(
                    startX,
                    donutY,
                    diameter,
                    diameter);

            float startAngle = -90f;

            foreach (
                ManagerDashboardChartItem item
                in _items)
            {
                if (item.Value <= 0)
                {
                    continue;
                }

                float sweepAngle =
                    (float)(
                        item.Value /
                        total *
                        360);

                using var brush =
                    new SolidBrush(item.Color);

                graphics.FillPie(
                    brush,
                    donutRect,
                    startAngle,
                    sweepAngle);

                startAngle += sweepAngle;
            }

            int holeSize =
                (int)(
                    diameter * 0.58);

            Rectangle holeRect =
                new Rectangle(
                    donutRect.X +
                    (diameter - holeSize) / 2,
                    donutRect.Y +
                    (diameter - holeSize) / 2,
                    holeSize,
                    holeSize);

            using var holeBrush =
                new SolidBrush(Color.White);

            graphics.FillEllipse(
                holeBrush,
                holeRect);

            float totalFontSize =
                diameter >= 200
                    ? 22f
                    : 17f;

            using var totalFont =
                new Font(
                    "Segoe UI",
                    totalFontSize,
                    FontStyle.Bold);

            using var totalBrush =
                new SolidBrush(TextDark);

            string totalText =
                FormatNumber(total);

            SizeF totalSize =
                graphics.MeasureString(
                    totalText,
                    totalFont);

            graphics.DrawString(
                totalText,
                totalFont,
                totalBrush,
                donutRect.X +
                (diameter - totalSize.Width) / 2f,
                donutRect.Y +
                diameter / 2f -
                totalSize.Height * 0.65f);

            using var centerFont =
                new Font(
                    "Segoe UI",
                    8f,
                    FontStyle.Bold);

            using var centerBrush =
                new SolidBrush(TextMuted);

            string centerText = "BEDS";

            SizeF centerSize =
                graphics.MeasureString(
                    centerText,
                    centerFont);

            graphics.DrawString(
                centerText,
                centerFont,
                centerBrush,
                donutRect.X +
                (diameter - centerSize.Width) / 2f,
                donutRect.Y +
                diameter / 2f +
                8);

            int legendX =
                donutRect.Right +
                gapBetween;

            int legendItemHeight = 42;

            int legendBlockHeight =
                _items.Count *
                legendItemHeight;

            int legendY =
                donutRect.Y +
                Math.Max(
                    0,
                    (diameter -
                     legendBlockHeight) / 2);

            using var legendFont =
                new Font(
                    "Segoe UI",
                    9.5f);

            using var legendValueFont =
                new Font(
                    "Segoe UI",
                    11f,
                    FontStyle.Bold);

            foreach (
                ManagerDashboardChartItem item
                in _items)
            {
                using var legendBrush =
                    new SolidBrush(item.Color);

                graphics.FillRectangle(
                    legendBrush,
                    legendX,
                    legendY + 4,
                    12,
                    12);

                using var labelBrush =
                    new SolidBrush(TextMuted);

                using var valueBrush =
                    new SolidBrush(TextDark);

                graphics.DrawString(
                    item.Label,
                    legendFont,
                    labelBrush,
                    legendX + 20,
                    legendY);

                graphics.DrawString(
                    FormatNumber(item.Value),
                    legendValueFont,
                    valueBrush,
                    legendX + 20,
                    legendY + 18);

                legendY += legendItemHeight;
            }
        }

        // =========================================================
        // LINE GRAPH
        // =========================================================

        private void DrawLineChart(Graphics graphics)
        {
            if (_items.Count == 0)
            {
                DrawNoData(graphics);
                return;
            }

            int left = 62;
            int right = 30;
            int top = 78;
            int bottom = 50;

            int chartWidth =
                Math.Max(
                    100,
                    Width - left - right);

            int chartHeight =
                Math.Max(
                    80,
                    Height - top - bottom);

            double maxValue =
                Math.Max(
                    1,
                    _items.Max(x => x.Value));

            using var gridPen =
                new Pen(GridColor);

            using var axisPen =
                new Pen(
                    Color.FromArgb(
                        210,
                        205,
                        198));

            using var axisFont =
                new Font(
                    "Segoe UI",
                    7.5f);

            using var axisBrush =
                new SolidBrush(TextMuted);

            for (int i = 0; i <= 4; i++)
            {
                float ratio =
                    i / 4f;

                float y =
                    top +
                    chartHeight -
                    chartHeight * ratio;

                graphics.DrawLine(
                    gridPen,
                    left,
                    y,
                    left + chartWidth,
                    y);

                graphics.DrawString(
                    FormatCurrencyShort(
                        maxValue * ratio),
                    axisFont,
                    axisBrush,
                    10,
                    y - 8);
            }

            graphics.DrawLine(
                axisPen,
                left,
                top,
                left,
                top + chartHeight);

            graphics.DrawLine(
                axisPen,
                left,
                top + chartHeight,
                left + chartWidth,
                top + chartHeight);

            if (_items.Count == 1)
            {
                DrawSinglePoint(
                    graphics,
                    _items[0],
                    left,
                    top,
                    chartWidth,
                    chartHeight,
                    maxValue);

                return;
            }

            float spacing =
                chartWidth /
                (float)(_items.Count - 1);

            List<PointF> points = new();

            for (int i = 0; i < _items.Count; i++)
            {
                float x =
                    left +
                    spacing * i;

                float y =
                    top +
                    chartHeight -
                    (float)(
                        _items[i].Value /
                        maxValue *
                        chartHeight);

                points.Add(
                    new PointF(x, y));
            }

            using var linePen =
                new Pen(
                    ChartAccent,
                    3f);

            linePen.StartCap =
                LineCap.Round;

            linePen.EndCap =
                LineCap.Round;

            linePen.LineJoin =
                LineJoin.Round;

            graphics.DrawLines(
                linePen,
                points.ToArray());

            using var pointBrush =
                new SolidBrush(ChartAccent);

            using var pointBorderPen =
                new Pen(
                    Color.White,
                    2f);

            using var valueFont =
                new Font(
                    "Segoe UI",
                    8,
                    FontStyle.Bold);

            using var valueBrush =
                new SolidBrush(TextDark);

            using var labelFont =
                new Font(
                    "Segoe UI",
                    7.5f);

            using var labelBrush =
                new SolidBrush(TextMuted);

            for (int i = 0; i < points.Count; i++)
            {
                PointF point =
                    points[i];

                graphics.FillEllipse(
                    pointBrush,
                    point.X - 5,
                    point.Y - 5,
                    10,
                    10);

                graphics.DrawEllipse(
                    pointBorderPen,
                    point.X - 5,
                    point.Y - 5,
                    10,
                    10);

                string valueText =
                    FormatCurrencyShort(
                        _items[i].Value);

                SizeF valueSize =
                    graphics.MeasureString(
                        valueText,
                        valueFont);

                graphics.DrawString(
                    valueText,
                    valueFont,
                    valueBrush,
                    point.X -
                    valueSize.Width / 2f,
                    Math.Max(
                        top,
                        point.Y - 23));

                string label =
                    ShortenLabel(
                        _items[i].Label,
                        10);

                SizeF labelSize =
                    graphics.MeasureString(
                        label,
                        labelFont);

                graphics.DrawString(
                    label,
                    labelFont,
                    labelBrush,
                    point.X -
                    labelSize.Width / 2f,
                    top +
                    chartHeight +
                    10);
            }
        }

        private void DrawSinglePoint(
            Graphics graphics,
            ManagerDashboardChartItem item,
            int left,
            int top,
            int chartWidth,
            int chartHeight,
            double maxValue)
        {
            float x =
                left +
                chartWidth / 2f;

            float y =
                top +
                chartHeight -
                (float)(
                    item.Value /
                    maxValue *
                    chartHeight);

            using var brush =
                new SolidBrush(ChartAccent);

            graphics.FillEllipse(
                brush,
                x - 6,
                y - 6,
                12,
                12);
        }

        // =========================================================
        // NO DATA
        // =========================================================

        private void DrawNoData(Graphics graphics)
        {
            using var font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);

            using var brush =
                new SolidBrush(TextMuted);

            const string text =
                "No data available yet";

            SizeF size =
                graphics.MeasureString(
                    text,
                    font);

            graphics.DrawString(
                text,
                font,
                brush,
                (Width - size.Width) / 2f,
                (Height - size.Height) / 2f);
        }

        // =========================================================
        // FORMATTERS
        // =========================================================

        private string FormatNumber(double value)
        {
            if (value >= 1000000)
            {
                return $"{value / 1000000:0.#}M";
            }

            if (value >= 1000)
            {
                return $"{value / 1000:0.#}K";
            }

            if (
                Math.Abs(
                    value -
                    Math.Round(value)) < 0.01)
            {
                return value.ToString("0");
            }

            return value.ToString("0.##");
        }

        private string FormatCurrencyShort(double value)
        {
            if (value >= 1000000)
            {
                return $"₱{value / 1000000:0.#}M";
            }

            if (value >= 1000)
            {
                return $"₱{value / 1000:0.#}K";
            }

            return $"₱{value:0}";
        }

        private string ShortenLabel(
            string text,
            int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            if (text.Length <= maxLength)
            {
                return text;
            }

            return
                text.Substring(
                    0,
                    Math.Max(
                        1,
                        maxLength - 1))
                + "…";
        }
    }

    // =============================================================
    // MANAGER DASHBOARD FORM
    // =============================================================

    public class ManagerDashboardForm : Form
    {
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

        private static readonly Color BorderColor =
            Color.FromArgb(225, 215, 200);

        private static readonly Color SidebarHover =
            Color.FromArgb(58, 44, 31);

        private static readonly Color SidebarSelected =
            Color.FromArgb(80, 59, 40);

        // =========================================================
        // MAIN PANELS
        // =========================================================

        private Panel pnlSidebar = null!;

        private Panel pnlContent = null!;

        private Panel pnlDashboard = null!;

        // =========================================================
        // CHARTS
        // =========================================================

        private ManagerDashboardChartPanel
            pnlTenantStatusChart = null!;

        private ManagerDashboardChartPanel
            pnlOccupancyChart = null!;

        private ManagerDashboardChartPanel
            pnlRevenueChart = null!;

        // =========================================================
        // MANAGER OVERVIEW
        // =========================================================

        private ManagerBorderedPanel
            pnlActivity = null!;

        // =========================================================
        // HEADER
        // =========================================================

        private Label lblPageTitle = null!;

        private Label lblWelcome = null!;

        private Label lblDate = null!;

        private Button btnRefreshDashboard = null!;

        // =========================================================
        // SUMMARY CARDS
        // =========================================================

        private readonly List<ManagerBorderedPanel>
            _summaryCards = new();

        private readonly Dictionary<
            Panel,
            (Label title,
             Label value,
             Label desc)>
            _cardLabels = new();

        // =========================================================
        // SIDEBAR
        // =========================================================

        private readonly Dictionary<
            string,
            Button>
            _sidebarButtons = new();

        private Form? _currentModuleForm;

        // =========================================================
        // API
        // =========================================================

        private readonly string _fullName;

        private readonly ApiService _apiService;

        // =========================================================
        // SUMMARY VALUES
        // =========================================================

        private Label lblTenantsValue = null!;

        private Label lblRoomsValue = null!;

        private Label lblOccupiedBedsValue = null!;

        private Label lblPaymentsValue = null!;

        // =========================================================
        // OVERVIEW VALUES
        // =========================================================

        private Label lblPendingRegistrations = null!;

        private Label lblMaintenanceRequests = null!;

        private Label lblPendingPayments = null!;

        private Label lblRenewals = null!;

        private Label lblFeedback = null!;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ManagerDashboardForm(
            ApiService apiService,
            string fullName)
        {
            _apiService =
                apiService
                ?? throw new ArgumentNullException(
                    nameof(apiService));

            _fullName =
                string.IsNullOrWhiteSpace(fullName)
                    ? "Branch Manager"
                    : fullName;

            BuildDashboard();
        }

        public ManagerDashboardForm(
            ApiService apiService)
        {
            _apiService =
                apiService
                ?? throw new ArgumentNullException(
                    nameof(apiService));

            _fullName =
                "Branch Manager";

            BuildDashboard();
        }

        // =========================================================
        // BUILD DASHBOARD
        // =========================================================

        private void BuildDashboard()
        {
            Text =
                "PBCRM2 - Manager Dashboard";

            StartPosition =
                FormStartPosition.CenterScreen;

            ClientSize =
                new Size(1250, 820);

            MinimumSize =
                new Size(1050, 720);

            BackColor =
                ContentBg;

            FormBorderStyle =
                FormBorderStyle.Sizable;

            MaximizeBox = true;

            MinimizeBox = true;

            DoubleBuffered = true;

            BuildSidebar();

            BuildContent();

            Controls.Add(pnlContent);

            Controls.Add(pnlSidebar);

            Resize +=
                (s, e) =>
                {
                    PerformResponsiveLayout();
                };

            Load +=
                (s, e) =>
                {
                    BeginInvoke(
                        new Action(
                            () =>
                            {
                                PerformResponsiveLayout();
                            }));
                };

            Shown +=
                async (s, e) =>
                {
                    PerformResponsiveLayout();

                    await LoadDashboardDataAsync();

                    PerformResponsiveLayout();
                };

            ShowDashboard();
        }

        // =========================================================
        // SIDEBAR
        // =========================================================

        private void BuildSidebar()
        {
            pnlSidebar =
                new Panel
                {
                    Dock = DockStyle.Left,
                    Width = 250,
                    BackColor = BrandBg
                };

            var picLogo =
                new PictureBox
                {
                    SizeMode =
                        PictureBoxSizeMode.Zoom,

                    Size =
                        new Size(190, 65),

                    Location =
                        new Point(30, 18),

                    BackColor =
                        Color.Transparent
                };

            try
            {
                string logoPath =
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Resources",
                        "logo.png");

                if (File.Exists(logoPath))
                {
                    using var stream =
                        new FileStream(
                            logoPath,
                            FileMode.Open,
                            FileAccess.Read);

                    using var image =
                        Image.FromStream(stream);

                    picLogo.Image =
                        new Bitmap(image);
                }
            }
            catch
            {
            }

            var lblSubtitle =
                new Label
                {
                    Text =
                        "BOARDING HOUSE CRM",

                    ForeColor =
                        Color.FromArgb(
                            175,
                            155,
                            130),

                    Font =
                        new Font(
                            "Segoe UI",
                            8,
                            FontStyle.Bold),

                    AutoSize = false,

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Size =
                        new Size(250, 25),

                    Location =
                        new Point(0, 88)
                };

            var divider =
                new Panel
                {
                    Size =
                        new Size(190, 1),

                    Location =
                        new Point(30, 120),

                    BackColor =
                        Color.FromArgb(
                            85,
                            65,
                            45)
                };

            pnlSidebar.Controls.Add(picLogo);

            pnlSidebar.Controls.Add(lblSubtitle);

            pnlSidebar.Controls.Add(divider);

            int y = 140;

            AddSidebarButton(
                "⌂",
                "Dashboard",
                y,
                true);

            y += 50;

            AddSidebarButton(
                "♙",
                "Tenant Management",
                y);

            y += 50;

            AddSidebarButton(
                "▣",
                "Rooms & Beds",
                y);

            y += 50;

            AddSidebarButton(
                "₱",
                "Billing & Payments",
                y);

            y += 50;

            AddSidebarButton(
                "⚒",
                "Maintenance",
                y);

            y += 50;

            AddSidebarButton(
                "☷",
                "Tenant Support",
                y);

            y += 50;

            AddSidebarButton(
                "★",
                "Feedback & Satisfaction",
                y);

            y += 50;

            AddSidebarButton(
                "↻",
                "Renewal & Retention",
                y);

            y += 50;

            AddSidebarButton(
                "▤",
                "Reports",
                y);

            y += 50;

            AddSidebarButton(
                "⌂",
                "Branch Management",
                y);

            BuildUserPanel();
        }

        private void AddSidebarButton(
            string icon,
            string text,
            int y,
            bool selected = false)
        {
            var button =
                new Button
                {
                    Text =
                        $"  {icon}    {text}",

                    Location =
                        new Point(15, y),

                    Size =
                        new Size(220, 42),

                    FlatStyle =
                        FlatStyle.Flat,

                    TextAlign =
                        ContentAlignment.MiddleLeft,

                    Font =
                        new Font(
                            "Segoe UI",
                            9.3f),

                    Cursor =
                        Cursors.Hand,

                    BackColor =
                        selected
                            ? SidebarSelected
                            : Color.Transparent,

                    ForeColor =
                        selected
                            ? BrandAccent
                            : Color.FromArgb(
                                215,
                                205,
                                192),

                    TabStop = false,

                    Tag = text
                };

            button.FlatAppearance.BorderSize = 0;

            _sidebarButtons[text] = button;

            button.MouseEnter +=
                (s, e) =>
                {
                    if (
                        button.BackColor !=
                        SidebarSelected)
                    {
                        button.BackColor =
                            SidebarHover;

                        button.ForeColor =
                            BrandAccent;
                    }
                };

            button.MouseLeave +=
                (s, e) =>
                {
                    if (
                        button.BackColor !=
                        SidebarSelected)
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
                async (s, e) =>
                {
                    await HandleSidebarClickAsync(text);
                };

            pnlSidebar.Controls.Add(button);
        }

        // =========================================================
        // USER PANEL
        // =========================================================

        private void BuildUserPanel()
        {
            var userPanel =
                new Panel
                {
                    Dock =
                        DockStyle.Bottom,

                    Height = 105,

                    BackColor =
                        Color.FromArgb(
                            42,
                            31,
                            23)
                };

            var lblUser =
                new Label
                {
                    Text =
                        _fullName,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            9,
                            FontStyle.Bold),

                    AutoSize = false,

                    TextAlign =
                        ContentAlignment.MiddleLeft,

                    Size =
                        new Size(190, 25),

                    Location =
                        new Point(25, 15)
                };

            var lblRole =
                new Label
                {
                    Text =
                        "Branch Manager",

                    ForeColor =
                        BrandAccent,

                    Font =
                        new Font(
                            "Segoe UI",
                            8),

                    AutoSize = false,

                    TextAlign =
                        ContentAlignment.MiddleLeft,

                    Size =
                        new Size(190, 20),

                    Location =
                        new Point(25, 40)
                };

            var btnLogout =
                new Button
                {
                    Text =
                        "LOG OUT",

                    Location =
                        new Point(25, 67),

                    Size =
                        new Size(90, 28),

                    FlatStyle =
                        FlatStyle.Flat,

                    BackColor =
                        Color.Transparent,

                    ForeColor =
                        Color.FromArgb(
                            210,
                            190,
                            165),

                    Font =
                        new Font(
                            "Segoe UI",
                            8,
                            FontStyle.Bold),

                    Cursor =
                        Cursors.Hand
                };

            btnLogout.FlatAppearance.BorderColor =
                Color.FromArgb(
                    100,
                    80,
                    60);

            btnLogout.FlatAppearance.BorderSize = 1;

            btnLogout.Click +=
                (s, e) =>
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
                        _apiService.ClearToken();

                        DialogResult =
                            DialogResult.Retry;

                        Close();
                    }
                };

            userPanel.Controls.Add(lblUser);

            userPanel.Controls.Add(lblRole);

            userPanel.Controls.Add(btnLogout);

            pnlSidebar.Controls.Add(userPanel);
        }

        // =========================================================
        // CONTENT
        // =========================================================

        private void BuildContent()
        {
            pnlContent =
                new Panel
                {
                    Dock =
                        DockStyle.Fill,

                    BackColor =
                        ContentBg,

                    AutoScroll =
                        true
                };

            BuildDashboardPanel();

            pnlContent.Controls.Add(pnlDashboard);
        }

        // =========================================================
        // DASHBOARD PANEL
        // =========================================================

        private void BuildDashboardPanel()
        {
            pnlDashboard =
                new Panel
                {
                    Dock =
                        DockStyle.Fill,

                    BackColor =
                        ContentBg,

                    AutoScroll =
                        true
                };

            lblPageTitle =
                new Label
                {
                    Text =
                        "Dashboard",

                    ForeColor =
                        TextDark,

                    Font =
                        new Font(
                            "Segoe UI",
                            24,
                            FontStyle.Bold),

                    AutoSize = true,

                    Location =
                        new Point(35, 25)
                };

            lblWelcome =
                new Label
                {
                    Text =
                        $"Welcome back, {_fullName}",

                    ForeColor =
                        TextMuted,

                    Font =
                        new Font(
                            "Segoe UI",
                            10.5f),

                    AutoSize = true,

                    Location =
                        new Point(38, 72)
                };

            lblDate =
                new Label
                {
                    Text =
                        DateTime.Now.ToString(
                            "dddd, MMMM dd, yyyy"),

                    ForeColor =
                        TextMuted,

                    Font =
                        new Font(
                            "Segoe UI",
                            9),

                    AutoSize = true
                };

            btnRefreshDashboard =
                new Button
                {
                    Text =
                        "↻  Refresh",

                    Size =
                        new Size(95, 32),

                    FlatStyle =
                        FlatStyle.Flat,

                    BackColor =
                        Color.Transparent,

                    ForeColor =
                        CtaColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            8.5f,
                            FontStyle.Bold),

                    Cursor =
                        Cursors.Hand
                };

            btnRefreshDashboard.FlatAppearance.BorderColor =
                CtaColor;

            btnRefreshDashboard.FlatAppearance.BorderSize = 1;

            btnRefreshDashboard.Click +=
                async (s, e) =>
                {
                    await LoadDashboardDataAsync();
                };

            pnlDashboard.Controls.Add(lblPageTitle);

            pnlDashboard.Controls.Add(lblWelcome);

            pnlDashboard.Controls.Add(lblDate);

            pnlDashboard.Controls.Add(btnRefreshDashboard);

            BuildSummaryCards();

            BuildCharts();

            BuildActivityPanel();
        }

        // =========================================================
        // SUMMARY CARDS
        // =========================================================

        private void BuildSummaryCards()
        {
            var tenantsCard =
                CreateSummaryCard(
                    "ACTIVE TENANTS",
                    "0",
                    "Currently active");

            lblTenantsValue =
                _cardLabels[tenantsCard].value;

            _summaryCards.Add(tenantsCard);

            pnlDashboard.Controls.Add(tenantsCard);

            var roomsCard =
                CreateSummaryCard(
                    "ROOMS",
                    "0",
                    "Total rooms");

            lblRoomsValue =
                _cardLabels[roomsCard].value;

            _summaryCards.Add(roomsCard);

            pnlDashboard.Controls.Add(roomsCard);

            var occupiedCard =
                CreateSummaryCard(
                    "OCCUPIED BEDS",
                    "0",
                    "Current occupancy");

            lblOccupiedBedsValue =
                _cardLabels[occupiedCard].value;

            _summaryCards.Add(occupiedCard);

            pnlDashboard.Controls.Add(occupiedCard);

            var paymentsCard =
                CreateSummaryCard(
                    "PAYMENTS",
                    "₱0.00",
                    "Collected this month");

            lblPaymentsValue =
                _cardLabels[paymentsCard].value;

            _summaryCards.Add(paymentsCard);

            pnlDashboard.Controls.Add(paymentsCard);
        }

        private ManagerBorderedPanel CreateSummaryCard(
            string title,
            string value,
            string description)
        {
            var card =
                new ManagerBorderedPanel
                {
                    Size =
                        new Size(200, 100),

                    BackColor =
                        CardBg,

                    BorderColor =
                        BorderColor
                };

            var lblTitle =
                new Label
                {
                    Text =
                        title,

                    ForeColor =
                        TextMuted,

                    Font =
                        new Font(
                            "Segoe UI",
                            8,
                            FontStyle.Bold),

                    AutoSize = false,

                    Size =
                        new Size(170, 20),

                    Location =
                        new Point(15, 10),

                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            var lblValue =
                new Label
                {
                    Text =
                        value,

                    ForeColor =
                        CtaColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            19,
                            FontStyle.Bold),

                    AutoSize = false,

                    Size =
                        new Size(170, 38),

                    Location =
                        new Point(15, 30),

                    TextAlign =
                        ContentAlignment.MiddleRight
                };

            var lblDescription =
                new Label
                {
                    Text =
                        description,

                    ForeColor =
                        TextMuted,

                    Font =
                        new Font(
                            "Segoe UI",
                            8),

                    AutoSize = false,

                    Size =
                        new Size(170, 18),

                    Location =
                        new Point(15, 72),

                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            card.Controls.Add(lblTitle);

            card.Controls.Add(lblValue);

            card.Controls.Add(lblDescription);

            _cardLabels[card] =
                (
                    lblTitle,
                    lblValue,
                    lblDescription);

            return card;
        }

        // =========================================================
        // CHARTS
        // =========================================================

        private void BuildCharts()
        {
            pnlTenantStatusChart =
                new ManagerDashboardChartPanel
                {
                    Title =
                        "Tenant Status",

                    Subtitle =
                        "Current tenant distribution",

                    ChartType =
                        ManagerDashboardChartType.Bar,

                    BackColor =
                        Color.White
                };

            pnlOccupancyChart =
                new ManagerDashboardChartPanel
                {
                    Title =
                        "Bed Occupancy",

                    Subtitle =
                        "Occupied versus available beds",

                    ChartType =
                        ManagerDashboardChartType.Donut,

                    BackColor =
                        Color.White
                };

            pnlRevenueChart =
                new ManagerDashboardChartPanel
                {
                    Title =
                        "Revenue Trend",

                    Subtitle =
                        "Payment collection over the last 6 months",

                    ChartType =
                        ManagerDashboardChartType.Line,

                    BackColor =
                        Color.White
                };

            pnlDashboard.Controls.Add(
                pnlTenantStatusChart);

            pnlDashboard.Controls.Add(
                pnlOccupancyChart);

            pnlDashboard.Controls.Add(
                pnlRevenueChart);
        }

        // =========================================================
        // MANAGER OVERVIEW
        // =========================================================

        private void BuildActivityPanel()
        {
            pnlActivity =
                new ManagerBorderedPanel
                {
                    BackColor =
                        Color.White,

                    BorderColor =
                        BorderColor
                };

            var lblTitle =
                new Label
                {
                    Text =
                        "Manager Overview",

                    ForeColor =
                        TextDark,

                    Font =
                        new Font(
                            "Segoe UI",
                            14,
                            FontStyle.Bold),

                    AutoSize = true,

                    Location =
                        new Point(25, 20)
                };

            var lblSubtitle =
                new Label
                {
                    Text =
                        "Key areas requiring attention.",

                    ForeColor =
                        TextMuted,

                    Font =
                        new Font(
                            "Segoe UI",
                            9),

                    AutoSize = true,

                    Location =
                        new Point(25, 52)
                };

            pnlActivity.Controls.Add(lblTitle);

            pnlActivity.Controls.Add(lblSubtitle);

            lblPendingRegistrations =
                AddStatusItem(
                    pnlActivity,
                    "Pending Registrations",
                    "0",
                    "Review new tenant applications.",
                    95);

            lblMaintenanceRequests =
                AddStatusItem(
                    pnlActivity,
                    "Maintenance Requests",
                    "0",
                    "Requests awaiting action.",
                    145);

            lblPendingPayments =
                AddStatusItem(
                    pnlActivity,
                    "Pending Payments",
                    "0",
                    "Payments requiring attention.",
                    195);

            lblRenewals =
                AddStatusItem(
                    pnlActivity,
                    "Renewals",
                    "0",
                    "Tenants approaching renewal.",
                    245);

            lblFeedback =
                AddStatusItem(
                    pnlActivity,
                    "Feedback",
                    "0",
                    "Recent tenant feedback.",
                    295);

            pnlDashboard.Controls.Add(pnlActivity);
        }

        private Label AddStatusItem(
            Panel parent,
            string title,
            string value,
            string description,
            int y)
        {
            var lblValue =
                new Label
                {
                    Text =
                        value,

                    ForeColor =
                        CtaColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            13,
                            FontStyle.Bold),

                    AutoSize = false,

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Size =
                        new Size(45, 35),

                    Location =
                        new Point(25, y)
                };

            var lblTitle =
                new Label
                {
                    Text =
                        title,

                    ForeColor =
                        TextDark,

                    Font =
                        new Font(
                            "Segoe UI",
                            9,
                            FontStyle.Bold),

                    AutoSize = false,

                    Size =
                        new Size(220, 20),

                    Location =
                        new Point(85, y)
                };

            var lblDescription =
                new Label
                {
                    Text =
                        description,

                    ForeColor =
                        TextMuted,

                    Font =
                        new Font(
                            "Segoe UI",
                            8),

                    AutoSize = false,

                    Size =
                        new Size(390, 20),

                    Location =
                        new Point(85, y + 19)
                };

            parent.Controls.Add(lblValue);

            parent.Controls.Add(lblTitle);

            parent.Controls.Add(lblDescription);

            return lblValue;
        }

        // =========================================================
        // LOAD DASHBOARD DATA
        // =========================================================

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                btnRefreshDashboard.Enabled = false;

                btnRefreshDashboard.Text =
                    "Loading...";

                // -------------------------------------------------
                // TENANTS
                // -------------------------------------------------

                List<JsonElement> tenants =
                    await GetJsonArrayAsync(
                        "api/Tenants");

                int activeTenants =
                    CountStatus(
                        tenants,
                        "Active");

                int pendingRegistrations =
                    CountStatus(
                        tenants,
                        "Pending");

                lblTenantsValue.Text =
                    activeTenants.ToString();

                lblPendingRegistrations.Text =
                    pendingRegistrations.ToString();

                // -------------------------------------------------
                // TENANT GRAPH
                // -------------------------------------------------

                int inactiveTenants =
                    CountStatus(
                        tenants,
                        "Inactive");

                int movedOutTenants =
                    CountStatus(
                        tenants,
                        "Moved Out");

                int otherTenants =
                    Math.Max(
                        0,
                        tenants.Count -
                        activeTenants -
                        pendingRegistrations -
                        inactiveTenants -
                        movedOutTenants);

                var tenantData =
                    new List<ManagerDashboardChartItem>
                    {
                        new()
                        {
                            Label =
                                "Active",

                            Value =
                                activeTenants,

                            Color =
                                Color.FromArgb(
                                    170,
                                    130,
                                    80)
                        },

                        new()
                        {
                            Label =
                                "Pending",

                            Value =
                                pendingRegistrations,

                            Color =
                                Color.FromArgb(
                                    205,
                                    164,
                                    95)
                        }
                    };

                if (movedOutTenants > 0)
                {
                    tenantData.Add(
                        new ManagerDashboardChartItem
                        {
                            Label =
                                "Moved Out",

                            Value =
                                movedOutTenants,

                            Color =
                                Color.FromArgb(
                                    132,
                                    145,
                                    165)
                        });
                }

                if (inactiveTenants > 0)
                {
                    tenantData.Add(
                        new ManagerDashboardChartItem
                        {
                            Label =
                                "Inactive",

                            Value =
                                inactiveTenants,

                            Color =
                                Color.FromArgb(
                                    178,
                                    112,
                                    105)
                        });
                }

                if (otherTenants > 0)
                {
                    tenantData.Add(
                        new ManagerDashboardChartItem
                        {
                            Label =
                                "Other",

                            Value =
                                otherTenants,

                            Color =
                                Color.FromArgb(
                                    150,
                                    150,
                                    150)
                        });
                }

                pnlTenantStatusChart.SetData(
                    tenantData);

                // -------------------------------------------------
                // ROOMS
                // -------------------------------------------------

                List<JsonElement> rooms =
                    await GetJsonArrayAsync(
                        "api/Rooms");

                lblRoomsValue.Text =
                    rooms.Count.ToString();

                // -------------------------------------------------
                // BEDS
                // -------------------------------------------------

                (
                    int totalBeds,
                    int occupiedBeds) =
                    await GetBedStatisticsAsync(
                        rooms);

                lblOccupiedBedsValue.Text =
                    occupiedBeds.ToString();

                int availableBeds =
                    Math.Max(
                        0,
                        totalBeds -
                        occupiedBeds);

                pnlOccupancyChart.SetData(
                    new[]
                    {
                        new ManagerDashboardChartItem
                        {
                            Label =
                                "Occupied",

                            Value =
                                occupiedBeds,

                            Color =
                                Color.FromArgb(
                                    170,
                                    130,
                                    80)
                        },

                        new ManagerDashboardChartItem
                        {
                            Label =
                                "Available",

                            Value =
                                availableBeds,

                            Color =
                                Color.FromArgb(
                                    119,
                                    151,
                                    127)
                        }
                    });

                // -------------------------------------------------
                // BILLING
                // -------------------------------------------------

                List<JsonElement> billings =
                    await GetJsonArrayAsync(
                        "api/Billing");

                lblPendingPayments.Text =
                    CountPendingBillings(
                        billings)
                    .ToString();

                // -------------------------------------------------
                // REVENUE
                // -------------------------------------------------

                (
                    decimal monthlyTotal,
                    List<ManagerDashboardChartItem>
                        revenueData) =
                    await GetRevenueStatisticsAsync(
                        billings);

                lblPaymentsValue.Text =
                    monthlyTotal.ToString(
                        "₱#,##0.00");

                pnlRevenueChart.SetData(
                    revenueData);

                // -------------------------------------------------
                // MAINTENANCE
                // -------------------------------------------------

                List<JsonElement> maintenance =
                    await GetJsonArrayAsync(
                        "api/MaintenanceRequests");

                lblMaintenanceRequests.Text =
                    CountStatus(
                        maintenance,
                        "Pending")
                    .ToString();

                // -------------------------------------------------
                // FEEDBACK
                // -------------------------------------------------

                List<JsonElement> feedback =
                    await GetJsonArrayAsync(
                        "api/Feedback");

                lblFeedback.Text =
                    feedback.Count.ToString();

                // -------------------------------------------------
                // RENEWALS
                // -------------------------------------------------

                List<JsonElement> renewals =
                    await GetJsonArrayAsync(
                        "api/Renewals");

                lblRenewals.Text =
                    renewals.Count.ToString();
            }
            catch
            {
                // Keep dashboard usable even when APIs fail.
            }
            finally
            {
                btnRefreshDashboard.Enabled = true;

                btnRefreshDashboard.Text =
                    "↻  Refresh";

                PerformResponsiveLayout();
            }
        }

        // =========================================================
        // GET ARRAY
        // =========================================================

        private async Task<List<JsonElement>>
            GetJsonArrayAsync(
                string endpoint)
        {
            try
            {
                JsonElement result =
                    await _apiService
                        .GetSilentAsync<JsonElement>(
                            endpoint);

                if (
                    result.ValueKind ==
                    JsonValueKind.Undefined ||
                    result.ValueKind ==
                    JsonValueKind.Null)
                {
                    return new List<JsonElement>();
                }

                return ExtractArray(result);
            }
            catch
            {
                return new List<JsonElement>();
            }
        }

        private List<JsonElement> ExtractArray(
            JsonElement element)
        {
            if (
                element.ValueKind ==
                JsonValueKind.Array)
            {
                return element
                    .EnumerateArray()
                    .ToList();
            }

            if (
                element.ValueKind ==
                JsonValueKind.Object)
            {
                string[] possibleProperties =
                {
                    "data",
                    "items",
                    "results",
                    "tenants",
                    "rooms",
                    "billings",
                    "payments",
                    "maintenanceRequests",
                    "feedbacks",
                    "renewals"
                };

                foreach (
                    string propertyName
                    in possibleProperties)
                {
                    if (
                        TryGetPropertyIgnoreCase(
                            element,
                            propertyName,
                            out JsonElement value) &&
                        value.ValueKind ==
                        JsonValueKind.Array)
                    {
                        return value
                            .EnumerateArray()
                            .ToList();
                    }
                }
            }

            return new List<JsonElement>();
        }

        // =========================================================
        // BED STATISTICS
        // =========================================================

        private async Task<(
            int totalBeds,
            int occupiedBeds)>
            GetBedStatisticsAsync(
                List<JsonElement> rooms)
        {
            int totalBeds = 0;

            int occupiedBeds = 0;

            foreach (
                JsonElement room
                in rooms)
            {
                if (
                    TryGetPropertyIgnoreCase(
                        room,
                        "beds",
                        out JsonElement embeddedBeds) &&
                    embeddedBeds.ValueKind ==
                    JsonValueKind.Array)
                {
                    List<JsonElement> beds =
                        embeddedBeds
                            .EnumerateArray()
                            .ToList();

                    totalBeds += beds.Count;

                    occupiedBeds +=
                        CountOccupiedBeds(beds);

                    continue;
                }

                int bedCount =
                    GetIntProperty(
                        room,
                        "bedCount");

                if (bedCount <= 0)
                {
                    bedCount =
                        GetIntProperty(
                            room,
                            "totalBeds");
                }

                int occupied =
                    GetIntProperty(
                        room,
                        "occupiedBeds");

                if (occupied <= 0)
                {
                    occupied =
                        GetIntProperty(
                            room,
                            "occupied");
                }

                if (bedCount > 0)
                {
                    totalBeds += bedCount;

                    occupiedBeds +=
                        Math.Min(
                            occupied,
                            bedCount);
                }
            }

            await Task.CompletedTask;

            return (
                totalBeds,
                occupiedBeds);
        }

        private int CountOccupiedBeds(
            List<JsonElement> beds)
        {
            int occupied = 0;

            foreach (
                JsonElement bed
                in beds)
            {
                if (
                    TryGetPropertyIgnoreCase(
                        bed,
                        "tenantId",
                        out JsonElement tenantId))
                {
                    if (
                        tenantId.ValueKind !=
                        JsonValueKind.Null &&
                        tenantId.ValueKind !=
                        JsonValueKind.Undefined)
                    {
                        if (
                            tenantId.ValueKind ==
                            JsonValueKind.Number)
                        {
                            if (
                                tenantId.GetInt32() >
                                0)
                            {
                                occupied++;
                            }
                        }
                        else if (
                            tenantId.ValueKind ==
                            JsonValueKind.String &&
                            !string.IsNullOrWhiteSpace(
                                tenantId.GetString()))
                        {
                            occupied++;
                        }
                    }
                }
            }

            return occupied;
        }

        // =========================================================
        // REVENUE
        // =========================================================

        private async Task<(
            decimal monthlyTotal,
            List<ManagerDashboardChartItem>
                revenueData)>
            GetRevenueStatisticsAsync(
                List<JsonElement> billings)
        {
            DateTime firstMonth =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1)
                .AddMonths(-5);

            DateTime currentMonth =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            var monthlyTotals =
                new Dictionary<
                    DateTime,
                    decimal>();

            for (int i = 0; i < 6; i++)
            {
                DateTime month =
                    firstMonth.AddMonths(i);

                monthlyTotals[month] = 0;
            }

            foreach (
                JsonElement billing
                in billings)
            {
                decimal amount =
                    GetDecimalProperty(
                        billing,
                        "totalPaid");

                if (amount <= 0)
                {
                    amount =
                        GetDecimalProperty(
                            billing,
                            "paidAmount");
                }

                if (amount <= 0)
                {
                    amount =
                        GetDecimalProperty(
                            billing,
                            "amountPaid");
                }

                if (amount <= 0)
                {
                    continue;
                }

                DateTime billingDate =
                    GetDateTimeProperty(
                        billing,
                        "dueDate");

                if (billingDate ==
                    DateTime.MinValue)
                {
                    billingDate =
                        GetDateTimeProperty(
                            billing,
                            "billingPeriodStart");
                }

                if (billingDate ==
                    DateTime.MinValue)
                {
                    billingDate =
                        GetDateTimeProperty(
                            billing,
                            "billingPeriodEnd");
                }

                if (billingDate ==
                    DateTime.MinValue)
                {
                    billingDate =
                        GetDateTimeProperty(
                            billing,
                            "paymentDate");
                }

                if (billingDate ==
                    DateTime.MinValue)
                {
                    billingDate =
                        DateTime.Today;
                }

                DateTime billingMonth =
                    new DateTime(
                        billingDate.Year,
                        billingDate.Month,
                        1);

                if (
                    monthlyTotals.ContainsKey(
                        billingMonth))
                {
                    monthlyTotals[billingMonth] +=
                        amount;
                }
                else if (
                    billingMonth >= firstMonth &&
                    billingMonth <= currentMonth)
                {
                    monthlyTotals[billingMonth] =
                        amount;
                }
            }

            decimal monthlyTotal =
                monthlyTotals.ContainsKey(
                    currentMonth)
                    ? monthlyTotals[currentMonth]
                    : 0;

            var revenueData =
                new List<ManagerDashboardChartItem>();

            foreach (
                var item
                in monthlyTotals.OrderBy(
                    x => x.Key))
            {
                revenueData.Add(
                    new ManagerDashboardChartItem
                    {
                        Label =
                            item.Key.ToString("MMM"),

                        Value =
                            (double)item.Value,

                        Color =
                            CtaColor
                    });
            }

            await Task.CompletedTask;

            return (
                monthlyTotal,
                revenueData);
        }

        // =========================================================
        // PENDING BILLINGS
        // =========================================================

        private int CountPendingBillings(
            List<JsonElement> billings)
        {
            return billings.Count(
                billing =>
                {
                    string status =
                        GetStringProperty(
                            billing,
                            "status");

                    return
                        status.Equals(
                            "Pending",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        status.Equals(
                            "Overdue",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        status.Equals(
                            "Partially Paid",
                            StringComparison.OrdinalIgnoreCase);
                });
        }

        // =========================================================
        // STATUS
        // =========================================================

        private int CountStatus(
            List<JsonElement> items,
            string status)
        {
            int count = 0;

            foreach (
                JsonElement item
                in items)
            {
                string currentStatus =
                    GetStringProperty(
                        item,
                        "status");

                if (
                    currentStatus.Equals(
                        status,
                        StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        // =========================================================
        // JSON HELPERS
        // =========================================================

        private bool TryGetPropertyIgnoreCase(
            JsonElement element,
            string propertyName,
            out JsonElement value)
        {
            if (
                element.ValueKind ==
                JsonValueKind.Object)
            {
                foreach (
                    JsonProperty property
                    in element.EnumerateObject())
                {
                    if (
                        property.Name.Equals(
                            propertyName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        value =
                            property.Value;

                        return true;
                    }
                }
            }

            value = default;

            return false;
        }

        private string GetStringProperty(
            JsonElement element,
            string propertyName)
        {
            if (
                TryGetPropertyIgnoreCase(
                    element,
                    propertyName,
                    out JsonElement value))
            {
                if (
                    value.ValueKind ==
                    JsonValueKind.String)
                {
                    return value.GetString()
                           ?? string.Empty;
                }

                return value.ToString();
            }

            return string.Empty;
        }

        private int GetIntProperty(
            JsonElement element,
            string propertyName)
        {
            if (
                TryGetPropertyIgnoreCase(
                    element,
                    propertyName,
                    out JsonElement value))
            {
                if (
                    value.ValueKind ==
                    JsonValueKind.Number &&
                    value.TryGetInt32(
                        out int number))
                {
                    return number;
                }

                if (
                    value.ValueKind ==
                    JsonValueKind.String &&
                    int.TryParse(
                        value.GetString(),
                        out number))
                {
                    return number;
                }
            }

            return 0;
        }

        private decimal GetDecimalProperty(
            JsonElement element,
            string propertyName)
        {
            if (
                TryGetPropertyIgnoreCase(
                    element,
                    propertyName,
                    out JsonElement value))
            {
                if (
                    value.ValueKind ==
                    JsonValueKind.Number &&
                    value.TryGetDecimal(
                        out decimal number))
                {
                    return number;
                }

                if (
                    value.ValueKind ==
                    JsonValueKind.String &&
                    decimal.TryParse(
                        value.GetString(),
                        out number))
                {
                    return number;
                }
            }

            return 0;
        }

        private DateTime GetDateTimeProperty(
            JsonElement element,
            string propertyName)
        {
            if (
                TryGetPropertyIgnoreCase(
                    element,
                    propertyName,
                    out JsonElement value))
            {
                if (
                    value.ValueKind ==
                    JsonValueKind.String &&
                    DateTime.TryParse(
                        value.GetString(),
                        out DateTime date))
                {
                    return date;
                }
            }

            return DateTime.MinValue;
        }

        // =========================================================
        // SIDEBAR NAVIGATION
        // =========================================================

        private async Task HandleSidebarClickAsync(
            string moduleName)
        {
            SetActiveSidebar(moduleName);

            switch (moduleName)
            {
                case "Dashboard":

                    ShowDashboard();

                    await LoadDashboardDataAsync();

                    break;

                case "Tenant Management":

                    OpenTenantManagement();

                    break;

                case "Rooms & Beds":

                    OpenRoomsBeds();

                    break;

                case "Billing & Payments":

                    OpenBillingPayments();

                    break;

                case "Maintenance":

                    OpenMaintenance();

                    break;

                case "Tenant Support":

                    OpenTenantSupport();

                    break;

                case "Feedback & Satisfaction":

                    OpenFeedback();

                    break;

                case "Renewal & Retention":

                    OpenRenewalRetention();

                    break;

                case "Reports":

                    OpenReports();

                    break;

                case "Branch Management":

                    await OpenBranchManagementAsync();

                    break;
            }
        }

        private void SetActiveSidebar(
            string selectedText)
        {
            foreach (
                var item
                in _sidebarButtons)
            {
                Button button = item.Value;

                if (
                    item.Key ==
                    selectedText)
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

        // =========================================================
        // DASHBOARD
        // =========================================================

        private void ShowDashboard()
        {
            CloseCurrentModule();

            SetActiveSidebar("Dashboard");

            pnlContent.Controls.Clear();

            pnlContent.Controls.Add(
                pnlDashboard);

            pnlDashboard.Dock =
                DockStyle.Fill;

            pnlDashboard.Visible = true;

            PerformResponsiveLayout();
        }

        // =========================================================
        // MODULE CLEANUP
        // =========================================================

        private void CloseCurrentModule()
        {
            Form? oldForm =
                _currentModuleForm;

            _currentModuleForm = null;

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

            if (
                pnlContent != null &&
                !pnlContent.IsDisposed)
            {
                pnlContent.Controls.Clear();
            }
        }

        // =========================================================
        // EMBED MODULE
        // =========================================================

        private void EmbedModule(Form form)
        {
            if (
                form == null ||
                form.IsDisposed)
            {
                return;
            }

            CloseCurrentModule();

            try
            {
                form.TopLevel = false;

                form.FormBorderStyle =
                    FormBorderStyle.None;

                form.ShowInTaskbar = false;

                form.Dock =
                    DockStyle.Fill;

                form.Margin =
                    Padding.Empty;

                form.Padding =
                    Padding.Empty;

                form.StartPosition =
                    FormStartPosition.Manual;

                form.BackColor =
                    ContentBg;

                _currentModuleForm =
                    form;

                pnlContent.SuspendLayout();

                pnlContent.AutoScroll = false;

                pnlContent.Controls.Add(form);

                form.Show();

                form.BringToFront();

                pnlContent.ResumeLayout(true);
            }
            catch
            {
                _currentModuleForm = null;

                try
                {
                    if (!form.IsDisposed)
                    {
                        form.Close();
                    }
                }
                catch
                {
                }

                try
                {
                    if (!form.IsDisposed)
                    {
                        form.Dispose();
                    }
                }
                catch
                {
                }

                pnlContent.Controls.Clear();

                pnlContent.AutoScroll = true;

                throw;
            }
        }

        // =========================================================
        // TENANT MANAGEMENT
        // =========================================================

        private void OpenTenantManagement()
        {
            SetActiveSidebar(
                "Tenant Management");

            EmbedModule(
                new TenantManagementForm(
                    _apiService));
        }

        // =========================================================
        // ROOMS & BEDS
        // =========================================================

        private void OpenRoomsBeds()
        {
            SetActiveSidebar(
                "Rooms & Beds");

            EmbedModule(
                new RoomsBedsManagementForm(
                    _apiService));
        }

        // =========================================================
        // BILLING & PAYMENTS
        // =========================================================

        private void OpenBillingPayments()
        {
            SetActiveSidebar(
                "Billing & Payments");

            EmbedModule(
                new ManagerBillingForm(
                    _apiService));
        }

        // =========================================================
        // MAINTENANCE
        // =========================================================

        private void OpenMaintenance()
        {
            SetActiveSidebar(
                "Maintenance");

            EmbedModule(
                new ManagerMaintenanceForm(
                    _apiService));
        }

        // =========================================================
        // FEEDBACK
        // =========================================================

        private void OpenFeedback()
        {
            SetActiveSidebar(
                "Feedback & Satisfaction");

            EmbedModule(
                new ManagerFeedbackForm(
                    _apiService));
        }

        // =========================================================
        // REPORTS
        // =========================================================

        private void OpenReports()
        {
            SetActiveSidebar(
                "Reports");

            EmbedModule(
                new ReportsForm());
        }

        // =========================================================
        // TENANT SUPPORT
        // =========================================================

        private void OpenTenantSupport()
        {
            SetActiveSidebar(
                "Tenant Support");

            EmbedModule(
                new StaffTenantSupportForm(
                    _apiService));
        }

        // =========================================================
        // RENEWAL & RETENTION
        // =========================================================
        //
        // IMPORTANT:
        // Manager uses ManagerRenewalRetentionForm.
        //
        // Do NOT use AdminRenewalRetentionForm here.
        //
        // The ManagerRenewalRetentionForm you provided has:
        //   - Overview
        //   - Renewal Follow-up
        //   - Promotions & Offers
        //
        // =========================================================

        private void OpenRenewalRetention()
        {
            try
            {
                SetActiveSidebar(
                    "Renewal & Retention");

                var form =
                    new PBCRM2.ManagerRenewalRetentionForm(
                        _apiService);

                EmbedModule(form);
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

        private async Task OpenBranchManagementAsync()
        {
            SetActiveSidebar(
                "Branch Management");

            CloseCurrentModule();

            var panel =
                CreateInformationPanel(
                    "My Branch",
                    "Viewing the branch assigned to your Manager account.");

            pnlContent.Controls.Add(panel);

            try
            {
                if (
                    !ApiService.CurrentBranchId.HasValue)
                {
                    AddInformationText(
                        panel,
                        "Branch information is not available in the current login session.",
                        115);

                    return;
                }

                var branches =
                    await _apiService
                        .GetBranchesAsync(
                            ApiService.CurrentCompanyId ?? 0);

                if (branches == null)
                {
                    AddInformationText(
                        panel,
                        "Unable to load branch information.",
                        115);

                    return;
                }

                var branch =
                    branches.FirstOrDefault(
                        x =>
                            x.Id ==
                            ApiService.CurrentBranchId.Value);

                if (branch == null)
                {
                    AddInformationText(
                        panel,
                        "Your assigned branch could not be found.",
                        115);

                    return;
                }

                AddInformationText(
                    panel,
                    $"Assigned Branch: {branch.BranchName}",
                    115);

                AddInformationText(
                    panel,
                    $"Branch ID: {branch.Id}",
                    155);

                AddInformationText(
                    panel,
                    "Managers can view their assigned branch information here. Branch creation, editing, and Manager assignment remain administrative functions.",
                    205);
            }
            catch (Exception ex)
            {
                AddInformationText(
                    panel,
                    $"Unable to load branch information.\n\n{ex.Message}",
                    115);
            }
        }

        // =========================================================
        // INFORMATION PANEL
        // =========================================================

        private Panel CreateInformationPanel(
            string title,
            string description)
        {
            var container =
                new Panel
                {
                    Dock =
                        DockStyle.Fill,

                    BackColor =
                        ContentBg,

                    AutoScroll =
                        true
                };

            var header =
                new ManagerBorderedPanel
                {
                    Location =
                        new Point(35, 30),

                    Size =
                        new Size(800, 170),

                    BackColor =
                        CardBg,

                    BorderColor =
                        BorderColor
                };

            var lblTitle =
                new Label
                {
                    Text =
                        title,

                    ForeColor =
                        TextDark,

                    Font =
                        new Font(
                            "Segoe UI",
                            22,
                            FontStyle.Bold),

                    AutoSize = true,

                    Location =
                        new Point(25, 25)
                };

            var lblDescription =
                new Label
                {
                    Text =
                        description,

                    ForeColor =
                        TextMuted,

                    Font =
                        new Font(
                            "Segoe UI",
                            10),

                    AutoSize = false,

                    Size =
                        new Size(730, 55),

                    Location =
                        new Point(28, 75)
                };

            header.Controls.Add(lblTitle);

            header.Controls.Add(lblDescription);

            container.Controls.Add(header);

            return container;
        }

        private void AddInformationText(
            Panel parent,
            string text,
            int y)
        {
            var label =
                new Label
                {
                    Text =
                        text,

                    ForeColor =
                        TextDark,

                    Font =
                        new Font(
                            "Segoe UI",
                            10),

                    AutoSize = false,

                    Size =
                        new Size(730, 60),

                    Location =
                        new Point(35, y)
                };

            parent.Controls.Add(label);
        }

        // =========================================================
        // RESPONSIVE LAYOUT
        // =========================================================

        private void PerformResponsiveLayout()
        {
            if (
                pnlDashboard == null ||
                !pnlDashboard.Visible)
            {
                return;
            }

            int width =
                pnlDashboard.ClientSize.Width;

            if (width <= 0)
            {
                return;
            }

            pnlDashboard.SuspendLayout();

            try
            {
                const int sidePadding = 28;

                const int gap = 16;

                int formHeight =
                    Math.Max(
                        700,
                        pnlDashboard.ClientSize.Height);

                // =================================================
                // HEADER
                // =================================================

                if (lblDate != null)
                {
                    lblDate.Location =
                        new Point(
                            Math.Max(
                                sidePadding,
                                width -
                                lblDate.Width -
                                sidePadding),
                            24);
                }

                if (
                    btnRefreshDashboard !=
                    null)
                {
                    btnRefreshDashboard.Location =
                        new Point(
                            Math.Max(
                                sidePadding,
                                width -
                                btnRefreshDashboard.Width -
                                sidePadding),
                            58);
                }

                // =================================================
                // AVAILABLE WIDTH
                // =================================================

                int availableWidth =
                    Math.Max(
                        600,
                        width -
                        sidePadding * 2);

                // =================================================
                // SUMMARY CARDS
                // =================================================

                int cardY = 100;

                int cardHeight = 110;

                int cardWidth =
                    Math.Max(
                        160,
                        (
                            availableWidth -
                            gap * 3
                        ) / 4);

                for (
                    int i = 0;
                    i < _summaryCards.Count;
                    i++)
                {
                    ManagerBorderedPanel card =
                        _summaryCards[i];

                    int x =
                        sidePadding +
                        i *
                        (cardWidth + gap);

                    card.Location =
                        new Point(
                            x,
                            cardY);

                    card.Size =
                        new Size(
                            cardWidth,
                            cardHeight);

                    if (
                        _cardLabels.TryGetValue(
                            card,
                            out var labels))
                    {
                        int innerWidth =
                            Math.Max(
                                100,
                                cardWidth - 30);

                        labels.title.Width =
                            innerWidth;

                        labels.value.Width =
                            innerWidth;

                        labels.desc.Width =
                            innerWidth;
                    }

                    card.Invalidate();
                }

                // =================================================
                // TOP GRAPH ROW
                // Tenant Status | Bed Occupancy
                // =================================================

                int graphY =
                    cardY +
                    cardHeight +
                    gap;

                int remainingHeight =
                    Math.Max(
                        500,
                        formHeight -
                        graphY -
                        40);

                int graphHeight =
                    Math.Max(
                        260,
                        (int)(
                            remainingHeight *
                            0.46));

                int graphWidth =
                    Math.Max(
                        400,
                        (
                            availableWidth -
                            gap
                        ) / 2);

                if (width >= 1050)
                {
                    pnlTenantStatusChart.Location =
                        new Point(
                            sidePadding,
                            graphY);

                    pnlTenantStatusChart.Size =
                        new Size(
                            graphWidth,
                            graphHeight);

                    pnlOccupancyChart.Location =
                        new Point(
                            sidePadding +
                            graphWidth +
                            gap,
                            graphY);

                    pnlOccupancyChart.Size =
                        new Size(
                            graphWidth,
                            graphHeight);
                }
                else
                {
                    graphWidth =
                        availableWidth;

                    graphHeight =
                        Math.Max(
                            240,
                            (
                                remainingHeight -
                                gap * 2
                            ) / 3);

                    pnlTenantStatusChart.Location =
                        new Point(
                            sidePadding,
                            graphY);

                    pnlTenantStatusChart.Size =
                        new Size(
                            graphWidth,
                            graphHeight);

                    pnlOccupancyChart.Location =
                        new Point(
                            sidePadding,
                            graphY +
                            graphHeight +
                            gap);

                    pnlOccupancyChart.Size =
                        new Size(
                            graphWidth,
                            graphHeight);
                }

                // =================================================
                // BOTTOM ROW
                // Revenue Trend | Manager Overview
                // =================================================

                int bottomY;

                if (width >= 1050)
                {
                    bottomY =
                        graphY +
                        graphHeight +
                        gap;
                }
                else
                {
                    bottomY =
                        graphY +
                        graphHeight +
                        gap +
                        graphHeight +
                        gap;
                }

                int bottomHeight;

                if (width >= 1050)
                {
                    bottomHeight =
                        Math.Max(
                            280,
                            formHeight -
                            bottomY -
                            28);
                }
                else
                {
                    bottomHeight =
                        Math.Max(
                            260,
                            graphHeight);
                }

                int bottomWidth;

                if (width >= 1050)
                {
                    bottomWidth =
                        Math.Max(
                            400,
                            (
                                availableWidth -
                                gap
                            ) / 2);

                    // Revenue Trend LEFT
                    pnlRevenueChart.Location =
                        new Point(
                            sidePadding,
                            bottomY);

                    pnlRevenueChart.Size =
                        new Size(
                            bottomWidth,
                            bottomHeight);

                    // Manager Overview RIGHT
                    pnlActivity.Location =
                        new Point(
                            sidePadding +
                            bottomWidth +
                            gap,
                            bottomY);

                    pnlActivity.Size =
                        new Size(
                            bottomWidth,
                            bottomHeight);
                }
                else
                {
                    bottomWidth =
                        availableWidth;

                    // Revenue Trend
                    pnlRevenueChart.Location =
                        new Point(
                            sidePadding,
                            bottomY);

                    pnlRevenueChart.Size =
                        new Size(
                            bottomWidth,
                            bottomHeight);

                    // Manager Overview
                    pnlActivity.Location =
                        new Point(
                            sidePadding,
                            bottomY +
                            bottomHeight +
                            gap);

                    pnlActivity.Size =
                        new Size(
                            bottomWidth,
                            bottomHeight);
                }

                // =================================================
                // ACTIVITY LABEL WIDTHS
                // =================================================

                ResizeActivityLabels(
                    pnlActivity,
                    bottomWidth);

                // =================================================
                // SCROLL AREA
                // =================================================

                int bottom =
                    pnlActivity.Bottom + 30;

                pnlDashboard.AutoScrollMinSize =
                    new Size(
                        Math.Max(
                            width,
                            availableWidth +
                            sidePadding * 2),
                        bottom);
            }
            finally
            {
                pnlDashboard.ResumeLayout(true);

                pnlDashboard.PerformLayout();
            }
        }

        // =========================================================
        // RESIZE ACTIVITY LABELS
        // =========================================================

        private void ResizeActivityLabels(
            Panel panel,
            int panelWidth)
        {
            foreach (
                Control control
                in panel.Controls)
            {
                if (
                    control is Label label &&
                    label.Location.X >= 85)
                {
                    label.Width =
                        Math.Max(
                            150,
                            panelWidth -
                            label.Location.X -
                            25);
                }
            }
        }

        // =========================================================
        // FORM CLOSED
        // =========================================================

        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            try
            {
                Form? oldForm =
                    _currentModuleForm;

                _currentModuleForm = null;

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
            }
            catch
            {
            }

            base.OnFormClosed(e);
        }
    }
}
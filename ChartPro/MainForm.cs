using ChartPro.Charting;
using ChartPro.Charting.Interactions;
using ScottPlot;
using ScottPlot.WinForms;

namespace ChartPro;

public partial class MainForm : Form
{
    private readonly IChartInteractions _chartInteractions;
    private FormsPlot? _formsPlot;
    private List<OHLC> _candles = new();

    public MainForm(IChartInteractions chartInteractions)
    {
        _chartInteractions = chartInteractions ?? throw new ArgumentNullException(nameof(chartInteractions));
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        // MainForm setup
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 700);
        Name = "MainForm";
        Text = "ChartPro - Trading Chart with ScottPlot 5";
        KeyPreview = true; // Enable keyboard shortcuts

        // Create FormsPlot control
        _formsPlot = new FormsPlot
        {
            Dock = DockStyle.Fill,
            Location = new Point(0, 0),
            Name = "formsPlot1",
            Size = new Size(1000, 700)
        };

        // Create toolbar panel
        var toolbarPanel = new Panel
        {
            Dock = DockStyle.Right,
            Width = 200,
            Name = "toolbarPanel",
            BorderStyle = BorderStyle.FixedSingle
        };

        // Create buttons for drawing modes
        var yPos = 10;
        var btnNone = CreateToolButton("None", ChartDrawMode.None, ref yPos);
        var btnTrendLine = CreateToolButton("Trend Line", ChartDrawMode.TrendLine, ref yPos);
        var btnHorizontal = CreateToolButton("Horizontal Line", ChartDrawMode.HorizontalLine, ref yPos);
        var btnVertical = CreateToolButton("Vertical Line", ChartDrawMode.VerticalLine, ref yPos);
        var btnRectangle = CreateToolButton("Rectangle", ChartDrawMode.Rectangle, ref yPos);
        var btnCircle = CreateToolButton("Circle", ChartDrawMode.Circle, ref yPos);
        var btnFibonacci = CreateToolButton("Fibonacci", ChartDrawMode.FibonacciRetracement, ref yPos);

        yPos += 20;
        var btnGenerateSampleData = new Button
        {
            Text = "Generate Sample Data",
            Location = new Point(10, yPos),
            Width = 180,
            Height = 30
        };
        btnGenerateSampleData.Click += (s, e) => GenerateSampleData();

        toolbarPanel.Controls.Add(btnNone);
        toolbarPanel.Controls.Add(btnTrendLine);
        toolbarPanel.Controls.Add(btnHorizontal);
        toolbarPanel.Controls.Add(btnVertical);
        toolbarPanel.Controls.Add(btnRectangle);
        toolbarPanel.Controls.Add(btnCircle);
        toolbarPanel.Controls.Add(btnFibonacci);
        toolbarPanel.Controls.Add(btnGenerateSampleData);

        Controls.Add(_formsPlot);
        Controls.Add(toolbarPanel);

        Load += MainForm_Load;
        FormClosing += MainForm_FormClosing;
        KeyDown += MainForm_KeyDown;

        ResumeLayout(false);
    }

    private Button CreateToolButton(string text, ChartDrawMode mode, ref int yPos)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(10, yPos),
            Width = 180,
            Height = 30,
            Tag = mode
        };
        button.Click += ToolButton_Click;
        yPos += 35;
        return button;
    }

    private void ToolButton_Click(object? sender, EventArgs e)
    {
        if (sender is Button button && button.Tag is ChartDrawMode mode)
        {
            _chartInteractions.SetDrawMode(mode);
            UpdateButtonStyles(button);
        }
    }

    private void UpdateButtonStyles(Button activeButton)
    {
        foreach (Control control in Controls)
        {
            if (control is Panel panel)
            {
                foreach (Control panelControl in panel.Controls)
                {
                    if (panelControl is Button btn && btn.Tag is ChartDrawMode)
                    {
                        btn.BackColor = btn == activeButton ? SystemColors.Highlight : SystemColors.Control;
                        btn.ForeColor = btn == activeButton ? SystemColors.HighlightText : SystemColors.ControlText;
                    }
                }
            }
        }
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        if (_formsPlot == null)
            return;

        // Attach the chart interactions service
        _chartInteractions.Attach(_formsPlot, 0);
        _chartInteractions.EnableAll();

        // Setup initial chart
        _formsPlot.Plot.Title("ChartPro - Trading Chart");
        _formsPlot.Plot.Axes.Bottom.Label.Text = "Time";
        _formsPlot.Plot.Axes.Left.Label.Text = "Price";

        _formsPlot.Refresh();
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Dispose the chart interactions service to unhook event handlers
        _chartInteractions.Dispose();
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        // Undo: Ctrl+Z
        if (e.Control && e.KeyCode == Keys.Z)
        {
            if (_chartInteractions.ShapeManager.Undo())
            {
                e.Handled = true;
            }
        }
        // Redo: Ctrl+Y
        else if (e.Control && e.KeyCode == Keys.Y)
        {
            if (_chartInteractions.ShapeManager.Redo())
            {
                e.Handled = true;
            }
        }
    }

    private void GenerateSampleData()
    {
        if (_formsPlot == null)
            return;

        // Generate sample OHLC data
        _candles = GenerateRandomOHLC(100);

        // Bind candles to the service
        _chartInteractions.BindCandles(_candles);

        // Add candlestick plot
        _formsPlot.Plot.Clear();
        _formsPlot.Plot.Add.Candlestick(_candles);
        _formsPlot.Plot.Axes.AutoScale();

        _formsPlot.Refresh();
    }

    private List<OHLC> GenerateRandomOHLC(int count)
    {
        var random = new Random(0);
        var candles = new List<OHLC>();
        double price = 100;

        for (int i = 0; i < count; i++)
        {
            var open = price;
            var close = price + (random.NextDouble() - 0.5) * 5;
            var high = Math.Max(open, close) + random.NextDouble() * 2;
            var low = Math.Min(open, close) - random.NextDouble() * 2;

            candles.Add(new OHLC(open, high, low, close, DateTime.Now.AddDays(i), TimeSpan.FromDays(1)));
            price = close;
        }

        return candles;
    }
}

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
    private StatusStrip? _statusStrip;
    private ToolStripStatusLabel? _statusMode;
    private ToolStripStatusLabel? _statusCoordinates;
    private ToolStripStatusLabel? _statusShapeInfo;
    private Dictionary<ChartDrawMode, Button> _modeButtons = new();

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
        var btnNone = CreateToolButton("None (ESC)", ChartDrawMode.None, ref yPos, "Cancel drawing and enable pan/zoom");
        var btnTrendLine = CreateToolButton("Trend Line (1)", ChartDrawMode.TrendLine, ref yPos, "Draw a trend line");
        var btnHorizontal = CreateToolButton("Horizontal Line (2)", ChartDrawMode.HorizontalLine, ref yPos, "Draw a horizontal price level");
        var btnVertical = CreateToolButton("Vertical Line (3)", ChartDrawMode.VerticalLine, ref yPos, "Draw a vertical time line");
        var btnRectangle = CreateToolButton("Rectangle (4)", ChartDrawMode.Rectangle, ref yPos, "Draw a rectangular zone");
        var btnCircle = CreateToolButton("Circle (5)", ChartDrawMode.Circle, ref yPos, "Draw a circular shape");
        var btnFibonacci = CreateToolButton("Fibonacci (6)", ChartDrawMode.FibonacciRetracement, ref yPos, "Draw Fibonacci retracement");

        yPos += 20;
        
        // Add snap controls section
        var snapLabel = new System.Windows.Forms.Label
        {
            Text = "Snap/Magnet:",
            Location = new Point(10, yPos),
            Width = 180,
            Height = 20,
            Font = new Font(Font, System.Drawing.FontStyle.Bold)
        };
        yPos += 25;
        
        var chkSnapEnabled = new CheckBox
        {
            Text = "Enable Snap (or hold Shift)",
            Location = new Point(10, yPos),
            Width = 180,
            Height = 20
        };
        chkSnapEnabled.CheckedChanged += (s, e) => 
        {
            _chartInteractions.SnapEnabled = chkSnapEnabled.Checked;
        };
        yPos += 25;
        
        var rbSnapNone = new RadioButton
        {
            Text = "No Snap",
            Location = new Point(10, yPos),
            Width = 180,
            Height = 20,
            Checked = true
        };
        rbSnapNone.CheckedChanged += (s, e) => 
        {
            if (rbSnapNone.Checked)
                _chartInteractions.SnapMode = SnapMode.None;
        };
        yPos += 25;
        
        var rbSnapPrice = new RadioButton
        {
            Text = "Snap to Price Grid",
            Location = new Point(10, yPos),
            Width = 180,
            Height = 20
        };
        rbSnapPrice.CheckedChanged += (s, e) => 
        {
            if (rbSnapPrice.Checked)
                _chartInteractions.SnapMode = SnapMode.Price;
        };
        yPos += 25;
        
        var rbSnapCandle = new RadioButton
        {
            Text = "Snap to Candle OHLC",
            Location = new Point(10, yPos),
            Width = 180,
            Height = 20
        };
        rbSnapCandle.CheckedChanged += (s, e) => 
        {
            if (rbSnapCandle.Checked)
                _chartInteractions.SnapMode = SnapMode.CandleOHLC;
        };
        yPos += 35;

        var btnGenerateSampleData = new Button
        {
            Text = "Generate Sample Data",
            Location = new Point(10, yPos),
            Width = 180,
            Height = 30
        };
        btnGenerateSampleData.Click += (s, e) => GenerateSampleData();

        yPos += 40;
        var btnSaveAnnotations = new Button
        {
            Text = "Save Annotations",
            Location = new Point(10, yPos),
            Width = 180,
            Height = 30
        };
        btnSaveAnnotations.Click += (s, e) => SaveAnnotations();

        yPos += 35;
        var btnLoadAnnotations = new Button
        {
            Text = "Load Annotations",
            Location = new Point(10, yPos),
            Width = 180,
            Height = 30
        };
        btnLoadAnnotations.Click += (s, e) => LoadAnnotations();

        toolbarPanel.Controls.Add(btnNone);
        toolbarPanel.Controls.Add(btnTrendLine);
        toolbarPanel.Controls.Add(btnHorizontal);
        toolbarPanel.Controls.Add(btnVertical);
        toolbarPanel.Controls.Add(btnRectangle);
        toolbarPanel.Controls.Add(btnCircle);
        toolbarPanel.Controls.Add(btnFibonacci);
        toolbarPanel.Controls.Add(snapLabel);
        toolbarPanel.Controls.Add(chkSnapEnabled);
        toolbarPanel.Controls.Add(rbSnapNone);
        toolbarPanel.Controls.Add(rbSnapPrice);
        toolbarPanel.Controls.Add(rbSnapCandle);
        toolbarPanel.Controls.Add(btnGenerateSampleData);
        toolbarPanel.Controls.Add(btnSaveAnnotations);
        toolbarPanel.Controls.Add(btnLoadAnnotations);

        // Create status bar
        _statusStrip = new StatusStrip
        {
            Name = "statusStrip"
        };

        _statusMode = new ToolStripStatusLabel
        {
            Name = "statusMode",
            Text = "Mode: None",
            BorderSides = ToolStripStatusLabelBorderSides.Right,
            BorderStyle = Border3DStyle.Etched,
            Width = 150
        };

        _statusCoordinates = new ToolStripStatusLabel
        {
            Name = "statusCoordinates",
            Text = "X: 0.00, Y: 0.00",
            BorderSides = ToolStripStatusLabelBorderSides.Right,
            BorderStyle = Border3DStyle.Etched,
            Width = 200
        };

        _statusShapeInfo = new ToolStripStatusLabel
        {
            Name = "statusShapeInfo",
            Text = "",
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _statusStrip.Items.Add(_statusMode);
        _statusStrip.Items.Add(_statusCoordinates);
        _statusStrip.Items.Add(_statusShapeInfo);

        Controls.Add(_formsPlot);
        Controls.Add(toolbarPanel);
        Controls.Add(_statusStrip);

        Load += MainForm_Load;
        FormClosing += MainForm_FormClosing;
        KeyDown += MainForm_KeyDown;

        ResumeLayout(false);
        PerformLayout();
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        // Ctrl+Z - Undo
        if (e.Control && e.KeyCode == Keys.Z && !e.Shift)
        {
            if (_chartInteractions.Undo())
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        // Ctrl+Y or Ctrl+Shift+Z - Redo
        else if ((e.Control && e.KeyCode == Keys.Y) || (e.Control && e.Shift && e.KeyCode == Keys.Z))
        {
            if (_chartInteractions.Redo())
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        // Delete - Delete selected shapes
        else if (e.KeyCode == Keys.Delete)
        {
            _chartInteractions.DeleteSelectedShapes();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        // Escape - Cancel drawing mode
        else if (e.KeyCode == Keys.Escape)
        {
            _chartInteractions.SetDrawMode(ChartDrawMode.None);
            UpdateButtonStyles(null!);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
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
        
        if (!string.IsNullOrEmpty(tooltip))
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(button, tooltip);
        }

        // Store button reference for keyboard shortcuts
        _modeButtons[mode] = button;

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

        // Wire up chart interactions events
        _chartInteractions.DrawModeChanged += OnDrawModeChanged;
        _chartInteractions.MouseCoordinatesChanged += OnMouseCoordinatesChanged;
        _chartInteractions.ShapeInfoChanged += OnShapeInfoChanged;

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

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        // Handle ESC key to cancel drawing
        if (e.KeyCode == Keys.Escape)
        {
            _chartInteractions.SetDrawMode(ChartDrawMode.None);
            if (_modeButtons.TryGetValue(ChartDrawMode.None, out var button))
            {
                UpdateButtonStyles(button);
            }
            e.Handled = true;
            return;
        }

        // Handle number keys for tool selection
        ChartDrawMode? mode = e.KeyCode switch
        {
            Keys.D1 or Keys.NumPad1 => ChartDrawMode.TrendLine,
            Keys.D2 or Keys.NumPad2 => ChartDrawMode.HorizontalLine,
            Keys.D3 or Keys.NumPad3 => ChartDrawMode.VerticalLine,
            Keys.D4 or Keys.NumPad4 => ChartDrawMode.Rectangle,
            Keys.D5 or Keys.NumPad5 => ChartDrawMode.Circle,
            Keys.D6 or Keys.NumPad6 => ChartDrawMode.FibonacciRetracement,
            _ => null
        };

        if (mode.HasValue)
        {
            _chartInteractions.SetDrawMode(mode.Value);
            if (_modeButtons.TryGetValue(mode.Value, out var button))
            {
                UpdateButtonStyles(button);
            }
            e.Handled = true;
        }

        // TODO: Implement Ctrl+Z (Undo) and Ctrl+Y (Redo) in future
    }

    private void OnDrawModeChanged(object? sender, ChartDrawMode mode)
    {
        if (_statusMode == null)
            return;

        var modeText = mode switch
        {
            ChartDrawMode.None => "None",
            ChartDrawMode.TrendLine => "Trend Line",
            ChartDrawMode.HorizontalLine => "Horizontal Line",
            ChartDrawMode.VerticalLine => "Vertical Line",
            ChartDrawMode.Rectangle => "Rectangle",
            ChartDrawMode.Circle => "Circle",
            ChartDrawMode.FibonacciRetracement => "Fibonacci",
            _ => mode.ToString()
        };

        _statusMode.Text = $"Mode: {modeText}";
    }

    private void OnMouseCoordinatesChanged(object? sender, Coordinates coordinates)
    {
        if (_statusCoordinates == null)
            return;

        _statusCoordinates.Text = $"X: {coordinates.X:F2}, Y: {coordinates.Y:F2}";
    }

    private void OnShapeInfoChanged(object? sender, string info)
    {
        if (_statusShapeInfo == null)
            return;

        _statusShapeInfo.Text = info;
    }
}

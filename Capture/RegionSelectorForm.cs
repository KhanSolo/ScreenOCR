using System.Drawing.Drawing2D;

namespace ScreenOCR.Capture;

public sealed class RegionSelectorForm : Form
{
    private Point _startPoint;
    private Point _currentPoint;

    private bool _selecting;

    public Rectangle SelectedScreenRectangle { get; private set; }

    public RegionSelectorForm()
    {
        FormBorderStyle =
            FormBorderStyle.None;

        StartPosition =
            FormStartPosition.Manual;

        ShowInTaskbar = false;
        TopMost = true;

        DoubleBuffered = true;

        Cursor = Cursors.Cross;

        // Virtual screen — все подключённые мониторы.
        Bounds = SystemInformation.VirtualScreen;

        KeyPreview = true;

        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;

        KeyDown += OnKeyDown;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        Activate();
        Focus();

        Cursor.Position = Cursor.Position;
    }

    protected override void OnPaint(
        PaintEventArgs e)
    {
        base.OnPaint(e);

        var graphics = e.Graphics;

        graphics.SmoothingMode =
            SmoothingMode.AntiAlias;

        // Затемнение.
        using var overlayBrush =
            new SolidBrush(
                Color.FromArgb(100, 0, 0, 0));

        graphics.FillRectangle(
            overlayBrush,
            ClientRectangle);

        if (!_selecting)
            return;

        var rectangle =
            GetSelectionRectangle();

        if (rectangle.Width <= 0 ||
            rectangle.Height <= 0)
            return;

        // Область выделения немного светлее.
        using var selectionBrush =
            new SolidBrush(
                Color.FromArgb(
                    40,
                    30,
                    144,
                    255));

        graphics.FillRectangle(
            selectionBrush,
            rectangle);

        using var pen =
            new Pen(
                Color.FromArgb(
                    230,
                    30,
                    144,
                    255),
                2);

        graphics.DrawRectangle(
            pen,
            rectangle);

        // Размер выделения.
        var text =
            $"{rectangle.Width} × {rectangle.Height}";

        using var font =
            new Font(
                "Segoe UI",
                10,
                FontStyle.Regular);

        var textSize =
            graphics.MeasureString(
                text,
                font);

        var textX =
            rectangle.X;

        var textY =
            Math.Max(
                0,
                rectangle.Y - textSize.Height - 4);

        var background =
            new RectangleF(
                textX - 4,
                textY - 2,
                textSize.Width + 8,
                textSize.Height + 4);

        using var textBackground =
            new SolidBrush(
                Color.FromArgb(
                    220,
                    20,
                    20,
                    20));

        graphics.FillRectangle(
            textBackground,
            background);

        using var textBrush =
            new SolidBrush(Color.White);

        graphics.DrawString(
            text,
            font,
            textBrush,
            textX,
            textY);
    }

    private void OnMouseDown(
        object? sender,
        MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        _startPoint = e.Location;
        _currentPoint = e.Location;

        _selecting = true;

        Capture = true;

        Invalidate();
    }

    private void OnMouseMove(
        object? sender,
        MouseEventArgs e)
    {
        if (!_selecting)
            return;

        _currentPoint = e.Location;

        Invalidate();
    }

    private void OnMouseUp(
        object? sender,
        MouseEventArgs e)
    {
        if (!_selecting ||
            e.Button != MouseButtons.Left)
            return;

        _currentPoint = e.Location;

        _selecting = false;

        Capture = false;

        var rectangle =
            GetSelectionRectangle();

        if (rectangle.Width < 2 ||
            rectangle.Height < 2)
        {
            DialogResult = DialogResult.Cancel;

            Close();

            return;
        }

        // Перевод из координат формы
        // в координаты virtual screen.
        SelectedScreenRectangle =
            new Rectangle(
                rectangle.X + Bounds.X,
                rectangle.Y + Bounds.Y,
                rectangle.Width,
                rectangle.Height);

        DialogResult =
            DialogResult.OK;

        Close();
    }

    private void OnKeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult =
                DialogResult.Cancel;

            Close();

            return;
        }

        if (e.KeyCode == Keys.Enter &&
            _selecting)
        {
            var rectangle =
                GetSelectionRectangle();

            if (rectangle.Width >= 2 &&
                rectangle.Height >= 2)
            {
                SelectedScreenRectangle =
                    new Rectangle(
                        rectangle.X + Bounds.X,
                        rectangle.Y + Bounds.Y,
                        rectangle.Width,
                        rectangle.Height);

                DialogResult =
                    DialogResult.OK;

                Close();
            }
        }
    }

    private Rectangle GetSelectionRectangle()
    {
        return new Rectangle(
            Math.Min(
                _startPoint.X,
                _currentPoint.X),

            Math.Min(
                _startPoint.Y,
                _currentPoint.Y),

            Math.Abs(
                _currentPoint.X -
                _startPoint.X),

            Math.Abs(
                _currentPoint.Y -
                _startPoint.Y));
    }
}

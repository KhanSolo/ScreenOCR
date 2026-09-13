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
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;

        ShowInTaskbar = false;
        TopMost = true;

        // Очень важно:
        // НЕ используем Form.Opacity.
        Opacity = 0.4;
        // Затемнение рисуем самостоятельно.
        BackColor = Color.Black;

        DoubleBuffered = true;
        KeyPreview = true;

        Cursor = Cursors.Cross;

        // Вся виртуальная область всех мониторов.
        Bounds = SystemInformation.VirtualScreen;

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
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;

        g.SmoothingMode = SmoothingMode.AntiAlias;

        // 1. Затемняем весь экран
        using var overlayBrush =
            new SolidBrush(Color.FromArgb(
                    100,
                    0,
                    0,
                    0));

        g.FillRectangle(
            overlayBrush,
            ClientRectangle);


        // 2. Если ничего не выделяем — всё.
        if (!_selecting)
            return;

        var selection =   GetSelectionRectangle();

        if (selection.Width <= 0 || selection.Height <= 0)  return;
        
        // -------------------------------------------------
        // 3. "Вырезаем" затемнение из выбранной области.
        //
        // Используем режим Copy, чтобы рисовать
        // исходный screenshot-пиксель невозможно.
        //
        // Поэтому вместо настоящего "вырезания"
        // делаем область почти прозрачной.
        // -------------------------------------------------

        using var selectionBrush =
            new SolidBrush(
                Color.FromArgb(
                    10,
                    255,
                    255,
                    255));

        g.FillRectangle(
            selectionBrush,
            selection);

        // -------------------------------------------------
        // 4. Подсветка выбранной области
        // -------------------------------------------------

        using var borderPen =
            new Pen(
                Color.FromArgb(
                    240,
                    0,
                    160,
                    255),
                2);

        g.DrawRectangle(
            borderPen,
            selection);

        // -------------------------------------------------
        // 5. Размер выделения
        // -------------------------------------------------

        DrawSizeLabel(
            g,
            selection);
    }

    private void DrawSizeLabel(
        Graphics g,
        Rectangle rectangle)
    {
        var text =
            $"{rectangle.Width} × {rectangle.Height}";

        using var font =
            new Font(
                "Segoe UI",
                10,
                FontStyle.Regular);

        var size =
            g.MeasureString(
                text,
                font);

        const int padding = 5;

        var x = rectangle.X;

        var y =
            rectangle.Y - size.Height - padding * 2;

        if (y < 0)
            y = rectangle.Y + 3;

        var background =
            new RectangleF(
                x,
                y,
                size.Width + padding * 2,
                size.Height + padding * 2);

        using var backgroundBrush =
            new SolidBrush(
                Color.FromArgb(
                    220,
                    30,
                    30,
                    30));

        g.FillRectangle(
            backgroundBrush,
            background);

        using var textBrush =
            new SolidBrush(Color.White);

        g.DrawString(
            text,
            font,
            textBrush,
            x + padding,
            y + padding);
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
        {
            return;
        }

        _currentPoint = e.Location;

        _selecting = false;

        Capture = false;

        var rectangle =  GetSelectionRectangle();

        if (rectangle.Width < 2 ||
            rectangle.Height < 2)
        {
            DialogResult =      DialogResult.Cancel;

            Close();

            return;
        }

        // Перевод координат формы
        // в координаты виртуального экрана.
        SelectedScreenRectangle =
            new Rectangle(
                rectangle.X + Bounds.X,
                rectangle.Y + Bounds.Y,
                rectangle.Width,
                rectangle.Height);

        DialogResult =  DialogResult.OK;

        Close();
    }

    private void OnKeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult =   DialogResult.Cancel;

            Close();

            return;
        }

        if (e.KeyCode == Keys.Enter &&  _selecting)
        {
            var rectangle =  GetSelectionRectangle();

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
        return new (
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

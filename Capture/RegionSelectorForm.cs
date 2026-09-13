using System.Drawing.Drawing2D;
using ScreenOCR.Capture;

namespace ScreenOCR.Capture;

public sealed class RegionSelectorForm : Form
{
    private readonly Bitmap _screenSnapshot;
    private readonly Rectangle _virtualScreen;

    private Point _startPoint;
    private Point _currentPoint;

    private bool _selecting;

    public Rectangle SelectedScreenRectangle { get; private set; }

    public RegionSelectorForm()
    {
        // 1. Получаем геометрию virtual screen

        _virtualScreen = SystemInformation.VirtualScreen;

        // 2. Сначала снимаем весь экран.
        //
        // ВАЖНО:    // snapshot должен быть сделан ДО показа overlay.

        _screenSnapshot = ScreenCapture.Capture(_virtualScreen);

        // 3. Настраиваем overlay

        FormBorderStyle = FormBorderStyle.None;

        StartPosition = FormStartPosition.Manual;

        Bounds = _virtualScreen;

        ShowInTaskbar = false;

        TopMost = true;

        // Вот здесь используем Form.Opacity.
        Opacity = 0.35;

        // Цвет overlay.
        BackColor = Color.Black;
        Cursor = Cursors.Cross;
        DoubleBuffered = true;
        KeyPreview = true;

        // 4. Mouse

        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;

        // 5. Keyboard

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
        var graphics = e.Graphics;

        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Пока пользователь ничего не выделяет,
        // весь Form уже затемнён через Opacity.

        if (!_selecting)
            return;

        var selection = GetSelectionRectangle();

        if (selection.Width <= 0 || selection.Height <= 0)
            return;

        // -------------------------------------------------
        // Координаты selection относительно Form.
        //
        // Snapshot имеет координаты virtual screen,
        // поэтому нужно учитывать _virtualScreen.X/Y.
        // -------------------------------------------------

        var sourceRectangle = new Rectangle(
                selection.X,
                selection.Y,
                selection.Width,
                selection.Height);

        // -------------------------------------------------
        // Рисуем исходный snapshot поверх затемнённого
        // Form.
        //
        // Таким образом выделенная область становится
        // визуально "незатемнённой".
        // -------------------------------------------------

        graphics.DrawImage(
            _screenSnapshot,
            selection,
            sourceRectangle,
            GraphicsUnit.Pixel);

        // -------------------------------------------------
        // Рамка
        // -------------------------------------------------

        using var borderPen = new Pen(
                Color.FromArgb(
                    240,
                    0,
                    160,
                    255),
                2);

        graphics.DrawRectangle(borderPen, selection);


        // Размер выделения

        DrawSizeLabel(graphics, selection);
    }

    private void DrawSizeLabel(Graphics graphics, Rectangle rectangle)
    {
        var text = $"{rectangle.Width} × {rectangle.Height}";

        using var font =
            new Font(
                "Segoe UI",
                10,
                FontStyle.Regular);

        var textSize =
            graphics.MeasureString(
                text,
                font);

        const int padding = 5;

        var x = rectangle.X;

        var y = rectangle.Y -
            textSize.Height -
            padding * 2;

        if (y < 0)
        {
            y = rectangle.Y + 3;
        }

        var background = new RectangleF(
                x,
                y,
                textSize.Width + padding * 2,
                textSize.Height + padding * 2);

        using var backgroundBrush =
            new SolidBrush(
                Color.FromArgb(
                    220,
                    30,
                    30,
                    30));

        graphics.FillRectangle(
            backgroundBrush,
            background);

        using var textBrush = new SolidBrush(Color.White);

        graphics.DrawString(
            text,
            font,
            textBrush,
            x + padding,
            y + padding);
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        _startPoint = e.Location;

        _currentPoint = e.Location;

        _selecting = true;

        Capture = true;

        Invalidate();
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
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
            DialogResult = DialogResult.Cancel;

            Close();

            return;
        }

        // Перевод координат Form -> virtual screen.

        SelectedScreenRectangle =
            new Rectangle(
                rectangle.X + _virtualScreen.X,

                rectangle.Y + _virtualScreen.Y,

                rectangle.Width,
                rectangle.Height);

        DialogResult = DialogResult.OK;

        Close();
    }

    private void OnKeyDown(
        object? sender,
        KeyEventArgs e)
    {
        // ESC = отмена
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;

            Close();

            return;
        }

        // ENTER = подтверждение
        if (e.KeyCode == Keys.Enter && _selecting)
        {
            var rectangle = GetSelectionRectangle();

            if (rectangle.Width >= 2 &&
                rectangle.Height >= 2)
            {
                SelectedScreenRectangle =
                    new Rectangle(
                        rectangle.X + _virtualScreen.X,
                        rectangle.Y + _virtualScreen.Y,
                        rectangle.Width,
                        rectangle.Height);

                DialogResult = DialogResult.OK;

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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _screenSnapshot.Dispose();
        }

        base.Dispose(disposing);
    }
}

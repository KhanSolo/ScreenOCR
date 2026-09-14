using ScreenOCR.Capture;
using ScreenOCR.Clipboards;
using ScreenOCR.Hotkey;
using ScreenOCR.OCR;

namespace ScreenOCR;

public sealed class ScreenOcrContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly GlobalHotkey? _hotkey;
    private readonly IOcrEngine _ocrEngine;

    private bool _isBusy;

    public ScreenOcrContext()
    {
        _ocrEngine = new DummyOcrEngine();
        _trayIcon = CreateTrayIcon();

        try
        {
            _hotkey = new GlobalHotkey(Keys.T, HotkeyModifiers.Control | HotkeyModifiers.Shift);
            _hotkey.Pressed += OnHotkeyPressed;
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Screen OCR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ExitApplication();
        }
    }

    private NotifyIcon CreateTrayIcon()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Распознать текст",  null,  (_, _) => StartOcr());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Выход", null, (_, _) => ExitApplication());

        return new ()
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "Screen OCR",
            ContextMenuStrip = menu
        };
    }

    private void OnHotkeyPressed() =>  StartOcr();    

    private async void StartOcr()
    {
        if (_isBusy) return;

        _isBusy = true;

        try
        {
            using var selector = new RegionSelectorForm();

            var result = selector.ShowDialog();

            if (result != DialogResult.OK) return;

            var screenRectangle = selector.SelectedScreenRectangle;

            if (screenRectangle.Width <= 0 || screenRectangle.Height <= 0) return;

            using var bitmap = ScreenCapture.Capture(screenRectangle);

            var text = await _ocrEngine.RecognizeAsync(bitmap);

            if (string.IsNullOrWhiteSpace(text))
            {
                _trayIcon.ShowBalloonTip(
                    1500,
                    "Screen OCR",
                    "Текст не найден.",
                    ToolTipIcon.Info);

                return;
            }

            ClipboardService.SetText(text);

            _trayIcon.ShowBalloonTip(
                1000,
                "Screen OCR",
                "Текст скопирован в буфер обмена.",
                ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            _trayIcon.ShowBalloonTip(
                3000,
                "Screen OCR",
                $"Ошибка: {ex.Message}",
                ToolTipIcon.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private void ExitApplication() => ExitThread();

    protected override void ExitThreadCore()
    {
        _hotkey?.Dispose();

        _trayIcon.Visible = false;
        _trayIcon.Dispose();

        base.ExitThreadCore();
    }
}

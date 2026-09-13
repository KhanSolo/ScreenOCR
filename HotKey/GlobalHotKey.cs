using System.Runtime.InteropServices;

namespace ScreenOCR.Hotkey;

[Flags]
public enum HotkeyModifiers : uint
{
    None = 0x0000,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Win = 0x0008
}

public sealed class GlobalHotkey : NativeWindow, IDisposable
{
    private const int WM_HOTKEY = 0x0312;

    private const uint MOD_NOREPEAT = 0x4000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(
        IntPtr hWnd,
        int id,
        uint fsModifiers,
        uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(
        IntPtr hWnd,
        int id);

    private static int _nextId = 0x5000;

    private readonly int _id;
    private bool _registered;

    public event Action? Pressed;

    public GlobalHotkey(Keys key, HotkeyModifiers modifiers)
    {
        _id = Interlocked.Increment(ref _nextId);

        CreateHandle(new CreateParams());

        _registered = RegisterHotKey(
            Handle,
            _id,
            (uint)modifiers | MOD_NOREPEAT,
            (uint)key);

        if (!_registered)
        {
            var error = Marshal.GetLastWin32Error();

            DestroyHandle();

            throw new InvalidOperationException(
                $"Не удалось зарегистрировать горячую клавишу. " +
                $"Win32 error: {error}");
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == _id)
        {
            Pressed?.Invoke();
        }

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (_registered)
        {
            UnregisterHotKey(Handle, _id);

            _registered = false;
        }

        DestroyHandle();
        GC.SuppressFinalize(this);
    }
}

using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ScreenOCR.Clipboards;

public static class ClipboardService
{
    public static void SetText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        // Clipboard в Windows может временно быть занят другим процессом.
        const int attempts = 5;

        for (var i = 0; i < attempts; i++)
        {
            try
            {
                Clipboard.SetText(text);
                return;
            }
            catch (ExternalException)
            {
                Thread.Sleep(50);
            }
        }

        throw new InvalidOperationException("Не удалось записать текст в буфер обмена.");
    }
}

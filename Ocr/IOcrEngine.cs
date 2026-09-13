using System.Drawing;

namespace ScreenOCR.OCR;

public interface IOcrEngine
{
    Task<string> RecognizeAsync(
        Bitmap image,
        CancellationToken cancellationToken = default);
}

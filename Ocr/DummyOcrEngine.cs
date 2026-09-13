using System.Drawing;

namespace ScreenOCR.OCR;

public sealed class DummyOcrEngine : IOcrEngine
{
    public Task<string> RecognizeAsync(
        Bitmap image,
        CancellationToken cancellationToken = default)
    {
        // Здесь впоследствии будет RapidOCR.

        var text =
            $"OCR placeholder\n" +
            $"Image: {image.Width} × {image.Height}";

        return Task.FromResult(text);
    }
}

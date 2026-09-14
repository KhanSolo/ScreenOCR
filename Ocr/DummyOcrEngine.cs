using System.Text;

namespace ScreenOCR.OCR;

public sealed class DummyOcrEngine : IOcrEngine
{
    public Task<string> RecognizeAsync(Bitmap image, CancellationToken cancellationToken = default)
    {
        // Здесь впоследствии будет RapidOCR.
        var sb = new StringBuilder();
        sb.AppendLine("OCR placeholder");
        sb.AppendLine($"Image: {image.Width} × {image.Height}");
        var text = sb.ToString();
        return Task.FromResult(text);
    }
}

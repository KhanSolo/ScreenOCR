using System.Drawing;
using System.Drawing.Imaging;

namespace ScreenOCR.Capture;

public static class ScreenCapture
{
    public static Bitmap Capture(
        Rectangle screenRectangle)
    {
        if (screenRectangle.Width <= 0 ||
            screenRectangle.Height <= 0)
        {
            throw new ArgumentException(
                "Invalid capture rectangle.",
                nameof(screenRectangle));
        }

        var bitmap =
            new Bitmap(
                screenRectangle.Width,
                screenRectangle.Height,
                PixelFormat.Format32bppArgb);

        using var graphics =
            Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(
            screenRectangle.Location,
            Point.Empty,
            screenRectangle.Size,
            CopyPixelOperation.SourceCopy);

        return bitmap;
    }
}

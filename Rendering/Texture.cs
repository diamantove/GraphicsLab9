using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lab9.Core;

namespace Lab9.Rendering;

public sealed class Texture
{
    private readonly Vec3[] pixels;
    private readonly int width;
    private readonly int height;

    public Texture(int width, int height, Vec3[] pixels)
    {
        if (width < 1 || height < 1 || pixels.Length != width * height)
            throw new ArgumentException("Некорректный размер текстуры.");

        this.width = width;
        this.height = height;
        this.pixels = pixels;
    }

    public static Texture Load(string path)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(path, UriKind.Absolute);
        image.EndInit();
        var converted = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
        int width = converted.PixelWidth;
        int height = converted.PixelHeight;
        var bytes = new byte[checked(width * height * 4)];
        converted.CopyPixels(bytes, width * 4, 0);
        var pixels = new Vec3[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Vec3(bytes[i * 4 + 2], bytes[i * 4 + 1], bytes[i * 4]);

        return new Texture(width, height, pixels);
    }

    public static Texture Checkerboard()
    {
        const int size = 128;
        var pixels = new Vec3[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = (x / 16 + y / 16) % 2 == 0
                    ? new Vec3(245, 220, 160)
                    : new Vec3(45, 100, 180);

        return new Texture(size, size, pixels);
    }

    public Vec3 Sample(double u, double v)
    {
        double x = Math.Clamp(u, 0, 1) * (width - 1);
        double y = (1 - Math.Clamp(v, 0, 1)) * (height - 1);
        int x0 = (int)x;
        int y0 = (int)y;
        int x1 = Math.Min(x0 + 1, width - 1);
        int y1 = Math.Min(y0 + 1, height - 1);

        Vec3 top = Vec3.Lerp(pixels[y0 * width + x0], pixels[y0 * width + x1], x - x0);
        Vec3 bottom = Vec3.Lerp(pixels[y1 * width + x0], pixels[y1 * width + x1], x - x0);
        return Vec3.Lerp(top, bottom, y - y0);
    }
}

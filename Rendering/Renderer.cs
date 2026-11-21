using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lab9.Core;
using Lab9.Geometry;

namespace Lab9.Rendering;

public enum RenderMode
{
    Gouraud,
    PhongToon,
    Texture
}

public sealed class Renderer
{
    private readonly Matrix4 view = Matrix4.Rotation(0, 20) * Matrix4.Rotation(1, -30);

    private readonly record struct RasterVertex(
        double X, double Y, Vec3 Position, Vec3 Normal, Vec3 Color, Vec3 UV)
    {
        public static RasterVertex Lerp(RasterVertex a, RasterVertex b, double t) => new(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            Vec3.Lerp(a.Position, b.Position, t),
            Vec3.Lerp(a.Normal, b.Normal, t),
            Vec3.Lerp(a.Color, b.Color, t),
            Vec3.Lerp(a.UV, b.UV, t));
    }

    public BitmapSource Render(Mesh mesh, Matrix4 model, Vec3 light, Vec3 color,
        RenderMode mode, Texture texture, int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        var pixels = new byte[checked(width * height * 4)];
        var depth = new double[width * height];
        Array.Fill(depth, double.NegativeInfinity);
        for (int i = 0; i < depth.Length; i++)
        {
            pixels[i * 4] = 36;
            pixels[i * 4 + 1] = 30;
            pixels[i * 4 + 2] = 26;
            pixels[i * 4 + 3] = 255;
        }

        Matrix4 transform = view * model;
        Vec3[] positions = mesh.Vertices.Select(transform.Transform).ToArray();
        Vec3[] normals = mesh.Normals.Select(transform.TransformNormal).ToArray();
        Vec3 viewLight = view.Transform(light);
        double scale = Math.Min(width, height) / 6.0;

        foreach (Face face in mesh.Faces)
        {
            int[] indices = face.Indices;
            Vec3 faceNormal = Vec3.Cross(positions[indices[1]] - positions[indices[0]],
                positions[indices[2]] - positions[indices[0]]);
            if (faceNormal.Z <= 1e-12)
                continue;

            var polygon = new RasterVertex[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                Vec3 position = positions[index];
                polygon[i] = new RasterVertex(
                    width / 2.0 + position.X * scale,
                    height / 2.0 - position.Y * scale,
                    position, normals[index],
                    Lighting.GouraudColor(position, normals[index], viewLight, color), face.UV[i]);
            }

            FillPolygon(polygon, mode, texture, viewLight, color, pixels, depth, width, height);
        }

        return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
    }

    private static void FillPolygon(RasterVertex[] polygon, RenderMode mode, Texture texture,
        Vec3 light, Vec3 color, byte[] pixels, double[] depth, int width, int height)
    {
        // Работаем с центрами пикселей (x + 0.5, y + 0.5).
        int firstY = Math.Max(0, (int)Math.Ceiling(polygon.Min(p => p.Y) - 0.5));
        int lastY = Math.Min(height - 1, (int)Math.Ceiling(polygon.Max(p => p.Y) - 0.5) - 1);
        var intersections = new List<RasterVertex>(polygon.Length);

        for (int y = firstY; y <= lastY; y++)
        {
            intersections.Clear();
            double scanY = y + 0.5;
            for (int i = 0; i < polygon.Length; i++)
            {
                RasterVertex a = polygon[i];
                RasterVertex b = polygon[(i + 1) % polygon.Length];
                if (scanY < Math.Min(a.Y, b.Y) || scanY >= Math.Max(a.Y, b.Y))
                    continue;

                double t = (scanY - a.Y) / (b.Y - a.Y);
                intersections.Add(RasterVertex.Lerp(a, b, t));
            }

            if (intersections.Count < 2)
                continue;

            intersections.Sort((a, b) => a.X.CompareTo(b.X));
            RasterVertex left = intersections[0];
            RasterVertex right = intersections[^1];
            if (right.X - left.X < 1e-12)
                continue;

            int firstX = Math.Max(0, (int)Math.Ceiling(left.X - 0.5));
            int lastX = Math.Min(width - 1, (int)Math.Ceiling(right.X - 0.5) - 1);
            for (int x = firstX; x <= lastX; x++)
            {
                double t = (x + 0.5 - left.X) / (right.X - left.X);
                RasterVertex point = RasterVertex.Lerp(left, right, t);
                int index = y * width + x;
                if (point.Position.Z <= depth[index])
                    continue;

                depth[index] = point.Position.Z;
                Vec3 result = mode switch
                {
                    RenderMode.Gouraud => point.Color,
                    RenderMode.PhongToon => Lighting.ToonColor(
                        point.Position, point.Normal.Normalized(), light, color),
                    _ => texture.Sample(point.UV.X, point.UV.Y)
                };
                pixels[index * 4] = ToByte(result.Z);
                pixels[index * 4 + 1] = ToByte(result.Y);
                pixels[index * 4 + 2] = ToByte(result.X);
            }
        }
    }

    private static byte ToByte(double value) => (byte)Math.Clamp(Math.Round(value), 0, 255);
}

using Lab9.Core;

namespace Lab9.Rendering;

public static class Lighting
{
    public static double Lambert(Vec3 position, Vec3 normal, Vec3 light)
    {
        Vec3 direction = (light - position).Normalized();
        return Math.Max(0, Vec3.Dot(normal.Normalized(), direction));
    }

    public static Vec3 GouraudColor(Vec3 position, Vec3 normal, Vec3 light, Vec3 color) =>
        color * Lambert(position, normal, light);

    public static Vec3 ToonColor(Vec3 position, Vec3 normal, Vec3 light, Vec3 color)
    {
        double diffuse = 0.2 + Lambert(position, normal, light);
        double level = diffuse < 0.4 ? 0.3 : diffuse < 0.7 ? 1.0 : 1.3;
        return color * level;
    }
}

namespace Lab9.Core;

public readonly record struct Vec3(double X, double Y, double Z)
{
    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vec3 operator *(Vec3 v, double k) => new(v.X * k, v.Y * k, v.Z * k);
    public double Length => Math.Sqrt(Dot(this, this));

    public Vec3 Normalized()
    {
        double length = Length;
        return length < 1e-12 ? new Vec3() : this * (1 / length);
    }

    public static Vec3 Lerp(Vec3 a, Vec3 b, double t) => a * (1 - t) + b * t;
    public static double Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    public static Vec3 Cross(Vec3 a, Vec3 b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);
}

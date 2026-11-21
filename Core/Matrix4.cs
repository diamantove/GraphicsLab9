namespace Lab9.Core;

public sealed class Matrix4
{
    private readonly double[,] m;

    public Matrix4(double[,] values)
    {
        if (values.GetLength(0) != 4 || values.GetLength(1) != 4)
            throw new ArgumentException("Нужна матрица 4 × 4.");

        m = (double[,])values.Clone();
    }

    public static Matrix4 operator *(Matrix4 a, Matrix4 b)
    {
        var result = new double[4, 4];
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < 4; k++)
                    result[i, j] += a.m[i, k] * b.m[k, j];

        return new Matrix4(result);
    }

    public Vec3 Transform(Vec3 p) => new(
        m[0, 0] * p.X + m[0, 1] * p.Y + m[0, 2] * p.Z + m[0, 3],
        m[1, 0] * p.X + m[1, 1] * p.Y + m[1, 2] * p.Z + m[1, 3],
        m[2, 0] * p.X + m[2, 1] * p.Y + m[2, 2] * p.Z + m[2, 3]);

    public Vec3 TransformNormal(Vec3 n)
    {
        Vec3 a = new(m[0, 0], m[1, 0], m[2, 0]);
        Vec3 b = new(m[0, 1], m[1, 1], m[2, 1]);
        Vec3 c = new(m[0, 2], m[1, 2], m[2, 2]);
        double determinant = Vec3.Dot(a, Vec3.Cross(b, c));
        if (Math.Abs(determinant) < 1e-12)
            throw new ArgumentException("Масштаб не должен быть нулевым.");

        return ((Vec3.Cross(b, c) * n.X + Vec3.Cross(c, a) * n.Y +
                 Vec3.Cross(a, b) * n.Z) * (1 / determinant)).Normalized();
    }

    public static Matrix4 Translation(double x, double y, double z) => new(new double[,]
    {
        { 1, 0, 0, x }, { 0, 1, 0, y }, { 0, 0, 1, z }, { 0, 0, 0, 1 }
    });

    public static Matrix4 Scaling(double x, double y, double z) => new(new double[,]
    {
        { x, 0, 0, 0 }, { 0, y, 0, 0 }, { 0, 0, z, 0 }, { 0, 0, 0, 1 }
    });

    public static Matrix4 Rotation(int axis, double degrees)
    {
        double angle = degrees * Math.PI / 180;
        double c = Math.Cos(angle);
        double s = Math.Sin(angle);
        return axis switch
        {
            0 => new(new double[,] { { 1, 0, 0, 0 }, { 0, c, -s, 0 }, { 0, s, c, 0 }, { 0, 0, 0, 1 } }),
            1 => new(new double[,] { { c, 0, s, 0 }, { 0, 1, 0, 0 }, { -s, 0, c, 0 }, { 0, 0, 0, 1 } }),
            2 => new(new double[,] { { c, -s, 0, 0 }, { s, c, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } }),
            _ => throw new ArgumentException("Выберите ось X, Y или Z.")
        };
    }
}

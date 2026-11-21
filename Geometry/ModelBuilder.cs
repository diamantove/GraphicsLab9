using Lab9.Core;

namespace Lab9.Geometry;

public static class ModelBuilder
{
    public static Mesh Cube()
    {
        var vertices = new List<Vec3>();
        foreach (int x in new[] { -1, 1 })
            foreach (int y in new[] { -1, 1 })
                foreach (int z in new[] { -1, 1 })
                    vertices.Add(new Vec3(x, y, z));

        return Create(vertices.ToArray(),
        [
            [2, 0, 1, 3], [1, 0, 4, 5], [4, 0, 2, 6],
            [3, 1, 5, 7], [6, 2, 3, 7], [5, 4, 6, 7]
        ]);
    }

    public static Mesh Tetrahedron() => Create(
    [
        new(1, 1, 1), new(1, -1, -1), new(-1, 1, -1), new(-1, -1, 1)
    ],
    [
        [0, 1, 2], [0, 3, 1], [0, 2, 3], [1, 3, 2]
    ]);

    public static Mesh Octahedron() => Create(
    [
        new(1.5, 0, 0), new(-1.5, 0, 0), new(0, 1.5, 0),
        new(0, -1.5, 0), new(0, 0, 1.5), new(0, 0, -1.5)
    ],
    [
        [4, 0, 2], [4, 2, 1], [4, 1, 3], [4, 3, 0],
        [5, 2, 0], [5, 1, 2], [5, 3, 1], [5, 0, 3]
    ]);

    private static Mesh Create(Vec3[] vertices, int[][] polygons)
    {
        var faces = new List<Face>();
        foreach (int[] indices in polygons)
        {
            Vec3 a = vertices[indices[0]];
            Vec3 normal = Vec3.Cross(vertices[indices[1]] - a, vertices[indices[2]] - a);
            if (Vec3.Dot(normal, a) < 0)
                Array.Reverse(indices);

            Vec3[] uv = indices.Length == 4
                ? [new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)]
                : [new(0, 0, 0), new(1, 0, 0), new(0.5, 1, 0)];
            faces.Add(new Face(indices, uv));
        }

        return new Mesh(vertices, faces.ToArray());
    }
}

using Lab9.Core;

namespace Lab9.Geometry;

public sealed record Face(int[] Indices, Vec3[] UV);

public sealed class Mesh
{
    public Vec3[] Vertices { get; }
    public Face[] Faces { get; }
    public Vec3[] Normals { get; }

    public Mesh(Vec3[] vertices, Face[] faces)
    {
        Vertices = vertices;
        Faces = faces;
        Normals = new Vec3[vertices.Length];

        foreach (Face face in faces)
        {
            int[] indices = face.Indices;
            Vec3 normal = Vec3.Cross(vertices[indices[1]] - vertices[indices[0]],
                vertices[indices[2]] - vertices[indices[0]]).Normalized();

            foreach (int index in indices)
                Normals[index] += normal;
        }

        for (int i = 0; i < Normals.Length; i++)
            Normals[i] = Normals[i].Normalized();
    }
}

using OpenTK.Mathematics;

namespace TimboPhysics;

public class Shape
{
    protected double[][] _baseVertices;
    protected uint[] _baseIndices;
    public double[][] Vertices;
    public uint[] Indices; 
    
    private static Vector2d GetSphereCoord(Vector3d i) // Approximates texture vertex data based on sphere coords
    {
        var len = i.Length;
        Vector2d uv;
        uv.Y = Math.Acos(i.Y / len) / Math.PI;
        uv.X = -(Math.Atan2(i.Z, i.X) / Math.PI + 1.0f) * 0.5f;
        return uv;
    }

    protected double[][] FlattenVertices(Vector3d[] vertices)
    {
        return vertices.Select(a =>
        {
            var tx = GetSphereCoord(a);
            return new [] { a.X,a.Y,a.Z, 0, 0, 0, tx.X, tx.Y};
        }).ToArray();
    }
}
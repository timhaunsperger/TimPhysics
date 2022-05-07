using OpenTK.Mathematics;

namespace TimboPhysics;

public class RectPrism
{
    double[][] _baseVertices = {  // Positions of vertices for basic cube
        new [] { 1, 1,-1, 0, 1, 0, 1.0, 1.0 }, //top right back     0
        new [] { 1,-1,-1, 0, 0, 0, 0.0, 0.0 }, //bottom right back  1
        new [] {-1,-1,-1, 0, 0, 0, 1.0, 0.0 }, //bottom left back   2
        new [] {-1, 1,-1, 0, 1, 0, 0.0, 1.0 }, //top left back      3
        
        new [] { 1, 1, 1, 0, 1, 0, 1.0, 0.0 }, //top right front    4
        new [] { 1,-1, 1, 0, 0, 0, 0.0, 1.0 }, //bottom right front 5
        new [] {-1,-1, 1, 0, 0, 0, 1.0, 1.0 }, //bottom left front  6
        new [] {-1, 1, 1, 0, 1, 0, 0.0, 0.0 }, //top left front     7
    };
    
    uint[] _baseindices = {  // Faces of basic cube
        2, 1, 0,
        0, 3, 2,
        0, 1, 5,
        5, 4, 0,
        6, 2, 3,
        3, 7, 6,
        4, 5, 6,
        6, 7, 4,
        3, 0, 4,
        4, 7, 3,
        5, 1, 2,
        2, 6, 5
    };
    
    public double[][] Vertices;
    public uint[] Indices;
    
    public static Vector2d GetSphereCoord(Vector3d i)
    {
        var len = i.Length;
        Vector2d uv;
        uv.Y = Math.Acos(i.Y / len) / Math.PI;
        uv.X = -(Math.Atan2(i.Z, i.X) / Math.PI + 1.0f) * 0.5f;
        return uv;
    }
    
    public RectPrism(Vector3d offset, double width, double height, double depth, Quaterniond rotation)
    {
        var outVertices = new Vector3d[8];
        
        for (int i = 0; i < _baseVertices.Length; i++) // Scales position of cube vertex to size and rotation of rectangular prism
        {
            var scaledPos =  rotation * new Quaterniond(
                _baseVertices[i][0] * width, _baseVertices[i][1] * height, _baseVertices[i][2] * depth, 0) * Quaterniond.Conjugate(rotation);
            
            outVertices[i] = new Vector3d(scaledPos.Xyz);
        }
        Indices = _baseindices;
        Vertices = outVertices.Select(a =>
        {
            var tx = GetSphereCoord(a);
            return new [] { a.X + offset.X,a.Y + offset.Y,a.Z + offset.Z, a.X, a.Y, a.Z, tx.X, tx.Y};
        }).ToArray();
    }
}
using System.Security.Cryptography;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class Icosphere
{
    static float X=0.525731112119133606f;
    static float Z=0.850650808352039932f;
    static float N=0f;
    private float[][] _baseVertices=
    {
        new float[]{-X,N,Z}, 
        new float[]{X,N,Z}, 
        new float[]{-X,N,-Z}, 
        new float[]{X,N,-Z},
        new float[]{N,Z,X}, 
        new float[]{N,Z,-X}, 
        new float[]{N,-Z,X}, 
        new float[]{N,-Z,-X}, 
        new float[]{Z,X,N}, 
        new float[]{-Z,X, N}, 
        new float[]{Z,-X,N}, 
        new float[]{-Z,-X, N}
    };
    
    uint[] _baseindices = {
        0,4,1,
        0,9,4,
        9,5,4,
        4,5,8,
        4,8,1,
        8,10,1,
        8,3,10,
        5,3,8,
        5,2,3,
        2,7,3,
        7,10,3,
        7,6,10,
        7,11,6,
        11,0,6,
        0,1,6,
        6,1,10,
        9,0,11,
        9,11,2,
        9,2,5,
        7,2,11
    };

    public float[][] Vertices;
    public List<uint> Indices = new List<uint>(); 
    public List<uint> Edges = new List<uint>(); 

    private List<Face> _faces = new();
    private struct Face
    {
        public List<Vector3> faceVertices = new List<Vector3>();

        public Face(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            faceVertices.Insert(0, v1);
            faceVertices.Insert(1, v2);
            faceVertices.Insert(2, v3);
        }
    }

    public Icosphere(int recursion)
    {
        for (int i = 0; i < _baseindices.Length; i+=3)
        {
            var V1 = new Vector3(
                _baseVertices[_baseindices[i]][0], 
                _baseVertices[_baseindices[i]][1],
                _baseVertices[_baseindices[i]][2]);
            var V2 = new Vector3(
                _baseVertices[_baseindices[i+1]][0],
                _baseVertices[_baseindices[i+1]][1],
                _baseVertices[_baseindices[i+1]][2]);
            var V3 = new Vector3(
                _baseVertices[_baseindices[i+2]][0],
                _baseVertices[_baseindices[i+2]][1],
                _baseVertices[_baseindices[i+2]][2]);
            _faces.Add(new Face(V1, V2, V3));
        }
        
        for (int i = 0; i < recursion; i++)
        {
            List<Face> newFaces = new List<Face>();
            foreach (var face in _faces)
            {
                var V0 = face.faceVertices[0].Normalized();
                var V1 = face.faceVertices[1].Normalized();
                var V2 = face.faceVertices[2].Normalized();
                var V3 = ((face.faceVertices[0] + face.faceVertices[1]) / 2).Normalized();
                var V4 = ((face.faceVertices[1] + face.faceVertices[2]) / 2).Normalized();
                var V5 = ((face.faceVertices[2] + face.faceVertices[0]) / 2).Normalized();
                newFaces.Add(new Face(V0, V3, V5));
                newFaces.Add(new Face(V1, V3, V4));
                newFaces.Add(new Face(V2, V4, V5));
                newFaces.Add(new Face(V3, V4, V5));
            }
            _faces = newFaces;
        }

        var outVertices = new List<Vector3>();

        foreach (var face in _faces)
        {
            foreach (var vertex in face.faceVertices)
            {
                if (!outVertices.Contains(vertex))
                    outVertices.Add(vertex);
                Indices.Add(Convert.ToUInt32(outVertices.IndexOf(vertex)));
            }
            
        }

        float tX;
        float tY;
        Vertices = outVertices.Select(a =>
        {
            if (a.X < 0) tX = 0; else tX = 1;
            if (a.Y < 0) tY = 0; else tY = 1;

            return new float[] { a.X,a.Y,a.Z, tX, tY};
        }).ToArray();
    }
}
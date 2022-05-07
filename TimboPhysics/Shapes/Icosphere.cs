using OpenTK.Mathematics;

namespace TimboPhysics;

public class Icosphere
{
    static double X=0.525731112119133606;
    static double Z=0.850650808352039932;
    static double N=0;
    
    private double[][] _baseVertices =  //positions for basic icosahedron vertices
    {
        new []{-X,N,Z}, 
        new []{X,N,Z}, 
        new []{-X,N,-Z}, 
        new []{X,N,-Z},
        new []{N,Z,X}, 
        new []{N,Z,-X}, 
        new []{N,-Z,X}, 
        new []{N,-Z,-X}, 
        new []{Z,X,N}, 
        new []{-Z,X, N}, 
        new []{Z,-X,N}, 
        new []{-Z,-X, N}
    };
    uint[] _baseindices =   //indices for faces of basic icosahedron
    {  
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
    
    public double[][] Vertices;
    public uint[] Indices; 
    public Dictionary<Vector3d, uint> IndexLookup;
    private Face[] faces = new Face[20];
    
    private struct Face // represents one side of the shape
    {
        public Vector3d[] faceVertices = new Vector3d[3];

        public Face(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            faceVertices[0] = v0;
            faceVertices[1] = v1;
            faceVertices[2] = v2;
        }
    }
    
    public static Vector2d GetSphereCoord(Vector3d i)
    {
        var len = i.Length;
        Vector2d uv;
        uv.Y = Math.Acos(i.Y / len) / Math.PI;
        uv.X = -(Math.Atan2(i.Z, i.X) / Math.PI + 1.0f) * 0.5f;
        return uv;
    }
    
    public Icosphere(Icosphere sphere) //Cloning Constructor
    {
        Vertices = new double[sphere.Vertices.Length][];
        for (int i = 0; i < sphere.Vertices.Length; i++)
        {
            Vertices[i] = (double[])sphere.Vertices[i].Clone();
        }
        Indices = (uint[])sphere.Indices.Clone();
        IndexLookup = new Dictionary<Vector3d, uint>(sphere.IndexLookup);
        faces = (Face[])sphere.faces.Clone();
    }
    
    public Icosphere(int recursion, Vector3d offset)
    {
        for (int i = 0; i < _baseindices.Length; i+=3)
        {
            var v0 = new Vector3d(
                _baseVertices[_baseindices[i]][0], 
                _baseVertices[_baseindices[i]][1],
                _baseVertices[_baseindices[i]][2]);
            var v1 = new Vector3d(
                _baseVertices[_baseindices[i+1]][0],
                _baseVertices[_baseindices[i+1]][1],
                _baseVertices[_baseindices[i+1]][2]);
            var v2 = new Vector3d(
                _baseVertices[_baseindices[i+2]][0],
                _baseVertices[_baseindices[i+2]][1],
                _baseVertices[_baseindices[i+2]][2]);
            faces[i/3] = new Face(v0, v1, v2);
        }
        
        for (int i = 0; i < recursion; i++)
        {
            var newFaces = new Face[faces.Length*4];
            var ji = 0;
            for (int j = 0; j < faces.GetLength(0)*4; j+=4)
            {
                var v0 = faces[ji].faceVertices[0].Normalized();
                var v1 = faces[ji].faceVertices[1].Normalized();
                var v2 = faces[ji].faceVertices[2].Normalized();
                var v3 = ((v0 + v1) / 2).Normalized();
                var v4 = ((v1 + v2) / 2).Normalized();
                var v5 = ((v2 + v0) / 2).Normalized();
                
                newFaces[j+0] = new Face(v0, v3, v5);
                newFaces[j+1] = new Face(v3, v1, v4);
                newFaces[j+2] = new Face(v5, v4, v2);
                newFaces[j+3] = new Face(v3, v4, v5);
                ji++;
            }
            faces = newFaces;
        }

        var outVertices = new List<Vector3d>();
        uint lastVert = 0;
        
        IndexLookup = new Dictionary<Vector3d, uint>();
        Indices = new uint[faces.Length*3];

        var indexNum = 0;
        for (int i = 0; i < faces.Length; i++)
        {
            foreach (var vertex in faces[i].faceVertices)
            {
                if (!IndexLookup.ContainsKey(vertex + offset))
                {
                    outVertices.Add(vertex + offset);
                    IndexLookup[vertex + offset] = lastVert;
                    lastVert++;
                }
                Indices[indexNum] = IndexLookup[vertex + offset];
                indexNum++;
            }
        }

        var count = 0;
        Vertices = outVertices.Select(a =>
        {
            var tx = GetSphereCoord(a-offset);
            return new [] { a.X,a.Y,a.Z, 0, 0, 0, tx.X, tx.Y};
        }).ToArray();
        
    }
}
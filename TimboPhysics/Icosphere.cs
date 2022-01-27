using OpenTK.Mathematics;

namespace TimboPhysics;

public class Icosphere
{
    static float X=0.525731112119133606f;
    static float Z=0.850650808352039932f;
    static float N=0f;
    private float[][] _baseVertices=
    {
        new []{-X,N,2*Z}, 
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
    public uint[] Indices; 
    public Dictionary<Vector3, uint> IndexLookup;
    
    private struct Face
    {
        public Vector3[] faceVertices = new Vector3[3];

        public Face(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            faceVertices[0] = v0;
            faceVertices[1] = v1;
            faceVertices[2] = v2;
        }
    }
    
    public static Vector2 GetSphereCoord(Vector3 i)
    {
        var len = i.Length;
        Vector2 uv;
        uv.Y = (float)(Math.Acos(i.Y / len) / Math.PI);
        uv.X = -(float)((Math.Atan2(i.Z, i.X) / Math.PI + 1.0f) * 0.5f);
        return uv;
    }
    
    public Icosphere(int recursion, Game log)
    {
        var faces = new Vector3[20,3];
        for (int i = 0; i < _baseindices.Length; i+=3)
        {
            var v0 = new Vector3(
                _baseVertices[_baseindices[i]][0], 
                _baseVertices[_baseindices[i]][1],
                _baseVertices[_baseindices[i]][2]);
            var v1 = new Vector3(
                _baseVertices[_baseindices[i+1]][0],
                _baseVertices[_baseindices[i+1]][1],
                _baseVertices[_baseindices[i+1]][2]);
            var v2 = new Vector3(
                _baseVertices[_baseindices[i+2]][0],
                _baseVertices[_baseindices[i+2]][1],
                _baseVertices[_baseindices[i+2]][2]);
            //log.log = $"{i}";
            faces[i/3,0] = v0;
            faces[i/3,1] = v1;
            faces[i/3,2] = v2;
        }
        
        for (int i = 0; i < recursion; i++)
        {
            var newFaces = new Vector3[faces.GetLength(0)*4,3];
            var ji = 0;
            for (int j = 0; j < faces.GetLength(0)*4; j+=4)
            {
                var v0 = faces[ji,0].Normalized();
                var v1 = faces[ji,1].Normalized();
                var v2 = faces[ji,2].Normalized();
                var v3 = ((faces[ji,0] + faces[ji,1]) / 2).Normalized();
                var v4 = ((faces[ji,1] + faces[ji,2]) / 2).Normalized();
                var v5 = ((faces[ji,2] + faces[ji,0]) / 2).Normalized();
                newFaces[j+0,0] = v0;
                newFaces[j+0,1] = v3;
                newFaces[j+0,2] = v5;
                
                newFaces[j+1,0] = v1;
                newFaces[j+1,1] = v3;
                newFaces[j+1,2] = v4;
                
                newFaces[j+2,0] = v2;
                newFaces[j+2,1] = v4;
                newFaces[j+2,2] = v5;
                
                newFaces[j+3,0] = v3;
                newFaces[j+3,1] = v4;
                newFaces[j+3,2] = v5;
                ji++;
                log.log = i + " " + j;
            }
            faces = newFaces;
            log.log = faces.GetLength(0).ToString();
        }
        log.log = "vertArray loading";
        
        var outVertices = new List<Vector3>(10000);
        uint lastVert = 0;
        
        IndexLookup = new Dictionary<Vector3, uint>();
        Indices = new uint[faces.GetLength(0)*3];
        
        for (int i = 0; i < faces.GetLength(0)*3; i++)
        {
            if (!IndexLookup.ContainsKey(faces[i/3,i % 3]))
            {
                outVertices.Add(faces[i/3,i%3]);
                IndexLookup[faces[i / 3,i % 3]] = lastVert;
                lastVert++;
            }
            Indices[i] = IndexLookup[faces[i/3,i%3]];

            log.logInt = i;
        }

        log.log = "adding Textures";

        var count = 0;
        Vertices = outVertices.Select(a =>
        {
            log.logInt = count++;
            var tx = GetSphereCoord(a);
            return new [] { a.X,a.Y,a.Z, tx.X, tx.Y};
        }).ToArray();
        log.log = "Loaded";
        log.logInt = 0;
        ;
    }
}
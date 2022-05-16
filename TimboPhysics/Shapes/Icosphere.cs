using OpenTK.Mathematics;

namespace TimboPhysics;

public class Icosphere : Shape
{
    // Values for constructing icosahedron vertices
    static double X=0.525731112119133606;
    static double Z=0.850650808352039932;
    static double N=0;
    
    private Dictionary<Vector3d, uint> _indexLookup;
    private Face[] _faces = new Face[20];
    
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

    public Icosphere(Icosphere sphere) //Cloning Constructor
    {
        Vertices = new double[sphere.Vertices.Length][];
        for (int i = 0; i < sphere.Vertices.Length; i++)
        {
            Vertices[i] = (double[])sphere.Vertices[i].Clone();
        }
        Indices = (uint[])sphere.Indices.Clone();
        _indexLookup = new Dictionary<Vector3d, uint>(sphere._indexLookup);
        _faces = (Face[])sphere._faces.Clone();
    }
    
    public Icosphere(int recursion)
    {
        _baseIndices = new uint[] {  
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
        _baseVertices =  new [] {
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
        // Creates faces out of each set of three indices
        for (int i = 0; i < _baseIndices.Length; i+=3)
        {
            var v0 = new Vector3d(
                _baseVertices[_baseIndices[i]][0], 
                _baseVertices[_baseIndices[i]][1],
                _baseVertices[_baseIndices[i]][2]);
            var v1 = new Vector3d(
                _baseVertices[_baseIndices[i+1]][0],
                _baseVertices[_baseIndices[i+1]][1],
                _baseVertices[_baseIndices[i+1]][2]);
            var v2 = new Vector3d(
                _baseVertices[_baseIndices[i+2]][0],
                _baseVertices[_baseIndices[i+2]][1],
                _baseVertices[_baseIndices[i+2]][2]);
            _faces[i/3] = new Face(v0, v1, v2);
        }
        
        // Divides each face into 4 faces normalized to unit sphere
        for (int i = 0; i < recursion; i++)
        {
            var newFaces = new Face[_faces.Length*4];
            var ji = 0;
            for (int j = 0; j < _faces.GetLength(0)*4; j+=4)
            {
                var v0 = _faces[ji].faceVertices[0].Normalized();
                var v1 = _faces[ji].faceVertices[1].Normalized();
                var v2 = _faces[ji].faceVertices[2].Normalized();
                var v3 = ((v0 + v1) / 2).Normalized();
                var v4 = ((v1 + v2) / 2).Normalized();
                var v5 = ((v2 + v0) / 2).Normalized();
                
                newFaces[j+0] = new Face(v0, v3, v5);
                newFaces[j+1] = new Face(v3, v1, v4);
                newFaces[j+2] = new Face(v5, v4, v2);
                newFaces[j+3] = new Face(v3, v4, v5);
                ji++;
            }
            _faces = newFaces;
        }

        _indexLookup = new Dictionary<Vector3d, uint>();
        Indices = new uint[_faces.Length*3];
        var outVertices = new Vector3d[_faces.Length*3];
        
        uint lastVert = 0;
        uint lastVertPos = 0;
        uint indexNum = 0;
        // Creates list of indices and vertices from face data
        for (int i = 0; i < _faces.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var vertex = _faces[i].faceVertices[j];
                if (!_indexLookup.ContainsKey(vertex))
                {
                    outVertices[lastVertPos] = vertex;
                    lastVertPos++;
                    _indexLookup[vertex] = lastVert;
                    lastVert++;
                }
                Indices[indexNum] = _indexLookup[vertex];
                indexNum++;
            }
        }

        Vertices = FlattenVertices(outVertices);
    }
}
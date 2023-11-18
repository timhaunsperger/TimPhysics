using System.IO.Enumeration;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsObject : RenderObject
{ 
    // Helper field for Collision.cs
    public double Radius;
    public Vector3d Center;
    
    public Dictionary<uint, PhysicsVertex> _vertexLookup;
    public uint[][] Faces;  // Array of arrays storing which vertices are connected to form faces

    //Stores data for state of each vertex
    public struct PhysicsVertex(Vector3d position, Vector3d speed, double mass, PhysicsObject parent)
    {
        public Vector3d Position = position;
        public Vector3d Speed = speed;
        public double Mass = mass;
        public PhysicsObject Parent = parent;
    }

    protected PhysicsObject(Shape shape, Shader shader, double mass)
        : base(shape, shader)
    {
        var indices = shape.Indices;
        _vertexLookup = new Dictionary<uint, PhysicsVertex>();
        Faces = new uint[indices.Length/3][];
        
        for (int i = 0; i < indices.Length; i++)
        {
            if (true)
            {
                var vertexPos = new Vector3d(Vertices[indices[i]][0], Vertices[indices[i]][1], Vertices[indices[i]][2]);
                _vertexLookup[indices[i]] = new PhysicsVertex(vertexPos, Vector3d.Zero, mass/indices.Length, this);
                Center += vertexPos;
            }

            if (i%3==2)
            {
                Faces[i/3] = new uint[3];
                Faces[i/3][0] = indices[i-2];
                Faces[i/3][1] = indices[i-1];
                Faces[i/3][2] = indices[i-0];
            }
        }

        Center /= _vertexLookup.Count;
        UpdateValues(_vertexLookup, 0);
    }

    //Adds speed to position and calculates new max radius and center
    protected void UpdateValues(Dictionary<uint, PhysicsVertex> Vertices, double timeStep)
    {
        var vertexPosSum = Vector3d.Zero;
        Radius = 0d;
        for (uint i = 0; i < Vertices.Count; i++)
        {
            var vertex = Vertices[i];
            vertex.Position += vertex.Speed * timeStep;
            Vertices[i] = vertex;
                
            vertexPosSum += vertex.Position;
            if (Radius < (vertex.Position-Center).LengthFast)
            {
                Radius = (vertex.Position - Center).LengthFast;
            }
                
        }
        Center = vertexPosSum/Vertices.Count;
    }
    
    //Updates vertex array with new positions and normals from physics vertices then flattens for use in OpenGL
    public virtual void Update(double deltaTime)
    {
        for (uint i = 0; i < _vertexLookup.Count; i++)
        {
            var newPos = _vertexLookup[i].Position;
            // Vertex can not have an actual normal, but center to vertex vector is close enough approximation
            var fakeNormal = (newPos - Center).Normalized(); 
            Vertices[i] = new[] {newPos.X, newPos.Y, newPos.Z, fakeNormal.X, fakeNormal.Y, fakeNormal.Z, Vertices[i][6], Vertices[i][7]};
        }
        _flattenedVertices = Vertices.SelectMany(x => x).ToArray();
    }
    
    public void Assign(double[][] vertices)
    {
        for (uint i = 0; i < vertices.Length; i++)
        {
            var newPos = new Vector3d(vertices[i][0], vertices[i][1], vertices[i][2]);
            // Vertex can not have an actual normal, but center to vertex vector is close enough approximation
            var fakeNormal = (newPos - Center).Normalized(); 
            Vertices[i] = new[] {vertices[i][0], vertices[i][1], vertices[i][2], fakeNormal.X, fakeNormal.Y, fakeNormal.Z, Vertices[i][6], Vertices[i][7]};
        }
        _flattenedVertices = Vertices.SelectMany(x => x).ToArray();
    }
}
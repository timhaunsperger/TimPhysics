using System.IO.Enumeration;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsObject : RenderObject
{
    protected bool _collision;
    protected Vector3d _center;
    public Dictionary<uint, PhysicsVertex> _vertexLookup;
    protected uint[][] _faces;  // Array of arrays storing which vertices are connected to form faces
    protected double _maxRadius;

    //Stores data for state of each vertex
    public struct PhysicsVertex
    {
        public Vector3d Position;
        public Vector3d Speed;

        public PhysicsVertex(Vector3d position, Vector3d speed)
        {
            Position = position;
            Speed = speed;
        }
    }

    protected PhysicsObject(double[][] vertices, uint[] indices, Shader shader, bool collision)
        : base(vertices, indices, shader)
    {
        _collision = collision;
        _vertexLookup = new Dictionary<uint, PhysicsVertex>();
        _faces = new uint[indices.Length/3][];
        
        for (int i = 0; i < indices.Length; i++)
        {
            if (!_vertexLookup.ContainsKey(indices[i]))
            {
                var vertexPos = new Vector3d(vertices[indices[i]][0], vertices[indices[i]][1], vertices[indices[i]][2]);
                _vertexLookup[indices[i]] = new PhysicsVertex(vertexPos, Vector3d.Zero);
                _center += vertexPos;
            }

            if (i%3==2)
            {
                _faces[i/3] = new uint[3];
                _faces[i/3][0] = indices[i-2];
                _faces[i/3][1] = indices[i-1];
                _faces[i/3][2] = indices[i-0];
            }
        }

        _center /= _vertexLookup.Count;
    }
    
    //Checks for and responds to collisions with another specific physics object
    protected void Collision(PhysicsObject collisionObject, Dictionary<uint, PhysicsVertex> Vertices)
    {
        // Prevents collision with collision disabled objects
        if (!_collision || !collisionObject._collision)
        {
            return;
        }
        foreach (var vertex in Vertices.Keys)
        {
            if ((collisionObject._center-Vertices[vertex].Position).Length > collisionObject._maxRadius)
            {
                // Prevents unnecessary calculations if vertex is outside bounding radius of collisionObject
                continue;
            }
            var isColliding = true;
            var closestFaceDist = Double.PositiveInfinity;
            var closestFace = Array.Empty<uint>();
            // If vertex is behind all faces of other object it must be colliding
            foreach (var face in collisionObject._faces)
            {
                var distance = TMathUtils.PointPlaneDist(
                    collisionObject._vertexLookup[face[0]].Position,
                    collisionObject._vertexLookup[face[1]].Position,
                    collisionObject._vertexLookup[face[2]].Position,
                    Vertices[vertex].Position);
                if (distance < 0)
                {
                    isColliding = false;
                    break;
                }
                // Finds closest face in other object to the vertex
                if (distance < closestFaceDist)
                {
                    closestFaceDist = distance;
                    closestFace = face;
                }
                
            }
            // Moves vertex and closest face apart, then reflects their velocities
            if (isColliding)
            {
                var forceVertex = Vertices[vertex];
                var forceVector = (_center - forceVertex.Position).Normalized();
                for (int i = 0; i < 3; i++)
                {
                    var faceVertex = collisionObject._vertexLookup[closestFace[i]];
                    faceVertex.Position -= forceVector * closestFaceDist/2;
                    faceVertex.Speed += 2*Vector3d.Dot(forceVector * -1, faceVertex.Speed) * forceVector;
                    collisionObject._vertexLookup[closestFace[i]] = faceVertex;
                }
                forceVertex.Position += forceVector * closestFaceDist/2;
                forceVertex.Speed -= 2*Vector3d.Dot(forceVector, forceVertex.Speed) * forceVector;
                Vertices[vertex] = forceVertex;
            }
        }
    }

    //Adds speed to position and calculates new max radius and center
    protected void UpdateValues(Dictionary<uint, PhysicsVertex> Vertices, double timeStep)
    {
        var vertexPosSum = Vector3d.Zero;
        _maxRadius = 0d;
        for (uint i = 0; i < Vertices.Count; i++)
        {
            var vertex = Vertices[i];
            vertex.Position += vertex.Speed * timeStep;
            Vertices[i] = vertex;
                
            vertexPosSum += vertex.Position;
            if (_maxRadius < (vertex.Position-_center).Length)
            {
                _maxRadius = (vertex.Position - _center).Length;
            }
                
        }
        _center = vertexPosSum/Vertices.Count;
    }
    
    //Updates vertex array with new positions and normals from physics vertices then flattens for use in OpenGL
    private void UpdateVertices()
    {
        for (uint i = 0; i < _vertexLookup.Count; i++)
        {
            var newPos = _vertexLookup[i].Position;
            // Vertex can not have an actual normal, but center to vertex vector is close enough approximation
            var fakeNormal = (newPos - _center).Normalized(); 
            _vertices[i] = new[] {newPos.X, newPos.Y, newPos.Z, fakeNormal.X, fakeNormal.Y, fakeNormal.Z, _vertices[i][6], _vertices[i][7]};
        }
        _flattenedVertices = _vertices.SelectMany(x => x).ToArray();
    }
    
    //Allows for update method visibility when class extensions are saved to PhysicsObject list
    public virtual void Update(List<Softbody> collisionObjects, double deltaTime)
    {
        
    }

    //sets OpenGl buffer data to flattened vertex array
    public override void Render(Matrix4 view, Matrix4 projection, Vector3 viewPos)
    {
        UpdateVertices();
        GL.BindVertexArray(_VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _flattenedVertices.Length * sizeof(double), _flattenedVertices);
        base.Render(view, projection, viewPos);
        
    }
}
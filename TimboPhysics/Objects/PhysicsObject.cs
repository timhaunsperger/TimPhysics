using System.IO.Enumeration;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsObject : RenderObject
{ 
    // Helper fields for Collision.cs
    public List<int> CollidedObjects; 
    public double Radius;
    
    public Vector3d Center;
    protected bool IsCenterStatic;
    
    public Dictionary<uint, PhysicsVertex> _vertexLookup;
    protected uint[][] _faces;  // Array of arrays storing which vertices are connected to form faces

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

    protected PhysicsObject(Shape shape, Shader shader)
        : base(shape, shader)
    {
        var vertices = shape.Vertices;
        var indices = shape.Indices;
        _vertexLookup = new Dictionary<uint, PhysicsVertex>();
        _faces = new uint[indices.Length/3][];
        
        for (int i = 0; i < indices.Length; i++)
        {
            if (!_vertexLookup.ContainsKey(indices[i]))
            {
                var vertexPos = new Vector3d(vertices[indices[i]][0], vertices[indices[i]][1], vertices[indices[i]][2]);
                _vertexLookup[indices[i]] = new PhysicsVertex(vertexPos, Vector3d.Zero);
                Center += vertexPos;
            }

            if (i%3==2)
            {
                _faces[i/3] = new uint[3];
                _faces[i/3][0] = indices[i-2];
                _faces[i/3][1] = indices[i-1];
                _faces[i/3][2] = indices[i-0];
            }
        }

        Center /= _vertexLookup.Count;
        UpdateValues(_vertexLookup, 0);
    }
    
    //Checks for and responds to collisions with another specific physics object
    protected void Collision(PhysicsObject collisionObject, Dictionary<uint, PhysicsVertex> vertices)
    {
        foreach (var vertex in vertices.Keys)
        {
            if ((collisionObject.Center-vertices[vertex].Position).Length > collisionObject.Radius)
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
                    vertices[vertex].Position);
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
                var forceVertex = vertices[vertex];
                Vector3d forceVector;
                if (collisionObject.GetType() == typeof(Softbody))
                {
                    if ((Center - collisionObject.Center).LengthFast < Radius) // Prevent spheres from merging
                    {
                        Console.WriteLine("b");
                        forceVector = (Center - collisionObject.Center).Normalized(); // Determine collision vector
                    }
                    else
                    {
                        forceVector = (forceVertex.Position - collisionObject.Center).Normalized(); // Determine collision vector
                    }
                }
                else
                {
                    forceVector = TMathUtils.GetNormal(  // Uses normal of face if non-softbody
                        collisionObject._vertexLookup[closestFace[0]].Position,
                        collisionObject._vertexLookup[closestFace[1]].Position,
                        collisionObject._vertexLookup[closestFace[2]].Position);
                }

                for (int i = 0; i < 3; i++)
                {
                    if (collisionObject.GetType() != typeof(Staticbody)) // Staticbodies dont move during collisions
                    {
                        var faceVertex = collisionObject._vertexLookup[closestFace[i]];
                        faceVertex.Position += forceVector * closestFaceDist / 6;
                        if (Vector3d.Dot(faceVertex.Speed, forceVector * -1) < 0)
                        {
                            faceVertex.Speed -= 2 * Vector3d.Dot(faceVertex.Speed, forceVector * -1) * forceVector * -1;
                        }

                        faceVertex.Speed *= 0.98;
                        collisionObject._vertexLookup[closestFace[i]] = faceVertex;
                    }
                }
                // object responsible for full restitution if other is static
                forceVertex.Position += forceVector * closestFaceDist / (collisionObject.GetType() == typeof(Staticbody) ? 1 : 2); 
                // reflects velocity over collision vector 
                if (Vector3d.Dot(forceVertex.Speed, forceVector) < 0)
                {
                    forceVertex.Speed -= 2 * Vector3d.Dot(forceVertex.Speed, forceVector) * forceVector;
                }
                //friction
                forceVertex.Speed *= 0.94;
                vertices[vertex] = forceVertex;
            }
        }
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
            if (Radius < (vertex.Position-Center).Length)
            {
                Radius = (vertex.Position - Center).Length;
            }
                
        }
        Center = vertexPosSum/Vertices.Count;
    }
    
    //Updates vertex array with new positions and normals from physics vertices then flattens for use in OpenGL
    private void UpdateVertices()
    {
        for (uint i = 0; i < _vertexLookup.Count; i++)
        {
            var newPos = _vertexLookup[i].Position;
            // Vertex can not have an actual normal, but center to vertex vector is close enough approximation
            var fakeNormal = (newPos - Center).Normalized(); 
            _vertices[i] = new[] {newPos.X, newPos.Y, newPos.Z, fakeNormal.X, fakeNormal.Y, fakeNormal.Z, _vertices[i][6], _vertices[i][7]};
        }
        _flattenedVertices = _vertices.SelectMany(x => x).ToArray();
    }
    
    //Allows for update method visibility when class extensions are saved to PhysicsObject list
    public virtual void Update(List<PhysicsObject> collisionObjects, double deltaTime)
    {
        UpdateVertices();
    }

    //sets OpenGl buffer data to flattened vertex array
    public override void Render(Matrix4 view, Matrix4 projection, Vector3 viewPos)
    {
        GL.BindVertexArray(_VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _flattenedVertices.Length * sizeof(double), _flattenedVertices);
        base.Render(view, projection, viewPos);
        
    }
}
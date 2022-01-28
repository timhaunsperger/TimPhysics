using System.Numerics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace TimboPhysics;

public class PhysicsObject : RenderObject
{
    private bool _gravity;
    private bool _collision;
    private bool isUpdated;
    private Dictionary<Vector3d, uint> _indexLookup;
    private Dictionary<uint, PhysicsVertex> _vertexLookup;
    private HashSet<Face> _faces = new ();
    private double floor = -15f;
    
    public class PhysicsVertex
    {
        public Vector3d Position;
        public Vector3d Speed;
        public float Mass;
        public HashSet<uint> Connections;

        public PhysicsVertex(Vector3d position, Vector3d speed, float mass, HashSet<uint> connections)
        {
            Position = position;
            Speed = speed;
            Mass = mass;
            Connections = connections;
        }

        public void UpdatePos()
        {
            Position += Speed;
        }

        public void ApplyForce(Vector3d force)
        {
            Speed += force;
        }
    }
    
    private class Face
    {
        public uint[] Vertices;

        public Face(uint v0, uint v1, uint v2)
        {
            Vertices = new uint[3];
            Vertices[0] = v0;
            Vertices[1] = v1;
            Vertices[2] = v2;
        }

        public double GetArea(Dictionary<uint,PhysicsVertex> vertexLookup)
        {
            double a = Vector3d.Distance(vertexLookup[Vertices[0]].Position, vertexLookup[Vertices[1]].Position);
            double b = Vector3d.Distance(vertexLookup[Vertices[1]].Position, vertexLookup[Vertices[2]].Position);
            double c = Vector3d.Distance(vertexLookup[Vertices[2]].Position, vertexLookup[Vertices[0]].Position);
            double s = (a + b + c) / 2;
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        public Vector3d GetNormal(Dictionary<uint, PhysicsVertex> vertexLookup)
        {
            var v0 = vertexLookup[Vertices[1]].Position - vertexLookup[Vertices[0]].Position;
            var v1 = vertexLookup[Vertices[2]].Position - vertexLookup[Vertices[0]].Position;
            return Vector3d.Cross(v0, v1).Normalized(); ;

        }
        
        public double GetVolFromPoint(Dictionary<uint, PhysicsVertex> vertexLookup, Vector3d point)
        {
            var normal = GetNormal(vertexLookup);
            var center = GetCenter(vertexLookup);
            var distance = point - center;
            var height = Vector3d.Dot(normal, distance);
            
            return GetArea(vertexLookup)*height/3f;
        }

        public Vector3d GetCenter(Dictionary<uint,PhysicsVertex> vertexLookup)
        {
            return (vertexLookup[Vertices[0]].Position + vertexLookup[Vertices[1]].Position + vertexLookup[Vertices[2]].Position) / 3;
        }
    }
    
    public PhysicsObject(double[][] vertices, uint[] indices, Dictionary<Vector3d, uint> indexLookup, Shader shader, bool collision, bool gravity) 
        : base(vertices, indices, shader)
    {
        _gravity = gravity;
        _collision = collision;
        _indexLookup = indexLookup;
        _vertexLookup = new Dictionary<uint, PhysicsVertex>();

        for (int i = 0; i < indices.Length; i++)
        {
            if (!_vertexLookup.ContainsKey(indices[i]))
            {
                var vertexPos = new Vector3d(vertices[indices[i]][0], vertices[indices[i]][1], vertices[indices[i]][2]);
                _vertexLookup[indices[i]] = new PhysicsVertex(vertexPos, Vector3d.Zero, 1, new HashSet<uint>());
            }

            var physicsVertex = _vertexLookup[indices[i]];
            physicsVertex.Connections.Add(indices[i - i % 3 + 0]);
            physicsVertex.Connections.Add(indices[i - i % 3 + 1]);
            physicsVertex.Connections.Add(indices[i - i % 3 + 2]);
            physicsVertex.Connections.Remove(indices[i]);

            if (i%3==2)
            {
                _faces.Add(new Face(
                    indices[i-2],
                    indices[i-1],
                    indices[i]
                    ));
            }
        }
    }

    private void Update()
    {
        var centerPos = Vector3d.Zero;
        foreach (var vertexKey in _vertexLookup.Keys)
        {
            centerPos += _vertexLookup[vertexKey].Position;
        }
        centerPos /= _vertexLookup.Count;
        var volume = 0d;
        foreach (var face in _faces)
        {
            volume += face.GetVolFromPoint(_vertexLookup, centerPos);
        }
        foreach (var face in _faces)
        {
            foreach (var faceVertex in face.Vertices)
            {
                var normal = face.GetNormal(_vertexLookup);
                // Console.WriteLine(MathHelper.RadiansToDegrees(Vector3.CalculateAngle(normal, centerPos - face.GetCenter(_vertexLookup))));
                if (MathHelper.RadiansToDegrees(Vector3d.CalculateAngle(normal, centerPos - face.GetCenter(_vertexLookup))) < 90)
                {
                    normal *= -1;
                }
                //_vertexLookup[faceVertex].ApplyForce(normal * face.GetArea(_vertexLookup) / volume * 0.002f);
                
                // var vector = face.GetCenter(_vertexLookup) - _vertexLookup[faceVertex].Position;
                // _vertexLookup[faceVertex].ApplyForce(vector.Normalized() * (vector.Length-0.25f) * 0.25f);
            }
        }
        foreach (uint vertexKey in _vertexLookup.Keys)
        {
            
            var vertex = _vertexLookup[vertexKey];
            // vertex.Speed.Y *= 0.9f;
            // vertex.Speed.X *= 0.9f;
            // vertex.Speed.Z *= 0.9f;
            if (_gravity)
            {
                //vertex.ApplyForce( new Vector3(0f,-0.001f,0f));  // Apply gravity
                
                if (vertex.Position.Y <= floor)  // Floor collision
                {
                    vertex.Position.Y = floor;
                    if (vertex.Speed.Y < 0)
                        vertex.Speed.Y *= -0.2f;
                }
                if (vertex.Position.Y >= -floor)  // ceiling collision
                {
                    vertex.Position.Y = -floor;
                    if (vertex.Speed.Y > 0)
                        vertex.Speed.Y *= -0.2f;
                }
                if (vertex.Position.Z <= floor)  // Floor collision
                {
                    vertex.Position.Z = floor;
                    if (vertex.Speed.Z < 0)
                        vertex.Speed.Z *= -0.02f;
                }
                if (vertex.Position.Z >= -floor)  // ceiling collision
                {
                    vertex.Position.Z = -floor;
                    if (vertex.Speed.Z > 0)
                        vertex.Speed.Z *= -0.02f;
                }
                if (vertex.Position.X <= floor)  // Floor collision
                {
                    vertex.Position.X = floor;
                    if (vertex.Speed.X < 0)
                        vertex.Speed.X *= -0.02f;
                }
                if (vertex.Position.X >= -floor)  // ceiling collision
                {
                    vertex.Position.X = -floor;
                    if (vertex.Speed.X > 0)
                        vertex.Speed.X *= -0.02f;
                }
            }

            foreach (var connection in vertex.Connections)
            {
                var vector = _vertexLookup[connection].Position - vertex.Position;
                vertex.ApplyForce(vector * 0.0001f);
                var relSpeed = vertex.Speed - _vertexLookup[connection].Speed;
            }
            
            _vertexLookup[vertexKey].UpdatePos();
            _vertexLookup[vertexKey] = vertex;
        }
    }

    private void UpdateVertices()
    {
        for (int i = 0; i < _vertices.GetLength(0); i++)
        {
            var oldPos = new Vector3d(_vertices[i][0], _vertices[i][1], _vertices[i][2]);
            var index = _indexLookup[oldPos];
            var newPos = _vertexLookup[index].Position;
            _indexLookup.Remove(oldPos);
            _indexLookup[newPos] = index;
            _vertices[i] = new[] {newPos.X, newPos.Y, newPos.Z, _vertices[i][3], _vertices[i][4]};
        }
        _flattenedVertices = _vertices.SelectMany(x => x).ToArray();
    }

    public override void Render(Matrix4 view, Matrix4 projection)
    {
        Update();
        UpdateVertices();
        GL.BindVertexArray(_VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _flattenedVertices.Length * sizeof(double), _flattenedVertices);
        base.Render(view, projection);
    }
}
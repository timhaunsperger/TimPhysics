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
    private Dictionary<Vector3, uint> _indexLookup;
    private Dictionary<uint, PhysicsVertex> _vertexLookup;
    private float floor = -5f;
    
    public class PhysicsVertex
    {
        public Vector3 Position;
        public Vector3 Speed;
        public float Mass;
        public HashSet<uint> Connections;

        public PhysicsVertex(Vector3 position, Vector3 speed, float mass, HashSet<uint> connections)
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

        public void ApplyForce(Vector3 force)
        {
            Speed += force;
        }
    }
    
    public PhysicsObject(float[][] vertices, uint[] indices, Dictionary<Vector3, uint> indexLookup, Shader shader, bool collision, bool gravity) 
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
                var vertexPos = new Vector3(vertices[indices[i]][0], vertices[indices[i]][1], vertices[indices[i]][2]);
                _vertexLookup[indices[i]] = new PhysicsVertex(vertexPos, Vector3.Zero, 1, new HashSet<uint>());
            }

            var physicsVertex = _vertexLookup[indices[i]];
            physicsVertex.Connections.Add(indices[i - i % 3 + 0]);
            physicsVertex.Connections.Add(indices[i - i % 3 + 1]);
            physicsVertex.Connections.Add(indices[i - i % 3 + 2]);
            physicsVertex.Connections.Remove(indices[i]);
            
            
        }
    }

    private void Update()
    {
        var centerPos = Vector3.Zero;
        foreach (var vertexKey in _vertexLookup.Keys)
        {
            centerPos += _vertexLookup[vertexKey].Position;
        }

        centerPos /= _vertexLookup.Count;
        foreach (uint vertexKey in _vertexLookup.Keys)
        {
            
            var vertex = _vertexLookup[vertexKey];
            vertex.Speed.Y *= 1f;
            vertex.Speed.X *= 0.8f;
            vertex.Speed.Z *= 0.8f;
            if (_gravity)
            {
                vertex.ApplyForce( new Vector3(0f,-0.001f,0f));  // Apply gravity
                
                if (vertex.Position.Y <= floor)  // Floor collision
                {
                    vertex.Position.Y = floor;
                    if (vertex.Speed.Y < 0)
                        vertex.Speed.Y *= -0.02f;
                }
                if (vertex.Position.Y >= -floor)  // ceiling collision
                {
                    vertex.Position.Y = -floor;
                    if (vertex.Speed.Y > 0)
                        vertex.Speed.Y *= -0.5f;
                }
                if (vertex.Position.Z <= floor)  // Floor collision
                {
                    vertex.Position.Z = floor;
                    if (vertex.Speed.Z < 0)
                        vertex.Speed.Z *= -0.5f;
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
            var ctrVector = vertex.Position - centerPos;
            vertex.ApplyForce(ctrVector.Normalized() * (2-ctrVector.Length)*0.2f);
            //vertex.Position = (ctrVector.Normalized() * 2+centerPos);
            
            foreach (var connection in vertex.Connections)
            {
                var vector = _vertexLookup[connection].Position - vertex.Position;
                vertex.ApplyForce(vector.Normalized() * (vector.Length-0.5f) * 0.02f);

            }
            _vertexLookup[vertexKey].UpdatePos();

            _vertexLookup[vertexKey] = vertex;
        }
    }

    private void UpdateVertices()
    {
        for (int i = 0; i < _vertices.GetLength(0); i++)
        {
            var oldPos = new Vector3(_vertices[i][0], _vertices[i][1], _vertices[i][2]);
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
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _flattenedVertices.Length * sizeof(float), _flattenedVertices);
        base.Render(view, projection);
    }
}
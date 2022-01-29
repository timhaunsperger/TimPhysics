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
    private uint[][] _faces;
    private double floor = -15f;

    public class PhysicsVertex
    {
        public Vector3d Position;
        public Vector3d Speed;
        public float Mass;

        public PhysicsVertex(Vector3d position, Vector3d speed, float mass)
        {
            Position = position;
            Speed = speed;
            Mass = mass;
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

    public PhysicsObject(double[][] vertices, uint[] indices, Dictionary<Vector3d, uint> indexLookup, Shader shader, bool collision, bool gravity) 
        : base(vertices, indices, shader)
    {
        _gravity = gravity;
        _collision = collision;
        _indexLookup = indexLookup;
        _vertexLookup = new Dictionary<uint, PhysicsVertex>();
        _faces = new uint[indices.Length/3][];

        for (int i = 0; i < indices.Length; i++)
        {
            if (!_vertexLookup.ContainsKey(indices[i]))
            {
                var vertexPos = new Vector3d(vertices[indices[i]][0], vertices[indices[i]][1], vertices[indices[i]][2]);
                _vertexLookup[indices[i]] = new PhysicsVertex(vertexPos, Vector3d.Zero, 1);
            }

            if (i%3==2)
            {
                _faces[i/3] = new uint[3];
                _faces[i/3][0] = indices[i-2];
                _faces[i/3][1] = indices[i-1];
                _faces[i/3][2] = indices[i-0];
            }
        }
    }

    private void Update()
    {
        // Find center of object by averaging all vertices 
        var centerPos = Vector3d.Zero;
        foreach (var vertexKey in _vertexLookup.Keys)
        {
            centerPos += _vertexLookup[vertexKey].Position/_vertexLookup.Count;
        }

        // Find volume of object by sum of volume of tetrahedrons of faces and centerPos
        var volume = 0d;
        for (int i = 0; i < _faces.Length; i++)
        {
            volume += TMathUtils.GetVolume(
                _vertexLookup[_faces[i][0]].Position,
                _vertexLookup[_faces[i][1]].Position,
                _vertexLookup[_faces[i][2]].Position,
                centerPos);
        }

        const double springConst = 0.5;
        const double springOffset = 0.25;
        const double dampingFactor = 0.1;
        const double pressure = 1;

        foreach (var face in _faces)
        {
            PhysicsVertex[] vertices = {_vertexLookup[face[0]], _vertexLookup[face[1]], _vertexLookup[face[2]]};
            
            // Important values for calculating forces.
            var faceNormal = TMathUtils.GetNormal(vertices[0].Position, vertices[1].Position, vertices[2].Position) * -1;  // Flip vector because it faces inward by default
            var faceArea = TMathUtils.GetArea(vertices[0].Position, vertices[1].Position, vertices[2].Position);

            for (int i = 0; i < face.Length; i++)
            {
                var vertex = vertices[i];
                var nextVertex = vertices[(i + 1) % 3];
                
                //Apply Pressure Force
                vertex.ApplyForce(faceNormal * faceArea / volume * pressure);
                
                //Apply Spring Force
                var springVector = nextVertex.Position - vertex.Position;
                vertex.ApplyForce(springVector * (springVector.Length - springOffset) * springConst);
                nextVertex.ApplyForce(springVector * (springVector.Length - springOffset) * springConst * -1); // ensure net force on object as a whole is 0
                
                //Apply Damping Force
                var relSpeed = vertex.Speed - nextVertex.Speed;
                vertex.ApplyForce(relSpeed * -dampingFactor);
                nextVertex.ApplyForce(relSpeed * -dampingFactor * -1);
                
                //Apply Changes
                _vertexLookup[face[i]] = vertex;
                _vertexLookup[face[(i + 1) % 3]] = nextVertex;
            }
        }
        foreach (uint vertexKey in _vertexLookup.Keys)
        {
            
            var vertex = _vertexLookup[vertexKey];
            if (_gravity)
            {
                vertex.ApplyForce( new Vector3(0f,-0.001f,0f));  // Apply gravity
                
                if (vertex.Position.Y < floor)  // Floor collision
                {
                    vertex.Position.Y = floor + 0.001d;
                    //vertex.ApplyForce(new Vector3d(1,1,0));
                    vertex.Speed *= 0.5;
                }
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
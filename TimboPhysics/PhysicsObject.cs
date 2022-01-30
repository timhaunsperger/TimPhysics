using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsObject : RenderObject
{
    private bool _gravity;
    private bool _collision;
    private Dictionary<Vector3d, uint> _indexLookup;
    private Dictionary<uint, PhysicsVertex> _vertexLookup;
    private uint[][] _faces;  // Array of arrays storing which vertices are connected to form faces
    private double floor = -15f;

    public struct PhysicsVertex
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

    private Dictionary<uint,PhysicsVertex> NextPositions(Dictionary<uint,PhysicsVertex> baseVertices, Dictionary<uint,PhysicsVertex> outVertices, double timeStep)
    {
        // Clone input dictionaries because dict is reference type
        baseVertices = new Dictionary<uint, PhysicsVertex>(baseVertices);
        outVertices = new Dictionary<uint, PhysicsVertex>(outVertices);
        
        // Find center of object by averaging all vertices 
        var centerPos = Vector3d.Zero;
        foreach (var vertexKey in baseVertices.Keys)
        {
            centerPos += baseVertices[vertexKey].Position/baseVertices.Count;
        }

        // Find volume of object by sum of volume of tetrahedrons of faces and centerPos
        var volume = 0d;
        for (int i = 0; i < _faces.Length; i++)
        {
            volume += TMathUtils.GetVolume(
                baseVertices[_faces[i][0]].Position,
                baseVertices[_faces[i][1]].Position,
                baseVertices[_faces[i][2]].Position,
                centerPos);
        }

        const double springConst = 500;
        const double springOffset = 1;
        const double dampingFactor = 20;
        const double pressure = 4000;
        const double gravity = 0.5;
        const double collisionForce = 50;

        foreach (var face in _faces)
        {
            PhysicsVertex[] faceBaseVertices = {baseVertices[face[0]], baseVertices[face[1]], baseVertices[face[2]]};
            PhysicsVertex[] faceOutVertices = {outVertices[face[0]], outVertices[face[1]], outVertices[face[2]]};
            
            // Important values for calculating forces.
            var faceNormal = TMathUtils.GetNormal(
                faceBaseVertices[0].Position, 
                faceBaseVertices[1].Position, 
                faceBaseVertices[2].Position);

            var faceArea = TMathUtils.GetArea(
                faceBaseVertices[0].Position, 
                faceBaseVertices[1].Position, 
                faceBaseVertices[2].Position);

            for (uint i = 0; i < face.Length; i++)
            {
                //Apply Pressure Force
                faceOutVertices[i].Speed += faceNormal * faceArea / volume * pressure * timeStep;
                //Apply Spring Force
                var springVector = faceBaseVertices[(i + 1) % 3].Position - faceBaseVertices[i].Position;
                faceOutVertices[i].Speed += springVector.Normalized() * (springVector.Length - springOffset) * springConst * timeStep;
                faceOutVertices[(i + 1) % 3].Speed += springVector.Normalized() * (springVector.Length - springOffset) * springConst * timeStep * -1;
                
                //Apply Damping Force
                var relSpeed = faceBaseVertices[i].Speed - faceBaseVertices[(i + 1) % 3].Speed;
                faceOutVertices[i].Speed += (relSpeed * -dampingFactor * timeStep);
                faceOutVertices[(i + 1) % 3].Speed += (relSpeed * -dampingFactor * timeStep * -1);
                
                //Apply Gravity
                if (_gravity)
                {
                    faceOutVertices[i].Speed -= Vector3d.UnitY * gravity * timeStep;
    
                    if (faceBaseVertices[i].Position.Y < floor)  // Floor collision
                    {
                        faceOutVertices[i].Speed += Vector3d.UnitY * -(faceBaseVertices[i].Position.Y - floor)* timeStep * collisionForce; // Floor friction
                    }
                }
            }
            //Apply Changes
            outVertices[face[0]] = faceOutVertices[0];
            outVertices[face[1]] = faceOutVertices[1];
            outVertices[face[2]] = faceOutVertices[2];
        }
        
        for (uint i = 0; i < baseVertices.Count; i++)
        {
            var vertex = outVertices[i];
            vertex.Position += vertex.Speed * timeStep;
            outVertices[i] = vertex;
        }
        return outVertices;
    }

    public void UpdateVertices()
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

    public Dictionary<uint, PhysicsVertex> RK4Integrate(Dictionary<uint, PhysicsVertex> vertices, double timeStep)
    {
        var k1 = NextPositions(vertices, vertices, timeStep);
        var k1mid = NextPositions(vertices, vertices, timeStep/2);
        var k2 = NextPositions(k1mid, vertices, timeStep);
        var k2mid = NextPositions(k1mid, vertices, timeStep/2);
        var k3 = NextPositions(k2mid, vertices, timeStep);
        var k4 = NextPositions(k3, vertices, timeStep);

        var result = new Dictionary<uint, PhysicsVertex>();

        foreach (var key in vertices.Keys)
        {
            var entry = vertices[key];
            entry.Position = 
                (k1[key].Position + 2 * k2[key].Position + 2 * k3[key].Position + k4[key].Position)/6;
            entry.Speed = 
                (k1[key].Speed + 2 * k2[key].Speed + 2 * k3[key].Speed + k4[key].Speed)/6;
            result[key] = entry;
        }

        return result;
    }
    

    public override void Update(double deltaTime)
    {
        _vertexLookup = RK4Integrate(_vertexLookup, 0.005);
        var isColliding = true;
        foreach (var face in _faces)
        {
            if (!TMathUtils.IsPointBehindPlane(_vertexLookup[face[0]].Position,
                    _vertexLookup[face[1]].Position,
                    _vertexLookup[face[2]].Position,
                     new Vector3d(0,0,0)))
            {
                isColliding = false;
                break;
            };
        }
        Console.WriteLine(isColliding);
        base.Update(deltaTime);
    }

    public override void Render(Matrix4 view, Matrix4 projection)
    {
        UpdateVertices();
        GL.BindVertexArray(_VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _flattenedVertices.Length * sizeof(double), _flattenedVertices);
        base.Render(view, projection);
    }
}
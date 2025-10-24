using OpenTK.Mathematics;

namespace TimboPhysics;

public class SoftBody : PhysicsObject
{
    private bool _gravity;
    private bool _isBaloon;
    public Dictionary<uint, PhysicsVertex> _vertexLookup = new();

    public SoftBody(Shape shape, Shader shader, Vector3d velocity, float mass, bool gravity, bool isBaloon)
        : base(shape, shader, mass)
    {
        _gravity = gravity;
        _isBaloon = true;
        Position = shape.Center;
        var indices = shape.Indices;
        for (int i = 0; i < indices.Length; i++)
        {
            var vertexPos = new Vector3d(shape.Vertices[indices[i]][0], shape.Vertices[indices[i]][1],
                shape.Vertices[indices[i]][2]);
            _vertexLookup[indices[i]] =
                new PhysicsVertex(vertexPos, velocity, vertexPos - Position);
        }
    }

    public struct PhysicsVertex(Vector3d position, Vector3d speed, Vector3d initialOffset)
    {
        public Vector3d Position = position;
        public Vector3d InitialOffset = initialOffset;
        public Vector3d Speed = speed;
    }

    private Dictionary<uint, PhysicsVertex> NextPositions(Dictionary<uint, PhysicsVertex> Vertices, double timeStep)
    {
        // Clone input dictionary because dict is reference type
        Vertices = new Dictionary<uint, PhysicsVertex>(Vertices);

        const double springConst = 4000;
        const double springOffset = 0.35;
        const double dampingFactor = 2;
        const double pressure = 4000;
        const double gravity = 6;

        for (uint i = 0; i < Vertices.Count; i++)
        {
            var vertex1 = Vertices[i];
            // Apply Gravity
            if (_gravity)
            {
                vertex1.Speed -= Vector3d.UnitY * gravity * timeStep;
            }

            Vertices[i] = vertex1;

            if (!_isBaloon)
            {
                for (uint j = i + 1; j < Vertices.Count; j ++)
                {
                    var vertex2 = Vertices[j];
                    
                    // Apply Damping Force
                    var relSpeed = vertex1.Speed - vertex2.Speed;
                    vertex1.Speed -= relSpeed * dampingFactor * timeStep;
                    vertex2.Speed += relSpeed * dampingFactor * timeStep;
                    
                    //Apply Spring Force
                    var springVector = vertex1.Position - vertex2.Position;
                    var initDist = (vertex2.InitialOffset - vertex1.InitialOffset).Length;
                    var springForce = springVector.Normalized() * (springVector.Length - initDist) * springConst/10 * timeStep;
                    vertex1.Speed -= springForce;
                    vertex2.Speed += springForce;
                    
                    Vertices[j] = vertex2;
                }
            }
            Vertices[i] = vertex1;

        }
        
        if (_isBaloon)
        {
            // Find volume of object by sum of volume of tetrahedrons of faces and centerPos
            var volume = 0d;
            for (int i = 0; i < Faces.Length; i++)
            {
                volume += TMathUtils.GetVolume(
                    Vertices[Faces[i][0]].Position,
                    Vertices[Faces[i][1]].Position,
                    Vertices[Faces[i][2]].Position,
                    Position);
            }

            foreach (var face in Faces)
            {
                PhysicsVertex[] faceVertices = { Vertices[face[0]], Vertices[face[1]], Vertices[face[2]] };

                // Important values for calculating forces.
                var faceNormal = TMathUtils.GetNormal(
                    faceVertices[0].Position,
                    faceVertices[1].Position,
                    faceVertices[2].Position);

                var faceArea = TMathUtils.GetArea(faceVertices[0].Position, faceVertices[1].Position,
                    faceVertices[2].Position);

                for (int i = 0; i < face.Length; i++)
                {
                    //Apply Pressure Force
                    faceVertices[i].Speed += faceNormal * faceArea / volume * pressure * timeStep;
                    //Apply Spring Force
                    var springVector = faceVertices[(i + 1) % 3].Position - faceVertices[i].Position;
                    faceVertices[i].Speed += springVector.Normalized() * (springVector.Length - springOffset) *
                                             springConst * timeStep;
                    faceVertices[(i + 1) % 3].Speed +=
                        springVector.Normalized() * (springVector.Length - springOffset) *
                        springConst * timeStep * -1;

                    //Apply Damping Force
                    var relSpeed = faceVertices[i].Speed - faceVertices[(i + 1) % 3].Speed;
                    faceVertices[i].Speed += relSpeed * -dampingFactor * timeStep;
                    faceVertices[(i + 1) % 3].Speed += relSpeed * -dampingFactor * timeStep * -1;
                }

                //Apply Changes
                Vertices[face[0]] = faceVertices[0];
                Vertices[face[1]] = faceVertices[1];
                Vertices[face[2]] = faceVertices[2];
            }
        }

        return Vertices;
    }

    private void UpdateValues(double deltaTime)
    {
        var vertexPosSum = Vector3d.Zero;
        Radius = 0d;
        for (uint i = 0; i < _vertexLookup.Count; i++)
        {
            var vertex = _vertexLookup[i];
            vertex.Position += vertex.Speed * deltaTime;
            _vertexLookup[i] = vertex;
                
            vertexPosSum += vertex.Position;
            if (Radius < (vertex.Position - Position).Length)
            {
                Radius = (vertex.Position - Position).Length;
            }
                
        }
        Position = vertexPosSum/_vertexLookup.Count;
    }
    public override void Update(double deltaTime)
    {
        _vertexLookup = NextPositions(_vertexLookup, deltaTime);
        UpdateValues(deltaTime);
        for (uint i = 0; i < _vertexLookup.Count; i++)
        {
            var newPos = _vertexLookup[i].Position;
            // Vertex can not have an actual normal, but center to vertex vector is close enough approximation
            var fakeNormal = (newPos - Position).Normalized(); 
            Vertices[i] = new[] {newPos.X, newPos.Y, newPos.Z, fakeNormal.X, fakeNormal.Y, fakeNormal.Z, Vertices[i][6], Vertices[i][7]};
        }
        _flattenedVertices = Vertices.SelectMany(x => x).ToArray();
    }
}
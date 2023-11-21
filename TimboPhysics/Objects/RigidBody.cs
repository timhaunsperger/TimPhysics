using OpenTK.Mathematics;

namespace TimboPhysics;

public class RigidBody : PhysicsObject
{
    private bool _gravity;
    private Vector3d[] _vertexOffsets;
    public double AngVelocity = 2;
    public double Inertia = 1;
    public Vector3d rotAxis = Vector3d.UnitZ;
    
    public RigidBody(Shape shape, Shader shader, Vector3d velocity, double mass, bool gravity)
        : base(shape, shader, mass)
    {
        Position = shape.Center;
        Radius = shape.Radius;
        Velocity = velocity;
        
        _gravity = gravity;
        
        _vertexOffsets = new Vector3d[Vertices.Length];
        for (int i = 0; i < Vertices.Length; i++)
        {
            _vertexOffsets[i] = new Vector3d (Vertices[i][0], Vertices[i][1], Vertices[i][2] ) - shape.Center;

        }
    }

    public override void Update(double deltaTime)
    {
        Position += Velocity * deltaTime;
        var rot = Quaterniond.FromAxisAngle(rotAxis, AngVelocity * deltaTime);
        for (int i = 0; i < _vertexOffsets.Length; i++)
        {
            _vertexOffsets[i] = rot * _vertexOffsets[i];
            var vertexPos = Position + _vertexOffsets[i];
            Vertices[i][0] = vertexPos.X;
            Vertices[i][1] = vertexPos.Y;
            Vertices[i][2] = vertexPos.Z;
        }

        _flattenedVertices = TMathUtils.Flatten(Vertices);
        base.Update(deltaTime);
    }
}
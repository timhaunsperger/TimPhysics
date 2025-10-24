using OpenTK.Mathematics;

namespace TimboPhysics;

public class RigidBody : PhysicsObject
{
    private bool _gravity;
    private Vector3d[] _vertexOffsets;
    public double AngVelocity = 0;
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

    // public override Vector3d[] GetVertices()
    // {
    //     
    // }

    public override void Update(double deltaTime)
    {
        Position += Velocity * deltaTime;
        if (_gravity)
        {
            Velocity -= Vector3d.UnitY;
        }
        var rot = Quaterniond.FromAxisAngle(rotAxis, AngVelocity * deltaTime);
        for (int i = 0; i < _vertexOffsets.Length; i++)
        {
            _vertexOffsets[i] = rot * _vertexOffsets[i];
            var vertexPos = Position + _vertexOffsets[i];
            var fakeNormal = (vertexPos - Position).Normalized(); 
            Vertices[i] = new[] {vertexPos.X, vertexPos.Y, vertexPos.Z, fakeNormal.X, fakeNormal.Y, fakeNormal.Z, Vertices[i][6], Vertices[i][7]};
        }
        _flattenedVertices = TMathUtils.Flatten(Vertices);
        base.Update(deltaTime);
    }
}
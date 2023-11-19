using OpenTK.Mathematics;

namespace TimboPhysics;

public class RigidBody : PhysicsObject
{
    private bool _gravity;
    private Vector3d[] _vertexOffsets;
    
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
            _flattenedVertices[8*i+3] = _vertexOffsets[i].X;
            _flattenedVertices[8*i+4] = _vertexOffsets[i].Y;
            _flattenedVertices[8*i+5] = _vertexOffsets[i].Z;
        }
    }
}
using OpenTK.Mathematics;

namespace TimboPhysics;

public class RigidBody : PhysicsObject
{
    public Vector3d Position;
    public Vector3d Speed;
    public double Mass;
    private Vector3d[] _vertexOffsets;
    private bool _gravity;
    
    public RigidBody(Vector3d position, double size, int recursion, Shader shader, Vector3d speed, bool gravity) 
        : base(SphereCache.GetSphere(recursion, position, size), shader, size)
    {
        Position = position;
        Radius = size;
        Mass = Math.Pow(size,3);
        Speed = speed;
        _gravity = gravity;
        
        _vertexOffsets = new Vector3d[Vertices.Length];
        for (int i = 0; i < Vertices.Length; i++)
        {
            _vertexOffsets[i] = new Vector3d (Vertices[i][0], Vertices[i][1], Vertices[i][2] ) - position;
            _flattenedVertices[8*i+3] = _vertexOffsets[i].X;
            _flattenedVertices[8*i+4] = _vertexOffsets[i].Y;
            _flattenedVertices[8*i+5] = _vertexOffsets[i].Z;
        }
    }
}
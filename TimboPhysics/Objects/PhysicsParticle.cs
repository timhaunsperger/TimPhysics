using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsParticle : PhysicsObject
{
    public Vector3d[] _vertexOffsets;
    private bool _gravity;
    

    public PhysicsParticle(Vector3d position, double size, int recursion, Shader shader, Vector3d speed, bool gravity) 
        : base(SphereCache.GetSphere(recursion, position, size), shader, Math.Pow(size,3))
    {
        
        Radius = size;
        Velocity = speed;
        _gravity = gravity;
        
        _vertexOffsets = new Vector3d[Vertices.Length];
        for (int i = 0; i < Vertices.Length; i++)
        {
            _vertexOffsets[i] = new Vector3d (Vertices[i][0], Vertices[i][1], Vertices[i][2] ) - position;
        }
    }

    private void NextPosition(double deltaTime)
    {
        if (_gravity)
        {
            var gravity = new Vector3d(0, -9.8*10, 0);
            Velocity += gravity * deltaTime;
        }
        
        Position += Velocity * deltaTime;
    }
    
    public void Assign(double[] position)
    {
        for (uint i = 0; i < Vertices.Length; i++)
        {
            _flattenedVertices[8 * i] = position[0] + _vertexOffsets[i].X;
            _flattenedVertices[8 * i + 1] = position[1] + _vertexOffsets[i].Y;
            _flattenedVertices[8 * i + 2] = position[2] + _vertexOffsets[i].Z;
        }
    }

    
    public override void Update(double deltaTime)
    {
        NextPosition(deltaTime);

        // Updates values in vertex array
        for (uint i = 0; i < Vertices.Length; i++)
        {
            _flattenedVertices[8 * i] = Position.X + _vertexOffsets[i].X;
            _flattenedVertices[8 * i + 1] = Position.Y + _vertexOffsets[i].Y;
            _flattenedVertices[8 * i + 2] = Position.Z + _vertexOffsets[i].Z;
        }
    }
}
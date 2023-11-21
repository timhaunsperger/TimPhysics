using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsParticle : PhysicsObject
{
    private Vector3d[] _vertexOffsets;
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
            // contain particles
            var boxCenter = Vector3d.UnitY * 5 - Vector3d.UnitX * 20;
            var ctrDist = Position - boxCenter;
            var boxDims = new Vector3d( 5, 5, 5 );
            var OOBchecks = new[] { Math.Abs(ctrDist.X) > boxDims.X, Math.Abs(ctrDist.Y) > boxDims.Y, Math.Abs(ctrDist.Z) > boxDims.Z,};
            
            
            Velocity.X -= 2 * Velocity.X * Convert.ToInt16(OOBchecks[0]);
            Velocity.Y -= 2 * Velocity.Y * Convert.ToInt16(OOBchecks[1]);
            Velocity.Z -= 2 * Velocity.Z * Convert.ToInt16(OOBchecks[2]);
            
            var gravity = new Vector3d(0, -0.1, 0);
            Velocity += gravity;
            
        }

        if (Velocity.Y < 0 && Position.Y - Radius < -15)
        {
            Velocity.Y -= 2 * Velocity.Y;
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
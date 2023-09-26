using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsParticle : PhysicsObject
{
    public Vector3d Position;
    public Vector3d Speed;
    private Vector3d[] _vertexOffsets;

    public PhysicsParticle(int seed, Vector3d position, double size, Shader shader) 
        : base(SphereCache.GetSphere(1, position, size), shader)
    {
        var rand = new Random();
        Position = position;
        Radius = size;
        Speed = new Vector3d(rand.NextDouble() * seed * 4, rand.NextDouble() * seed * 4, rand.NextDouble() * seed * 4);
        IsCenterStatic = true;
        
        _vertexOffsets = new Vector3d[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertexOffsets[i] = new Vector3d (_vertices[i][0], _vertices[i][1], _vertices[i][2] ) - position;
        }
    }

    private void NextPositions(double deltaTime)
    {
        //attraction
        var attrCenter = Position - Vector3d.UnitY - (Vector3d.UnitX * 20);
        
        var gravity = new Vector3d(0, -0.5, 0);
        Speed += gravity;
        Speed *= Math.Pow(0.99, Speed.Length);
        if (attrCenter.Length > 8 && Vector3d.Dot(attrCenter, Speed) > 0)
        {
            Speed -= 2 * Vector3d.Dot(Speed, -attrCenter.Normalized()) * -attrCenter.Normalized();
        }
        Position += Speed * deltaTime;
    }

    
    //Allows for update method visibility when class extensions are saved to PhysicsObject list
    public virtual void Update(List<PhysicsParticle> collisionObjects, double deltaTime)
    {
        NextPositions(deltaTime);
        // updates position
        Center = Position;

        // Updates values in vertex array
        for (uint i = 0; i < _vertices.Length; i++)
        {
            _vertices[i] = new[] {
                Position.X + _vertexOffsets[i].X,
                Position.Y + _vertexOffsets[i].Y, 
                Position.Z + _vertexOffsets[i].Z, 
                _vertexOffsets[i].X, _vertexOffsets[i].Y, _vertexOffsets[i].Z, _vertices[i][6], _vertices[i][7]};  // Lighting normal of vertex is same as offset
        }

        for (int i = 0; i < _vertices.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                _flattenedVertices[i * 8 + j] = _vertices[i][j];
            }
        }
    }

    //sets OpenGl buffer data to flattened vertex array
    public override void Render(Matrix4 view, Matrix4 projection, Vector3 viewPos)
    {
        GL.BindVertexArray(_VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _flattenedVertices.Length * sizeof(double), _flattenedVertices);
        base.Render(view, projection, viewPos);
        
    }
}
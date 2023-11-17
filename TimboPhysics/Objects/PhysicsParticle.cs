using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsParticle : PhysicsObject
{
    public Vector3d Position;
    public Vector3d Speed;
    public double Mass;
    private Vector3d[] _vertexOffsets;
    private bool _gravity;
    

    public PhysicsParticle(Vector3d position, double size, Shader shader, Vector3d speed, bool gravity) 
        : base(SphereCache.GetSphere(2, position, size), shader, size)
    {
        var rand = new Random();
        Position = position;
        Radius = size;
        Mass = size;
        Speed = speed;
        _gravity = gravity;
        
        _vertexOffsets = new Vector3d[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertexOffsets[i] = new Vector3d (_vertices[i][0], _vertices[i][1], _vertices[i][2] ) - position;
        }
    }

    private void NextPositions(double deltaTime)
    {
        //attraction
        // var attrCenter = Position - Vector3d.UnitY - (Vector3d.UnitX * 20);
        //
        if (_gravity)
        {
            var gravity = new Vector3d(0, -0.2, 0);
            Speed += gravity;
        }
        
        // if (attrCenter.Length > 2 && Vector3d.Dot(attrCenter, Speed) > 0)
        // {
        //     Speed += 2 * Vector3d.Dot(Speed, -attrCenter.Normalized()) * attrCenter.Normalized();
        // }
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
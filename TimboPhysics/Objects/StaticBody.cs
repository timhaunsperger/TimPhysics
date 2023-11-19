using OpenTK.Mathematics;
namespace TimboPhysics;

public class StaticBody : PhysicsObject
{
    public Vector3d[] VertexPos;
    public StaticBody(Shape shape, Shader shader) 
        : base(shape, shader, 0)
    {
        Position = shape.Center;
        VertexPos = new Vector3d[Vertices.Length];
        Radius = 0d;
        for (int i = 0; i < Vertices.Length; i++)
        {
            VertexPos[i] = new Vector3d(Vertices[i][0], Vertices[i][1], Vertices[i][2]);
            if (Radius < (VertexPos[i] - Position).Length)
            {
                Radius = (VertexPos[i] - Position).Length;
            }
        }
        
    }
}
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
            _flattenedVertices[8*i+3] = Vertices[i][0] - Position.X;
            _flattenedVertices[8*i+4] = Vertices[i][1] - Position.Y;
            _flattenedVertices[8*i+5] = Vertices[i][2] - Position.Z;
        }
    }

    public override Vector3d[] GetVertices()
    {
        return VertexPos;
    }
}
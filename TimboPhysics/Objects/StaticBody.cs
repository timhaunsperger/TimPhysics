using OpenTK.Mathematics;
namespace TimboPhysics;

public class StaticBody : PhysicsObject
{
    public StaticBody(Shape shape, Shader shader) 
        : base(shape, shader, 0)
    {

    }
}
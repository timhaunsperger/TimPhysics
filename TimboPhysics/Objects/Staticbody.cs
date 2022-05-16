using OpenTK.Mathematics;
namespace TimboPhysics;

public class Staticbody : PhysicsObject
{
    public Staticbody(Shape shape, Shader shader) 
        : base(shape, shader)
    {
        IsCenterStatic = true;
    }
}
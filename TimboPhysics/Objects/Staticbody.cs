using OpenTK.Mathematics;
namespace TimboPhysics;

public class Staticbody : PhysicsObject
{
    public Staticbody(double[][] vertices, uint[] indices, Shader shader, bool collision) 
        : base(vertices, indices, shader, collision)
    {

    }
}
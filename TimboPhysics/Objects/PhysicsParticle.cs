using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsParticle : RenderObject
{
    public PhysicsParticle(Vector3d position, Shader shader) 
        : base(SphereCache.GetSphere(1, position).Vertices, SphereCache.GetSphere(1, position).Indices, shader)
    {
        
    }
}
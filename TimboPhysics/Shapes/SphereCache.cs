using OpenTK.Mathematics;

namespace TimboPhysics;

public static class SphereCache   
{
    private static Icosphere[] _sphereCache = new Icosphere[10];

    public static Icosphere GetSphere(int recursion, Vector3d position)
    {
        // Generates sphere if not in cache
        _sphereCache[recursion] = _sphereCache[recursion] == null ? new Icosphere(recursion, Vector3d.Zero) : _sphereCache[recursion];  
        
         var sphere = new Icosphere(_sphereCache[recursion]); 
        
        for (int i = 0; i < sphere.Vertices.Length; i++)  // Applies position offset before returning
        {
            sphere.Vertices[i][0] += position.X;
            sphere.Vertices[i][1] += position.Y;
            sphere.Vertices[i][2] += position.Z;
        }

        return sphere;
    }
}
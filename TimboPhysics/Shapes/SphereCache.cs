using System.Reflection.Emit;
using OpenTK.Mathematics;

namespace TimboPhysics;

public static class SphereCache   
{
    private static Icosphere[] _sphereCache = new Icosphere[10];

    public static Icosphere GetSphere(int recursion, Vector3d position, double size)
    {
        Icosphere sphere;
        // Generates sphere if not in cache
        if (_sphereCache[recursion] == null)
        {
            sphere = new Icosphere(recursion);
            _sphereCache[recursion] = sphere;
        }
        sphere = new Icosphere(_sphereCache[recursion]);

        for (int i = 0; i < sphere.Vertices.Length; i++)  // Applies position offset before returning
        {
            sphere.Vertices[i][0] = sphere.Vertices[i][0] * size + position.X;
            sphere.Vertices[i][1] = sphere.Vertices[i][1] * size + position.Y;
            sphere.Vertices[i][2] = sphere.Vertices[i][2] * size + position.Z;
        }
        return sphere;
    }
}
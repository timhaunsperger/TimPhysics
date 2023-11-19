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

        sphere.Center = position;
        sphere.Radius = size;
        for (int i = 0; i < sphere.Vertices.Length; i++)  // Applies position offset before returning
        {
            var vertex = sphere.Vertices[i];
            vertex[0] = vertex[0] * size + position.X;
            vertex[1] = vertex[1] * size + position.Y;
            vertex[2] = vertex[2] * size + position.Z;
        }
        return sphere;
    }
}
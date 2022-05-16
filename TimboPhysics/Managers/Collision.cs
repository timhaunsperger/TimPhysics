using System.Linq.Expressions;
using OpenTK.Mathematics;

namespace TimboPhysics;

public static class Collision
{
    public static void ResolveCollision(List<PhysicsParticle> objects)
    {
        var completedPairs = new bool[objects.Count][];
        for (int i = 0; i < objects.Count; i++)
        {
            var obj1 = objects[i];
            for (int j = 0; j < objects.Count; j++)
            {
                var obj2 = objects[j];
                var distance = (obj1.Position - obj2.Position).Length - (obj1.Radius + obj1.Radius);
                
                if (i==j || distance >= 0) // Prevents unnecessary collision tests
                { 
                    continue;
                }

                completedPairs[i] ??= new bool[objects.Count];
                completedPairs[j] ??= new bool[objects.Count];
                if (completedPairs[i][j]) // Prevents duplicate collisions
                {
                    continue;
                }
                completedPairs[j][i] = true;
                
                var collisionVector = (obj1.Position - obj2.Position).Normalized();
                //Collision position restitution
                obj1.Position -= collisionVector * distance / 2;
                obj2.Position += collisionVector * distance / 2;
                
                //Elastic collision speed restitution
                var dotP = Vector3d.Dot(obj1.Speed.Normalized(), obj2.Speed.Normalized());
                if (dotP < 0)
                {
                    continue; 
                }
                var relSpeed = dotP * (obj1.Speed - obj2.Speed).Length;
                obj1.Speed += relSpeed * collisionVector;
                obj2.Speed -= relSpeed * collisionVector;
            }
        }
    }
}
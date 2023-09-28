using System.Linq.Expressions;
using OpenTK.Mathematics;

namespace TimboPhysics;

public static class Collision
{
    public static void ResolveParticleCollision(List<PhysicsParticle> objects)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            var obj1 = objects[i];
            for (int j = i+1; j < objects.Count; j++)
            {
                var obj2 = objects[j];
                var distance = (obj1.Position - obj2.Position).Length - (obj1.Radius + obj1.Radius);
                
                if (distance >= 0) // Prevents unnecessary collision tests
                { 
                    continue;
                }
                
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
    public static void SoftbodyCollisionResolver(PhysicsObject collider1, PhysicsObject collider2)
    {
        // collider1 cannot be static
        if (collider1.GetType() == typeof(Staticbody)) 
        {
            return;
        }

        var dist = (collider1.Center - collider2.Center).LengthFast;

        // Resolve collision on each vertex of object 1
        foreach (var vertex in collider1._vertexLookup)
        {
            var collidingVertex = collider1._vertexLookup[vertex.Key]; 
            
            // Bypass checks if vertex too far from object to collide or object is not soft
            if ((collider2.Center - vertex.Value.Position).LengthFast > collider2.Radius)
            {
                continue;
            }
            
            // If vertex is behind all faces of other object it must be colliding
            var isColliding = true;
            var closestFaceDist = Double.PositiveInfinity;
            var closestFace = Array.Empty<uint>();
            
            foreach (var face in collider2.Faces)
            {
                var distance = TMathUtils.PointPlaneDist(
                    collider2._vertexLookup[face[0]].Position,
                    collider2._vertexLookup[face[1]].Position,
                    collider2._vertexLookup[face[2]].Position,
                    vertex.Value.Position);
                if (distance < 0)
                {
                    isColliding = false;
                    break;
                }
                // Finds closest face in other object to the vertex
                if (distance < closestFaceDist)
                {
                    closestFaceDist = distance;
                    closestFace = face;
                }
            }
            
            // Moves vertex and closest face apart, then reflects their velocities
            if (isColliding)
            {
                Vector3d collisionVector;
                
                // non-static object collision
                if (collider2.GetType() != typeof(Staticbody))
                {
                    if (dist < collider1.Radius)
                    {
                        collisionVector = (collider1.Center - collider2.Center).Normalized(); // prevent overlapping spheres
                    }
                    else
                    {
                        collisionVector = TMathUtils.GetNormal(  // Uses normal of face
                            collider2._vertexLookup[closestFace[0]].Position,
                            collider2._vertexLookup[closestFace[1]].Position,
                            collider2._vertexLookup[closestFace[2]].Position);
                    }
                    // get relative velocity
                    var faceVelocity = Vector3d.Zero;
                    for (int i = 0; i < 3; i++)
                    {
                        faceVelocity += collider2._vertexLookup[closestFace[i]].Speed / 3;
                    }
                    var relVelocity = collidingVertex.Speed - faceVelocity;
                    
                    // adds rel velocity and resolves pos
                    if (Vector3d.Dot(relVelocity,collisionVector) < 0)
                    {
                        collidingVertex.Speed -= relVelocity / 2;
                        collidingVertex.Position += collisionVector * closestFaceDist / 4;
                        collidingVertex.Speed *= 0.94; // friction
                    
                        // apply collision to collided face
                        for (int i = 0; i < 3; i++)
                        {
                            var faceVertex = collider2._vertexLookup[closestFace[i]];
                            faceVertex.Position += collisionVector * closestFaceDist / 12;
                            faceVertex.Speed += relVelocity / 6;

                            faceVertex.Speed *= 0.98; // friction
                            collider2._vertexLookup[closestFace[i]] = faceVertex;
                        }
                    }

                }
                // static object collision
                else
                {
                    collisionVector = TMathUtils.GetNormal(  // Uses normal of face
                        collider2._vertexLookup[closestFace[0]].Position,
                        collider2._vertexLookup[closestFace[1]].Position,
                        collider2._vertexLookup[closestFace[2]].Position);
                    
                    // object responsible for full restitution if other is static
                    collidingVertex.Position += collisionVector * closestFaceDist; 
                    
                    // reflects velocity over collision vector 
                    if (Vector3d.Dot(collidingVertex.Speed, collisionVector) < 0)
                    {
                        collidingVertex.Speed -= 2 * Vector3d.Dot(collidingVertex.Speed, collisionVector) * collisionVector;
                    }
                }
                
                // save changes to output
                collider1._vertexLookup[vertex.Key] = collidingVertex;
            }
        }
    }
    
    public static void ResolveSoftbodyCollision(List<PhysicsObject> collisionObjects)
    {
        // Loop through all objects, looking for colliding pairs
        var collisionPairs = new List<PhysicsObject[]>();
        for (int i = 0; i < collisionObjects.Count; i++)
        {
            // Loop through all potential pairings after an that object in the list, preventing duplicates
            for (int j = i+1; j < collisionObjects.Count; j++)
            {
                var objDist = (collisionObjects[i].Center - collisionObjects[j].Center).LengthFast;
                var sumRadius = collisionObjects[i].Radius + collisionObjects[j].Radius;
                if (objDist < sumRadius)
                {
                    collisionPairs.Add(new []{collisionObjects[i], collisionObjects[j]});
                }
            }
        }
        foreach (var collisionPair in collisionPairs)
        {
            SoftbodyCollisionResolver(collisionPair[0], collisionPair[1]);
            SoftbodyCollisionResolver(collisionPair[1], collisionPair[0]);
        }
    }
}
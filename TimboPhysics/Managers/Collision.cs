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
                
                var distVec = obj1.Position - obj2.Position;
                var dist = distVec.Length;
                var sep = dist - (obj1.Radius + obj2.Radius);

                if (sep > 0) { continue; } // Check if colliding
                
                var collisionVector = (obj1.Position - obj2.Position) / dist;
                var massSum = obj1.Mass + obj2.Mass;
                obj1.Position -= collisionVector * sep * obj2.Mass / massSum;
                obj2.Position += collisionVector * sep * obj1.Mass / massSum;
                
                if(obj1.Speed == Vector3d.Zero && obj2.Speed == Vector3d.Zero){ continue; }
                // transform velocities so one object stationary
                var vTransform = obj2.Speed; 
                obj1.Speed -= vTransform;
                obj2.Speed = Vector3d.Zero;
                
                //Elastic collision speed restitution
                var colAxisVel = Vector3d.Dot(obj1.Speed, collisionVector) * collisionVector; // find velocity along axis of collision
                var perpendicularVel = obj1.Speed - colAxisVel; // find residual of projection of velocity onto collision axis
                if (Vector3d.Dot(obj1.Speed, colAxisVel) > 0)
                {
                    // Solve velocities as linear elastic collision, then add transforms 
                    obj1.Speed = (obj1.Mass - obj2.Mass) / massSum * colAxisVel + perpendicularVel;
                    obj2.Speed = 2 * obj1.Mass / massSum * colAxisVel;
                }
                // reverse transform
                obj1.Speed += vTransform;
                obj2.Speed += vTransform;
            }
        }
    }
    public static void SoftBodyCollisionResolver(PhysicsObject collider1, PhysicsObject collider2)
    {
        // collider1 cannot be static
        if (collider1.GetType() == typeof(StaticBody)) 
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
                if (collider2.GetType() != typeof(StaticBody))
                {
                    if (dist < collider1.Radius)
                    {
                        collisionVector = (collider1.Center - collider2.Center).Normalized(); // prevent merging spheres
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

                    var netMass = collidingVertex.Mass + collider2._vertexLookup[closestFace[0]].Mass;
                    
                    // adds rel velocity and resolves pos
                    if (Vector3d.Dot(relVelocity,collisionVector) < 0)
                    {
                        collidingVertex.Speed -= relVelocity * collidingVertex.Mass / netMass / 2;
                        collidingVertex.Position += collisionVector * closestFaceDist  * collidingVertex.Mass / netMass / 4;
                    
                        // apply collision to collided face
                        for (int i = 0; i < 3; i++)
                        {
                            var faceVertex = collider2._vertexLookup[closestFace[i]];
                            
                            faceVertex.Speed += relVelocity  * faceVertex.Mass / netMass / 3;
                            faceVertex.Position += collisionVector * closestFaceDist * faceVertex.Mass / netMass / 6;
                            
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
                        collidingVertex.Speed *= 0.98; // friction
                    }
                }
                
                // save changes to output
                collider1._vertexLookup[vertex.Key] = collidingVertex;
            }
        }
    }
    
    public static void ResolveSoftBodyCollision(List<PhysicsObject> collisionObjects)
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
            SoftBodyCollisionResolver(collisionPair[0], collisionPair[1]);
            SoftBodyCollisionResolver(collisionPair[1], collisionPair[0]);
        }
    }
}
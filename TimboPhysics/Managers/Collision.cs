using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
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
                
                if(obj1.Velocity == Vector3d.Zero && obj2.Velocity == Vector3d.Zero){ continue; }
                
                //Elastic collision speed restitution
                var colAxisVel = Vector3d.Dot(obj1.Velocity - obj2.Velocity, collisionVector) * collisionVector; // find velocity along axis of collision
                
                obj1.Velocity -= 2 * obj2.Mass / massSum * colAxisVel;
                obj2.Velocity += 2 * obj1.Mass / massSum * colAxisVel;
            }
        }
    }

    public static void ResolveRigidbodyCollision(List<PhysicsParticle> objects)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            var obj1 = objects[i];
            for (int j = i + 1; j < objects.Count; j++)
            {
                var obj2 = objects[i];
                //var rotAxis = Vector3d.Cross()
            }
        }
    }

    public static void SoftBodyCollisionResolver(SoftBody collider1, SoftBody collider2)
    {
        //var c1Vertices = new Dictionary<uint, SoftBody.PhysicsVertex>(collider1._vertexLookup);
        //var c2Vertices = new Dictionary<uint, SoftBody.PhysicsVertex>(collider2._vertexLookup);
        var dist = (collider1.Position - collider2.Position).Length;

        // Resolve collision on each vertex of object 1
        foreach (var vertex in collider1._vertexLookup)
        {
            var collidingVertex = vertex.Value; 
            
            // Bypass checks if vertex too far from object to collide
            if ((collider2.Position - vertex.Value.Position).Length > collider2.Radius)
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
                Vector3d collisionVector = (collider1.Position - collider2.Position).Normalized(); // prevent merging spheres
                
                // get relative velocity
                var faceVelocity = Vector3d.Zero;
                for (int i = 0; i < 3; i++)
                {
                    faceVelocity += collider2._vertexLookup[closestFace[i]].Speed / 3;
                }
                var relVelocity = Vector3d.Dot(collidingVertex.Speed - faceVelocity, collisionVector) * collisionVector;
                
                // resolves pos and vel
                if (Vector3d.Dot(relVelocity, collisionVector) < 0)
                {
                    collidingVertex.Speed -= relVelocity * 2;
                    collidingVertex.Position += collisionVector * closestFaceDist;


                    // apply collision to collided face
                    for (int i = 0; i < 3; i++)
                    {
                        var faceVertex = collider2._vertexLookup[closestFace[i]];
                        
                        faceVertex.Speed += relVelocity * 2 / 3;
                        
                        faceVertex.Position -= collisionVector * closestFaceDist / 3;
                        collider2._vertexLookup[closestFace[i]] = faceVertex;
                    }
                }

                // save changes to output
                collider1._vertexLookup[vertex.Key] = collidingVertex;
            }
        }
    }


    public static void SoftStaticCollisionResolver(PhysicsObject[] colliders)
    {
        StaticBody colliderStatic = (StaticBody)(colliders[0].GetType() == typeof(StaticBody) ? colliders[0] : colliders[1]);
        SoftBody colliderSoft = (SoftBody)(colliders[0].GetType() == typeof(SoftBody) ? colliders[0] : colliders[1]);
        
        foreach (var vertex in colliderSoft._vertexLookup)
        {
            var collidingVertex = colliderSoft._vertexLookup[vertex.Key];

            // If vertex is behind all faces of other object it must be colliding
            var isColliding = true;
            var closestFaceDist = Double.PositiveInfinity;
            var closestFace = Array.Empty<uint>();

            foreach (var face in colliderStatic.Faces)
            {

                var distance = TMathUtils.PointPlaneDist(
                    colliderStatic.VertexPos[face[0]],
                    colliderStatic.VertexPos[face[1]],
                    colliderStatic.VertexPos[face[2]],
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
                var collisionVector = TMathUtils.GetNormal( // Uses normal of face
                    colliderStatic.VertexPos[closestFace[0]],
                    colliderStatic.VertexPos[closestFace[1]],
                    colliderStatic.VertexPos[closestFace[2]]);

                // object responsible for full restitution if other is static
                collidingVertex.Position += collisionVector * closestFaceDist;

                // reflects velocity over collision vector 
                if (Vector3d.Dot(collidingVertex.Speed, collisionVector) < 0)
                {
                    collidingVertex.Speed -= 2 * Vector3d.Dot(collidingVertex.Speed, collisionVector) * collisionVector;
                    collidingVertex.Speed *= 0.98; // friction
                }
                
                // save changes to output
                colliderSoft._vertexLookup[vertex.Key] = collidingVertex;
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
                if (collisionObjects[i].GetType() == typeof(StaticBody) && collisionObjects[j].GetType() == typeof(StaticBody)) { continue; }
                var objDist = (collisionObjects[i].Position - collisionObjects[j].Position).Length;
                var sumRadius = collisionObjects[i].Radius + collisionObjects[j].Radius;
                if (objDist < sumRadius)
                {
                    collisionPairs.Add(new []{collisionObjects[i], collisionObjects[j]});
                }
            }
        }
        foreach (var collisionPair in collisionPairs)
        {
            if (collisionPair[0].GetType() == typeof(StaticBody) || collisionPair[1].GetType() == typeof(StaticBody)) 
            {
                SoftStaticCollisionResolver(collisionPair);
                continue;
            }

            var c1 = (SoftBody)collisionPair[0];
            var c2 = (SoftBody)collisionPair[1];
            SoftBodyCollisionResolver(c1, c2);
            SoftBodyCollisionResolver(c2, c1);
        }
    }
}
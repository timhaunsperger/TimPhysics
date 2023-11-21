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
            if ((collider2.Position - vertex.Value.Position).LengthSquared > collider2.Radius * collider2.Radius)
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


    public static void SoftStaticCollisionResolver(SoftBody colliderSoft, StaticBody colliderStatic)
    {
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
                    collidingVertex.Speed *= 0.9; // friction
                }
                
                // save changes to output
                colliderSoft._vertexLookup[vertex.Key] = collidingVertex;
            }
        }
    }

    public static void ResolveSoftBodyCollision(List<SoftBody> softBodies, List<PhysicsObject> staticBodies)
    {
        // Loop through all objects, looking for colliding pairs
        for (int i = 0; i < softBodies.Count; i++)
        {
            // Loop through all potential pairings after an that object in the list, preventing duplicates
            for (int j = i+1; j < softBodies.Count; j++)
            {
                var objDist = (softBodies[i].Position - softBodies[j].Position).Length;
                var sumRadius = softBodies[i].Radius + softBodies[j].Radius;
                if (objDist < sumRadius)
                {
                    SoftBodyCollisionResolver(softBodies[i], softBodies[j]);
                    SoftBodyCollisionResolver(softBodies[j], softBodies[i]);
                }
            }
        }

        for (int i = 0; i < staticBodies.Count; i++)
        {
            for (int j = 0; j < softBodies.Count; j++)
            {
                SoftStaticCollisionResolver(softBodies[j], (StaticBody)staticBodies[i]);
            }
        }

    }
    // returns vertices colliding, closest face, and face dist
    private static List<Tuple<uint, uint[], double>> CollidingVertices(Vector3d[] vertices, PhysicsObject obj)
    {
        var objVertices = obj.GetVertices();
        var collidingVertices = new List<Tuple<uint, uint[], double>>();
        for (uint i = 0; i < vertices.Length; i++)
        {
            if ((vertices[i] - obj.Position).Length > obj.Radius)
            {
                continue;
            }
            
            var isColliding = true;
            var closestFaceDist = Double.PositiveInfinity;
            var closestFace = Array.Empty<uint>();
            foreach (var face in obj.Faces)
            {
                var distance = TMathUtils.PointPlaneDist(
                    objVertices[face[0]],
                    objVertices[face[1]],
                    objVertices[face[2]],
                    vertices[i]);
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

            if (isColliding)
            {
                var vertex = new Tuple<uint, uint[], double>(i, closestFace, closestFaceDist);
                collidingVertices.Add(vertex);
            }
        }

        return collidingVertices;
    }
    public static double CollisionImpulse(double mass1, double mass2, double inertia1, double inertia2, Vector3d colRad1, Vector3d colRad2, Vector3d relVel, Vector3d normal)
    {
        var num = -2 * Vector3d.Dot(relVel, normal);
        var den = 1 / mass1 + 1 / mass2 +
                    Vector3d.Dot(
                        Vector3d.Cross(colRad1, normal) / inertia1 + Vector3d.Cross(colRad2, normal) / inertia2,
                        normal);
        return num / den;
    }
    public static void RigidBodyCollisionResolver(RigidBody c1, RigidBody c2)
    {
        var vertices1 = c1.GetVertices();
        var vertices2 = c2.GetVertices();
            
        var cVertices1 = CollidingVertices(vertices1, c2);
        var cVertices2 = CollidingVertices(vertices2, c1);

        if (cVertices1.Count == 0 && cVertices2.Count == 0)
        {
            return;
        }
        
        // get collision point
        var cPoint = Vector3d.Zero;
        uint deepestPointId = 0;
        double deepestPointDepth = double.PositiveInfinity;
        uint[] deepestPointCollidedFace = Array.Empty<uint>();
        int deepestPointColObj = 0;
        foreach (var vtx in cVertices1)
        {
            cPoint += vertices1[vtx.Item1];
            if (vtx.Item3 < deepestPointDepth)
            {
                deepestPointDepth = vtx.Item3;
                deepestPointCollidedFace = vtx.Item2;
                deepestPointColObj = 2;
            }
        }
        foreach (var vtx in cVertices2)
        {
            cPoint += vertices2[vtx.Item1];
            if (vtx.Item3 < deepestPointDepth)
            {
                deepestPointDepth = vtx.Item3;
                deepestPointCollidedFace = vtx.Item2;
                deepestPointColObj = 1;
            }
        }
        cPoint /= cVertices1.Count + cVertices2.Count;
        // get rel vel at colision point
        var cPtVel1 = c1.Velocity + TMathUtils.LinearVelocity(c1.rotAxis, cPoint - c1.Position, c1.AngVelocity);
        var cPtVel2 = c2.Velocity + TMathUtils.LinearVelocity(c2.rotAxis, cPoint - c2.Position, c2.AngVelocity);
        var relVel = cPtVel1 - cPtVel2;
        // get collision normal
        var normal = Vector3d.Zero;
        var faceCompare = 0d;
        if (deepestPointColObj == 1)
        {
            for (int i = 0; i < c1.Faces.Length; i++)
            {
                var faceNormal = TMathUtils.GetNormal(vertices2[deepestPointCollidedFace[0]],
                    vertices2[deepestPointCollidedFace[1]], vertices2[deepestPointCollidedFace[2]]);
                var dot = Vector3d.Dot(faceNormal, relVel);
                if (faceCompare > dot)
                {
                    faceCompare = dot;
                    normal = faceNormal;
                }
            }
        }
        else
        {
            for (int i = 0; i < c1.Faces.Length; i++)
            {
                var faceNormal = TMathUtils.GetNormal(vertices1[deepestPointCollidedFace[0]],
                    vertices1[deepestPointCollidedFace[1]], vertices1[deepestPointCollidedFace[2]]);
                var dot = Vector3d.Dot(faceNormal, relVel);
                if (faceCompare > dot)
                {
                    faceCompare = dot;
                    normal = faceNormal;
                }
            }
        }
        var impulse = CollisionImpulse(c1.Mass, c2.Mass, c1.Inertia, c2.Inertia, cPoint - c1.Position,
            cPoint - c2.Position, relVel, normal);

        if (Vector3d.Dot(normal, (cPoint-c1.Position)) > 0)
        {
            c1.Velocity -= normal * impulse / c1.Mass;
            c2.Velocity += normal * impulse / c2.Mass;
        }
        else
        {
            c1.Velocity += normal * impulse / c1.Mass;
            c2.Velocity -= normal * impulse / c2.Mass;
        }
        
        
    }
    
    public static void ResolveRigidBodyCollision(List<RigidBody> rigidBodies)
    {
        // Loop through all objects, looking for colliding pairs
        for (int i = 0; i < rigidBodies.Count; i++)
        {
            // Loop through all potential pairings after an that object in the list, preventing duplicates
            for (int j = i+1; j < rigidBodies.Count; j++)
            {
                var objDist = (rigidBodies[i].Position - rigidBodies[j].Position).Length;
                var sumRadius = rigidBodies[i].Radius + rigidBodies[j].Radius;
                if (objDist < sumRadius)
                {
                    RigidBodyCollisionResolver(rigidBodies[i], rigidBodies[j]);
                }
            }
        }
    }
}
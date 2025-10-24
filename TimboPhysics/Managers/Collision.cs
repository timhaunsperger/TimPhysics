using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TimboPhysics;

public static class Collision
{
    public static void ResolveParticleCollision(List<PhysicsParticle> objects, List<PhysicsObject> staticBodies)
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
                var colAxisVel = Vector3d.Dot(obj1.Velocity - obj2.Velocity, collisionVector) * collisionVector; 
                
                obj1.Velocity -= 2 * obj2.Mass / massSum * colAxisVel;
                obj2.Velocity += 2 * obj1.Mass / massSum * colAxisVel;
            }
        }
        
        for (int i = 0; i < objects.Count; i++)
        {
            for (int j = 0; j < staticBodies.Count; j++)
            {
                //ParticleStaticCollisionResolver(objects[i], (StaticBody)staticBodies[j]);
            }
        }
    }
    
    public static void ParticleStaticCollisionResolver(PhysicsParticle colliderParticle, StaticBody colliderStatic)
    {
        
        foreach (var vertex in colliderParticle._vertexOffsets)
        {
            var collidingVertex = vertex + colliderParticle.Position;

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
                    collidingVertex);
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
                //colliderParticle.Position += collisionVector * closestFaceDist;

                // reflects velocity over collision vector 
                if (Vector3d.Dot(colliderParticle.Velocity, collisionVector) < 0)
                {
                    
                    colliderParticle.Velocity -= 2 * Vector3d.Dot(colliderParticle.Velocity, collisionVector) * collisionVector;
                }
                
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

    private static Vector3d Support(Vector3d point, Vector3d ray, List<Vector3d> shape)
    {
        Vector3d sPoint = Vector3d.NegativeInfinity;
        double sPointProduct = double.NegativeInfinity;
        foreach (var vertex in shape)
        {
            var product = Vector3d.Dot(point - vertex, ray);
            if (product > sPointProduct)
            {
                sPoint = vertex;
                sPointProduct = product;
            }
        }
        return sPoint;
    }

    private static Tuple<Vector3d,Vector3d> GetCollision(PhysicsObject shape1, PhysicsObject shape2)
    {
        // Find Minkowski Difference of shapes
        var mDiffDict = new Dictionary<Vector3d, List<Vector3d>>();
        var s1Vtx = shape1.GetVertices();
        var s2Vtx = shape2.GetVertices();
        for (int i = 0; i < s1Vtx.Length; i++)
        {
            for (int j = 0; j < s2Vtx.Length;j++)
            {
                mDiffDict[s1Vtx[i] - s2Vtx[j]] = new List<Vector3d>{s1Vtx[i], s2Vtx[j]};
            }
        }
        var mDiffSet = mDiffDict.Keys.ToList();
        
        
        // find rough center of minkowski difference, and ray from center to origin
        var centerDiff = Vector3d.Zero;
        for (int i = 0; i < mDiffSet.Count;i++)
        {
            centerDiff += mDiffSet[i] / mDiffSet.Count;
        }
        var originRay = centerDiff.Normalized();

        // Vector3d vtx0;
        // Vector3d vtx1;
        // Vector3d vtx2;
        // Vector3d vtx3;
        
        
        // find three support points most towards origin from center
        // vtx0 = Support(centerDiff, -originRay, mDiffSet);
        // mDiffSet.Remove(vtx0);
        var vtx1 = Support(centerDiff, originRay, mDiffSet);
        mDiffSet.Remove(vtx1);
        // while (true)
        // {
        //     var nextRay = Vector3d.Cross(Vector3d.Cross(vtx1 - vtx0, -vtx1), vtx1 - vtx0);
        //     vtx2 = Support(centerDiff, nextRay, mDiffSet);
        //     nextRay = Vector3d.Cross(Vector3d.Cross(vtx1 - vtx0, -vtx1), vtx1 - vtx0);
        //     if (Vector3d.Dot(nextRay, -centerDiff) < 0)
        //     {
        //         
        //     }
        //     vtx3 = Support(centerDiff, nextRay, mDiffSet);
        // }
        
        
        var vtx2 = Support(centerDiff, originRay, mDiffSet);
        mDiffSet.Remove(vtx2);
        var vtx3 = Support(centerDiff, originRay, mDiffSet);
        mDiffSet.Remove(vtx3);
        
        // ensure points are not co-linear with tolerance
        while (Vector3d.Cross(vtx1-vtx2, vtx1-vtx3).LengthSquared < 0.000001d)
        {
            vtx3 = Support(centerDiff, originRay, mDiffSet);
            mDiffSet.Remove(vtx3);
        }
        
        // check if origin and center are on same side of plane from three support points, and return collision point approx
        var collideDepth = TMathUtils.PointPlaneDist(vtx1, vtx2, vtx3, Vector3d.Zero);
        if (TMathUtils.PointPlaneDist(vtx1, vtx2, vtx3, centerDiff) > 0 != collideDepth > 0)
        {
            return new Tuple<Vector3d, Vector3d>(Vector3d.Zero,Vector3d.Zero);
        }


        var normal = TMathUtils.GetNormal(vtx1, vtx2, vtx3);
        var mAllVtx = mDiffDict.Keys.ToList();
        var mFaceVtx = new List<Vector3d>();
        var cPoint1 = Vector3d.Zero;
        var cPoint2 = Vector3d.Zero;
        
        foreach (var vertex in mAllVtx)
        {
            var vertexDepth = TMathUtils.PointPlaneDist(vtx1, vtx2, vtx3, vertex);
            if (Math.Abs(vertexDepth) < 0.001)
            {
                mFaceVtx.Add(vertex);
                cPoint1 += mDiffDict[vertex][0];
                cPoint2 += mDiffDict[vertex][1];
            }
        }
        cPoint1 /= mFaceVtx.Count;
        cPoint2 /= mFaceVtx.Count;
        
        
        // for (int i = 0; i < mFaceVtx.Count-1; i++)
        // {
        //     area = Vector3d.Dot(normal,
        //         Vector3d.Cross(mFaceVtx[i] - contactProj, mFaceVtx[i + 1] - contactProj));
        //     areas.Add(area);
        //     totalArea += area;
        // }
        // area = Vector3d.Dot(normal,
        //     Vector3d.Cross(mFaceVtx[^1] - contactProj, mFaceVtx[0] - contactProj));
        // areas.Add(area);
        // totalArea += area;
        //
        // // Find barycentric coords of origin projected onto face and convert to collision point
        // for (int i = 0; i < mFaceVtx.Count; i++)
        // {
        //     cPoint1 += areas[i] * mDiffDict[mFaceVtx[i]][0] / totalArea;
        //     cPoint2 += areas[i] * mDiffDict[mFaceVtx[i]][1] / totalArea;
        // }

        return new Tuple<Vector3d, Vector3d>(normal, (cPoint1+cPoint2)/2);
        

        
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
                if (distance < -0.001)
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
                  Vector3d.Dot(Vector3d.Cross(Vector3d.Cross(colRad1, normal), colRad1) / inertia1 + Vector3d.Cross(Vector3d.Cross(colRad2, normal), colRad2) / inertia2, normal);
        return num / den;
    }
    public static void RigidBodyCollisionResolver(RigidBody c1, RigidBody c2)
    {
        var vertices1 = c1.GetVertices();
        var vertices2 = c2.GetVertices();
            
        // var cVertices1 = CollidingVertices(vertices1, c2);
        // var cVertices2 = CollidingVertices(vertices2, c1);

        var collisionData = GetCollision(c1, c2);
        var vertices = collisionData.Item2;
        var normal = collisionData.Item1;
        
        if (vertices.Length == 0)
        {
            return;
        }
        // get collision point
        var cPoint = collisionData.Item2;
        // foreach (var vtx in vertices)
        // {
        //     cPoint += vtx;
        // }
        // cPoint /= vertices.Length;
        
        // get collision radii
        var rad1 = c1.Position - cPoint;
        var rad2 = c2.Position - cPoint;
        
        // get rel vel at colision point
        var cPtVel1 = c1.Velocity + TMathUtils.LinearVelocity(c1.rotAxis, rad1, c1.AngVelocity);
        var cPtVel2 = c2.Velocity + TMathUtils.LinearVelocity(c2.rotAxis, rad2, c2.AngVelocity);
        var relVel = cPtVel1 - cPtVel2;
        // get collision normal
        // Vector3d normal;
        // var faceCompare = double.PositiveInfinity;
        // var closestFace1Norm = Vector3d.Zero;
        // var closestFace2Norm = Vector3d.Zero;
        //
        // for (int i = 0; i < c1.Faces.Length; i++)
        // {
        //     var faceDist = TMathUtils.PointPlaneDist(vertices1[c1.Faces[i][0]],
        //         vertices1[c1.Faces[i][1]], vertices1[c1.Faces[i][2]], cPoint);
        //     if (faceCompare > faceDist)
        //     {
        //         faceCompare = faceDist;
        //         closestFace1Norm =  TMathUtils.GetNormal(vertices1[c1.Faces[i][0]],
        //             vertices1[c1.Faces[i][1]], vertices1[c1.Faces[i][2]]);
        //     }
        // }
        // faceCompare = double.PositiveInfinity;
        // for (int i = 0; i < c2.Faces.Length; i++)
        // {
        //     var faceDist = TMathUtils.PointPlaneDist(vertices2[c2.Faces[i][0]],
        //         vertices2[c2.Faces[i][1]], vertices2[c2.Faces[i][2]], cPoint);
        //     if (faceCompare > faceDist)
        //     {
        //         faceCompare = faceDist;
        //         closestFace2Norm =  TMathUtils.GetNormal(vertices2[c2.Faces[i][0]],
        //             vertices2[c2.Faces[i][1]], vertices2[c2.Faces[i][2]]);
        //     }
        // }
        // if (Math.Abs(Vector3d.Dot(closestFace1Norm, relVel)) > Math.Abs(Vector3d.Dot(closestFace2Norm, relVel)))
        // {
        //     normal = closestFace1Norm;
        //     normal *= -1;
        // }
        // else
        // {
        //     normal = closestFace2Norm;
        // }
        
        var impulse = CollisionImpulse(c1.Mass, c2.Mass, c1.Inertia, c2.Inertia, rad1,
            rad2, relVel, normal) * normal;
        var normRad1 = rad1.Normalized();
        var normRad2 = rad2.Normalized();
        
        var angularImpulse1 = Vector3d.Cross(impulse, normRad1);
        var angularImpulse2 = Vector3d.Cross(impulse, normRad2);
        
        var linearImpulse1 = impulse / c1.Mass;
        var linearImpulse2 = impulse / c2.Mass;
        
        if (true)
        {
            c1.Velocity += linearImpulse1 / c1.Mass;
            c2.Velocity -= linearImpulse2 / c2.Mass;
            
            var eVec1 = c1.rotAxis * c1.AngVelocity + angularImpulse1;
            var eVec2 = c2.rotAxis * c2.AngVelocity - angularImpulse2;
            if (eVec1 != Vector3d.Zero)
            {
                c1.rotAxis = eVec1.Normalized();
                c1.AngVelocity = eVec1.Length;
            }
            if (eVec2 != Vector3d.Zero)
            {
                c2.rotAxis = eVec2.Normalized();
                c2.AngVelocity = eVec2.Length;
            }
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
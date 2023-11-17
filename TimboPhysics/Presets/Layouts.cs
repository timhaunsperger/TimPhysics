using OpenTK.Mathematics;

namespace TimboPhysics.Presets;

public static class Layouts
{
    public static void SoftBodyTest1(List<PhysicsObject> physicsObjects, List<PhysicsParticle> physicsParticles,Shader shader)
    {
        var floor = new StaticBody(
            new RectPrism(
                new Vector3d(0,-15,0), 
                300, 
                0.5, 
                300, 
                Quaterniond.FromEulerAngles(0, 0, 0)), 
            shader);
        physicsObjects.Add(floor);
        
        
        for (int i = 0; i < 6; i++) // Add Platforms
        {
            physicsObjects.Add(new StaticBody(
                new RectPrism(
                    new Vector3d(i%2*10-5,i*5-10,0),
                    10, 
                    0.5, 
                    5, 
                    Quaterniond.FromEulerAngles(45*i%2>0?1:-1, 0, 0)), 
                shader));
        }

        var rand = new Random();
        for (var i = 0; i < 40; i++) // Add Particles
        {
            physicsParticles.Add(new PhysicsParticle(new Vector3d(
                (rand.NextDouble()-0.5)*7+20, 
                (rand.NextDouble()+0.5)*7, 
                (rand.NextDouble()-0.5)*7), (i % 2 + 1) * 0.2, shader, Vector3d.Zero, true));
        }
        physicsParticles.Add(new PhysicsParticle(new Vector3d(-10,5,6), 2, shader, new Vector3d( 1,0,0), false));
        physicsParticles.Add(new PhysicsParticle(new Vector3d( 10,6,6), 1, shader, new Vector3d(-1,0,0), false));


        
        for (int i = 0; i < 10; i++) // Add SoftBodies
        {
            physicsObjects.Add(new SoftBody(SphereCache.GetSphere(2, new Vector3d(i%4, i*2+10, 0), 0.78), shader, 1, true));
        }
        // Add more SoftBodies for collision demo
        physicsObjects.Add(new SoftBody(SphereCache.GetSphere(2, new Vector3d(5, 5, 7), 0.8), shader, 1, false));
        physicsObjects.Add(new SoftBody(SphereCache.GetSphere(2, new Vector3d(-5, 5, 7), 0.8), shader, 2, false));
        var last = physicsObjects.Count;
        foreach (var vertex in physicsObjects[last-1]._vertexLookup)
        {
            var physVertex = physicsObjects[last-1]._vertexLookup[vertex.Key];
            physVertex.Speed += new Vector3d(3,0,0);
            physicsObjects[last-1]._vertexLookup[vertex.Key] = physVertex;
        }
    }
}
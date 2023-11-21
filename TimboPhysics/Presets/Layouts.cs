using OpenTK.Mathematics;
namespace TimboPhysics.Presets;

public static class Layouts
{
    public static void Test1(Game game, Shader shader)
    {
        var floor = new StaticBody(
            new RectPrism(
                new Vector3d(0,-15,0), 
                300, 
                0.5, 
                300, 
                Quaterniond.FromEulerAngles(0, 0, 0)), 
            shader);
        game.AddObject(floor);
        
        
        for (int i = 0; i < 6; i++) // Add Platforms
        {
            game.AddObject(new StaticBody(
                new RectPrism(
                    new Vector3d(i%2*12-5,i*8-10,0),
                    10, 
                    0.25, 
                    5, 
                    Quaterniond.FromEulerAngles(45*i%2>0?1:-1, 0, 0)), 
                shader));
        }
        
        var rand = new Random();
        for (var i = 0; i < 500; i++) // Add Particles
        {
            game.AddObject(new PhysicsParticle(new Vector3d(
                (rand.NextDouble()-0.5)*10+20, 
                (rand.NextDouble()-0.5)*1 + 20, 
                (rand.NextDouble()-0.5)*10), (i % 2 + 1)* 0 + 0.2, 1, shader, Vector3d.Zero, false));
        }
        game.AddObject(new PhysicsParticle(new Vector3d( 20,60,0), 4, 2, shader, new Vector3d(0,-40,0), false));
        game.AddObject(new PhysicsParticle(new Vector3d( 20,-20,0), 4, 2, shader, new Vector3d(0,40,0), false));
        
         for (var i = 0; i < 500; i++) // Add Particles
         {
             game.AddObject(new PhysicsParticle(new Vector3d(
                 (rand.NextDouble()-0.5)*10-20, 
                 rand.NextDouble()*10, 
                 (rand.NextDouble()-0.5)*10), 1 * 0.3, 1, shader, Vector3d.Zero, true));
         }
        
        
        for (int i = 0; i < 4; i++) // Add SoftBodies
        {
            game.AddObject(new SoftBody(SphereCache.GetSphere(1, new Vector3d(i%4, i*3+10, 0), 1), shader, Vector3d.Zero, 1, true, false));
        }
        game.AddObject(new RigidBody(new RectPrism(
                new Vector3d(-3,11.5,0), 
                2, 
                2, 
                2, 
                Quaterniond.FromEulerAngles(Math.PI/3*0, 0, 1)), 
            shader, Vector3d.UnitX * 1, 100, false));
        game.AddObject(new RigidBody(SphereCache.GetSphere(1, new Vector3d(3, 12, 0), 1),
            shader, Vector3d.UnitX * 0, 1, false));
        // game.AddObject(new RigidBody(new RectPrism(
        //         new Vector3d(3,10,0), 
        //         2, 
        //         2, 
        //         2, 
        //         Quaterniond.FromEulerAngles(0, 0, 0)), 
        //     shader, -Vector3d.UnitX * 1, 1, false));
    }
}
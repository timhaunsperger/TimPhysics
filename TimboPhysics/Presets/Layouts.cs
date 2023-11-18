using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
        game.AddObject(floor, true, false);
        
        
        for (int i = 0; i < 6; i++) // Add Platforms
        {
            game.AddObject(new StaticBody(
                new RectPrism(
                    new Vector3d(i%2*10-5,i*5-10,0),
                    10, 
                    0.5, 
                    5, 
                    Quaterniond.FromEulerAngles(45*i%2>0?1:-1, 0, 0)), 
                shader), true, false);
        }

        var rand = new Random();
        for (var i = 0; i < 2000; i++) // Add Particles
        {
            game.AddObject(new PhysicsParticle(new Vector3d(
                (rand.NextDouble()-0.5)*10+20, 
                rand.NextDouble()*10 + 25, 
                (rand.NextDouble()-0.5)*10), (i % 2 + 1) * 0.3, 1, shader, Vector3d.Zero, false), false, true);
        }
        game.AddObject(new PhysicsParticle(new Vector3d( 20,80,0), 4, 2, shader, new Vector3d(0,-100,0), false), false, true);
        game.AddObject(new PhysicsParticle(new Vector3d( 20,-20,0), 4, 2, shader, new Vector3d(0,100,0), false), false, true);
        
         for (var i = 0; i < 500; i++) // Add Particles
         {
             game.AddObject(new PhysicsParticle(new Vector3d(
                 (rand.NextDouble()-0.5)*10-20, 
                 rand.NextDouble()*10, 
                 (rand.NextDouble()-0.5)*10), 1 * 0.3, 1, shader, Vector3d.Zero, true), false, true);
         }
        
        
        for (int i = 0; i < 10; i++) // Add SoftBodies
        {
            game.AddObject(new SoftBody(SphereCache.GetSphere(2, new Vector3d(i%4, i*4+10, 0), 0.78), shader, 1, true), true, false);
        }
    }
}
using OpenTK.Mathematics;
namespace TimboPhysics.Presets;

public static class Layouts
{
    public static void ActiveTest(Game game, Shader shader)
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
        for (var i = 0; i < 100; i++) // Add Particles
        {
            game.AddObject(new PhysicsParticle(new Vector3d(
                (rand.NextDouble()-0.5)*5+20, 
                (rand.NextDouble()-0.5)*1 + 20, 
                (rand.NextDouble()-0.5)*3), (i % 2 + 1)* 0 + 0.2, 1, shader, Vector3d.Zero, false));
        }
        game.AddObject(new PhysicsParticle(new Vector3d( 20,60,0), 4, 2, shader, new Vector3d(0,-40,0), false));
        game.AddObject(new PhysicsParticle(new Vector3d( 20,-20,0), 4, 2, shader, new Vector3d(0,40,0), false));
        
         // for (var i = 0; i < 500; i++) // Add Particles
         // {
         //     game.AddObject(new PhysicsParticle(new Vector3d(
         //         (rand.NextDouble()-0.5)*10-20, 
         //         rand.NextDouble()*10, 
         //         (rand.NextDouble()-0.5)*10), 1 * 0.3, 1, shader, Vector3d.Zero, true));
         // }
        
        
        for (int i = 0; i < 8; i++) // Add SoftBodies
        {
            game.AddObject(new SoftBody(SphereCache.GetSphere(2, new Vector3d(i%4, i*2.5+25, 0), 1), shader, Vector3d.Zero, 1, true, false));
        }
        game.AddObject(new RigidBody(new RectPrism(
                new Vector3d(-3,11,10), 
                1, 
                1, 
                2, 
                Quaterniond.FromEulerAngles(Math.PI/3*0, 10, 1)), 
            shader, Vector3d.UnitX * 10, 1, false));
        // game.AddObject(new RigidBody(SphereCache.GetSphere(1, new Vector3d(3, 12, 10), 1),
        //     shader, Vector3d.UnitX * 0, 1, false));
        // game.AddObject(new RigidBody(new RectPrism(
        //         new Vector3d(-1, 0 ,0), 
        //         1, 
        //         1, 
        //         1, 
        //         Quaterniond.FromEulerAngles(double.Pi/1, double.Pi/1,1)), 
        //     shader, Vector3d.UnitX * 0, 1, false));
        // game.AddObject(new RigidBody(new RectPrism(
        //         new Vector3d(3.5,2,0), 
        //         1, 
        //         1, 
        //         1, 
        //         Quaterniond.FromEulerAngles(double.Pi/1, 0, 0)), 
        //     shader, Vector3d.UnitX * -2, 1, false));
        for (int i = 0; i < 10; i++)
        {
            game.AddObject(new RigidBody(new RectPrism(
                    new Vector3d(3.5*i+6,11,10), 
                    1, 
                    1, 
                    1, 
                    Quaterniond.FromEulerAngles(rand.Next() * 0, rand.Next() * 0, rand.Next() * 0)), 
                shader, Vector3d.UnitX * 0, 1, false));
        }
        
        
        // game.AddObject(new RigidBody(new RectPrism(
        //         new Vector3d(3,10,0), 
        //         2, 
        //         2, 
        //         2, 
        //         Quaterniond.FromEulerAngles(0, 0, 0)), 
        //     shader, -Vector3d.UnitX * 1, 1, false));
    }
    
    public static void Test2(Game game, Shader shader)
    {
        game.AddObject(new PhysicsParticle(new Vector3d( 0,10,-10), 4, 2, shader, new Vector3d(50,0,0), true));
    }
    
    public static void Test3(Game game, Shader shader)
    {
        game.AddObject(new PhysicsParticle(new Vector3d( 0,10,-10), 4, 2, shader, new Vector3d(50,0,0), true));
        
        var floor = new StaticBody(
            new RectPrism(
                new Vector3d(0,-15,0), 
                300, 
                0.5, 
                300, 
                Quaterniond.FromEulerAngles(0, 0, 0)), 
            shader);
        game.AddObject(floor);
    }
    
    public static void Test4(Game game, Shader shader)
    {
        game.AddObject(new PhysicsParticle(new Vector3d( -20,10,-10), 2, 2, shader, new Vector3d(30,0,0), true));
        game.AddObject(new PhysicsParticle(new Vector3d( 20,10,-11), 2, 2, shader, new Vector3d(-30,0,0), true));

        
        var floor = new StaticBody(
            new RectPrism(
                new Vector3d(0,-15,0), 
                100, 
                0.5, 
                100, 
                Quaterniond.FromEulerAngles(0, 0, 0)), 
            shader);
        game.AddObject(floor);
    }
}
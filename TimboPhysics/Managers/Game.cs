using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TimboPhysics;

public class Game : GameWindow
{
    private List<RenderObject> _renderObjects = new ();
    private List<PhysicsObject> _physicsObjects = new ();
    private List<PhysicsParticle> _physicsParticles = new ();
    public List<RenderObject> _collisionObjects = new();
    private Shader _shader;
    private float _AspectRatio = 1;
    private Camera _camera;

    public Game(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings) 
        : base(gameSettings, nativeSettings)
    {
        _camera = new Camera(new Vector3(0, 5,10), _AspectRatio);
        CursorVisible = false;
        CursorGrabbed = true;
    }

    private void AddObject(RenderObject newObject, bool render, bool physics, bool particle, bool collision)
    {
        if(render){ _renderObjects.Add(newObject);}
        if(collision){_collisionObjects.Add(newObject);}
        if(physics){_physicsObjects.Add((PhysicsObject)newObject);}
        if(particle){_physicsParticles.Add((PhysicsParticle)newObject);}
    }
    
    protected override void OnLoad()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(new Color4(0.2f,0.2f,1f,1f));
        
        _shader = new Shader("Shaders/lighting.vert", "Shaders/lighting.frag");
        
        var floor = new Staticbody(
            new RectPrism(
                new Vector3d(0,-15,0), 
                300, 
                0.5, 
                300, 
                Quaterniond.FromEulerAngles(0, 0, 0)), 
            _shader);
        _physicsObjects.Add(floor);
        
        
        for (int i = 0; i < 6; i++) // Add Platforms
        {
            _physicsObjects.Add(new Staticbody(
                new RectPrism(
                    new Vector3d(i%2*10-5,i*5-10,0),
                    10, 
                    0.5, 
                    5, 
                    Quaterniond.FromEulerAngles(45*i%2>0?1:-1, 0, 0)), 
                _shader));
        }

        var rand = new Random();
        for (int i = 0; i < 40; i++) // Add Particles
        {
            var pm = i % 2 == 1 ? 1 : -1;
            _physicsParticles.Add(new PhysicsParticle(pm,new Vector3d(
                (rand.NextDouble()-0.5)*7+20, 
                (rand.NextDouble()+0.5)*7, 
                (rand.NextDouble()-0.5)*7), (i % 2 + 1) * 0.2, _shader));
        }
        for (int i = 0; i < 20; i++) // Add Softbodies
        {
            _physicsObjects.Add(new Softbody(SphereCache.GetSphere(2, new Vector3d(i%4, i*2+10, 0), 0.78), _shader, true));
        }
        // Add more softbodies for collision demo
        _physicsObjects.Add(new Softbody(SphereCache.GetSphere(2, new Vector3d(5, 5, 7), 0.78), _shader, false));
        _physicsObjects.Add(new Softbody(SphereCache.GetSphere(2, new Vector3d(-5, 5, 7), 0.78), _shader, false));
        var last = _physicsObjects.Count;
        foreach (var vertex in _physicsObjects[last-1]._vertexLookup)
        {
            var physVertex = _physicsObjects[last-1]._vertexLookup[vertex.Key];
            physVertex.Speed += new Vector3d(3,0,0);
            _physicsObjects[last-1]._vertexLookup[vertex.Key] = physVertex;
        }
        
        base.OnLoad();
    }

    protected override void OnUnload()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        _shader.Dispose();
        base.OnUnload();
        Environment.Exit(0);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        _camera.AspectRatio = e.Width / (float)e.Height;
        base.OnResize(e);
    }
    
    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        if (IsFocused)
        {
            _camera.MouseMove(e.Delta);
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        _camera.Fov += e.OffsetY;
        base.OnMouseWheel(e);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        var input = KeyboardState;
        if (input.IsKeyDown(Keys.Escape))
        {
            Close();
            throw new Exception("closing program");
        }

        _camera.Move(input, (float)args.Time);
        
        for (int i = 0; i < _physicsObjects.Count; i++)
        {
            if (input.IsKeyDown(Keys.U)) // lift softbodies if u pressed
            {
                for (uint j = 0; j < _physicsObjects[i]._vertexLookup.Count; j++)
                {
                    var vertex = _physicsObjects[i]._vertexLookup[j];
                    vertex.Speed += Vector3d.UnitY/6;
                    _physicsObjects[i]._vertexLookup[j] = vertex;
                }
            }
            _physicsObjects[i].Update(args.Time);
        }
        for (int i = 0; i < _physicsParticles.Count; i++)
        {
            var taskNum = i;
            _physicsParticles[taskNum].Update(_physicsParticles, args.Time);
        }
        Collision.ResolveParticleCollision(_physicsParticles);
        Collision.ResolveSoftbodyCollision(_physicsObjects);
        base.OnUpdateFrame(args);
    }
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        for (int i = 0; i < _renderObjects.Count; i++)
        {
            _renderObjects[i].Render(_camera.GetViewMatrix(), _camera.GetProjectionMatrix(), _camera.Position);
        }
        for (int i = 0; i < _physicsObjects.Count; i++)
        {
            _physicsObjects[i].Render(_camera.GetViewMatrix(), _camera.GetProjectionMatrix(), _camera.Position);
        }
        for (int i = 0; i < _physicsParticles.Count; i++)
        {
            _physicsParticles[i].Render(_camera.GetViewMatrix(), _camera.GetProjectionMatrix(), _camera.Position);
        }
        
        Context.SwapBuffers();
        
        base.OnRenderFrame(args);
    }
}
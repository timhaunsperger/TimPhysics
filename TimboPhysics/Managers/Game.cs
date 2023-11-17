using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TimboPhysics.Presets;

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
        
        // Load lighting shader
        _shader = new Shader("Shaders/lighting.vert", "Shaders/lighting.frag");
        
        // Load world layout
        Layouts.SoftBodyTest1(_physicsObjects, _physicsParticles, _shader);
        
        base.OnLoad();
    }

    protected override void OnUnload()
    {
        // Cleanup on close
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        _shader.Dispose();
        base.OnUnload();
        Environment.Exit(0);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        // Adjust camera on window resize
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
            if (input.IsKeyDown(Keys.U)) // Lift SoftBodies if "u" pressed
            {
                for (uint j = 0; j < _physicsObjects[i]._vertexLookup.Count; j++)
                {
                    var vertex = _physicsObjects[i]._vertexLookup[j];
                    vertex.Speed += Vector3d.UnitY/6;
                    _physicsObjects[i]._vertexLookup[j] = vertex;
                }
            }
            // Update all physics objects
            _physicsObjects[i].Update(args.Time);
        }
        
        // Update all physics particles
        for (int i = 0; i < _physicsParticles.Count; i++)
        {
            var taskNum = i;
            _physicsParticles[taskNum].Update(_physicsParticles, args.Time);
        }
        // Resolve Object Collisions
        Collision.ResolveParticleCollision(_physicsParticles);
        Collision.ResolveSoftBodyCollision(_physicsObjects);
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
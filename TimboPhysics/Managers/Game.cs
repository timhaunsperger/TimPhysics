using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
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
    private Shader _shader;
    private float _AspectRatio = 1;
    private Camera _camera;
    private List<Tuple<List<double[][]>, List<double[]>>> _frames = new ();
    private int _frameCounter = 0;
    private bool _recording = true;

    public Game(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings) 
        : base(gameSettings, nativeSettings)
    {
        _camera = new Camera(new Vector3(0, 5,10), _AspectRatio);
        CursorVisible = false;
        CursorGrabbed = true;
    }

    public void AddObject(PhysicsObject newObject)
    {
        if (newObject.GetType() == typeof(PhysicsParticle))
        {
            _physicsParticles.Add((PhysicsParticle)newObject);
        }
        else
        {
            _physicsObjects.Add(newObject);
        }
    }
    
    protected override void OnLoad()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(new Color4(0.2f,0.2f,1f,1f));
        
        // Load lighting shader
        _shader = new Shader("Shaders/lighting.vert", "Shaders/lighting.frag");
        
        // Load world layout
        
        Layouts.Test1(this, _shader);

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

        _camera.Move(input, 0.025f);
        
        if (input.IsKeyDown(Keys.R)) // Playback Recording if "p" pressed
        {
            _recording = false;
            _frameCounter = 0;
        }

        if (_recording)
        {
            for (int i = 0; i < _physicsObjects.Count; i++)
            {
                if (input.IsKeyDown(Keys.U) && _physicsObjects.GetType() == typeof(SoftBody)) // Lift SoftBodies if "u" pressed
                {
                    var obj = (SoftBody)_physicsObjects[i];
                    for (uint j = 0; j < obj._vertexLookup.Count; j++)
                    {
                        var vertex = obj._vertexLookup[j];
                        vertex.Speed += Vector3d.UnitY / 6;
                        obj._vertexLookup[j] = vertex;
                    }
                    // Update all softbodies
                }
                
                _physicsObjects[i].Update(0.005);
            }
            
            // Update all physics particles
            for (int i = 0; i < _physicsParticles.Count; i++)
            {
                var taskNum = i;
                ThreadPool.QueueUserWorkItem(c => _physicsParticles[taskNum].Update(0.005));
            }

            while (ThreadPool.PendingWorkItemCount != 0)
            {
                    
            }

            // Resolve Object Collisions
            Collision.ResolveParticleCollision(_physicsParticles);
            Collision.ResolveSoftBodyCollision(_physicsObjects);

            _frames.Add(new Tuple<List<double[][]>, List<double[]>>(new List<double[][]>(capacity:_physicsObjects.Count), new List<double[]>(capacity:_physicsParticles.Count)));
            for (int i = 0; i < _physicsObjects.Count; i++)
            {
                _frames[_frameCounter].Item1.Add(new List<double[]>(_physicsObjects[i].Vertices).ToArray());
            }
            for (int i = 0; i < _physicsParticles.Count; i++)
            {
                _frames[_frameCounter].Item2.Add(new List<double>() {_physicsParticles[i].Position.X, _physicsParticles[i].Position.Y, _physicsParticles[i].Position.Z}.ToArray());
            }
            _frameCounter ++;
        }
        else
        {
            if (input.IsKeyDown(Keys.R)) // Playback Recording if "r" pressed
            {
                _frameCounter = 0;
            }
            if (_frameCounter < _frames.Count - 1)
            {
                for (int i = 0; i < _physicsObjects.Count; i++)
                {
                    _physicsObjects[i].Assign(_frames[_frameCounter].Item1[i]);
                }
                for (int i = 0; i < _physicsParticles.Count; i++)
                {
                    _physicsParticles[i].Assign(_frames[_frameCounter].Item2[i]);
                }
                _frameCounter++;
            }
        }
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
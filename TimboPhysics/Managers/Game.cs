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
    private List<RenderObject> _objects = new ();
    private List<PhysicsObject> _staticObjects = new ();
    private List<SoftBody> _softObjects = new ();
    private List<RigidBody> _hardObjects = new ();
    private List<PhysicsParticle> _particles = new ();
    private Shader _shader;
    private float _AspectRatio = 1;
    private Camera _camera;
    private List<List<double[]>[]> _frames = new ();
    private int _frameCounter = 0;
    private bool _recording = true;
    private bool _doRecording = true;

    public Game(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings) 
        : base(gameSettings, nativeSettings)
    {
        _camera = new Camera(new Vector3(0, 5,10), _AspectRatio);
        CursorVisible = false;
        CursorGrabbed = true;
    }

    public void AddObject(PhysicsObject newObject)
    {
        _objects.Add(newObject);
        var type = newObject.GetType();
        if (type == typeof(PhysicsParticle))
        {
            _particles.Add((PhysicsParticle)newObject);
        }
        if (type == typeof(StaticBody))
        {
            _staticObjects.Add((StaticBody)newObject);
        }
        if (type == typeof(RigidBody))
        {
            _hardObjects.Add((RigidBody)newObject);
        }
        if (type == typeof(SoftBody))
        {
            _softObjects.Add((SoftBody)newObject);
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
            for (int i = 0; i < _softObjects.Count; i++)
            {
                if (input.IsKeyDown(Keys.U)) // Lift SoftBodies if "u" pressed
                {
                    for (uint j = 0; j < _softObjects[i]._vertexLookup.Count; j++)
                    {
                        var vertex = _softObjects[i]._vertexLookup[j];
                        vertex.Speed += Vector3d.UnitY / 6;
                        _softObjects[i]._vertexLookup[j] = vertex;
                    }
                }
            }
            
            // Update all physics particles
            for (int i = 0; i < _objects.Count; i++)
            {
                var taskNum = i;
                _objects[taskNum].Update(0.005);
            }


            // Resolve Object Collisions
            Collision.ResolveParticleCollision(_particles);
            Collision.ResolveSoftBodyCollision(_softObjects, _staticObjects);
            Collision.ResolveRigidBodyCollision(_hardObjects);

            if (_doRecording)
            {
                _frames.Add(new List<double[]>[3]);
                _frames[_frameCounter][0] = new List<double[]>();
                _frames[_frameCounter][1] = new List<double[]>();
                _frames[_frameCounter][2] = new List<double[]>();
                for (int i = 0; i < _softObjects.Count; i++)
                {
                    _frames[_frameCounter][0].Add(_softObjects[i]._flattenedVertices);
                }
                for (int i = 0; i < _particles.Count; i++)
                {
                    _frames[_frameCounter][1].Add(new []{_particles[i].Position.X, _particles[i].Position.Y, _particles[i].Position.Z});
                }
                for (int i = 0; i < _hardObjects.Count; i++)
                {
                    _frames[_frameCounter][2].Add(_hardObjects[i]._flattenedVertices);
                }
                _frameCounter ++;
            }
        }
        else
        {
            if (input.IsKeyDown(Keys.R)) // Playback Recording if "r" pressed
            {
                _frameCounter = 0;
            }
            if (_frameCounter < _frames.Count - 1)
            {
                for (int i = 0; i < _softObjects.Count; i++)
                {
                    _softObjects[i].AssignVertices(_frames[_frameCounter][0][i]);
                }
                for (int i = 0; i < _particles.Count; i++)
                {
                    _particles[i].Assign(_frames[_frameCounter][1][i]);
                }
                for (int i = 0; i < _hardObjects.Count; i++)
                {
                    _hardObjects[i].AssignVertices(_frames[_frameCounter][2][i]);
                }
                _frameCounter++;
            }
        }
        base.OnUpdateFrame(args);
    }
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        for (int i = 0; i < _objects.Count; i++)
        {
            _objects[i].Render(_camera.GetViewMatrix(), _camera.GetProjectionMatrix(), _camera.Position);
        }
        
        Context.SwapBuffers();
        base.OnRenderFrame(args);
    }
}
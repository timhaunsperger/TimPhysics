using System;
using System.Diagnostics;
using System.Security.Cryptography;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static OpenTK.Mathematics.MathHelper;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TimboPhysics;

public class Game : GameWindow
{
    double[][] _vertices = {
        new [] {15,  15, -15,  1.0, 1.0}, //top right back     0
        new [] {15, -15, -15,  0.0, 0.0}, //bottom right back  1
        new [] {-15, -15, -15,  1.0, 0.0}, //bottom left back   2
        new [] {-15,  15, -15,  0.0, 1.0}, //top left back      3
        
        new [] {15,  15,  15,  1.0, 0.0}, //top right front    4
        new [] {15, -15,  15,  0.0, 1.0}, //bottom right front 5
        new [] {-15, -15,  15,  1.0, 1.0}, //bottom left front  6
        new [] {-15,  15,  15,  0.0, 0.0}, //top left front     7
    };
    
    static float X=0.525731112119133606f;
    static float Z=0.850650808352039932f;
    static float N=0f;
    
    uint[] _indices = {
        0, 1, 2,
        2, 3, 0,
        0, 1, 5,
        5, 4, 0,
        3, 2, 6,
        6, 7, 3,
        4, 5, 6,
        6, 7, 4,
        3, 0, 4,
        4, 7, 3,
        2, 1, 5,
        5, 6, 2
    };

    private List<RenderObject> _renderObjects = new ();
    private Shader _shader;
    private Stopwatch _timer;
    private float _AspectRatio = 1;
    private Camera _camera;

    public Game(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings) 
        : base(gameSettings, nativeSettings)
    {
        _camera = new Camera(Vector3.Zero, _AspectRatio);
        CursorVisible = false;
        CursorGrabbed = true;
    }

    public string log = "initial";
    public int logInt = 0;

    public void Log()
    {
        while (true)
        {
            Console.WriteLine(log + logInt);
            Thread.Sleep(250);
        }
    }
    protected override void OnLoad()
    {
        _timer = new Stopwatch();
        _timer.Start();
        GL.Enable(EnableCap.DepthTest);
        // GL.Enable(EnableCap.CullFace);
        GL.ClearColor(new Color4(0.2f,0.2f,1f,1f));
        
        _shader = new Shader("Shaders/texture.vert", "Shaders/texture.frag");
        var logThread = new Thread(Log);
        logThread.Start();
        
        FileStream vertFileStream = File.Create(@"Shapes\IcoSphere12\Vertices");
        FileStream indFileStream = File.Create(@"Shapes\IcoSphere12\Indices");
        
        var icosphere = new Icosphere(0, this);
        var icoVertices = icosphere.Vertices; 
        var icoIndices = icosphere.Indices;

        log = "Files Loaded";
        _renderObjects.Insert(0,new RenderObject(_vertices, _indices, _shader));
        for (int i = 0; i < 1; i++)
        {
            _renderObjects.Insert(i, new PhysicsObject(icoVertices, icoIndices, icosphere.IndexLookup, _shader, false, true));
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
        _camera.AspectRatio = (float)e.Width / (float)e.Height;
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
        if (!IsFocused)
        {
            return;
        }
        var input = KeyboardState;
        if (input.IsKeyDown(Keys.Escape))
        {
            Close();
            throw new Exception("closing program");
        }
        
        _camera.Move(input, (float)args.Time);
        
        base.OnUpdateFrame(args);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        var oscilator = (float)Sin(_timer.Elapsed.TotalSeconds);
        for (int i = 0; i < _renderObjects.Count; i++)
        {
            _renderObjects[i].Render(_camera.GetViewMatrix(), _camera.GetProjectionMatrix());
        }
        Context.SwapBuffers();
        
        base.OnRenderFrame(args);
    }
}
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static OpenTK.Mathematics.MathHelper;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TimboPhysics;

public class Game : GameWindow
{
    float[,] _vertices = {
        {0.5f,  0.5f, -0.5f,  1.0f, 1.0f}, //top right back     0
        {0.5f, -0.5f, -0.5f,  0.0f, 0.0f}, //bottom right back  1
        {-0.5f, -0.5f, -0.5f,  1.0f, 0.0f}, //bottom left back   2
        {-0.5f,  0.5f, -0.5f,  0.0f, 1.0f}, //top left back      3
        
        {0.5f,  0.5f,  0.5f,  1.0f, 0.0f}, //top right front    4
        {0.5f, -0.5f,  0.5f,  0.0f, 1.0f}, //bottom right front 5
        {-0.5f, -0.5f,  0.5f,  1.0f, 1.0f}, //bottom left front  6
        {-0.5f,  0.5f,  0.5f,  0.0f, 0.0f}, //top left front     7
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

    protected override void OnLoad()
    {
        _timer = new Stopwatch();
        _timer.Start();
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(new Color4(0.2f,0.2f,1f,1f));
        
        _shader = new Shader("Shaders/texture.vert", "Shaders/texture.frag");
        for (int i = 0; i < 50; i++)
        {
            var icosphere = new Icosphere(2);
            _renderObjects.Insert(i, new RenderObject(icosphere.Vertices.SelectMany(x=>x).ToArray(), icosphere.Indices.ToArray(), _shader));
            _renderObjects[i].Rotation = Quaternion.FromEulerAngles(
                RandomNumberGenerator.GetInt32(-90, 90),
                RandomNumberGenerator.GetInt32(-90, 90),
                RandomNumberGenerator.GetInt32(-90, 90));
            _renderObjects[i].Position = new Vector3(
                RandomNumberGenerator.GetInt32(-10, 10),
                RandomNumberGenerator.GetInt32(-10, 10),
                RandomNumberGenerator.GetInt32(-10, 10));
            foreach (var vertex in icosphere.Vertices)
            {
                foreach (var a in vertex)
                {
                    Console.WriteLine(a);
                }
            }
        }
        
        
        
        base.OnLoad();
    }

    protected override void OnUnload()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        _shader.Dispose();
        base.OnUnload();
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
        }
        
        _camera.Move(input, (float)args.Time);
        
        base.OnUpdateFrame(args);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        for (int i = 0; i < _renderObjects.Count; i++)
        {
            _renderObjects[i].Rotation *= Quaternion.FromEulerAngles(
                RandomNumberGenerator.GetInt32(0, 1)/100f,
                RandomNumberGenerator.GetInt32(0, 1)/100f,
                RandomNumberGenerator.GetInt32(0, 1)/100f);
            _renderObjects[i].Render(_camera.GetViewMatrix(), _camera.GetProjectionMatrix());
        }
        Context.SwapBuffers();
        
        base.OnRenderFrame(args);
    }
}
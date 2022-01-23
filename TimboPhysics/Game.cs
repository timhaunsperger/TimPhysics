using System;
using System.Diagnostics;
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
    private List<float[]> _vertices = new List<float[]>();
    float[] _vertices0 = {
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f, //top right back     0
         0.5f, -0.5f, -0.5f,  0.0f, 0.0f, //bottom right back  1
        -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, //bottom left back   2
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, //top left back      3
        
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f, //top right front    4
         0.5f, -0.5f,  0.5f,  0.0f, 1.0f, //bottom right front 5
        -0.5f, -0.5f,  0.5f,  1.0f, 1.0f, //bottom left front  6
        -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, //top left front     7
    };
    
    float[] _vertices1 = {
        0.5f,  0.5f, -0.5f,  1.0f, 1.0f, //top right back     0
        0.5f, -0.5f, -0.5f,  0.0f, 0.0f, //bottom right back  1
        0.5f,  0.5f,  0.5f,  1.0f, 0.0f, //top right front    4
        0.5f, -0.5f,  0.5f,  0.0f, 1.0f, //bottom right front 5
        
        -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, //bottom left back   2
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, //top left back      3
        -0.5f, -0.5f,  0.5f,  1.0f, 1.0f, //bottom left front  6
        -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, //top left front     7
    };
    
    float[] _vertices2 = {
        0.5f,  0.5f, -0.5f,  1.0f, 1.0f, //top right back     0
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, //top left back      3
        0.5f,  0.5f,  0.5f,  1.0f, 0.0f, //top right front    4
        -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, //top left front     7
        
         0.5f, -0.5f,  0.5f,  0.0f, 1.0f, //bottom right front 5
         0.5f, -0.5f, -0.5f,  0.0f, 0.0f, //bottom right back  1
        -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, //bottom left back   2
        -0.5f, -0.5f,  0.5f,  1.0f, 1.0f, //bottom left front  6
        
    };
    
    uint[] _indices = {  // note that we start from 0!
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

    private int[] _VBOs = new int[200];
    private int _EBO;
    private int _VAO;
    private Shader _shader;
    private Texture _texture0;
    private Texture _texture1;
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
        _vertices.Add(_vertices0);
        _timer = new Stopwatch();
        _timer.Start();
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(new Color4(0.2f,0.2f,1f,1f));

        _VAO = GL.GenVertexArray();
        GL.BindVertexArray(_VAO);

        GL.GenBuffers(1, _VBOs);

        for (int i = 0; i < _vertices.Count; i++)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VBOs[i]);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float)*_vertices[i].Length, _vertices[i], BufferUsageHint.StaticDraw);
        }
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        
        _EBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _EBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);
        
        _shader = new Shader("Shaders/texture.vert", "Shaders/texture.frag");
        _shader.Use();

        int aPositionLocation = _shader.GetAttribLocation("aPosition");
        GL.EnableVertexAttribArray(aPositionLocation);
        //GL.VertexAttribPointer(aPositionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        
        int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
        GL.EnableVertexAttribArray(texCoordLocation);
        //GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        
        _texture0 = new Texture("Textures/container.jpg");
        _texture1 = new Texture("Textures/garfield.png");
        _shader.SetInt("texture0", 0);
        _shader.SetInt("texture1", 1);

        base.OnLoad();
    }

    protected override void OnUnload()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        _VBOs.Select(x => { OpenTK.Graphics.OpenGL.GL.DeleteBuffer(x);
            return x;
        });
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
        GL.BindVertexArray(_VAO);

        float oscelator = (float)Sin(_timer.Elapsed.TotalSeconds)*2;
        
        Matrix4 model = Matrix4.CreateRotationX(DegreesToRadians(oscelator*10))*Matrix4.CreateRotationY(DegreesToRadians(oscelator*10));
        Matrix4 view = _camera.GetViewMatrix();
        Matrix4 projection = _camera.GetProjectionMatrix();
        
        _shader.SetMatrix4("model", model);
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);
        
        _texture0.Use(TextureUnit.Texture0);
        _texture1.Use(TextureUnit.Texture1);
        _shader.Use();
        
        GL.BindVertexBuffer(_shader.GetAttribLocation("aTexCoord"), _VBOs[0],new IntPtr(3*sizeof(float)), 5 * sizeof(float));
        for (int i = 0; i < _vertices.Count; i++)
        {
            GL.BindVertexBuffer(_shader.GetAttribLocation("aPosition"), _VBOs[i],IntPtr.Zero, 5 * sizeof(float));
            GL.DrawElements(PrimitiveType.LineLoop, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }
        
        
        Context.SwapBuffers();
        
        base.OnRenderFrame(args);
    }
}
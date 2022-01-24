using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TimboPhysics;

class RenderObject
{

    private readonly Shader _shader;
    private readonly float[] _vertices;
    private readonly uint[] _indices; 
    private int _VAO;
    private int _VBO;
    private int _EBO;
    private Texture _texture0;
    private Texture _texture1;
    
    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    
    public struct PhysicsVertex
    {
        public Vector3 position;
        public Vector3 speed;
        public float mass;
        public List<PhysicsVertex> connections;
    }
    
    public RenderObject(float[] vertices, uint[] indecies, Shader shader)
    {
        _vertices = vertices;
        _indices = indecies;
        _shader = shader;
        
        _VAO = GL.GenVertexArray();
        GL.BindVertexArray(_VAO);
        
        _VBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float)*_vertices.Length, _vertices, BufferUsageHint.StaticDraw);

        _EBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _EBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        int aPositionLocation = _shader.GetAttribLocation("aPosition");
        GL.EnableVertexAttribArray(aPositionLocation);
        GL.VertexAttribPointer(aPositionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

        int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
        GL.EnableVertexAttribArray(texCoordLocation);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        
        _shader.Use();
        _texture0 = new Texture("Textures/container.jpg");
        _texture1 = new Texture("Textures/garfield.png");
        _shader.SetInt("texture0", 0);
        _shader.SetInt("texture1", 1);
    }

    public void Render(Matrix4 view, Matrix4 projection)
    {
        GL.BindVertexArray(_VAO);
        
        Matrix4 model = Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
        _shader.SetMatrix4("model", model);
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);
        
        _texture0.Use(TextureUnit.Texture0);
        _texture1.Use(TextureUnit.Texture1);
        
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    }
}
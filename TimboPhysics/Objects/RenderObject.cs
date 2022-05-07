using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class RenderObject
{

    private readonly Shader _shader;
    protected double[][] _vertices;
    protected double[] _flattenedVertices;
    private readonly uint[] _indices;
    protected int _VAO;
    protected int _VBO;
    private int _EBO;
    private Texture _texture0;
    private Texture _texture1;

    public RenderObject(double[][] vertices, uint[] indecies, Shader shader)
    {
        _vertices = vertices;
        _flattenedVertices = vertices.SelectMany(x => x).ToArray();
        _indices = indecies;
        _shader = shader;
        
        _VAO = GL.GenVertexArray();
        GL.BindVertexArray(_VAO);
        
        _VBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(double)*_flattenedVertices.Length, _flattenedVertices, BufferUsageHint.DynamicDraw);

        _EBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _EBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        int aPositionLocation = _shader.GetAttribLocation("aPosition");
        GL.EnableVertexAttribArray(aPositionLocation);
        GL.VertexAttribPointer(aPositionLocation, 3, VertexAttribPointerType.Double, false, 8 * sizeof(double), 0);

        int aNormalLocation = _shader.GetAttribLocation("aNormal");
        GL.EnableVertexAttribArray(aNormalLocation);
        GL.VertexAttribPointer(aNormalLocation, 3, VertexAttribPointerType.Double, false, 8 * sizeof(double), 3 * sizeof(double));
        
        int aTexCoordsLocation = _shader.GetAttribLocation("aTexCoord");
        GL.EnableVertexAttribArray(aTexCoordsLocation);
        GL.VertexAttribPointer(aTexCoordsLocation, 2, VertexAttribPointerType.Double, false, 8 * sizeof(double), 6 * sizeof(double));
        
        _shader.Use();
        _texture0 = new Texture("Textures/container.jpg");
        _texture1 = new Texture("Textures/garfield.png");
        _shader.SetInt("texture0", 0);
        _shader.SetInt("texture1", 1);
    }
    
    public virtual void Render(Matrix4 view, Matrix4 projection, Vector3 viewPos)
    {
        GL.BindVertexArray(_VAO);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
        
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);
        _shader.SetVector3("viewPos", viewPos);
        _shader.Use();
        
        _texture0.Use(TextureUnit.Texture0);
        _texture1.Use(TextureUnit.Texture1);

        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    }
}
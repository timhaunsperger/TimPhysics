using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class Shader
{
    public int Handle;
    public Dictionary<string, int> _uniformLocations;

    public Shader(string vertexPath, string fragmentPath)
    {
        string VertexShaderSource;
        StreamReader vertReader = new StreamReader(vertexPath, Encoding.UTF8);
        VertexShaderSource = vertReader.ReadToEnd();
        
        string FragmentShaderSource;
        StreamReader fragReader = new StreamReader(fragmentPath, Encoding.UTF8);
        FragmentShaderSource = fragReader.ReadToEnd();
        
        var VertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(VertexShader, VertexShaderSource);
        GL.CompileShader(VertexShader);
        string infoLogVert = GL.GetShaderInfoLog(VertexShader);
        if (infoLogVert != System.String.Empty)
            System.Console.WriteLine(infoLogVert);
        
        var FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(FragmentShader, FragmentShaderSource);
        GL.CompileShader(FragmentShader);
        string infoLogFrag = GL.GetShaderInfoLog(FragmentShader);
        if (infoLogFrag != String.Empty)
            Console.WriteLine(infoLogFrag);
        
        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, VertexShader);
        GL.AttachShader(Handle, FragmentShader);
        GL.LinkProgram(Handle);
        
        GL.DetachShader(Handle, VertexShader);
        GL.DetachShader(Handle, FragmentShader);
        GL.DeleteShader(FragmentShader);
        GL.DeleteShader(VertexShader);
        
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
        _uniformLocations = new Dictionary<string, int>();
        for (var i = 0; i < numberOfUniforms; i++)
        {
            var key = GL.GetActiveUniform(Handle, i, out _, out _);
            var location = GL.GetUniformLocation(Handle, key);
            _uniformLocations.Add(key, location);
        }
    }
    
    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(Handle, attribName);
    }
    
    public void SetInt(string name, int data)
    {
        int location = GL.GetUniformLocation(Handle, name);

        GL.Uniform1(_uniformLocations[name], data);
    }
    
    public void SetFloat(string name, float data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(_uniformLocations[name], data);
    }
    
    public void SetVector3(string name, Vector3 data)
    {
        GL.UseProgram(Handle);
        GL.Uniform3(_uniformLocations[name], data);
    }
    
    public void SetMatrix4(string name, Matrix4 data)
    {
        GL.UseProgram(Handle);
        GL.UniformMatrix4(_uniformLocations[name], true, ref data);
    }
    
    public void Use()
    {
        GL.UseProgram(Handle);
    }
    
    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            GL.DeleteProgram(Handle);

            disposedValue = true;
        }
    }

    ~Shader()
    {
        GL.DeleteProgram(Handle);
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
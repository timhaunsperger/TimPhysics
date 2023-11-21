using System.IO.Enumeration;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsObject : RenderObject
{ 
    public double Radius;
    public Vector3d Position;
    public uint[][] Faces;  // Array of arrays storing which vertices are connected to form faces
    public Vector3d Velocity = Vector3d.Zero;
    public double Mass;
    
    protected PhysicsObject(Shape shape, Shader shader, double mass)
        : base(shape, shader)
    {
        var indices = shape.Indices;
        Position = shape.Center;
        Faces = new uint[indices.Length/3][];
        Mass = mass;
        
        for (int i = 0; i < indices.Length; i++)
        {
            if (i%3==2)
            {
                Faces[i/3] = new uint[3];
                Faces[i/3][0] = indices[i-2];
                Faces[i/3][1] = indices[i-1];
                Faces[i/3][2] = indices[i-0];
            }
        }
    }
    public virtual Vector3d[] GetVertices()
    {
        var vertices = new Vector3d[Vertices.Length];
        for (int i = 0; i < Vertices.Length; i++)
        {
            vertices[i] = new Vector3d(Vertices[i][0], Vertices[i][1], Vertices[i][2]);
        }
        return vertices;
        
    }
}
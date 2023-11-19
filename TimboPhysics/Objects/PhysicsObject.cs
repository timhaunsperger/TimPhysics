using System.IO.Enumeration;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsObject : RenderObject
{ 
    public double Radius;
    public Vector3d Position;
    public uint[][] Faces;  // Array of arrays storing which vertices are connected to form faces
    public Vector3d Velocity;
    public double Mass;
    
    protected PhysicsObject(Shape shape, Shader shader, double mass)
        : base(shape, shader)
    {
        var indices = shape.Indices;
        
        Faces = new uint[indices.Length/3][];
        Velocity = Vector3d.Zero;
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

    //Adds speed to position and calculates new max radius and center
    protected virtual void UpdateValues(double timeStep)
    {
        Radius = 0d;
        for (uint i = 0; i < Vertices.Length; i++)
        {
            var vertex = Vertices[i];
            vertex[0] += Velocity.X * timeStep;
            vertex[1] += Velocity.Y * timeStep;
            vertex[2] += Velocity.Z * timeStep;
            Vertices[i] = vertex;
        }
    }
    
    public virtual void Update(double deltaTime)
    {
        
    }
    
    public void Assign(double[][] vertices)
    {
        for (uint i = 0; i < vertices.Length; i++)
        {
            // Vertex can not have an actual normal, but center to vertex vector is close enough approximation
            var fakeNormal = (new Vector3d(vertices[i][0], vertices[i][1], vertices[i][0]) - Position).Normalized(); 
            vertices[i][3] = fakeNormal.X;
            vertices[i][4] = fakeNormal.Y;
            vertices[i][5] = fakeNormal.Z;
        }
        _flattenedVertices = vertices.SelectMany(x => x).ToArray();
    }
}
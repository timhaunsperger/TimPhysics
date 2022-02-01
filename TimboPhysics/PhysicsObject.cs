using System.IO.Enumeration;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TimboPhysics;

public class PhysicsObject : RenderObject
{
    private bool _gravity;
    private bool _collision;
    private Vector3d _center;
    private Dictionary<Vector3d, uint> _indexLookup;
    private Dictionary<uint, PhysicsVertex> _vertexLookup;
    private uint[][] _faces;  // Array of arrays storing which vertices are connected to form faces
    private double floor = -15f;
    private double _maxRadius;

    public struct PhysicsVertex
    {
        public Vector3d Position;
        public Vector3d Speed;
        public float Mass;

        public PhysicsVertex(Vector3d position, Vector3d speed, float mass)
        {
            Position = position;
            Speed = speed;
            Mass = mass;
        }
    }

    public PhysicsObject(double[][] vertices, uint[] indices, Dictionary<Vector3d, uint> indexLookup, Shader shader, bool collision, bool gravity) 
        : base(vertices, indices, shader)
    {
        _gravity = gravity;
        _collision = collision;
        _indexLookup = indexLookup;
        _vertexLookup = new Dictionary<uint, PhysicsVertex>();
        _faces = new uint[indices.Length/3][];
        
        for (int i = 0; i < indices.Length; i++)
        {
            if (!_vertexLookup.ContainsKey(indices[i]))
            {
                var vertexPos = new Vector3d(vertices[indices[i]][0], vertices[indices[i]][1], vertices[indices[i]][2]);
                _vertexLookup[indices[i]] = new PhysicsVertex(vertexPos, Vector3d.Zero, 1);
                _center += vertexPos;
            }

            if (i%3==2)
            {
                _faces[i/3] = new uint[3];
                _faces[i/3][0] = indices[i-2];
                _faces[i/3][1] = indices[i-1];
                _faces[i/3][2] = indices[i-0];
            }
        }

        _center /= _vertexLookup.Count;
    }

    private Dictionary<uint,PhysicsVertex> NextPositions(
        Dictionary<uint,PhysicsVertex> baseVertices, 
        Dictionary<uint,PhysicsVertex> outVertices, 
        List<PhysicsObject> collisionObjects, 
        double timeStep)
    {
        // Clone input dictionaries because dict is reference type
        baseVertices = new Dictionary<uint, PhysicsVertex>(baseVertices);
        outVertices = new Dictionary<uint, PhysicsVertex>(outVertices);
        
        // Find center of object by averaging all vertices 
        var centerPos = Vector3d.Zero;
        foreach (var vertexKey in baseVertices.Keys)
        {
            centerPos += baseVertices[vertexKey].Position/baseVertices.Count;
        }

        // Find volume of object by sum of volume of tetrahedrons of faces and centerPos
        var volume = 0d;
        for (int i = 0; i < _faces.Length; i++)
        {
            volume += TMathUtils.GetVolume(
                baseVertices[_faces[i][0]].Position,
                baseVertices[_faces[i][1]].Position,
                baseVertices[_faces[i][2]].Position,
                centerPos);
        }

        const double springConst = 4000;
        const double springOffset = 0.25;
        const double dampingFactor = 2;
        const double pressure = 4000;
        const double gravity = 0.5;

        foreach (var face in _faces)
        {
            PhysicsVertex[] faceBaseVertices = {baseVertices[face[0]], baseVertices[face[1]], baseVertices[face[2]]};
            PhysicsVertex[] faceOutVertices = {outVertices[face[0]], outVertices[face[1]], outVertices[face[2]]};

            // Important values for calculating forces.
            var faceNormal = TMathUtils.GetNormal(
                faceBaseVertices[0].Position, 
                faceBaseVertices[1].Position, 
                faceBaseVertices[2].Position);

            var faceArea = TMathUtils.GetArea(
                faceBaseVertices[0].Position, 
                faceBaseVertices[1].Position, 
                faceBaseVertices[2].Position);

            for (uint i = 0; i < face.Length; i++)
            {
                //Apply Pressure Force
                faceOutVertices[i].Speed += faceNormal * faceArea / volume * pressure * timeStep;
                //Apply Spring Force
                var springVector = faceBaseVertices[(i + 1) % 3].Position - faceBaseVertices[i].Position;
                faceOutVertices[i].Speed += springVector.Normalized() * (springVector.Length - springOffset) * springConst * timeStep;
                faceOutVertices[(i + 1) % 3].Speed += springVector.Normalized() * (springVector.Length - springOffset) * springConst * timeStep * -1;
                
                //Apply Damping Force
                var relSpeed = faceBaseVertices[i].Speed - faceBaseVertices[(i + 1) % 3].Speed;
                faceOutVertices[i].Speed += (relSpeed * -dampingFactor * timeStep);
                faceOutVertices[(i + 1) % 3].Speed += (relSpeed * -dampingFactor * timeStep * -1);
                
                //Apply Gravity
                if (_gravity)
                {
                    faceOutVertices[i].Speed -= Vector3d.UnitY * gravity * timeStep;

                    var x2 = faceBaseVertices[i].Position.X * faceBaseVertices[i].Position.X / 4;
                    var z2 = faceBaseVertices[i].Position.Z * faceBaseVertices[i].Position.Z / 4;
                    faceOutVertices[i].Speed += (Vector3d.UnitY * -15 - faceBaseVertices[i].Position) * timeStep * 0.05f;
                    if (faceBaseVertices[i].Position.Y < floor)  // Floor collision
                    {
                        faceOutVertices[i].Position.Y = floor;
                        if (faceBaseVertices[i].Speed.Y < 0)
                        {
                            faceOutVertices[i].Speed.Y = 0;
                        }
                    }
                }
            }
            //Apply Changes
            outVertices[face[0]] = faceOutVertices[0];
            outVertices[face[1]] = faceOutVertices[1];
            outVertices[face[2]] = faceOutVertices[2];
        }
        
        //Collision
        foreach (var collisionObject in collisionObjects)
        {
            if (collisionObject != this && (collisionObject._center-_center).Length < collisionObject._maxRadius + _maxRadius)
            {
                foreach (var vertex in baseVertices.Keys)
                {
                    if ((collisionObject._center-baseVertices[vertex].Position).Length > collisionObject._maxRadius)
                    {
                        continue;
                    }
                    var isColliding = true;
                    var closestFaceDist = Double.PositiveInfinity;
                    var closestFace = Array.Empty<uint>();
                    foreach (var face in collisionObject._faces)
                    {
                        var distance = TMathUtils.PointPlaneDist(
                            collisionObject._vertexLookup[face[0]].Position,
                            collisionObject._vertexLookup[face[1]].Position,
                            collisionObject._vertexLookup[face[2]].Position,
                            baseVertices[vertex].Position);
                        if (distance < 0)
                        {
                            isColliding = false;
                            break;
                        }

                        if (distance < closestFaceDist)
                        {
                            closestFaceDist = distance;
                            closestFace = face;
                        }
                        
                    }
                    if (isColliding)
                    {
                        var forceVertex = outVertices[vertex];
                        var forceVector = _center - forceVertex.Position;
                        for (int i = 0; i < 3; i++)
                        {
                            var faceVertex = collisionObject._vertexLookup[closestFace[i]];
                            faceVertex.Position -= forceVector * closestFaceDist/2;
                            faceVertex.Speed -= 2*Vector3d.Dot(forceVector, faceVertex.Speed.Normalized()) * forceVector;
                            collisionObject._vertexLookup[closestFace[i]] = faceVertex;
                        }
                        forceVertex.Position += forceVector * closestFaceDist/2;
                        forceVertex.Speed -= 2*Vector3d.Dot(forceVector, forceVertex.Speed.Normalized()) * forceVector;
                        outVertices[vertex] = forceVertex;
                    }
                }
            }
        }

        
        _center = Vector3d.Zero;
        _maxRadius = 0d;
        for (uint i = 0; i < baseVertices.Count; i++)
        {
            var vertex = outVertices[i];
            vertex.Position += vertex.Speed * timeStep;
            outVertices[i] = vertex;
        }
        for (uint i = 0; i < baseVertices.Count; i++)
        {
            var vertex = outVertices[i];
            _center += vertex.Position;
        }
        _center /= baseVertices.Count;
        for (uint i = 0; i < baseVertices.Count; i++)
        {
            var vertex = outVertices[i];
            if (_maxRadius < (vertex.Position-_center).Length)
            {
                _maxRadius = (vertex.Position - _center).Length;
            }
        }
        return outVertices;
    }

    public void UpdateVertices()
    {
        for (int i = 0; i < _vertices.GetLength(0); i++)
        {
            var oldPos = new Vector3d(_vertices[i][0], _vertices[i][1], _vertices[i][2]);
            var index = _indexLookup[oldPos];
            var newPos = _vertexLookup[index].Position;
            _indexLookup.Remove(oldPos);
            _indexLookup[newPos] = index;
            _vertices[i] = new[] {newPos.X, newPos.Y, newPos.Z, _vertices[i][3], _vertices[i][4]};
        }
        _flattenedVertices = _vertices.SelectMany(x => x).ToArray();
    }

    public Dictionary<uint, PhysicsVertex> RK4Integrate(Dictionary<uint, PhysicsVertex> vertices, List<PhysicsObject> collisionObjects, double timeStep)
    {
        var k1 = NextPositions(vertices, vertices, collisionObjects, timeStep);
        var k1mid = NextPositions(vertices, vertices, collisionObjects, timeStep/2);
        var k2 = NextPositions(k1mid, vertices, collisionObjects, timeStep);
        var k2mid = NextPositions(k1mid, vertices, collisionObjects, timeStep/2);
        var k3 = NextPositions(k2mid, vertices, collisionObjects, timeStep);
        var k4 = NextPositions(k3, vertices, collisionObjects, timeStep);

        var result = new Dictionary<uint, PhysicsVertex>();

        foreach (var key in vertices.Keys)
        {
            var entry = vertices[key];
            entry.Position = 
                (k1[key].Position + 2 * k2[key].Position + 2 * k3[key].Position + k4[key].Position)/6;
            entry.Speed = 
                (k1[key].Speed + 2 * k2[key].Speed + 2 * k3[key].Speed + k4[key].Speed)/6;
            result[key] = entry;
        }

        return result;
    }
    

    public override void Update(List<PhysicsObject> collisionObjects, double deltaTime)
    {
        //_vertexLookup = RK4Integrate(_vertexLookup,collisionObjects, 0.0005);
        _vertexLookup = NextPositions(_vertexLookup,  _vertexLookup, collisionObjects, 0.005);
        base.Update(collisionObjects, deltaTime);
    }

    public override void Render(Matrix4 view, Matrix4 projection)
    {
        UpdateVertices();
        GL.BindVertexArray(_VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _flattenedVertices.Length * sizeof(double), _flattenedVertices);
        base.Render(view, projection);
    }
}
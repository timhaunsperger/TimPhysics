using OpenTK.Mathematics;

namespace TimboPhysics;

public class Softbody : PhysicsObject
{
    private bool _gravity;
    private double floor = -15f;

    public Softbody(double[][] vertices, uint[] indices, Shader shader, bool collision, bool gravity) 
        : base(vertices, indices, shader, collision)
    {
        _gravity = gravity;
        _collision = collision;
        _vertexLookup = new Dictionary<uint, PhysicsVertex>();
        _faces = new uint[indices.Length/3][];
        
        for (int i = 0; i < indices.Length; i++)
        {
            if (!_vertexLookup.ContainsKey(indices[i]))
            {
                var vertexPos = new Vector3d(vertices[indices[i]][0], vertices[indices[i]][1], vertices[indices[i]][2]);
                _vertexLookup[indices[i]] = new PhysicsVertex(vertexPos, Vector3d.Zero);
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
        Dictionary<uint,PhysicsVertex> Vertices, List<PhysicsObject> collisionObjects, double timeStep)
    {
        // Clone input dictionary because dict is reference type
        Vertices = new Dictionary<uint, PhysicsVertex>(Vertices);

        // Find volume of object by sum of volume of tetrahedrons of faces and centerPos
        var volume = 0d;
        for (int i = 0; i < _faces.Length; i++)
        {
            volume += TMathUtils.GetVolume(
                Vertices[_faces[i][0]].Position,
                Vertices[_faces[i][1]].Position,
                Vertices[_faces[i][2]].Position,
                _center);
        }

        const double springConst = 2000;
        const double springOffset = 0.25;
        const double dampingFactor = 2;
        const double pressure = 4000;
        const double gravity = 0.5;
        const double attraction = 0;

        foreach (var face in _faces)
        {
            PhysicsVertex[] faceVertices = {Vertices[face[0]], Vertices[face[1]], Vertices[face[2]]};

            // Important values for calculating forces.
            var faceNormal = TMathUtils.GetNormal(
                faceVertices[0].Position, 
                faceVertices[1].Position, 
                faceVertices[2].Position);

            var faceArea = TMathUtils.GetArea(faceVertices[0].Position, faceVertices[1].Position, faceVertices[2].Position);

            for (uint i = 0; i < face.Length; i++)
            {
                //Apply Pressure Force
                faceVertices[i].Speed += faceNormal * faceArea / volume * pressure * timeStep;
                //Apply Spring Force
                var springVector = faceVertices[(i + 1) % 3].Position - faceVertices[i].Position;
                faceVertices[i].Speed += springVector.Normalized() * (springVector.Length - springOffset) * springConst * timeStep;
                faceVertices[(i + 1) % 3].Speed += springVector.Normalized() * (springVector.Length - springOffset) * springConst * timeStep * -1;
                
                //Apply Damping Force
                var relSpeed = faceVertices[i].Speed - faceVertices[(i + 1) % 3].Speed;
                faceVertices[i].Speed += (relSpeed * -dampingFactor * timeStep);
                faceVertices[(i + 1) % 3].Speed += (relSpeed * -dampingFactor * timeStep * -1);
                
                //Apply Gravity
                if (_gravity)
                {
                    faceVertices[i].Speed -= Vector3d.UnitY * gravity * timeStep;
                    faceVertices[i].Speed += (Vector3d.UnitY * -15 - faceVertices[i].Position) * timeStep * attraction;
                    
                    // if (faceVertices[i].Position.Y < floor)  // Floor collision
                    // {
                    //     faceVertices[i].Position.Y = floor;
                    //     if (faceVertices[i].Speed.Y < 0)
                    //     {
                    //         faceVertices[i].Speed.Y = 0;
                    //         faceVertices[i].Speed *= 0.98;
                    //     }
                    // }
                }
            }
            //Apply Changes
            Vertices[face[0]] = faceVertices[0];
            Vertices[face[1]] = faceVertices[1];
            Vertices[face[2]] = faceVertices[2];
        }
        
        //Collision
        foreach (var collisionObject in collisionObjects)
        {
            if (collisionObject != this && (collisionObject._center-_center).Length < collisionObject._maxRadius + _maxRadius)
            {
                Collision(collisionObject, Vertices);
            }
        }

        UpdateValues(Vertices, timeStep);
        
        return Vertices;
    }
    public override void Update(List<PhysicsObject> collisionObjects, double deltaTime)
    {
        _vertexLookup = NextPositions(_vertexLookup, collisionObjects, 0.005);
        base.Update(collisionObjects, deltaTime);
    }
}
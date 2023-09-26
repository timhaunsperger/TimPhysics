using OpenTK.Mathematics;

namespace TimboPhysics;

public class Softbody : PhysicsObject
{
    private bool _gravity;

    public Softbody(Shape shape, Shader shader, bool gravity) 
        : base(shape, shader)
    {
        _gravity = gravity;
        IsCenterStatic = false;
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
                Center);
        }

        const double springConst = 1000;
        const double springOffset = 0.1;
        const double dampingFactor = 0.5;
        const double pressure = 3000;
        const double gravity = 1;

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
                faceVertices[i].Speed += relSpeed * -dampingFactor * timeStep;
                faceVertices[(i + 1) % 3].Speed += relSpeed * -dampingFactor * timeStep * -1;
            }
            for (uint i = 0; i < face.Length; i++)
            {
                //Apply Gravity
                if (_gravity)
                {
                    faceVertices[i].Speed -= Vector3d.UnitY * gravity * timeStep;
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
            if (collisionObject != this && (collisionObject.Center-Center).Length < collisionObject.Radius + Radius)
            {
                Collision(collisionObject, Vertices);
            }
        }

        UpdateValues(Vertices, timeStep);
        
        return Vertices;
    }
    public override void Update(List<PhysicsObject> collisionObjects, double deltaTime)
    {
        _vertexLookup = NextPositions(_vertexLookup, collisionObjects, 0.01);
        base.Update(collisionObjects, 0.01);
        
    }
}
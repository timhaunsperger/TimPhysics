using OpenTK.Mathematics;

namespace TimboPhysics;

public class Staticbody : PhysicsObject
{
    public Staticbody(double[][] vertices, uint[] indices, Shader shader, bool collision) 
        : base(vertices, indices, shader, collision)
    {
        _collision = collision;
        _vertexLookup = new Dictionary<uint, PhysicsVertex>();
        _faces = new uint[indices.Length/3][];
        
        for (int i = 0; i < indices.Length; i++)
        {
            if (!_vertexLookup.ContainsKey(indices[i]))
            {
                var vertexPos = new Vector3d(vertices[indices[i]][0], vertices[indices[i]][1], vertices[indices[i]][2]);
                _vertexLookup[indices[i]] = new PhysicsVertex(vertexPos, Vector3d.Zero);
            }

            if (i%3==2)
            {
                _faces[i/3] = new uint[3];
                _faces[i/3][0] = indices[i-2];
                _faces[i/3][1] = indices[i-1];
                _faces[i/3][2] = indices[i-0];
            }
        }
        UpdateValues(_vertexLookup, 0);
    }
    public override void Update(List<PhysicsObject> collisionObjects, double deltaTime)
    {
        // for (int i = 0; i < _faces.Length; i++)
        // {
        //     var normal = TMathUtils.GetNormal(
        //         _vertexLookup[_faces[i][0]].Position, _vertexLookup[_faces[i][1]].Position, _vertexLookup[_faces[i][2]].Position);
        //     for (int j = 0; j < 3; j++)
        //     {
        //         var vertex = _vertexLookup[_faces[i][j]];
        //         vertex.Position += normal / 200;
        //         _vertexLookup[_faces[i][j]] = vertex;
        //     }
        //     
        // }
    }
}
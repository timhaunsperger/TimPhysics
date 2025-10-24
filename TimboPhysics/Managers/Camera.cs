using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TimboPhysics;

public class Camera
{
    public Vector3 Front = -Vector3.UnitZ;
    public Vector3 Up = Vector3.UnitY;
    public Vector3 Right = Vector3.UnitX;
    private float _pitch;
    private float _yaw = -MathHelper.PiOver2;
    private float _fov = MathHelper.PiOver2;

    public float Speed = 1;
    public Vector3 Position;
    public float AspectRatio;

    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set
        {
            var angle = MathHelper.Clamp(value, -89f, 89f);
            _pitch = MathHelper.DegreesToRadians(angle);
            UpdateVectors();
        }
    }
    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(_yaw);
        set
        {
            _yaw = MathHelper.DegreesToRadians(value);
            UpdateVectors();
        }
    }
    public float Fov
    {
        get => MathHelper.RadiansToDegrees(_fov);
        set
        {
            var angle = MathHelper.Clamp(value, 1f, 45f);
            _fov = MathHelper.DegreesToRadians(angle);
        }
    }
    
    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + Front, Up);
    }
    
    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 1000f);
    }

    public void MouseMove(Vector2 delta)
    {
        Yaw += delta.X/2;
        Pitch -= delta.Y/2;
    }
    
    public void Move(KeyboardState input, float deltaTime)
    {
        Position -= input.IsKeyDown(Keys.W)? Vector3.Zero : Front * Speed * deltaTime/2;
        Position += input.IsKeyDown(Keys.S)? Vector3.Zero : Front * Speed * deltaTime/2;
        Position -= input.IsKeyDown(Keys.D)? Vector3.Zero : Vector3.Normalize(Vector3.Cross(Front, Up)) * Speed * deltaTime/2;
        Position += input.IsKeyDown(Keys.A)? Vector3.Zero : Vector3.Normalize(Vector3.Cross(Front, Up)) * Speed * deltaTime/2;
        Position -= input.IsKeyDown(Keys.Space) ? Vector3.Zero : Vector3.UnitY * Speed * deltaTime / 2;
        Position += input.IsKeyDown(Keys.LeftShift)? Vector3.Zero : Vector3.UnitY * Speed * deltaTime/2;
    }
    
    private void UpdateVectors()
    {
        Front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
        Front.Y = MathF.Sin(_pitch);
        Front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
        Front = Vector3.Normalize(Front);
        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY)); 
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }
    
    public Camera(Vector3 position, float aspectRatio)
    {
        Position = position;
        AspectRatio = aspectRatio;
    }
}
using System.Numerics;
using static LearnOpenGL.Common.GLMaths;

namespace LearnOpenGL;

internal class Camera
{
    public Camera()
    {
        UpdateCameraVectors();
    }

    public Camera(Vector3 position)
    {
        Position = position;
        UpdateCameraVectors();
    }

    public Camera(Vector3 position, Vector3 worldUp)
    {
        Position = position;
        WorldUp  = worldUp;
        UpdateCameraVectors();
    }

    public Vector3 Position { get; private set; } = new(0.0f, 0.0f, 0.0f);
    public Vector3 Up { get; private set; }
    public Vector3 Front { get; private set; } = new(0.0f, 0.0f, -1.0f);
    public Vector3 Right { get; private set; }

    public Vector3 WorldUp { get; private set; } = new(0.0f, 1.0f, 0.0f);

    public float Pitch { get; private set; } = 0.0f;
    public float Yaw { get; private set; } = -90.0f;
    public float Zoom { get; private set; } = 45.0f;

    public float MovementSpeed { get; set; } = 2.5f;
    public float MouseSensitivity { get; set; } = 0.1f;

    public Matrix4x4 ViewMatrix => LookAt();//Matrix4x4.CreateLookAt(Position, Position + Front, Up);

    private Matrix4x4 LookAt()
    {
        var cameraTarget = Position + Front;

        var direction = Vector3.Normalize(Position - cameraTarget);
        var right = Vector3.Normalize(Vector3.Cross(Vector3.Normalize(Up), direction));
        var up = Vector3.Cross(direction, right);

        Matrix4x4 rotation = Matrix4x4.Identity;
        rotation[0,0] = right.X;
        rotation[0,1] = right.Y;
        rotation[0,2] = right.Z;
        rotation[1,0] = up.X;
        rotation[1,1] = up.Y;
        rotation[1,2] = up.Z;
        rotation[2,0] = direction.X;
        rotation[2,1] = direction.Y;
        rotation[2,2] = direction.Z;

        Matrix4x4 translation = Matrix4x4.Identity;
        translation[3,0] = -Position.X;
        translation[3,1] = -Position.Y;
        translation[3,2] = -Position.Z;

        return rotation * translation;
    }

    public void MoveForward(float dt) => Position += Front * (MovementSpeed * dt);
    public void MoveBackward(float dt) => Position -= Front * (MovementSpeed * dt);
    public void MoveLeft(float dt) => Position -= Vector3.Normalize(Vector3.Cross(Position, Up)) * (MovementSpeed * dt);
    public void MoveRight(float dt) => Position += Vector3.Normalize(Vector3.Cross(Position, Up)) * (MovementSpeed * dt);

    public void ProcessMouseMovement(Vector2 movement, bool constrainPitch = true)
    {
        Yaw += movement.X * MouseSensitivity;
        Pitch += movement.Y * MouseSensitivity;
        if (constrainPitch)
        {
            Pitch = Constrain(Pitch, -89.0f, 89.0f);
        }

        UpdateCameraVectors();
    }

    public void ProcessZoom(float offset)
    {
        Zoom = Constrain(Zoom - offset, 1.0f, 45.0f);
    }

    private void UpdateCameraVectors()
    {
        Vector3 front = new()
        {
            X = MathF.Cos(Radians(Yaw)) * MathF.Cos(Radians(Pitch)),
            Y = MathF.Sin(Radians(Pitch)),
            Z = MathF.Sin(Radians(Yaw)) * MathF.Cos(Radians(Pitch))
        };
        Front = Vector3.Normalize(front);
        Right = Vector3.Normalize(Vector3.Cross(Front, WorldUp));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }

    private static float Constrain(float value, float minValue, float maxValue)
    {
        if (value < minValue)
        {
            return minValue;
        }
        if (value > maxValue)
        {
            return maxValue;
        }

        return value;
    }
}

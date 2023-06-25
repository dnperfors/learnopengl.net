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

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Position + Front, Up);

    public void MoveForward(float dt) => Position += Front * (MovementSpeed * dt);
    public void MoveBackward(float dt) => Position -= Front * (MovementSpeed * dt);
    public void MoveLeft(float dt) => Position -= Vector3.Normalize(Vector3.Cross(Position, Up)) * (MovementSpeed * dt);
    public void MoveRight(float dt) => Position += Vector3.Normalize(Vector3.Cross(Position, Up)) * (MovementSpeed * dt);

    public void ProcessMouseMovement(Vector2 movement, bool constrainPitch = true)
    {
        Yaw += (movement.X * MouseSensitivity);
        Pitch += (movement.Y * MouseSensitivity);
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

    private float Constrain(float value, float minValue, float maxValue)
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

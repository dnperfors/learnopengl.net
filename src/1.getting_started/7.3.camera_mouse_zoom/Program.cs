using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

using StbImageSharp;
using System.Numerics;
using Shader = LearnOpenGL.Common.Shader;
using static LearnOpenGL.Common.GLMaths;
using Silk.NET.GLFW;
using Silk.NET.Maths;

StbImage.stbi_set_flip_vertically_on_load(1);

int width = 800;
int height = 600;

var options = WindowOptions.Default with
{
    Size = new(width, height),
    Title = "Learn OpenGL",
    PreferredDepthBufferBits = 16
};

IWindow window = Window.Create(options);
GL? gl = null;

uint? vao = null;
Shader? shaderProgram = null;
uint[] textures = new uint[2];

Vector3[] cubePositions = new[]
{
    new Vector3( 0.0f,  0.0f,   0.0f),
    new Vector3( 2.0f,  5.0f, -15.0f),
    new Vector3(-1.5f, -2.2f,  -2.5f),
    new Vector3(-3.8f, -2.0f, -12.3f),
    new Vector3( 2.4f, -0.4f,  -3.5f),
    new Vector3(-1.7f,  3.0f,  -7.5f),
    new Vector3( 1.3f, -2.0f,  -2.5f),
    new Vector3( 1.5f,  2.0f,  -2.5f),
    new Vector3( 1.5f,  0.2f,  -1.5f),
    new Vector3(-1.3f,  1.0f,  -1.5f),
};

Vector2 lastMousePosition = default;
float pitch = 0.0f;
float yaw = -90.0f;
float fov = 45.0f;

Vector3 cameraPos = new(0.0f, 0.0f, 3.0f);
Vector3 cameraFront = new(0.0f, 0.0f, -1.0f);
Vector3 cameraUp = new(0.0f, 1.0f, 0.0f);

IKeyboard? keyboard = null;

window.Load += OnLoad;
window.Update += OnUpdate;
window.Render += OnRender;
window.Resize += OnResize;

void OnResize(Vector2D<int> d)
{
    if (gl is null) return;
    gl.Viewport(0, 0, (uint)d.X, (uint)d.Y);
}

window.Run();


unsafe void OnLoad()
{
    var input = window.CreateInput();
    keyboard = input.Keyboards[0];
    if (keyboard != null)
    {
        keyboard.KeyDown += OnKeyDown;
    }

    foreach (var mouse in input.Mice)
    {
        mouse.MouseMove += OnMouseMove;
        mouse.Scroll += OnMouseScroll;
    }

    gl = window.CreateOpenGL();

    shaderProgram = new Shader(gl, "shader.vector", "shader.fragment");

    float[] vertices = new[] {
    -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
     0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
    -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
    -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

    -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
    -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
     0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
     0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
     0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
     0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

    -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
    -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
    };

    vao = gl.GenVertexArrays(1);
    uint vbo = gl.GenBuffers(1);

    gl.BindVertexArray(vao.Value);
    gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
    fixed (float* buf = vertices)
    {
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
    }

    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 5 * sizeof(float), (void*)0);
    gl.EnableVertexAttribArray(0);

    gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
    gl.EnableVertexAttribArray(1);

    gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    gl.BindVertexArray(0);

    // uncomment this call to draw in wireframe polygons.
    //gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);

    textures[0] = gl.GenTexture();
    textures[1] = gl.GenTexture();

    gl.BindTexture(GLEnum.Texture2D, textures[0]);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

    var image1 = ImageResult.FromStream(File.Open("container.jpg", FileMode.Open, FileAccess.Read));
    fixed (byte* buffer = image1.Data)
    {
        gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgb, (uint)image1.Width, (uint)image1.Height, 0, GLEnum.Rgb, GLEnum.UnsignedByte, buffer);
    }
    gl.GenerateMipmap(GLEnum.Texture2D);

    gl.BindTexture(GLEnum.Texture2D, textures[1]);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

    var image2 = ImageResult.FromStream(File.Open("awesomeface.png", FileMode.Open, FileAccess.Read));
    fixed (byte* buffer = image2.Data)
    {
        gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba, (uint)image2.Width, (uint)image2.Height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, buffer);
    }
    gl.GenerateMipmap(GLEnum.Texture2D);

    shaderProgram.Use();
    shaderProgram.Set("texture1", 0);
    shaderProgram.Set("texture2", 1);
}

void OnMouseScroll(IMouse mouse, ScrollWheel wheel)
{
    fov -= wheel.Y;
    if (fov < 1.0f)
    {
        fov = 1.0f;
    }
    if (fov > 45.0f)
    {
        fov = 45.0f;
    }
}

void OnMouseMove(IMouse mouse, Vector2 mousePosition)
{
    if (lastMousePosition == default)
    {
        lastMousePosition = mousePosition;
    }

    var xOffset = mousePosition.X - lastMousePosition.X;
    var yOffset = mousePosition.Y - lastMousePosition.Y;
    lastMousePosition = mousePosition;

    float sensitive = 0.1f;
    xOffset *= sensitive;
    yOffset *= sensitive;

    yaw += xOffset;
    pitch += yOffset;

    if (pitch > 89.0f)
    {
        pitch = 89.0f;
    }
    if (pitch < -89.0f)
    {
        pitch = -89.0f;
    }

    Vector3 direction = new()
    {
        X = MathF.Cos(Radians(yaw)) * MathF.Cos(Radians(pitch)),
        Y = MathF.Sin(Radians(pitch)),
        Z = MathF.Sin(Radians(yaw) * MathF.Cos(Radians(pitch)))
    };
    cameraFront = Vector3.Normalize(direction);
}

void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
{
    if (key == Key.Escape)
    {
        window.Close();
    }
}

unsafe void OnUpdate(double dt)
{
    float cameraSpeed = 2.5f * (float)dt;
    
    if (keyboard is null) return;
    if (keyboard.IsKeyPressed(Key.W))
    {
        cameraPos += cameraSpeed * cameraFront;
    }
    if (keyboard.IsKeyPressed(Key.S))
    {
        cameraPos -= cameraSpeed * cameraFront;
    }

    if (keyboard.IsKeyPressed(Key.A))
    {
        cameraPos -= Vector3.Normalize(Vector3.Cross(cameraPos, cameraUp)) * cameraSpeed;
    }
    if (keyboard.IsKeyPressed(Key.D))
    {
        cameraPos += Vector3.Normalize(Vector3.Cross(cameraPos, cameraUp)) * cameraSpeed;
    }
}

unsafe void OnRender(double dt)
{
    if (gl is null || shaderProgram is null || vao is null || textures is null) return;

    gl.Enable(EnableCap.DepthTest);
    gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

    gl.ActiveTexture(GLEnum.Texture0);
    gl.BindTexture(GLEnum.Texture2D, textures[0]);
    gl.ActiveTexture(GLEnum.Texture1);
    gl.BindTexture(GLEnum.Texture2D, textures[1]);

    Matrix4x4 view = Matrix4x4.CreateLookAt(
        cameraPos,
        cameraPos + cameraFront,
        cameraUp);
    Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(Radians(fov), (float)width / (float)height, 0.1f, 100.0f);

    shaderProgram.Use();
    shaderProgram.Set("view", view);
    shaderProgram.Set("projection", projection);

    gl.BindVertexArray(vao.Value);
    for (int i = 0; i < 10; i++)
    {
        float angle = Radians(20.0f * (i + 1.0f));

        Matrix4x4 model = Matrix4x4.Identity;
        model *= Matrix4x4.CreateTranslation(cubePositions[i]);
        model *= Matrix4x4.CreateRotationZ(angle * 0.5f);
        model *= Matrix4x4.CreateRotationY(angle * 0.3f);
        model *= Matrix4x4.CreateRotationX(angle);

        shaderProgram.Set("model", model);

        gl.DrawArrays(GLEnum.Triangles, 0, 36);
    }
}

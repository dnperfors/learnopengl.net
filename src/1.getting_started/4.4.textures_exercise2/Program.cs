using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

using StbImageSharp;

using Shader = LearnOpenGL.Common.Shader;

StbImage.stbi_set_flip_vertically_on_load(1);

var options = WindowOptions.Default with
{
    Size = new Vector2D<int>(800, 600),
    Title = "Learn OpenGL"
};

IWindow window = Window.Create(options);
GL? gl = null;

uint? vao = null;
Shader? shaderProgram = null;
uint[] textures = new uint[2];

window.Load += OnLoad;
window.Update += OnUpdate;
window.Render += OnRender;

window.Run();

unsafe void OnLoad()
{
    var input = window.CreateInput();
    foreach (var keyboard in input.Keyboards)
    {
        keyboard.KeyDown += OnKeyDown;
    }
    gl = window.CreateOpenGL();

    shaderProgram = new Shader(gl, "shader.vector", "shader.fragment");

    float[] vertices = new[] {
        // positions          // colors           // texture coords
         0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   2.0f, 2.0f,   // top right
         0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   2.0f, 0.0f,   // bottom right
        -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 0.0f,   // bottom left
        -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 2.0f    // top left 
    };

    uint[] indices = new uint[]
    {
        0, 1, 3,
        1, 2, 3,
    };

    vao = gl.GenVertexArrays(1);
    uint vbo = gl.GenBuffers(1);
    uint ebo = gl.GenBuffer();

    gl.BindVertexArray(vao.Value);
    gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
    fixed (float* buf = vertices)
    {
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
    }

    gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
    fixed (uint* buf = indices)
    {
        gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
    }

    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)0);
    gl.EnableVertexAttribArray(0);

    gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
    gl.EnableVertexAttribArray(1);

    gl.VertexAttribPointer(2, 2, GLEnum.Float, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
    gl.EnableVertexAttribArray(2);

    gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    gl.BindVertexArray(0);

    // uncomment this call to draw in wireframe polygons.
    //gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);


    textures[0] = gl.GenTexture();
    textures[1] = gl.GenTexture();

    gl.BindTexture(GLEnum.Texture2D, textures[0]);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

    var image1 = ImageResult.FromStream(File.Open("container.jpg", FileMode.Open, FileAccess.Read));
    fixed (byte* buffer = image1.Data)
    {
        gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgb, (uint)image1.Width, (uint)image1.Height, 0, GLEnum.Rgb, GLEnum.UnsignedByte, buffer);
    }
    gl.GenerateMipmap(GLEnum.Texture2D);

    gl.BindTexture(GLEnum.Texture2D, textures[1]);
    gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
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

void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
{
    if (key == Key.Escape)
    {
        window.Close();
    }
}

void OnUpdate(double dt)
{
}

unsafe void OnRender(double dt)
{
    if (gl is null || shaderProgram is null || vao is null || textures is null) return;

    gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    gl.Clear(ClearBufferMask.ColorBufferBit);

    gl.ActiveTexture(GLEnum.Texture0);
    gl.BindTexture(GLEnum.Texture2D, textures[0]);
    gl.ActiveTexture(GLEnum.Texture1);
    gl.BindTexture(GLEnum.Texture2D, textures[1]);

    shaderProgram.Use();
    gl.BindVertexArray(vao.Value);
    gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, null);
}

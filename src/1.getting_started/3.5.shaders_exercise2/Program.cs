using LearnOpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

var options = WindowOptions.Default with
{
    Size = new Vector2D<int>(800, 600),
    Title = "Learn OpenGL"
};

IWindow window = Window.Create(options);
GL? gl = null;

uint? vao = null;
ShaderProgram? shaderProgram = null;

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

    shaderProgram = new ShaderProgram(gl, "shader.vector", "shader.fragment");

    float[] vertices = new[]
    {
         0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, // bottom right
        -0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, // bottom left
         0.0f,  0.5f, 0.0f, 0.0f, 0.0f, 1.0f,   // top 
    };

    vao = gl.GenVertexArrays(1);
    uint vbo = gl.GenBuffers(1);
    
    gl.BindVertexArray(vao.Value);
    gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
    fixed (float* buf = vertices)
    {
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
    }
        
    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)0);
    gl.EnableVertexAttribArray(0);

    gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)(3* sizeof(float)));
    gl.EnableVertexAttribArray(1);

    gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    gl.BindVertexArray(0);

    // uncomment this call to draw in wireframe polygons.
    //gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);
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
    if (gl is null || shaderProgram is null || vao is null) return;

    gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    gl.Clear(ClearBufferMask.ColorBufferBit);
    
    shaderProgram.Use();
    shaderProgram.Set("horizontalOffset", 0.5f);
    gl.BindVertexArray(vao.Value);
    gl.DrawArrays(GLEnum.Triangles, 0, 3);
}

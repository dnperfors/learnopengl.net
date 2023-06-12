using Silk.NET.GLFW;
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

var vertexShaderSource = """
    #version 330 core
    layout(location = 0) in vec3 aPos;
    layout(location = 1) in vec3 aColor;

    out vec3 ourColor;

    void main()
    {
        gl_Position = vec4(aPos, 1.0);
        ourColor = aColor;
    }
    """
    ;

var fragmentShaderSource = """
    #version 330 core
    out vec4 FragColor;

    in vec3 ourColor;

    void main()
    {
        FragColor = vec4(ourColor, 1.0);// vec4(1.0, 0.5, 0.2, 1.0);
    } 
    """;

uint? vao = null;
uint? shaderProgram = null;

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

    uint vertexShader = gl.CreateShader(GLEnum.VertexShader);
    gl.ShaderSource(vertexShader, vertexShaderSource);
    gl.CompileShader(vertexShader);

    string infoLog = gl.GetShaderInfoLog(vertexShader);
    if (!string.IsNullOrWhiteSpace(infoLog))
    {
        throw new Exception($"Error compiling vertex shader, failed with error {infoLog}");
    }

    uint fragmentShader = gl.CreateShader(GLEnum.FragmentShader);
    gl.ShaderSource(fragmentShader, fragmentShaderSource);
    gl.CompileShader(fragmentShader);

    infoLog = gl.GetShaderInfoLog(fragmentShader);
    if (!string.IsNullOrWhiteSpace(infoLog))
    {
        throw new Exception($"Error compiling fragment shader, failed with error {infoLog}");
    }

    shaderProgram = gl.CreateProgram();

    gl.AttachShader(shaderProgram.Value, vertexShader);
    gl.AttachShader(shaderProgram.Value, fragmentShader);
    gl.LinkProgram(shaderProgram.Value);

    gl.GetProgram(shaderProgram.Value, ProgramPropertyARB.LinkStatus, out int lStatus);
    if (lStatus != (int)GLEnum.True)
        throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(shaderProgram.Value));

    gl.DetachShader(shaderProgram.Value, vertexShader);
    gl.DetachShader(shaderProgram.Value, fragmentShader);
    gl.DeleteShader(vertexShader);
    gl.DeleteShader(fragmentShader);

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
    
    gl.UseProgram(shaderProgram.Value);
    int vertexColorLocation = gl.GetUniformLocation(shaderProgram.Value, "ourColor");

    float greenValue = (((float)Math.Sin(Glfw.GetApi().GetTime())) / 2.0f) + 0.5f;
    gl.Uniform4(vertexColorLocation, 0, greenValue, 0, 1);
    gl.BindVertexArray(vao.Value);
    gl.DrawArrays(GLEnum.Triangles, 0, 3);
}

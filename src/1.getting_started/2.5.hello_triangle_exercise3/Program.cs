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

    void main()
    {
        gl_Position = vec4(aPos, 1.0);
    }
    """
    ;

var fragmentShaderSourceOrange = """
    #version 330 core
    out vec4 FragColor;

    void main()
    {
        FragColor = vec4(1.0, 0.5, 0.2, 1.0);
    } 
    """;

var fragmentShaderSourceYellow = """
    #version 330 core
    out vec4 FragColor;

    void main()
    {
        FragColor = vec4(1.0, 1.0, 0.2, 1.0);
    } 
    """;

uint[] vao = Array.Empty<uint>();
uint[] shaderPrograms = Array.Empty<uint>();

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

    uint fragmentShaderOrange = gl.CreateShader(GLEnum.FragmentShader);
    gl.ShaderSource(fragmentShaderOrange, fragmentShaderSourceOrange);
    gl.CompileShader(fragmentShaderOrange);

    infoLog = gl.GetShaderInfoLog(fragmentShaderOrange);
    if (!string.IsNullOrWhiteSpace(infoLog))
    {
        throw new Exception($"Error compiling fragment shader, failed with error {infoLog}");
    }

    uint fragmentShaderYellow = gl.CreateShader(GLEnum.FragmentShader);
    gl.ShaderSource(fragmentShaderYellow, fragmentShaderSourceYellow);
    gl.CompileShader(fragmentShaderYellow);

    infoLog = gl.GetShaderInfoLog(fragmentShaderYellow);
    if (!string.IsNullOrWhiteSpace(infoLog))
    {
        throw new Exception($"Error compiling fragment shader, failed with error {infoLog}");
    }

    uint shaderProgramOrange = gl.CreateProgram();

    gl.AttachShader(shaderProgramOrange, vertexShader);
    gl.AttachShader(shaderProgramOrange, fragmentShaderOrange);
    gl.LinkProgram(shaderProgramOrange);

    gl.GetProgram(shaderProgramOrange, ProgramPropertyARB.LinkStatus, out int lStatus);
    if (lStatus != (int)GLEnum.True)
        throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(shaderProgramOrange));

    uint shaderProgramYellow = gl.CreateProgram();

    gl.AttachShader(shaderProgramYellow, vertexShader);
    gl.AttachShader(shaderProgramYellow, fragmentShaderYellow);
    gl.LinkProgram(shaderProgramYellow);

    gl.GetProgram(shaderProgramOrange, ProgramPropertyARB.LinkStatus, out lStatus);
    if (lStatus != (int)GLEnum.True)
        throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(shaderProgramYellow));

    shaderPrograms = new[]
    {
        shaderProgramOrange,
        shaderProgramYellow,
    };

    gl.DeleteShader(vertexShader);
    gl.DeleteShader(fragmentShaderOrange);
    gl.DeleteShader(fragmentShaderYellow);

    float[] firstTriangle = new[]
    {
        -1.0f, -0.5f, 0.0f, // left
         0.0f, -0.5f, 0.0f, // right
        -0.5f,  0.5f, 0.0f, // top
    };

    float[] secondTriangle = new[]
    {
         0.0f,  0.5f, 0.0f, // left
         1.0f,  0.5f, 0.0f, // right
         0.5f, -0.5f, 0.0f, // bottom
    };

    vao = new uint[2];
    gl.GenVertexArrays(vao.AsSpan());

    uint[] vbo = new uint[2];
    gl.GenBuffers(vbo.AsSpan());;

    gl.BindVertexArray(vao[0]);
    gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo[0]);
    fixed (float* buf = firstTriangle)
    {
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(firstTriangle.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
    }
    
    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), (void*)0);
    gl.EnableVertexAttribArray(0);

    gl.BindVertexArray(vao[1]);
    gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo[1]);
    fixed (float* buf = secondTriangle)
    {
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(secondTriangle.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
    }
    
    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), (void*)0);
    gl.EnableVertexAttribArray(0);

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

void OnRender(double dt)
{
    if (gl is null) return;

    gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    gl.Clear(ClearBufferMask.ColorBufferBit);
    
    gl.UseProgram(shaderPrograms[0]);
    gl.BindVertexArray(vao[0]);
    gl.DrawArrays(GLEnum.Triangles, 0, 3);
    
    gl.UseProgram(shaderPrograms[1]);
    gl.BindVertexArray(vao[1]);
    gl.DrawArrays(GLEnum.Triangles, 0, 3);
}

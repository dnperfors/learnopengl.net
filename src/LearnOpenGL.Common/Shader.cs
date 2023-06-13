using Silk.NET.OpenGL;

namespace LearnOpenGL.Common;

public class Shader
{
    private readonly GL gl;

    public uint ProgramId { get; }

    public Shader(GL gl, string vertexShaderFileName, string fragmentShaderFileName)
    {
        this.gl = gl;

        string vertextShaderSource = ReadShaderSource(vertexShaderFileName);
        string fragmentShaderSource = ReadShaderSource(fragmentShaderFileName);

        uint vertexShader = CompileShader(gl, vertextShaderSource, GLEnum.VertexShader);
        uint fragmentShader = CompileShader(gl, fragmentShaderSource, GLEnum.FragmentShader);

        ProgramId = CompileShaderProgram(gl, vertexShader, fragmentShader);

        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
    }

    private static string ReadShaderSource(string filename) => File.ReadAllText(filename);
    private static uint CompileShader(GL gl, string shaderSource, GLEnum shaderType)
    {
        uint shader = gl.CreateShader(shaderType);
        gl.ShaderSource(shader, shaderSource);
        gl.CompileShader(shader);
        string infoLog = gl.GetShaderInfoLog(shader);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader, failed with error {infoLog}");
        }
        return shader;
    }

    private static uint CompileShaderProgram(GL gl, params uint[] shaders)
    {
        uint programId = gl.CreateProgram();
        foreach(uint shader in shaders)
        {
            gl.AttachShader(programId, shader);
        }
        gl.LinkProgram(programId);
        gl.GetProgram(programId, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
        {
            throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(programId));
        }
        return programId;
    }

    public void Use()
    {
        gl.UseProgram(ProgramId);
    }

    public void Set(string name, bool value)
    {
        gl.Uniform1(gl.GetUniformLocation(ProgramId, name), value?1:0);
    }

    public void Set(string name, int value)
    {
        gl.Uniform1(gl.GetUniformLocation(ProgramId, name), value);
    }

    public void Set(string name, float value)
    {
        gl.Uniform1(gl.GetUniformLocation(ProgramId, name), value);
    }
}
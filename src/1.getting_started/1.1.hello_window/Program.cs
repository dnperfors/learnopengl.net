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

window.Load += OnLoad;
window.Update += OnUpdate;
window.Render += OnRender;

window.Run();

void OnLoad()
{
    var input = window.CreateInput();
    gl = window.CreateOpenGL();

    foreach(var keyboard in input.Keyboards)
    {
        keyboard.KeyDown += OnKeyDown;
    }
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
}

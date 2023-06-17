namespace LearnOpenGL.Common;

public static class GLMaths
{
    private const float degToRad = MathF.PI / 180.0f;
    public static float Radians(float degrees) => degToRad * degrees;
}

using Avalonia.OpenGL;
using OpenTK.Graphics.OpenGL;

namespace lab3.Controls.GL;

public static class OpenGlUtils
{
    public static void CheckError()
    {
        var error = OpenTK.Graphics.OpenGL.GL.GetError();

        if (error == ErrorCode.NoError) return;

        throw new OpenGlException(error.ToString());
    }
}
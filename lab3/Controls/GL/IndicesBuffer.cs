using Avalonia.OpenGL;

namespace lab3.Controls.GL;

public class IndicesBuffer : OpenGLHelper
{
    public IndicesBuffer(GlInterface GL)
    {
        _gl = GL;
    }
}
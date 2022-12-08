using System;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;

public abstract class OpenGLHelper
{
    protected GlInterface _gl;

    protected OpenGLHelper(GlInterface GL)
    {
        _gl = GL;
    }

    protected void CheckError()
    {
        int err;
        while ((err = _gl.GetError()) != GL_NO_ERROR)
        {
            if (err != 1280) throw new OpenGlException(Convert.ToString(err));
        }
    }
}
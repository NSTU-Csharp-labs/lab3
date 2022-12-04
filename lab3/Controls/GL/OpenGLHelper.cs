using System;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;


public abstract class OpenGLHelper
{
    protected GlInterface _gl;
    protected void CheckError()
    {
        int err;
        while ((err = _gl.GetError()) != GL_NO_ERROR)
        {
            throw new OpenGlException(Convert.ToString(err));
        }
    }
}


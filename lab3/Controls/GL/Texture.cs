using System;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;

public class Texture : OpenGLHelper
{
    public int _texture;

    public Texture(GlInterface GL)
    {
        unsafe
        {
            _gl = GL;
            _gl.GenTextures(1, (int*)(_texture));
            _texture = _gl.GenTexture();
            _gl.BindTexture(GL_TEXTURE_2D, _texture);
            _gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            _gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            _gl.BindTexture(GL_TEXTURE_2D, 0);

            CheckError();
        }
    }

    public unsafe void SetPixels(byte[] pixels, float width, float height)
    {
        fixed (byte* p = pixels)
        {
            SetPixels(p, width, height);
        }

    }

    public unsafe void SetPixels(byte* p, float width, float height)
    {
        _gl.BindTexture(GL_TEXTURE_2D, _texture);
      
        _gl.TexImage2D(
            GL_TEXTURE_2D,
            0,
            GL_RGBA,
            (int)width,
            (int)height,
            0,
            GL_RGBA,
            GL_UNSIGNED_BYTE,
            new IntPtr(p));
        
        _gl.BindTexture(GL_TEXTURE_2D, 0);
        CheckError();
    }

    public void Use()
    {
        _gl.BindTexture(GL_TEXTURE_2D, _texture);
        CheckError();
    }
    
}
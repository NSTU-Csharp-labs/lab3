using System;
using Avalonia.OpenGL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;

public class Texture : IDisposable
{
    private readonly TextureHandle _texture;

    public Texture()
    {
        _texture = OpenTK.Graphics.OpenGL.GL.GenTexture();

        OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, _texture);
        OpenTK.Graphics.OpenGL.GL.TexParameteri(TextureTarget.Texture2d,
            TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        OpenTK.Graphics.OpenGL.GL.TexParameteri(TextureTarget.Texture2d, 
            TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, TextureHandle.Zero);

        OpenGlUtils.CheckError();
    }

    public void SetPixels(byte[] pixels, int width, int height)
    {
        OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, _texture);

        OpenTK.Graphics.OpenGL.GL.TexImage2D(
            TextureTarget.Texture2d, 0,
            InternalFormat.Rgba,
            width,
            height,
            border: 0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            new ReadOnlySpan<byte>(pixels)
        );
        
        OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, TextureHandle.Zero);
    }


    public void Use()
    {
        OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, _texture);
        OpenGlUtils.CheckError();
    }

    public void Dispose()
    {
        OpenTK.Graphics.OpenGL.GL.DeleteTexture(_texture);
        OpenGlUtils.CheckError();
    }
}
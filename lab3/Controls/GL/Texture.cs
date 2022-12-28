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

        OpenTK.Graphics.OpenGL.GL.BindTexture(
            TextureTarget.Texture2d,
            _texture);
        
        OpenTK.Graphics.OpenGL.GL.TexParameteri(
            TextureTarget.Texture2d,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear
            );
        
        
        
        OpenTK.Graphics.OpenGL.GL.TexParameteri(
            TextureTarget.Texture2d,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear
            );
        
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

        OpenGlUtils.CheckError();

        OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, TextureHandle.Zero);
    }

    public void SetPixels(int width, int height)
    {
        OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, _texture);

        OpenTK.Graphics.OpenGL.GL.TexImage2D(
            TextureTarget.Texture2d,
            level: 0,
            InternalFormat.Rgba,
            width,
            height,
            border: 0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            new IntPtr()
        );

        OpenGlUtils.CheckError();

        OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, TextureHandle.Zero);
    }

    public void AttachToFramebuffer(FramebufferTarget target, FramebufferAttachment attachment)
    {
        OpenTK.Graphics.OpenGL.GL.FramebufferTexture2D(target,
            attachment, TextureTarget.Texture2d, _texture, 0);
    }

    public byte[] ReadPixels(int pixelsNumber)
    {
        var pixels = new byte[4 * pixelsNumber];
        
        OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, _texture);
        OpenTK.Graphics.OpenGL.GL.GetTexImage(TextureTarget.Texture2d, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, pixels);
        OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, TextureHandle.Zero);
        
        return pixels;
    }

    public IDisposable Use()
    {
        OpenTK.Graphics.OpenGL.GL.ActiveTexture(TextureUnit.Texture0);
        OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, _texture);

        return new DisposableUsing(() =>
        {
            OpenTK.Graphics.OpenGL.GL.BindTexture(TextureTarget.Texture2d, TextureHandle.Zero);
        });
    }

    public void Dispose()
    {
        OpenTK.Graphics.OpenGL.GL.DeleteTexture(_texture);
    }
}
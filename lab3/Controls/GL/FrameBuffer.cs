using System;
using Avalonia.OpenGL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace lab3.Controls.GL;

public class FrameBuffer
{
    private readonly FramebufferHandle _frameBuffer;
    private readonly Texture _texColorBuffer;
    private readonly int _width;
    private readonly int _height;

    public FrameBuffer(int width, int height)
    {
        _width = width;
        _height = height;

        OpenTK.Graphics.OpenGL.GL.GenFramebuffers(1, ref _frameBuffer);

        OpenTK.Graphics.OpenGL.GL.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBuffer);

        OpenGlUtils.CheckError();

        _texColorBuffer = new Texture();
        _texColorBuffer.SetPixels(_width, _height);
        _texColorBuffer.AttachToFramebuffer(FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0);

        OpenGlUtils.CheckError();

        var renderbuffer = OpenTK.Graphics.OpenGL.GL.GenRenderbuffer();
        OpenTK.Graphics.OpenGL.GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);

        OpenGlUtils.CheckError();

        OpenTK.Graphics.OpenGL.GL.RenderbufferStorage(
            RenderbufferTarget.Renderbuffer,
            InternalFormat.Depth24Stencil8,
            _width,
            _height
            );
        OpenTK.Graphics.OpenGL.GL.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer,
            renderbuffer
            );

        OpenGlUtils.CheckError();

        CheckStatus();

        OpenTK.Graphics.OpenGL.GL.BindFramebuffer(FramebufferTarget.Framebuffer,
            FramebufferHandle.Zero);
    }

    public ImgBitmap ReadBitmap()
    {
        OpenTK.Graphics.OpenGL.GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _frameBuffer);
        var pixels = _texColorBuffer.ReadPixels(_width * _height);
        OpenTK.Graphics.OpenGL.GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer,
            FramebufferHandle.Zero);

        return new ImgBitmap(_width, _height, pixels);
    }

    public IDisposable Use()
    {
        OpenTK.Graphics.OpenGL.GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _frameBuffer);
        return new DisposableUsing(() =>
        {
            OpenTK.Graphics.OpenGL.GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer,
                FramebufferHandle.Zero);
        });
    }

    private void CheckStatus()
    {
        var status = OpenTK.Graphics.OpenGL.GL.CheckNamedFramebufferStatus(_frameBuffer,
            FramebufferTarget.Framebuffer);

        if (status != FramebufferStatus.FramebufferComplete && status != 0)
        {
            throw new OpenGlException($"Error. Failed to create frame buffer. {status}");
        }
    }
}
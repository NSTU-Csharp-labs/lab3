using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.OpenGL;
using OpenTK.Graphics;

namespace lab3.Controls.GL;

public class ImagePostprocessor : IDisposable
{
    private readonly IGlContext _context;
    private IEnumerable<Filter> _filters;

    public ImagePostprocessor(IEnumerable<Filter> filters)
    {
        var feature = AvaloniaLocator.Current.GetService<IPlatformOpenGlInterface>();
        _filters = filters;
        _context = feature.CreateSharedContext();
    }

    public ImgBitmap Apply(ImgBitmap bitmap)
    {
        using (_context.MakeCurrent())
        {
            GLLoader.LoadBindings(new AvaloniaBindingsContext(_context.GlInterface));
            SkipAvaloniaOpenGlError();
            
            var frameBuffer = new FrameBuffer(bitmap.Width, bitmap.Height);
            using (frameBuffer.Use())
            {
                DrawToFramebuffer(bitmap);
                var bitmapWithPostprocessing = frameBuffer.ReadBitmap();
                
                return bitmapWithPostprocessing;
            }
        }
    }

    private void SkipAvaloniaOpenGlError()
    {
        try
        {
            OpenGlUtils.CheckError();
        }
        catch (Exception)
        {
            // IGNORE
        }
    }

    private void DrawToFramebuffer(ImgBitmap bitmap)
    {
        using var painter = new BitmapPainter(_filters ,isPostprocessing: true);

        painter.Paint(bitmap.Adjust(bitmap.Width, bitmap.Height));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
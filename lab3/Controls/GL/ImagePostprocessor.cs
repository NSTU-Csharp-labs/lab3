using System;
using Avalonia;
using Avalonia.OpenGL;
using OpenTK.Graphics;

namespace lab3.Controls.GL;

public class ImagePostprocessor : IDisposable
{
    private readonly IGlContext _context;

    private bool _useBlackAndWhiteFilter;
    private bool _useRedFilter;
    private bool _useGreenFilter;
    private bool _useBlueFilter;

    public ImagePostprocessor()
    {
        var feature = AvaloniaLocator.Current.GetService<IPlatformOpenGlInterface>();
        _context = feature.CreateSharedContext();
    }

    public ImagePostprocessor UseBlackAndWhiteFilter()
    {
        _useBlackAndWhiteFilter = true;
        return this;
    }

    public ImagePostprocessor UseRedFilter()
    {
        _useRedFilter = true;
        return this;
    }
    
    public ImagePostprocessor UseGreenFilter()
    {
        _useGreenFilter = true;
        return this;
    }
    
    public ImagePostprocessor UseBlueFilter()
    {
        _useBlueFilter = true;
        return this;
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
        using var painter = new BitmapPainter(isPostprocessing: true);

        painter.UseBlackAndWhiteFilter = _useBlackAndWhiteFilter;
        painter.UseRedFilter = _useRedFilter;
        painter.UseGreenFilter = _useGreenFilter;
        painter.UseBlueFilter = _useBlueFilter;

        painter.Paint(bitmap.Adjust(bitmap.Width, bitmap.Height));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
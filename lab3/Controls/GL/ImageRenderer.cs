using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using OpenTK.Graphics;

namespace lab3.Controls.GL;

public class ImageRenderer : OpenGlControlBase
{
    public ImageRenderer()
    {
        AffectsRender<ImageRenderer>(
            FiltersProperty,
            ImgProperty
        );
    }

    public ImgBitmap Img
    {
        get => GetValue(ImgProperty);
        set => SetValue(ImgProperty, value);
    }

    public static readonly StyledProperty<IEnumerable<Filter>> FiltersProperty =
        AvaloniaProperty.Register<ImageRenderer, IEnumerable<Filter>>(nameof(Filters));

    public IEnumerable<Filter> Filters
    {
        get => GetValue(FiltersProperty);
        set => SetValue(FiltersProperty, value);
    }

    public static readonly StyledProperty<ImgBitmap> ImgProperty =
        AvaloniaProperty.Register<ImageRenderer, ImgBitmap>(nameof(Img));


    protected override void OnOpenGlInit(GlInterface GL, int fb)
    {
        GLLoader.LoadBindings(new AvaloniaBindingsContext(GL));
        
        try
        {
            OpenGlUtils.CheckError();
        }
        catch
        {
            // ignored
        }
    }

    protected override void OnOpenGlRender(GlInterface _, int __)
    {
        try
        {
            new BitmapPainter(Filters).Paint(Img.Adjust((int)Bounds.Width, (int)Bounds.Height));
        }
        catch (OpenGlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
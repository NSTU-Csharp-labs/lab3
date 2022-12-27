using System;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;

namespace lab3.Controls.GL;

public class ImageRenderer : OpenGlControlBase
{
    public static readonly StyledProperty<ImgBitmap> ImgProperty =
        AvaloniaProperty.Register<ImageRenderer, ImgBitmap>(nameof(Img));

    public static readonly StyledProperty<bool> BlackAndWhiteFilterProperty =
        AvaloniaProperty.Register<ImageRenderer, bool>(nameof(BlackAndWhiteFilter));

    public static readonly StyledProperty<bool> RedFilterProperty =
        AvaloniaProperty.Register<ImageRenderer, bool>(nameof(RedFilter));

    public static readonly StyledProperty<bool> GreenFilterProperty =
        AvaloniaProperty.Register<ImageRenderer, bool>(nameof(GreenFilter));

    public static readonly StyledProperty<bool> BlueFilterProperty =
        AvaloniaProperty.Register<ImageRenderer, bool>(nameof(BlueFilter));

    private Painter _painter;

    public ImageRenderer()
    {
        AffectsRender<ImageRenderer>(
            BlackAndWhiteFilterProperty,
            RedFilterProperty,
            GreenFilterProperty,
            BlueFilterProperty,
            ImgProperty
        );
    }

    public ImgBitmap Img
    {
        get => GetValue(ImgProperty);
        set => SetValue(ImgProperty, value);
    }

    public bool BlackAndWhiteFilter
    {
        get => GetValue(BlackAndWhiteFilterProperty);
        set => SetValue(BlackAndWhiteFilterProperty, value);
    }

    public bool RedFilter
    {
        get => GetValue(RedFilterProperty);
        set => SetValue(RedFilterProperty, value);
    }

    public bool GreenFilter
    {
        get => GetValue(GreenFilterProperty);
        set => SetValue(GreenFilterProperty, value);
    }

    public bool BlueFilter
    {
        get => GetValue(BlueFilterProperty);
        set => SetValue(BlueFilterProperty, value);
    }

    protected override void OnOpenGlInit(GlInterface GL, int fb)
    {
        try
        {
            _painter = new Painter(GL, GlProfileType.OpenGLES);
        }
        catch (OpenGlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override void OnOpenGlRender(GlInterface GL, int fb)
    {
        try
        {
            _painter.UseBlackAndWhiteFilter = BlackAndWhiteFilter;

            _painter.UseRedFilter = RedFilter;
            _painter.UseGreenFilter = GreenFilter;
            _painter.UseBlueFilter = BlueFilter;

            _painter.Paint(Img, (float)Bounds.Width, (float)Bounds.Height);
        }
        catch (OpenGlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override void OnOpenGlDeinit(GlInterface GL, int fb)
    {
        _painter.Dispose();
    }
}
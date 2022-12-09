using System;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;

namespace lab3.Controls.GL;

public class ImageRenderer : OpenGlControlBase
{
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

    public static readonly StyledProperty<ImgBitmap> ImgProperty =
        AvaloniaProperty.Register<ImageRenderer, ImgBitmap>(nameof(Img));

    public bool BlackAndWhiteFilter
    {
        get => GetValue(BlackAndWhiteFilterProperty);
        set => SetValue(BlackAndWhiteFilterProperty, value);
    }

    public static readonly StyledProperty<bool> BlackAndWhiteFilterProperty =
        AvaloniaProperty.Register<ImageRenderer, bool>(nameof(BlackAndWhiteFilter));

    public bool RedFilter
    {
        get => GetValue(RedFilterProperty);
        set => SetValue(RedFilterProperty, value);
    }

    public static readonly StyledProperty<bool> RedFilterProperty =
        AvaloniaProperty.Register<ImageRenderer, bool>(nameof(RedFilter));

    public bool GreenFilter
    {
        get => GetValue(GreenFilterProperty);
        set => SetValue(GreenFilterProperty, value);
    }

    public static readonly StyledProperty<bool> GreenFilterProperty =
        AvaloniaProperty.Register<ImageRenderer, bool>(nameof(GreenFilter));

    public bool BlueFilter
    {
        get => GetValue(BlueFilterProperty);
        set => SetValue(BlueFilterProperty, value);
    }

    public static readonly StyledProperty<bool> BlueFilterProperty =
        AvaloniaProperty.Register<ImageRenderer, bool>(nameof(BlueFilter));

    private Painter _painter;

    protected override void OnOpenGlInit(GlInterface GL, int _)
    {
        OpenTK.Graphics.GLLoader.LoadBindings(new AvaloniaBindingsContext(GL));

        try
        {
            _painter = new Painter();
        }
        catch (OpenGlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override void OnOpenGlRender(GlInterface _, int __)
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

    protected override void OnOpenGlDeinit(GlInterface _, int __)
    {
        _painter.Dispose();
    }
}
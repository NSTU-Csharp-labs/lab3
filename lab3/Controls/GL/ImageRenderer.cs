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

    private BitmapPainter _bitmapPainter;

    protected override void OnOpenGlInit(GlInterface GL, int _)
    {
        OpenTK.Graphics.GLLoader.LoadBindings(new AvaloniaBindingsContext(GL));

        try
        {
            _bitmapPainter = new BitmapPainter();
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
            _bitmapPainter.UseBlackAndWhiteFilter = BlackAndWhiteFilter;

            _bitmapPainter.UseRedFilter = RedFilter;
            _bitmapPainter.UseGreenFilter = GreenFilter;
            _bitmapPainter.UseBlueFilter = BlueFilter;

            var adjustedBitmap = Img.Adjust((int)Bounds.Width, (int)Bounds.Height);
            var matricesProvider = new MatricesOfDisplayedImage(adjustedBitmap);
            
            _bitmapPainter.Paint(adjustedBitmap, matricesProvider);
        }
        catch (OpenGlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override void OnOpenGlDeinit(GlInterface _, int __)
    {
        _bitmapPainter.Dispose();
    }
}
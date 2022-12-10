using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using OpenTK.Graphics;
using ReactiveUI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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

        Observable
            .Return(Unit.Default)
            .SelectMany(async _ =>
            {
                await Test().ConfigureAwait(true);
                return Unit.Default;
            })
            .Subscribe();
    }

    private async Task Test()
    {
        var feature = AvaloniaLocator.Current.GetService<IPlatformOpenGlInterface>();
        using var ctx = feature.CreateSharedContext();
        using var _ = ctx.MakeCurrent();
        GLLoader.LoadBindings(new AvaloniaBindingsContext(ctx.GlInterface));

        // await Task.Delay(TimeSpan.FromSeconds(5));

        var p = ApplyPostprocessing(Img);

        var newImage = Image.LoadPixelData<Rgba32>(p.Pixels, p.Width, p.Height);
        newImage.Save("testimage.jpg");
        newImage.Dispose();
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

            _bitmapPainter.Paint(adjustedBitmap);

            // var p = ApplyPostprocessing(Img);
            //
            // var newImage = Image.LoadPixelData<Rgba32>(p.Pixels, p.Width, p.Height);
            // newImage.Save("testimage.png");
            // newImage.Dispose();
        }
        catch (OpenGlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public ImgBitmap ApplyPostprocessing(ImgBitmap bitmap)
    {
        // try
        // {
        //     OpenGlUtils.CheckError();
        // }
        // catch
        // {
        //     // ignored
        // }

        var frameBuffer = new FrameBuffer(bitmap.Width, bitmap.Height);
        using var _ = frameBuffer.Use();

        using var painter = new BitmapPainter(isPostprocessing: true);

        painter.UseBlackAndWhiteFilter = BlackAndWhiteFilter;
        painter.UseRedFilter = RedFilter;
        painter.UseGreenFilter = GreenFilter;
        painter.UseBlueFilter = BlueFilter;

        painter.Paint(bitmap.Adjust(bitmap.Width, bitmap.Height));

        var bitmapWithPostprocessing = frameBuffer.ReadBitmap();

        painter.Dispose();

        return bitmapWithPostprocessing;
    }


    protected override void OnOpenGlDeinit(GlInterface _, int __)
    {
        _bitmapPainter.Dispose();
    }
}
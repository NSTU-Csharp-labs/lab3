using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using lab3.Controls.GL;
using ReactiveUI;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace lab3.Controls.MainWindow;

public class ImageManager
{
    private List<string> _imageExtentions;
    private string _pathToCurrentPicture;
    private ImgBitmap _img;
    private int _rotateMode;
    private string[] _picturesInFolder;

    public readonly Interaction<Unit, string[]> ShowOpenImageDialog;
    public ReactiveCommand<Unit, Unit> OpenImage { get; }
    public ReactiveCommand<Unit, Unit> Rotate { get; }


    public ImageManager()
    {
        _imageExtentions = new List<string>() { ".JPG", ".JPEG", ".PNG" };
        _rotateMode =  (int)RotateMode.None;
        PathToCurrentPicture = new string("../../../Assets/texture.jpg");
        ShowOpenImageDialog = new Interaction<Unit, string[]>();
        SetPicture();
        OpenImage = ReactiveCommand.CreateFromTask(OnOpenImage);
        Rotate = ReactiveCommand.Create(DoRotation);
    }

    public string PathToCurrentPicture
    {
        get => _pathToCurrentPicture;
        set
        {
            foreach (var extention in _imageExtentions)
            {
                if (value.ToUpper().EndsWith(extention))
                {
                     _pathToCurrentPicture =  value;
                    break;
                }
            }

            throw new ImageManagerException("picture is not jpg or png");
        }
    }
    private ImgBitmap Img
    {
        get => _img;
        set
        {
            if (value is not null)
            {
                _img = value;
            }
            else
            {
                Img = new ImgBitmap();
                throw new ImageManagerException("Field Img: ImgBitmap isn't nullable");
            }
        }
    }
    
    private void DoRotation()
    {
        _rotateMode = (RotateMode)_rotateMode switch
        {
            RotateMode.None => (int)RotateMode.Rotate270,
            RotateMode.Rotate270 => (int)RotateMode.Rotate180,
            RotateMode.Rotate180 => (int)RotateMode.Rotate90,
            RotateMode.Rotate90 => (int)RotateMode.None,
            _ => throw new ArgumentOutOfRangeException()
        };
        SetPicture();
    }
    public void SetPicture()
    {
        var img = Image.Load<Rgba32>(PathToCurrentPicture);
        img.Mutate(context => context.Rotate((RotateMode)_rotateMode));

        var pixels = new byte[img.Width * 4 * img.Height];
        img.CopyPixelDataTo(pixels);
        Img = new ImgBitmap(img.Width, img.Height, pixels);
        img.Dispose();
    }
    

    private async Task OnOpenImage()
    {
        try
        {
            var dirPictures = await ShowOpenImageDialog.Handle(Unit.Default);
            if (dirPictures is not null)
            {
                _rotateMode = (int)RotateMode.None;
                PicturesInFolder = dirPictures;
                PathToCurrentPicture = dirPictures[0];
                SetPicture();
            }
        }
        catch
        {
            // ignored, because picture will net reseted
        }
    }
    

    public string[] PicturesInFolder
    {
        get => _picturesInFolder;
        set =>  _picturesInFolder =  value;
    }
    
    
    
}

public class ImageManagerException : Exception
{
    public  ImageManagerException(string message) : base(message){}
}

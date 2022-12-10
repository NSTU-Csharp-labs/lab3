using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Xml.Serialization;
using DynamicData;
using lab3.Controls.GL;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace lab3.Controls.MainWindow;

[Serializable]
public class ImageManager : IDisposable
{
    private readonly List<string> _imageExtentions;

    private Image<Rgba32>? _image;

    private string _currentPicture;

    private ImgBitmap _img;

    private Subject<ImgBitmap> _bitmap;
    [field: NonSerialized] [XmlIgnore] public IObservable<ImgBitmap> BitmapChanged => _bitmap.AsObservable();

    public int RotateMode { get; set; }
    private string[] _picturesInFolder;


    public void SwipeLeft()
    {
        var newIndex = (PicturesInFolder.IndexOf(CurrentPicture) - 1);
        CurrentPicture = PicturesInFolder[newIndex > -1 ? newIndex : PicturesInFolder.Length - 1];
        SetPicture();
    }

    public void SwipeRight()
    {
        var newIndex = PicturesInFolder.IndexOf(CurrentPicture) + 1;
        CurrentPicture = PicturesInFolder[newIndex < PicturesInFolder.Length ? newIndex : 0];
        SetPicture();
    }


    public ImageManager()
    {
        _bitmap = new Subject<ImgBitmap>();
        _imageExtentions = new List<string>() { ".JPG", ".JPEG", ".PNG" };
        RotateMode = (int)SixLabors.ImageSharp.Processing.RotateMode.None;
        PicturesInFolder = new[] {"../../../Assets/texture.jpg"};
        SetPicture();
    }

    public string CurrentPicture
    {
        get => _currentPicture;
        set
        {
            try
            {
                if (PicturesInFolder.Any(pic => pic.Equals(value)))
                {
                    _currentPicture = value;
                    return;
                }
            }
            catch (Exception)
            {
                PicturesInFolder = new[] { value };
            }
        }
    }


    public void DoRotation()
    {
        if (_image is null) return;

        RotateMode = (RotateMode)RotateMode switch
        {
            SixLabors.ImageSharp.Processing.RotateMode.None =>
                (int)SixLabors.ImageSharp.Processing.RotateMode.Rotate270,
            SixLabors.ImageSharp.Processing.RotateMode.Rotate270 => (int)SixLabors.ImageSharp.Processing.RotateMode
                .Rotate180,
            SixLabors.ImageSharp.Processing.RotateMode.Rotate180 => (int)SixLabors.ImageSharp.Processing.RotateMode
                .Rotate90,
            SixLabors.ImageSharp.Processing.RotateMode.Rotate90 => (int)SixLabors.ImageSharp.Processing.RotateMode.None,
            _ => throw new ArgumentOutOfRangeException()
        };

        _image.Mutate(context => { context.Rotate(SixLabors.ImageSharp.Processing.RotateMode.Rotate270); });
        var pixels = new byte[_image.Width * 4 * _image.Height];
        _image.CopyPixelDataTo(pixels);
        var bitmap = new ImgBitmap(_image.Width, _image.Height, pixels);
        _bitmap.OnNext(bitmap);
    }

    public async void SetPicture()
    {
        _image?.Dispose();
        _image = await Image.LoadAsync<Rgba32>(CurrentPicture);
        _image.Mutate(context => context.Rotate((RotateMode)RotateMode));
        var pixels = new byte[_image.Width * 4 * _image.Height];
        _image.CopyPixelDataTo(pixels);
        var bitmap = new ImgBitmap(_image.Width, _image.Height, pixels);

        _bitmap.OnNext(bitmap);
    }

    public void ResetPictures()
    {
        RotateMode = (int)SixLabors.ImageSharp.Processing.RotateMode.None;
        PicturesInFolder = new[] { "../../../Assets/texture.jpg" };
        CurrentPicture = PicturesInFolder[0];
        SetPicture();
    }

    public string[] PicturesInFolder
    {
        get => _picturesInFolder;
        set
        {
            if (value is not null)
            {
                _picturesInFolder = value.Where(name =>
                {
                    foreach (var extention in _imageExtentions)
                    {
                        if (name.ToUpper().EndsWith(extention))
                        {
                            return true;
                        }
                    }

                    return false;
                }).ToArray();
                if (_picturesInFolder.Length > 0)
                {
                    _currentPicture = _picturesInFolder[0];
                    SetPicture();
                }
                else
                {
                    ResetPictures();
                }
            }
            else
            {
                ResetPictures();
            }
        }
    }

    public void Dispose()
    {
        _image?.Dispose();
        _bitmap.Dispose();
    }
}

public class ImageManagerException : Exception
{
    public ImageManagerException(string message) : base(message)
    {
    }
}
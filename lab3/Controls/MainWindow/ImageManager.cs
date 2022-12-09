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
    private List<string> _imageExtentions;

    private Image<Rgba32>? _image;

    private string _currentPicture;
    [field: NonSerialized] [XmlIgnore] private ImgBitmap _img;
    private BehaviorSubject<ImgBitmap> _bitmap;
    public IObservable<ImgBitmap> BitmapChanged => _bitmap.AsObservable();

    private int _rotateMode;
    private string[] _picturesInFolder;


    public void SwipeLeft()
    {
        var newIndex = (PicturesInFolder.IndexOf(CurrentPicture) - 1);
        CurrentPicture = PicturesInFolder[newIndex > -1 ? newIndex : PicturesInFolder.Length];
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
        _bitmap = new BehaviorSubject<ImgBitmap>(new ImgBitmap(0, 0, new byte[] { }));
        _imageExtentions = new List<string>() { ".JPG", ".JPEG", ".PNG" };
        _rotateMode = (int)RotateMode.None;
        CurrentPicture = new string("../../../Assets/texture.jpg");
        SetPicture();
    }

    public string CurrentPicture { get; private set; }


    public void DoRotation()
    {
        if (_image is null) return;
        
        var newRot = (RotateMode)_rotateMode switch
        {
            RotateMode.None => (int)RotateMode.Rotate270,
            RotateMode.Rotate270 => (int)RotateMode.Rotate180,
            RotateMode.Rotate180 => (int)RotateMode.Rotate90,
            RotateMode.Rotate90 => (int)RotateMode.None,
            _ => throw new ArgumentOutOfRangeException()
        };

        _image.Mutate(context =>
        {
            context.Rotate((RotateMode)(Math.Abs(newRot - _rotateMode)));
        });
        _rotateMode = newRot;
        var pixels = new byte[_image.Width * 4 * _image.Height];
        _image.CopyPixelDataTo(pixels);
        var bitmap = new ImgBitmap(_image.Width, _image.Height, pixels);
        _bitmap.OnNext(bitmap);
    }

    public async void SetPicture()
    {
        _image?.Dispose();
        _image = await Image.LoadAsync<Rgba32>(CurrentPicture);
        _image.Mutate(context => context.Rotate((RotateMode)_rotateMode));
        var pixels = new byte[_image.Width * 4 * _image.Height];
        _image.CopyPixelDataTo(pixels);
        var bitmap = new ImgBitmap(_image.Width, _image.Height, pixels);

        _bitmap.OnNext(bitmap);
    }

    public void ResetPictures()
    {
        _rotateMode = (int)RotateMode.None;
        _picturesInFolder = new[] { "../../../Assets/texture.jpg" };
        if (_picturesInFolder.Length > 0)
        {
            CurrentPicture = _picturesInFolder[0];
        }
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
                    CurrentPicture = _picturesInFolder[0];
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
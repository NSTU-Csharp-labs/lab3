using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Xml.Serialization;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
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

    private Subject<ImgBitmap> _bitmap;
    [XmlIgnore] public IObservable<ImgBitmap> BitmapChanged => _bitmap.AsObservable();
    public RotateMode CurrentRotationMode { get; set; }
    
    private string[] _picturesInFolder;

    public ImageManager()
    {
        _bitmap = new Subject<ImgBitmap>();
        _imageExtentions = new List<string>() { ".JPG", ".JPEG", ".PNG" };
        CurrentRotationMode = RotateMode.None;
        PicturesInFolder = new[] { "../../../Assets/texture.jpg" };
        SetPicture();
    }
    
    public string[] PicturesInFolder
    {
        get => _picturesInFolder;
        set
        {
            if (value is null)
            {
                ResetPictures();
                return;
            }
            CurrentRotationMode = RotateMode.None;

            _picturesInFolder = value.Where(name =>
                    _imageExtentions.Any(extention =>
                        name.ToUpper()
                            .EndsWith(extention)))
                .ToArray();
            if (PicturesInFolder.Length > 0)
            {
                CurrentPicture = PicturesInFolder[0];
            }
            else
            {
                ResetPictures();
            }
            SetPicture();
        }
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
                }
                else
                {
                    PicturesInFolder = new[] { value };
                }
                SetPicture();
            }
            catch (Exception)
            {
                // Сообщение об ошибке надо сделать
                PicturesInFolder = new[] { "../../../Assets/texture.jpg" };
            }
        }
    }



    public void DoRightRotation()
    {
        if (_image is null) return;

        CurrentRotationMode = CurrentRotationMode switch
        {
            RotateMode.None => RotateMode.Rotate90,
            RotateMode.Rotate90 => RotateMode.Rotate180,
            RotateMode.Rotate180 => RotateMode.Rotate270,
            RotateMode.Rotate270 => RotateMode.None,
            _ => throw new ArgumentOutOfRangeException()
        };

        _image.Mutate(context => { context.Rotate(RotateMode.Rotate90); });
        SetPixels();
    }

  

    public async void SetPicture()
    {
        _image?.Dispose();
        try
        {
            _image = await Image.LoadAsync<Rgba32>(CurrentPicture);
            _image.Mutate(context => context.Rotate(CurrentRotationMode));
            SetPixels();
        }
        catch
        {
            var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBoxStandardParams
                {
                    ContentTitle = "Ошибка",
                    ContentMessage = "Не обработанная ошибка, обратитесь, пожалуйста в поддержку"
                });
            await messageBoxStandardWindow.Show();
        }
    }

   

   
    
    
    public void ResetPictures()
    {
        CurrentRotationMode = RotateMode.None;
        PicturesInFolder = new[] { "../../../Assets/texture.jpg" };
        CurrentPicture = PicturesInFolder[0];
        SetPicture();
    }
    
    private void SetPixels()
    {
        if (_image is null) return;
        var pixels = new byte[_image.Width * 4 * _image.Height];
        _image.CopyPixelDataTo(pixels);
        _bitmap.OnNext(new ImgBitmap(_image.Width, _image.Height, pixels));
    }


    
    public void SwipeLeft()
    {
        CurrentRotationMode = RotateMode.None;

        var newIndex = (PicturesInFolder.IndexOf(CurrentPicture) - 1);
        CurrentPicture = PicturesInFolder[newIndex > -1 ? newIndex : PicturesInFolder.Length - 1];
        SetPicture();
    }

    public void SwipeRight()
    {
        CurrentRotationMode = RotateMode.None;

        var newIndex = PicturesInFolder.IndexOf(CurrentPicture) + 1;
        CurrentPicture = PicturesInFolder[newIndex < PicturesInFolder.Length ? newIndex : 0];
        SetPicture();
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
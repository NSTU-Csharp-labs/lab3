using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DynamicData;
using lab3.Controls.GL;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace lab3.Controls.MainWindow;

[Serializable]
public class ImageManager : IDisposable
{
    private readonly List<string> _imageExtentions;

    private Subject<ImgBitmap> _bitmap;

    private string _currentPicture;

    private Image<Rgba32> _image;
    public FiltersManager FiltersManager { get; }
    private string[] _picturesInFolder;

    public ImageManager()
    {
        _bitmap = new Subject<ImgBitmap>();
        FiltersManager = new FiltersManager();
        _imageExtentions = new List<string> { ".JPG", ".JPEG", ".PNG" };
        CurrentRotationMode = RotateMode.None;
        PicturesInFolder = new[] { "../../../Assets/texture.jpg" };
        SetPicture();
    }

    [XmlIgnore] public IObservable<ImgBitmap> BitmapChanged => _bitmap.AsObservable();
    public RotateMode CurrentRotationMode { get; set; }
    
    
    public string CurrentPicture
    {
        get => _currentPicture;
        set
        {
            try
            {
                if (PicturesInFolder is not null && PicturesInFolder.Any(pic => pic.Equals(value)))
                    _currentPicture = value;
                else
                    PicturesInFolder = new[] { value };
                SetPicture();
            }
            catch (Exception)
            {
                // Сообщение об ошибке надо сделать
                ResetPictures("../../../Assets/CurrentPicture.png");
            }
        }
    }

    public string[] PicturesInFolder
    {
        get => _picturesInFolder;
        set
        {
            CurrentRotationMode = RotateMode.None;

            if (value is null)
            {
                ResetPictures("../../../Assets/PicturesInFolder.png");
                return;
            }

            _picturesInFolder = value.Where(ValidatePictureName).ToArray();

            if (PicturesInFolder is not null && PicturesInFolder.Length > 0)
                CurrentPicture = PicturesInFolder[0];
            else
                ResetPictures("../../../Assets/PicturesInFolder.png");
            SetPicture();
        }
    }

  

    public void Dispose()
    {
        _image?.Dispose();
        _bitmap.Dispose();
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
            ResetPictures("../../../Assets/SetPicture.png");
            await ShowErrorMessageBox("Необработанная ошибка. Обратитесь, пожалуйста, в поддержку");
        }
    }

    

    public void ResetPictures(string pathToErrorImage)
    {
        CurrentRotationMode = RotateMode.None;
        PicturesInFolder = new[] { pathToErrorImage };
        SetPicture();
    }

    private void SetPixels()
    {
        var pixels = new byte[_image.Width * 4 * _image.Height];
        _image.CopyPixelDataTo(pixels);
        _bitmap.OnNext(new ImgBitmap(_image.Width, _image.Height, pixels));
    }

    public void SwipeLeft()
    {
        CurrentRotationMode = RotateMode.None;

        if (PicturesInFolder is not null)
        {
            var newIndex = PicturesInFolder.IndexOf(CurrentPicture) - 1;
            CurrentPicture = PicturesInFolder[newIndex > -1 ? newIndex : PicturesInFolder.Length - 1];
        }

        SetPicture();
    }

    public void SwipeRight()
    {
        CurrentRotationMode = RotateMode.None;

        if (PicturesInFolder is not null)
        {
            var newIndex = PicturesInFolder.IndexOf(CurrentPicture) + 1;
            CurrentPicture = PicturesInFolder[newIndex < PicturesInFolder.Length ? newIndex : 0];
        }

        SetPicture();
    }

    public void DoRightRotation()
    {
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

    private bool ValidatePictureName(string name)
    {
        return _imageExtentions.Any(extention => name.ToUpper().EndsWith(extention));
    }


    private async Task ShowErrorMessageBox(string message)
    {
        var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams
            {
                ContentTitle = "Ошибка",
                ContentMessage = message
            });
        await messageBoxStandardWindow.Show();
    }
}

public class ImageManagerException : Exception
{
    public ImageManagerException(string message) : base(message)
    {
    }
}
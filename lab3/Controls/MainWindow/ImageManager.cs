using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DynamicData;
using lab3.Controls.GL;
using lab3.Serialization;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace lab3.Controls.MainWindow;

[Serializable]
public class ImageManager : IDisposable
{
    private readonly IEnumerable<string> _imageExtensions;

    private BehaviorSubject<ImgBitmap> _bitmap;

    private string _currentPicture;

    private Image<Rgba32> _image;
    [XmlIgnore] public FilterManager FilterManager { get; }
    
    private string[] _picturesInFolder;
    
    [XmlIgnore] public IObservable<ImgBitmap> BitmapChanged => _bitmap.AsObservable();
    public RotateMode CurrentRotationMode { get; set; }

    private IImageManagerSerializer _serializer;

    public ImageManager()
    {
        _serializer = new BackUpImageManagerSerializer("../../../ImageManagerBackUp.xml");
        
        _bitmap = new BehaviorSubject<ImgBitmap>(new ImgBitmap());
        
        FilterManager = new FilterManager();

        FilterManager = FilterManager.Deseralize();
        
        _imageExtensions = new[] { ".JPG", ".JPEG", ".PNG" };
        
        CurrentRotationMode = RotateMode.None;

        PicturesInFolder = new[] { "../../../Assets/texture.jpg" };
        
        var t = _serializer.LoadBackUp();

        PicturesInFolder = t.PicturesInFolder;
        CurrentRotationMode = t.CurrentRotationMode;
        CurrentPicture = t.CurrentPicture;
        
        SetPicture();
    }
    
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
        
        _serializer.BackUp(this);
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
        return _imageExtensions.Any(extention => name.ToUpper().EndsWith(extention));
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
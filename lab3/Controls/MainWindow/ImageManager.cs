using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DynamicData;
using lab3.Controls.GL;
using lab3.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static lab3.Controls.MainWindow.UIMessageBox;

namespace lab3.Controls.MainWindow;

public class ImageManagerState
{
    public string CurrentPicture;

    public string[] PicturesInFolder;
    public RotateMode RotateMode;

    public ImageManagerState()
    {
    }

    public ImageManagerState(ImageManager manager)
    {
        RotateMode = manager.CurrentRotationMode;
        CurrentPicture = manager.CurrentPicture;
        PicturesInFolder = manager.PicturesInFolder;
    }
}

[Serializable]
public class ImageManager : IDisposable
{
    private readonly IEnumerable<string> _imageExtensions;

    private BehaviorSubject<ImgBitmap> _bitmap;

    private string _currentPicture;

    private Image<Rgba32> _image;

    private string[] _picturesInFolder;

    private IImageManagerSerializer _serializer;

    private ImageManager(ImageManagerState state)
    {
        _serializer = new BackUpImageManagerSerializer("../../../ImageManagerBackUp.xml");

        _bitmap = new BehaviorSubject<ImgBitmap>(new ImgBitmap());

        FilterManager = FilterManager.Deseralize();

        _imageExtensions = new[] { ".JPG", ".JPEG", ".PNG" };

        CurrentRotationMode = RotateMode.None;

        PicturesInFolder = new[] { "../../../Assets/START.png" };

        PicturesInFolder = state.PicturesInFolder;
        CurrentRotationMode = state.RotateMode;
        CurrentPicture = state.CurrentPicture;
    }

    [XmlIgnore] public FilterManager FilterManager { get; }

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
                ShowError("Некорректное значение для просматриваемой картинки");
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
                ShowError("Некорректное значение для списка картинок в папке");
                return;
            }
            var pictures = value.Where(ValidatePictureName).ToArray();

            if (pictures is not null && pictures.Length > 0)
            {
                _picturesInFolder = pictures;
                CurrentPicture = PicturesInFolder[0];
                SetPicture();
            }
            else
            {
                ShowError("В папке нет ни одной фотографии корректного расширения\n" + "PNG, JPEG, JPG");
            }
        }
    }


    public void Dispose()
    {
        _image?.Dispose();
        _bitmap.Dispose();
    }

    public static ImageManager Deserialize()
    {
        try
        {
            var serializer = new BackUpImageManagerSerializer("../../../ImageManagerBackUp.xml");
            var manager = new ImageManager(serializer.LoadBackUp());
            return manager;
        }
        catch (Exception)
        {
            return new ImageManager(new ImageManagerState());
        }
    }


    public void SetPicture()
    {
        _image?.Dispose();
        try
        {
            _image = Image.Load<Rgba32>(CurrentPicture);
            _image.Mutate(context => context.Rotate(CurrentRotationMode));
            SetPixels();
        }
        catch (DirectoryNotFoundException dirEx)
        {
            ShowError("Не удалось открыть файл\nПопробуйте открыть другую папку");
            CurrentRotationMode = RotateMode.None;
            PicturesInFolder = new[] { "../../../Assets/texture.jpg" };
            SetPicture();
        }
        catch (Exception exception)
        {
            ShowError("Необработанная ошибка. Обратитесь, пожалуйста, в поддержку");
            CurrentRotationMode = RotateMode.None;
            PicturesInFolder = new[] { "../../../Assets/texture.jpg" };
            SetPicture();
        }
    }


    private void SetPixels()
    {
        var pixels = new byte[_image.Width * 4 * _image.Height];
        _image.CopyPixelDataTo(pixels);
        _bitmap.OnNext(new ImgBitmap(_image.Width, _image.Height, pixels));

        _serializer.BackUp(new ImageManagerState(this));
    }

    public Task SwipeLeft()
    {
        CurrentRotationMode = RotateMode.None;
        FilterManager.DisableAllFilters();

        if (PicturesInFolder is not null)
        {
            var newIndex = PicturesInFolder.IndexOf(CurrentPicture) - 1;
            CurrentPicture =
                PicturesInFolder[newIndex > -1 ? newIndex : PicturesInFolder.Length - 1];
        }

        SetPicture();
        return Task.CompletedTask;
    }

    public Task SwipeRight()
    {
        CurrentRotationMode = RotateMode.None;
        FilterManager.DisableAllFilters();

        if (PicturesInFolder is not null)
        {
            var newIndex = PicturesInFolder.IndexOf(CurrentPicture) + 1;
            CurrentPicture = PicturesInFolder[newIndex < PicturesInFolder.Length ? newIndex : 0];
        }

        SetPicture();
        return Task.CompletedTask;
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
}

public class ImageManagerException : Exception
{
    public ImageManagerException(string message) : base(message)
    {
    }
}
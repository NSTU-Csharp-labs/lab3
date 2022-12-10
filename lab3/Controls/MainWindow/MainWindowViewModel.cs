﻿using System;using System.Collections.Generic;using System.Reactive;using System.Reactive.Linq;using System.Threading.Tasks;using System.Xml.Serialization;using DynamicData;using lab3.Controls.GL;using lab3.ViewModels;using ReactiveUI;using SixLabors.ImageSharp;using SixLabors.ImageSharp.PixelFormats;using SixLabors.ImageSharp.Processing;namespace lab3.Controls.MainWindow;[Serializable]public class MainWindowViewModel : ViewModelBase{    private bool _isVisibleEffectsElements = false;    private bool _isVisibleEffectBtn = true;    private bool _isVisibleRotateBtn = true;    private bool _isVisibleDrawBtn = true;    private bool _isVisibleSaveBtn = true;    private bool _blackAndWhiteFilter = false;    private bool _blueFilter = false;    private bool _greenFilter = false;    private bool _redFilter = false;    private bool _isCheckedBW = false;    private bool _isCheckedR = false;    private bool _isCheckedG = false;    private bool _isCheckedB = false;    private BackUpSerializer _serializer;    [field: NonSerialized] [XmlIgnore] private ImgBitmap _img;       public MainWindowViewModel()    {        _imageExtentions = new List<string>() {".JPG", ".JPEG", ".PNG" };                _rotateMode =  (int)RotateMode.None;                PathToCurrentPicture = new string("../../../Assets/texture.jpg");        _serializer = new BackUpSerializer("../../../BackUp.xml");        ShowOpenImageDialog = new Interaction<Unit, string[]>();        SetPicture();        OpenImage = ReactiveCommand.CreateFromTask(OnOpenImage);            Rotate = ReactiveCommand.Create(DoRotation);                SwipeLeft = ReactiveCommand.Create(() =>        {            var newIndex = (PicturesInFolder.IndexOf(PathToCurrentPicture) - 1) ;            PathToCurrentPicture = PicturesInFolder[newIndex > -1 ? newIndex : PicturesInFolder.Length];            SetPicture();        });        SwipeRight = ReactiveCommand.Create(() =>        {            var newIndex =PicturesInFolder.IndexOf(PathToCurrentPicture) + 1;            PathToCurrentPicture = PicturesInFolder[newIndex < PicturesInFolder.Length ? newIndex : 0];            SetPicture();        });                        EnableEffects = ReactiveCommand.Create(() =>        {            BlueFilter = IsCheckedB;            GreenFilter = IsCheckedG;            RedFilter = IsCheckedR;            BlackAndWhiteFilter = IsCheckedBW;        });        OpenEffects = ReactiveCommand.Create(() =>        {            IsVisibleEffectsElements = true;            IsVisibleSaveBtn = false;            IsVisibleDrawBtn = false;            IsVisibleEffectBtn = false;            IsVisibleRotateBtn = false;        });        BackFromEffects = ReactiveCommand.Create(() =>        {            IsVisibleEffectsElements = false;            IsVisibleSaveBtn = true;            IsVisibleDrawBtn = true;            IsVisibleEffectBtn = true;            IsVisibleRotateBtn = true;        });    }    public readonly Interaction<Unit, string[]> ShowOpenImageDialog;    public ReactiveCommand<Unit, Unit> OpenImage { get; }    private string[] _picturesInFolder;    public string[] PicturesInFolder    {        get => _picturesInFolder;        set => this.RaiseAndSetIfChanged(ref _picturesInFolder, value);    }    private string _pathToCurrentPicture;    public string PathToCurrentPicture    {        get => _pathToCurrentPicture;        set        {            foreach (var extention in _imageExtentions)            {                if (value.ToUpper().EndsWith(extention))                {                    this.RaiseAndSetIfChanged(ref _pathToCurrentPicture, value);                    break;                }            }                    }    }    private List<string> _imageExtentions;    public void SetPicture()    {        var img = Image.Load<Rgba32>(PathToCurrentPicture);        img.Mutate(context => context.Rotate((RotateMode)_rotateMode));        var pixels = new byte[img.Width * 4 * img.Height];        img.CopyPixelDataTo(pixels);        Img = new ImgBitmap(img.Width, img.Height, pixels);        img.Dispose();    }    public int _rotateMode { get; set; }        private void DoRotation()    {        _rotateMode = (RotateMode)_rotateMode switch        {            RotateMode.None => (int)RotateMode.Rotate270,            RotateMode.Rotate270 => (int)RotateMode.Rotate180,            RotateMode.Rotate180 => (int)RotateMode.Rotate90,            RotateMode.Rotate90 => (int)RotateMode.None,            _ => throw new ArgumentOutOfRangeException()        };        SetPicture();    }    private async Task OnOpenImage()    {        try        {            var dirPictures = await ShowOpenImageDialog.Handle(Unit.Default);            if (dirPictures is not null)            {                _rotateMode = (int)RotateMode.None;                PicturesInFolder = dirPictures;                PathToCurrentPicture = dirPictures[0];                SetPicture();            }        }        catch        {            // ignored        }    }    public ImgBitmap Img    {        get => _img;        set        {            this.RaiseAndSetIfChanged(ref _img, value);            _serializer.BackUp(this);        }    }    public bool IsCheckedB    {        get => _isCheckedB;        set        {            this.RaiseAndSetIfChanged(ref _isCheckedB, value);            _serializer.BackUp(this);        }    }    public bool IsCheckedG    {        get => _isCheckedG;        set        {            this.RaiseAndSetIfChanged(ref _isCheckedG, value);            _serializer.BackUp(this);        }    }    public bool IsCheckedR    {        get => _isCheckedR;        set        {            this.RaiseAndSetIfChanged(ref _isCheckedR, value);            _serializer.BackUp(this);        }    }    public bool IsCheckedBW    {        get => _isCheckedBW;        set        {            this.RaiseAndSetIfChanged(ref _isCheckedBW, value);            _serializer.BackUp(this);        }    }    public bool BlackAndWhiteFilter    {        get => _blackAndWhiteFilter;        set        {            this.RaiseAndSetIfChanged(ref _blackAndWhiteFilter, value);            _serializer.BackUp(this);        }    }    public bool GreenFilter    {        get => _greenFilter;        set        {            this.RaiseAndSetIfChanged(ref _greenFilter, value);            _serializer.BackUp(this);        }    }    public bool RedFilter    {        get => _redFilter;        set        {            this.RaiseAndSetIfChanged(ref _redFilter, value);            _serializer.BackUp(this);        }    }    public bool BlueFilter    {        get => _blueFilter;        set        {            this.RaiseAndSetIfChanged(ref _blueFilter, value);            _serializer.BackUp(this);        }    }    public bool IsVisibleEffectBtn    {        get => _isVisibleEffectBtn;        set        {            this.RaiseAndSetIfChanged(ref _isVisibleEffectBtn, value);            _serializer.BackUp(this);        }    }    public bool IsVisibleSaveBtn    {        get => _isVisibleSaveBtn;        set        {            this.RaiseAndSetIfChanged(ref _isVisibleSaveBtn, value);            _serializer.BackUp(this);        }    }    public bool IsVisibleRotateBtn    {        get => _isVisibleRotateBtn;        set        {            this.RaiseAndSetIfChanged(ref _isVisibleRotateBtn, value);            _serializer.BackUp(this);        }    }    public bool IsVisibleDrawBtn    {        get => _isVisibleDrawBtn;        set        {            this.RaiseAndSetIfChanged(ref _isVisibleDrawBtn, value);            _serializer.BackUp(this);        }    }    public bool IsVisibleEffectsElements    {        get => _isVisibleEffectsElements;        set        {            this.RaiseAndSetIfChanged(ref _isVisibleEffectsElements, value);            _serializer.BackUp(this);        }    }    public ReactiveCommand<Unit, Unit> OpenEffects { get; }    public ReactiveCommand<Unit, Unit> EnableEffects { get; }    public ReactiveCommand<Unit, Unit> BackFromEffects { get; }    public ReactiveCommand<Unit, Unit> Rotate { get; }    public ReactiveCommand<Unit, Unit> SwipeLeft { get; }    public ReactiveCommand<Unit, Unit> SwipeRight { get; }}
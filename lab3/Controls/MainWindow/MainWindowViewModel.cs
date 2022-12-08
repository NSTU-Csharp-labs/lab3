﻿using System.Reactive;using System.Reactive.Linq;using lab3.Controls.GL;using lab3.ViewModels;using ReactiveUI;using SixLabors.ImageSharp;using SixLabors.ImageSharp.PixelFormats;namespace lab3.Controls.MainWindow;public class MainWindowViewModel : ViewModelBase{    private bool _isVisibleEffectsElements = false;    private bool _isVisibleEffectBtn = true;    private bool _isVisibleRotateBtn = true;    private bool _isVisibleDrawBtn = true;    private bool _isVisibleSaveBtn = true;    private bool _blackAndWhiteFilter = false;    private bool _blueFilter = false;    private bool _greenFilter = false;    private bool _redFilter = false;    private bool _isCheckedBW = false;    private bool _isCheckedR = false;    private bool _isCheckedG = false;    private bool _isCheckedB = false;    public ImgBitmap Img    {        get => _img;        set => this.RaiseAndSetIfChanged(ref _img, value);    }    private ImgBitmap _img;        public ReactiveCommand<Unit, Unit> Save { get; }    public Interaction<Unit, string> ShowOpenImageDialog;    public ReactiveCommand<Unit, Unit> OpenImage { get; }    private string _pathToPicture;    public string PathToPicture    {        get => _pathToPicture;        set => this.RaiseAndSetIfChanged(ref _pathToPicture, value);    }    private void SetPicture()    {        var img = Image.Load<Rgba32>(PathToPicture);        byte[] pixels = new byte[img.Width * 4 * img.Height];        img.CopyPixelDataTo(pixels);        Img = new ImgBitmap(img.Width, img.Height, pixels);    }    public MainWindowViewModel()    {        PathToPicture = new string("../../../Assets/texture.jpg");        SetPicture();        ShowOpenImageDialog = new Interaction<Unit, string>();        OpenImage = ReactiveCommand.CreateFromTask(async () =>        {            var path =  await ShowOpenImageDialog.Handle(Unit.Default);            if (path is not null)            {                PathToPicture = path;                SetPicture();            }        });                PathToPicture = new string("../../../Assets/texture.jpg");        // Save = ReactiveCommand.Create((_));                EnableEffects = ReactiveCommand.Create(() =>        {            BlueFilter = IsCheckedB;            GreenFilter = IsCheckedG;            RedFilter = IsCheckedR;            BlackAndWhiteFilter = IsCheckedBW;        });                OpenEffects = ReactiveCommand.Create(() =>        {            IsVisibleEffectsElements = true;            IsVisibleSaveBtn = false;            IsVisibleDrawBtn = false;            IsVisibleEffectBtn = false;            IsVisibleRotateBtn = false;        });                BackFromEffects = ReactiveCommand.Create(() =>        {            IsVisibleEffectsElements = false;            IsVisibleSaveBtn = true;            IsVisibleDrawBtn = true;            IsVisibleEffectBtn = true;            IsVisibleRotateBtn = true;        });    }    public bool IsCheckedB    {        get => _isCheckedB;        set => this.RaiseAndSetIfChanged(ref _isCheckedB, value);    }        public bool IsCheckedG    {        get => _isCheckedG;        set => this.RaiseAndSetIfChanged(ref _isCheckedG, value);    }        public bool IsCheckedR    {        get => _isCheckedR;        set => this.RaiseAndSetIfChanged(ref _isCheckedR, value);    }        public bool IsCheckedBW    {        get => _isCheckedBW;        set => this.RaiseAndSetIfChanged(ref _isCheckedBW, value);    }        public bool BlackAndWhiteFilter    {        get => _blackAndWhiteFilter;        set => this.RaiseAndSetIfChanged(ref _blackAndWhiteFilter, value);    }        public bool GreenFilter    {        get => _greenFilter;        set => this.RaiseAndSetIfChanged(ref _greenFilter, value);    }        public bool RedFilter    {        get => _redFilter;        set => this.RaiseAndSetIfChanged(ref _redFilter, value);    }        public bool BlueFilter    {        get => _blueFilter;        set => this.RaiseAndSetIfChanged(ref _blueFilter, value);    }        public bool IsVisibleEffectBtn    {        get => _isVisibleEffectBtn;        set => this.RaiseAndSetIfChanged(ref _isVisibleEffectBtn, value);    }        public bool IsVisibleSaveBtn    {        get => _isVisibleSaveBtn;        set => this.RaiseAndSetIfChanged(ref _isVisibleSaveBtn, value);    }        public bool IsVisibleRotateBtn    {        get => _isVisibleRotateBtn;        set => this.RaiseAndSetIfChanged(ref _isVisibleRotateBtn, value);    }        public bool IsVisibleDrawBtn    {        get => _isVisibleDrawBtn;        set => this.RaiseAndSetIfChanged(ref _isVisibleDrawBtn, value);    }        public bool IsVisibleEffectsElements    {        get => _isVisibleEffectsElements;        set => this.RaiseAndSetIfChanged(ref _isVisibleEffectsElements, value);    }        public ReactiveCommand<Unit, Unit> OpenEffects { get; }    public ReactiveCommand<Unit, Unit> EnableEffects { get; }    public ReactiveCommand<Unit, Unit> BackFromEffects { get; }}
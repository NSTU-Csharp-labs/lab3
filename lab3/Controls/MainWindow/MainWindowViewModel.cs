﻿using System;using System.Collections.Generic;using System.IO;using System.Linq;using System.Reactive;using System.Reactive.Disposables;using System.Reactive.Linq;using System.Text.Json;using System.Threading;using System.Threading.Tasks;using System.Xml;using System.Xml.Serialization;using lab3.Controls.GL;using lab3.Serialization;using lab3.ViewModels;using OpenTK.Windowing.GraphicsLibraryFramework;using ReactiveUI;using ReactiveUI.Fody.Helpers;using SixLabors.ImageSharp;using SixLabors.ImageSharp.Formats.Jpeg;using SixLabors.ImageSharp.PixelFormats;using Image = SixLabors.ImageSharp.Image;namespace lab3.Controls.MainWindow;[Serializable]public class MainWindowViewModel : ViewModelBase, IActivatableViewModel{    public readonly Interaction<Unit, string[]> ShowOpenDirectoryDialog;        private FiltersManager _filtersManager;        public IEnumerable<FilterButtonViewModel> FilterButtons { get; }    private ImgBitmap _bitmap;    private bool _isActionsVisible = true;    private IImageManagerSerializer _imageManagerSerializer;    private IEnumerable<Filter>? AllFilters { get; }    private const string PathToFilters = "../../../Filters.xml";         public MainWindowViewModel()    {        _imageManagerSerializer = new BackUpImageManagerSerializer("../../../BackUp.xml");        ImageManager = _imageManagerSerializer.LoadBackUp();        AllFilters = new FilterSerializer(PathToFilters).LoadFilters();                Activator = new ViewModelActivator();        Bitmap = new ImgBitmap(0, 0, new byte[] { });                ShowOpenDirectoryDialog = new Interaction<Unit, string[]>();                OpenImage = ReactiveCommand.CreateFromTask(OnOpenDirectory);                _filtersManager = ImageManager.FiltersManager;                FilterButtons = AllFilters.Select(filter =>        {            var enabledFilters = _filtersManager.SelectedFilters.Select(filters =>                filters.Where(f => f.Name != filter.Name));                        return new FilterButtonViewModel(enabledFilters, filter, ImageManager);        });                        CloseFilters = ReactiveCommand.Create(() =>        {                    });                // g.SaveAsJpeg("/home/xpomin/RiderProjects/lab3/lab3/Assets/ASD.jpeg");        SaveImage = ReactiveCommand.Create(() =>        {                });                Rotate = ReactiveCommand.CreateFromTask(async () =>        {            ImageManager.DoRightRotation();            await _imageManagerSerializer.BackUp(ImageManager);        });                SwipeLeft = ReactiveCommand.CreateFromTask(async () =>        {            ImageManager.SwipeLeft();            await _imageManagerSerializer.BackUp(ImageManager);        });                SwipeRight = ReactiveCommand.CreateFromTask(async () =>        {            ImageManager.SwipeRight();            await _imageManagerSerializer.BackUp(ImageManager);        });        // EnableEffects = ReactiveCommand.Create(() =>        // {        //     BlueFilter = IsCheckedB;        //     GreenFilter = IsCheckedG;        //     RedFilter = IsCheckedR;        //     BlackAndWhiteFilter = IsCheckedBW;        // });                OpenFilters = ReactiveCommand.Create(() =>        {            IsFiltersVisible = true;            IsActionsVisible = false;        });                CloseFilters = ReactiveCommand.Create(() =>        {            IsFiltersVisible = false;            IsActionsVisible = true;        });                this.WhenActivated(d =>            {                ImageManager.BitmapChanged.Subscribe(async bitmap =>                {                    Bitmap = bitmap;                    await _imageManagerSerializer.BackUp(ImageManager);                }).DisposeWith(d);                ImageManager.DisposeWith(d);            }        );    }    public ImageManager ImageManager { get; set; }    [XmlIgnore]    public ImgBitmap Bitmap    {        get => _bitmap;        private set => this.RaiseAndSetIfChanged(ref _bitmap, value);    }        [Reactive] public bool IsActionsVisible { get; set; }    [Reactive] public bool IsFiltersVisible { get; set; }    public ReactiveCommand<Unit, Unit> SaveImage { get; }    public ReactiveCommand<Unit, Unit> OpenFilters { get; }    public ReactiveCommand<Unit, Unit> CloseFilters { get; }    [XmlIgnore] public ReactiveCommand<Unit, Unit> Rotate { get; }    [XmlIgnore] public ReactiveCommand<Unit, Unit> SwipeLeft { get; }    [XmlIgnore] public ReactiveCommand<Unit, Unit> SwipeRight { get; }    public ReactiveCommand<Unit, Unit> OpenImage { get; }    public ViewModelActivator Activator { get; }    private async Task OnOpenDirectory()    {        try        {            ImageManager.PicturesInFolder = await ShowOpenDirectoryDialog.Handle(Unit.Default);            await _imageManagerSerializer.BackUp(ImageManager);        }        catch (Exception)        {            // ImageManager.ResetPictures();        }    }}
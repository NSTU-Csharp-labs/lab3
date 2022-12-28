﻿using System;using System.Collections.Generic;using System.Linq;using System.Reactive;using System.Reactive.Disposables;using System.Reactive.Linq;using System.Threading.Tasks;using lab3.Controls.GL;using lab3.ViewModels;using ReactiveUI;using ReactiveUI.Fody.Helpers;namespace lab3.Controls.MainWindow;public class MainWindowViewModel : ViewModelBase, IActivatableViewModel{    public readonly Interaction<Unit, string[]> ShowOpenDirectoryDialog;    public IEnumerable<FilterButtonViewModel> FilterButtons { get; set; }    [ObservableAsProperty] public ImgBitmap Bitmap { get; }    [ObservableAsProperty] public IEnumerable<Filter> SelectedFilters { get; }    public MainWindowViewModel()    {        Activator = new ViewModelActivator();        ImageManager = ImageManager.Deserialize();        ImageManager.FilterManager.SelectedFilters.ToPropertyEx(this, vm => vm.SelectedFilters);        ImageManager.BitmapChanged.ToPropertyEx(this, vm => vm.Bitmap);        FilterButtons = ImageManager.FilterManager.AllFilters.Select(filter =>            {                return new FilterButtonViewModel(                    filter,                    ImageManager);            }        );        this.WhenActivated(d => ImageManager.DisposeWith(d));        IsActionsVisible = true;        ShowOpenDirectoryDialog = new Interaction<Unit, string[]>();        OpenImage = ReactiveCommand.CreateFromTask(OnOpenDirectory);        CloseFilters = ReactiveCommand.Create(() => { });        SaveImage = ReactiveCommand.Create(() => { });        Rotate = ReactiveCommand.Create(() => ImageManager.DoRightRotation());        SwipeLeft = ReactiveCommand.CreateFromTask(() => ImageManager.SwipeLeft());        SwipeRight = ReactiveCommand.CreateFromTask(() => ImageManager.SwipeRight());        OpenFilters = ReactiveCommand.Create(() =>        {            IsFiltersVisible = true;            IsActionsVisible = false;        });        CloseFilters = ReactiveCommand.Create(() =>        {            IsFiltersVisible = false;            IsActionsVisible = true;        });    }    public ImageManager ImageManager { get; set; }    [Reactive] public bool IsActionsVisible { get; set; }    [Reactive] public bool IsFiltersVisible { get; set; }    public ReactiveCommand<Unit, Unit> SaveImage { get; }    public ReactiveCommand<Unit, Unit> OpenFilters { get; }    public ReactiveCommand<Unit, Unit> CloseFilters { get; }    public ReactiveCommand<Unit, Unit> Rotate { get; }    public ReactiveCommand<Unit, Unit> SwipeLeft { get; }    public ReactiveCommand<Unit, Unit> SwipeRight { get; }    public ReactiveCommand<Unit, Unit> OpenImage { get; }    public ViewModelActivator Activator { get; }    private async Task OnOpenDirectory()    {        try        {            ImageManager.PicturesInFolder = await ShowOpenDirectoryDialog.Handle(Unit.Default);        }        catch (Exception)        {            // ImageManager.ResetPictures();        }    }}
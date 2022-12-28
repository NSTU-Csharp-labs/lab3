using System;using System.Collections.Generic;using System.Reactive;using System.Reactive.Linq;using lab3.Controls.GL;using ReactiveUI;using ReactiveUI.Fody.Helpers;namespace lab3.Controls.MainWindow;public class FilterButtonViewModel : ReactiveObject{    [Reactive] public bool IsEnabled { get; set; }    public IEnumerable<Filter> EnabledFilters { get; }    [ObservableAsProperty] public ImgBitmap ImgBitmap { get; }    public string Title { get; }        public FilterButtonViewModel(Filter currentFilter, ImageManager imageManager)    {        EnabledFilters = new Filter[] { currentFilter };        Title = currentFilter.Title;        imageManager.BitmapChanged.ToPropertyEx(this, vm => vm.ImgBitmap);                Switch = ReactiveCommand.Create(() =>        {            if (IsEnabled)            {                imageManager.FilterManager.EnableFilter(currentFilter);            }            else            {                imageManager.FilterManager.DisableFilter(currentFilter);            }        });    }        public ReactiveCommand<Unit, Unit> Switch { get; }}
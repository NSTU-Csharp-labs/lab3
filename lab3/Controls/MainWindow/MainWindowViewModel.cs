﻿using System;using System.Reactive;using ReactiveUI;using lab3.ViewModels;namespace lab3.Controls.MainWindow{    public class MainWindowViewModel : ViewModelBase    {        public MainWindowViewModel()        {            Test = ReactiveCommand.Create(() => Console.WriteLine("good"));        }        public ReactiveCommand<Unit, Unit> Test { get;  }    }}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Extensions;
using ADVNow.Models;
using ADVNow.Commands;
using System.IO;
using NovelGameLib.Database;
using NovelGameLib;
using NovelGameLib.Entity;
using System.Linq;
using ModernWpf.Controls;
using System.Reflection.Metadata;

namespace ADVNow.ViewModels
{
    internal class SettingGameViewModel : INotifyPropertyChanged, IAddGameViewModel
    {
        public ReactiveProperty<string> Path { get; set; } = new ReactiveProperty<string>();

        public ICommand SelectGamePathCmd { get; set; }

        public ICommand SettingGameSubmitCmd { get; set; }

        public Action CloseWindowAction { get; set; }

        public MainWindowViewModel MainVM { get => this._mainVM; }

        public MainWindowViewModel _mainVM;

        public SettingGameViewModel(MainWindowViewModel vm)
        {
            this._mainVM = vm;
            this.Path.Value = this._mainVM.CurrentGame.Value.Path;
            this.SelectGamePathCmd = new SelectGamePathCommand(this);
            this.SettingGameSubmitCmd = new SettingGameSubmitCommand(this);
        }

        public bool canExcuteCommand()
        {
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}

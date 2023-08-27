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
    internal class AddGameViewModel : INotifyPropertyChanged
    {
        public ReactiveProperty<string> SearchGameString { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> Path { get; set; } = new ReactiveProperty<string>();

        public ReactiveCollection<string> SuggestedGameStrings { get; set; } = new ReactiveCollection<string>();

        public ReactiveCollection<Game> SuggestedGames { get; set; } = new ReactiveCollection<Game>();

        public ICommand SelectGamePathCmd { get; set; }

        public ICommand AddNewGameCmd { get; set; }

        public Action CloseWindowAction { get; set; }

        public MainWindowViewModel _mainVM;

        public AddGameViewModel(MainWindowViewModel vm)
        {
            this._mainVM = vm;
            this.SearchGameString.Value = "";
            this.Path.Value = "";
            this.SelectGamePathCmd = new SelectGamePathCommand(this);
            this.AddNewGameCmd = new AddNewGameCommand(this);
            this.SearchGameString.Subscribe(async (t) =>
            {
                this.SuggestedGameStrings.Clear();
                if (t == "") return;
                List<NovelGame> games = await this._mainVM.API.SearchGames(t);
                foreach (NovelGame game in games)
                {
                    if (!this.SuggestedGameStrings.ToList().Contains(game.Title))
                    {
                        this.SuggestedGameStrings.Add(game.Title);
                    }
                }
            });
        }

        public bool canExcuteCommand()
        {
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}

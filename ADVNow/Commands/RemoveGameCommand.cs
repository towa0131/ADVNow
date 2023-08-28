using ADVNow.Models;
using ADVNow.ViewModels;
using DiscordRPC;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Timers;
using NovelGameLib.Entity;

namespace ADVNow.Commands
{
    internal class RemoveGameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _vm;

        public RemoveGameCommand(MainWindowViewModel vm)
        {
            this._vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Game game = this._vm.CurrentGame.Value;
            if (MessageBox.Show(game.Title + " を削除しますか？", "", MessageBoxButton.YesNo,
                MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                this._vm.AllGames.Remove(game);
            }
        }
    }
}
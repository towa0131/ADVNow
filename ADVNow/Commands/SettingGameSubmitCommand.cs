using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ADVNow.Views;
using ADVNow.Models;
using ADVNow.ViewModels;
using NovelGameLib.Entity;

namespace ADVNow.Commands
{
    internal class SettingGameSubmitCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SettingGameViewModel _vm;

        public SettingGameSubmitCommand(SettingGameViewModel vm)
        {
            this._vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (!System.IO.File.Exists(this._vm.Path.Value))
            {
                MessageBox.Show("実行ファイルが見つかりませんでした。");
                return;
            }
            else
            {
                Game game = this._vm.MainVM.CurrentGame.Value;
                game.Path = this._vm.Path.Value;

                for (int i = 0; i < this._vm.MainVM.AllGames.Count(); i++)
                {
                    if (this._vm.MainVM.AllGames[i].Title == game.Title)
                    {
                        this._vm.MainVM.AllGames[i] = game;
                        break;
                    }
                }

                this._vm.CloseWindowAction();
            }
        }
    }
}
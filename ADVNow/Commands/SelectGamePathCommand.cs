using ADVNow.Models;
using ADVNow.ViewModels;
using Microsoft.Win32;
using NovelGameLib.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ADVNow.Commands
{
    internal class SelectGamePathCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AddGameViewModel _vm;

        public SelectGamePathCommand(AddGameViewModel vm)
        {
            this._vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "exeファイル|*.exe";

            if (dialog.ShowDialog() == true)
            {
                string file = dialog.FileName;
                this._vm.Path.Value = file;
                string? folderPath = System.IO.Path.GetDirectoryName(file);
                if (folderPath != null)
                {
                    string folderName = System.IO.Path.GetFileName(folderPath);
                    NovelGame? game = await this._vm._mainVM.API.SearchGameByName(folderName);
                    if (game != null) this._vm.SearchGameString.Value = game.Title;
                }
            }
        }
    }
}
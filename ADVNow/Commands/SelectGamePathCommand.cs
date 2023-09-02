using ADVNow.Models;
using ADVNow.ViewModels;
using Microsoft.Win32;
using NovelGameLib.Entity;
using System;
using System.Collections.Generic;
using System.IO;
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

        private IAddGameViewModel _vm;

        public SelectGamePathCommand(IAddGameViewModel vm)
        {
            this._vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            string directory = "";
            if (this._vm.MainVM.CurrentGame.Value != null) directory = Path.GetDirectoryName(this._vm.MainVM.CurrentGame.Value.Path);
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "exeファイル|*.exe";
            if (Path.Exists(directory))
            {
                dialog.InitialDirectory = directory;
            }
            else
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ADVNow";
            }
            if (dialog.ShowDialog() == true)
            {
                string file = dialog.FileName;
                this._vm.Path.Value = file;
            }
        }
    }
}
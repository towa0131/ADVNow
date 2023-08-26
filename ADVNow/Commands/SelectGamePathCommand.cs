using ADVNow.Models;
using ADVNow.ViewModels;
using Microsoft.Win32;
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

        public void Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "exeファイル|*.exe";

            if (dialog.ShowDialog() == true)
            {
                string file = dialog.FileName;
                this._vm.Path.Value = file;
            }
        }
    }
}
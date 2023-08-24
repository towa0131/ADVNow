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
    internal class SetBackgroundCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _vm;

        public SetBackgroundCommand(MainWindowViewModel vm)
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
            dialog.Filter = "画像ファイル|*.png;*.bmp;*.jpg";

            if (dialog.ShowDialog() == true)
            {
                string file = dialog.FileName;
                this._vm.BackgroundImage.Value = file;
            }
        }
    }
}
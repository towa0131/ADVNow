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

namespace ADVNow.Commands
{
    internal class SettingGameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _vm;

        public SettingGameCommand(MainWindowViewModel vm)
        {
            this._vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            SettingGameViewModel vm = new SettingGameViewModel(this._vm);
            Window window = new SettingGameWindow()
            {
                DataContext = vm
            };
            vm.CloseWindowAction = new Action(window.Close);
            window.ShowDialog();
        }
    }
}
using ADVNow.Models;
using ADVNow.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ADVNow.Commands
{
    internal class LaunchGameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _vm;

        public LaunchGameCommand(MainWindowViewModel vm)
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
            if (game != null)
            {
                try
                {
                    string path = game.Path;
                    ProcessStartInfo pinfo = new ProcessStartInfo();
                    pinfo.FileName = path;
                    Process.Start(pinfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ゲームを起動することができませんでした。\n" +  ex.Message);
                }
            }
        }
    }
}
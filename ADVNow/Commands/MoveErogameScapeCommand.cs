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
    internal class MoveErogameScapeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _vm;

        public MoveErogameScapeCommand(MainWindowViewModel vm)
        {
            this._vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (this._vm.CurrentGame != null)
            {
                ProcessStartInfo pinfo = new ProcessStartInfo()
                {
                    FileName = "https://erogamescape.dyndns.org/~ap2/ero/toukei_kaiseki/game.php?game=" + this._vm.CurrentGame.Value.Id,
                    UseShellExecute = true,
                };

                Process.Start(pinfo);
            }
        }
    }
}
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
    internal class ShareGameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _vm;

        public ShareGameCommand(MainWindowViewModel vm)
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
                List<string> t = this._vm.PlayingTimeString.Value.Split(":").ToList();
                int totalPlayMinutes = this._vm.CurrentGame.Value.TotalPlayMinutes + Convert.ToInt32(t[0]) * 60 + Convert.ToInt32(t[1]);
                int hour = totalPlayMinutes / 60;
                int min = totalPlayMinutes % 60;
                string text = this._vm.CurrentGame.Value.Title + "をプレイ中！%0a" +
                              "現在のプレイ時間 : " + t[0] + "時間" + t[1] + "分%0a" +
                              "トータルプレイ時間 : " + String.Format("{0:00}", hour) + "時間" + String.Format("{0:00}", min) + "分%0a" +
                              "%23ADVNow";
                ProcessStartInfo pinfo = new ProcessStartInfo()
                {
                    FileName = "https://twitter.com/intent/tweet?text=" + text,
                    UseShellExecute = true,
                };

                Process.Start(pinfo);
            }
        }
    }
}
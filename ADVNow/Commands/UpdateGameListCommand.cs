using ADVNow.Models;
using ADVNow.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ADVNow.Commands
{
    internal class UpdateGameListCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _vm;

        public UpdateGameListCommand(MainWindowViewModel vm)
        {
            this._vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            List<Game> games = parameter as List<Game>;
            int type = this._vm.ShowType.Value;
            if (this._vm.SelectedList.Value == -1) return;
            switch (type)
            {
                case 0:
                    foreach (Game game in games)
                    {
                        if (this._vm.SelectedList.Value == 0 || this._vm.BrandList[this._vm.SelectedList.Value - 1] == game.Brand)
                        {
                            this._vm.Games.Add(game);
                        }
                    }
                    break;
                case 1:
                    foreach (Game game in games)
                    {
                        int year = Convert.ToInt32(game.SellDay.ToString("yyyy"));
                        if (this._vm.SelectedList.Value == 0 || this._vm.YearList[this._vm.SelectedList.Value - 1] == year)
                        {
                            this._vm.Games.Add(game);
                        }
                    }
                    break;
            }
        }
    }
}
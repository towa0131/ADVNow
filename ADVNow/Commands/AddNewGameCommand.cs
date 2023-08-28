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
    internal class AddNewGameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AddGameViewModel _vm;

        public AddNewGameCommand(AddGameViewModel vm)
        {
            this._vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            string gameStr = this._vm.SearchGameString.Value;
            NovelGame game = (await this._vm._mainVM.API.SearchGames(gameStr)).Where(game => game.Model == "PC").First();
            if (!System.IO.File.Exists(this._vm.Path.Value))
            {
                MessageBox.Show("実行ファイルが見つかりませんでした。");
                return;
            }
            if (game == null)
            {
                MessageBox.Show("ゲームが見つかりませんでした。");
                return;
            }
            else
            {
                if (game.BrandId.HasValue)
                {
                    this._vm.CloseWindowAction();
                    Brand brand = await this._vm._mainVM.API.SearchBrandById(game.BrandId.Value);
                    Game g = new Game()
                    {
                        Title = game?.Title ?? "",
                        Id = game?.Id ?? -1,
                        Brand = brand?.Name ?? "",
                        Path = this._vm.Path.Value,
                        SellDay = game.SellDay ?? new DateTime(0),
                        LastPlay = DateTime.Now,
                        TotalPlayMinutes = 0
                    };
                    this._vm._mainVM.AllGames.Add(g);
                    MessageBox.Show(gameStr + " を追加しました。");
                }
                else
                {
                    MessageBox.Show("ブランドが見つかりませんでした。");
                }
            }
        }
    }
}
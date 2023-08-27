using ADVNow.Models;
using ADVNow.ViewModels;
using DiscordRPC;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Timers;
using NovelGameLib.Entity;

namespace ADVNow.Commands
{
    internal class LaunchGameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _vm;

        private DiscordRpcClient _rpcClient;

        public string _token;

        public LaunchGameCommand(MainWindowViewModel vm, string token)
        {
            this._vm = vm;
            this._token = token;
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
                    Task launchTask = new Task(async () =>
                    {
                        string path = game.Path;
                        ProcessStartInfo pinfo = new ProcessStartInfo();
                        pinfo.FileName = path;
                        Process p = Process.Start(pinfo);
                        _rpcClient = new DiscordRpcClient(this._token);
                        this._rpcClient.Initialize();
                        Timer timer = new Timer(1000);
                        int totalSec = 0;
                        timer.Elapsed += (sender, args) => {
                            this._rpcClient.Invoke();
                            totalSec++;
                            int hour = totalSec / 3600;
                            int min = (totalSec % 3600) / 60;
                            int sec = totalSec % 60;
                            this._vm.PlayingTimeString.Value = String.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
                        };
                        timer.Start();
                        NovelGame ng = await this._vm.API.SearchGameByName(game.Title);
                        this._rpcClient.SetPresence(new RichPresence()
                        {
                            Details = game.Title + " をプレイ中",
                            State = game.Brand,
                            Timestamps = new Timestamps(DateTime.UtcNow),
                            Buttons = new Button[1]
                            {
                                new Button()
                                {
                                    Url = "https://example.com",
                                    Label = "Dummy"
                                }
                            }
                        });
                        if (ng != null)
                        {
                            if (Uri.IsWellFormedUriString(ng.OHP, UriKind.Absolute))
                            {
                                this._rpcClient.SetButton(new Button()
                                {
                                    Url = ng.OHP,
                                    Label = "ホームページ"
                                });
                            }
                            else
                            {
                                // Remove All Buttons
                                this._rpcClient.UpdateButtons();
                            }
                        }
                        this._vm.PlayingGameString.Value = game.Title + " をプレイ中";
                        this._vm.PlayingTimeString.Value = "00:00:00";
                        p.WaitForExit();
                        timer.EndInit();
                        this._rpcClient.Dispose();
                        this._vm.PlayingGameString.Value = "---";
                        this._vm.PlayingTimeString.Value = "";
                        this._vm.AllGames.Remove(game);
                        game.TotalPlayMinutes += totalSec / 60;
                        game.LastPlay = DateTime.Now;
                        this._vm.AllGames.Add(game);
                    });
                    launchTask.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ゲームを起動することができませんでした。\n" +  ex.Message);
                }
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Timers;
using System.Drawing;
using System.Net.Http;
using NovelGameLib.Entity;
using ADVNow.Models;
using ADVNow.ViewModels;
using Imgur.API.Endpoints;
using Imgur.API.Models;
using Imgur.API.Authentication;
using DiscordRPC;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using ADVNow.Utils;
using System.Drawing.Imaging;

namespace ADVNow.Commands
{
    internal class LaunchGameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _vm;

        private DiscordRpcClient _rpcClient;

        private ApiClient _imgurClient;

        private DateTime _startTime;

        private string _imageUrl;

        private bool _isShowing;

        private string _token;

        private Bitmap? _icon;

        public LaunchGameCommand(MainWindowViewModel vm, string rpcToken, string imgurToken)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            this._vm = vm;
            this._token = rpcToken;
            this._imgurClient = new ApiClient(imgurToken);
            this._imageUrl = "";
            this._icon = null;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this._isShowing = this._vm.DiscordStatus.Value;
            Game game = this._vm.CurrentGame.Value;
            if (this._vm.PlayingGameProcessId != -1)
            {
                MessageBox.Show("他のゲームがすでに起動されています。");
                return;
            }
            if (game != null)
            {
                if (File.Exists(game.Path))
                {
                    try
                    {
                        Task launchTask = new Task(async () =>
                        {
                            string path = game.Path;
                            string documentFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ADVNow";
                            string imagePath = documentFolder + "\\tmp.png";
                            ProcessStartInfo pinfo = new ProcessStartInfo();
                            pinfo.WorkingDirectory = Path.GetDirectoryName(path);
                            pinfo.FileName = path;
                            pinfo.UseShellExecute = false;
                            Process? p = Process.Start(pinfo);
                            this._vm.PlayingGameProcessId = p.Id;
                            _rpcClient = new DiscordRpcClient(this._token);
                            this._rpcClient.Initialize();
                            Timer timer = new Timer(1000);
                            this._startTime = DateTime.UtcNow;
                            int totalSec = 0;
                            timer.Elapsed += async (sender, args) => {
                                if (this._isShowing != this._vm.DiscordStatus.Value)
                                {
                                    this._isShowing = this._vm.DiscordStatus.Value;
                                    if (this._vm.DiscordStatus.Value)
                                    {
                                        this.SetPresence(game);
                                    }
                                    else
                                    {
                                        this._rpcClient.ClearPresence();
                                    }
                                }
                                if (this._isShowing) this._rpcClient.Invoke();
                                totalSec++;
                                int hour = totalSec / 3600;
                                int min = (totalSec % 3600) / 60;
                                int sec = totalSec % 60;
                                this._vm.PlayingTimeString.Value = String.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);

                                if (this._icon == null)
                                {
                                    IntPtr hwnd = p.MainWindowHandle;
                                    Bitmap? icon = WindowUtil.GetSmallWindowIcon(hwnd);
                                    if (icon != null)
                                    {
                                        this._icon = icon;
                                        try
                                        {
                                            using (MemoryStream memory = new MemoryStream())
                                            {
                                                using (FileStream fs = new FileStream(imagePath, FileMode.Create, FileAccess.ReadWrite))
                                                {
                                                    icon?.Save(memory, ImageFormat.Png);
                                                    byte[] bytes = memory.ToArray();
                                                    fs.Write(bytes, 0, bytes.Length);

                                                }
                                            }
                                            using (FileStream fileStream = File.OpenRead(imagePath))
                                            {
                                                using (HttpClient httpClient = new HttpClient())
                                                {
                                                    ImageEndpoint imageEndpoint = new ImageEndpoint(this._imgurClient, httpClient);
                                                    IImage imageUpload = await imageEndpoint.UploadImageAsync(fileStream);
                                                    this._imageUrl = imageUpload.Link;
                                                }
                                            }
                                            this.SetPresence(game);
                                            File.Delete(imagePath);
                                        }
                                        catch
                                        {
                                            // アイコンファイルがロックされている
                                        }
                                    }
                                }
                            };

                            if (this._isShowing) this.SetPresence(game);

                            this._vm.PlayingGameString.Value = game.Title + " をプレイ中";
                            this._vm.PlayingTimeString.Value = "00:00:00";
                            this._vm.ShareButtonVisibility.Value = "Visible";
                            timer.Start();
                            p?.WaitForExit();
                            timer.EndInit();
                            this._rpcClient.Dispose();
                            game.TotalPlayMinutes += totalSec / 60;
                            game.LastPlay = DateTime.Now;
                            for (int i = 0; i < this._vm.AllGames.Count(); i++)
                            {
                                if (this._vm.AllGames[i].Title == game.Title)
                                {
                                    this._vm.AllGames[i] = game;
                                    break;
                                }
                            }
                            this._vm.LoadGamesFromDB();
                            this._vm.PlayingGameString.Value = "---";
                            this._vm.PlayingTimeString.Value = "";
                            this._vm.ShareButtonVisibility.Value = "Hidden";
                            this._vm.PlayingGameProcessId = -1;
                            this._imageUrl = "";
                            this._icon = null;
                        });
                        launchTask.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ゲームを起動することができませんでした。\n" + ex.Message);
                    }
                }
                else
                {
                    if (MessageBox.Show("実行ファイルが見つかりません。\n" + game.Title + " を削除しますか？", "", MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        this._vm.AllGames.Remove(game);
                    }
                }
            }
        }

        private async void SetPresence(Game game)
        {
            NovelGame? ng = await this._vm.API.SearchGameByName(game.Title);
            RichPresence presence = new RichPresence()
            {
                Details = game.Title + " をプレイ中",
                State = game.Brand,
                Timestamps = new Timestamps(this._startTime),
                Buttons = new Button[1]
                {
                    new Button()
                    {
                        Url = Uri.IsWellFormedUriString(ng?.OHP, UriKind.Absolute) ? ng.OHP : "https://example.com",
                        Label = "ホームページ"
                    }
                },
                Assets = new Assets()
                {
                    LargeImageKey = this._imageUrl
                }
            };
            this._rpcClient.SetPresence(presence);
            if (ng == null)
            {
                // Remove All Buttons
                this._rpcClient.UpdateButtons();
            }
        }
    }
}
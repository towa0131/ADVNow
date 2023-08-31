using ADVNow.Utils;
using ADVNow.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Drawing;
using Imgur.API.Endpoints;
using Imgur.API.Models;
using System.IO;
using System.Net.Http;
using Imgur.API.Authentication;

namespace ADVNow.Commands
{
    internal class ShareGameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _vm;

        private ApiClient _imgurClient;

        public ShareGameCommand(MainWindowViewModel vm, string imgurToken)
        {
            this._vm = vm;
            this._imgurClient = new ApiClient(imgurToken);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (this._vm.CurrentGame != null)
            {
                string link = "";
                int pid = this._vm.PlayingGameProcessId;
                if (pid != -1 && this._vm.ShareWithImage.Value)
                {
                    Process p = Process.GetProcessById(pid);
                    if (p != null)
                    {
                        IntPtr hwnd = p.MainWindowHandle;
                        Bitmap? screenshot = WindowUtil.CaptureWindow(hwnd);
                        string imagePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ADVNow\\ss.png";
                        screenshot?.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

                        using (FileStream fileStream = File.OpenRead(imagePath))
                        {
                            using (HttpClient httpClient = new HttpClient())
                            {
                                ImageEndpoint imageEndpoint = new ImageEndpoint(this._imgurClient, httpClient);
                                IImage imageUpload = await imageEndpoint.UploadImageAsync(fileStream);
                                link = Path.ChangeExtension(imageUpload.Link, null);
                            }
                        }
                    }
                }

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
                    FileName = "https://twitter.com/intent/tweet?text=" + text + "&url=" + link,
                    UseShellExecute = true,
                };

                Process.Start(pinfo);
            }
        }
    }
}
using ADVNow.Utils;
using ADVNow.ViewModels;
using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Drawing;
using Microsoft.Toolkit.Uwp.Notifications;
using System.IO;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace ADVNow.Commands
{
    internal class ScreenshotCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowViewModel _vm;

        public ScreenshotCommand(MainWindowViewModel vm)
        {
            this._vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (this._vm.CurrentGame != null)
            {
                int pid = this._vm.PlayingGameProcessId;
                if (pid != -1 && this._vm.ShareWithImage.Value)
                {
                    Process p = Process.GetProcessById(pid);
                    if (p != null)
                    {
                        IntPtr hwnd = p.MainWindowHandle;
                        Bitmap? screenshot = WindowUtil.CaptureWindow(hwnd);
                        string ssDir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ADVNOW\\sceenshots";

                        if (!Directory.Exists(ssDir))
                        {
                            Directory.CreateDirectory(ssDir);
                        }


                        string titleDir = ssDir + "\\" + this._vm.CurrentGame.Value.Title;

                        if (!Directory.Exists(titleDir))
                        {
                            Directory.CreateDirectory(titleDir);
                        }

                        string imagePath = titleDir + "\\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss").ToString() + ".png";
                        Uri imageUri = new Uri("file:///" + imagePath);
                        screenshot?.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
                        new ToastContentBuilder().AddText("スクリーンショット完了！").AddInlineImage(imageUri).Show();
                    }
                }
            }
        }
    }
}
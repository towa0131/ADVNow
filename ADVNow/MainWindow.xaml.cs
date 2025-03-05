using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ADVNow.Models;
using ADVNow.ViewModels;
using ModernWpf;

namespace ADVNow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int HOTKEY_ID = 9000;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(WndProc);

            // Ctrl + Shift + A
            RegisterHotKey(handle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, 65);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                ((MainWindowViewModel)this.DataContext).ScreenshotCmd.Execute(null);
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).LaunchGameCmd.Execute(null);
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).SettingGameCmd.Execute(null);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).RemoveGameCmd.Execute(null);
        }

        private void ErogameScape_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).MoveErogameScapeCmd.Execute(null);
        }

        private void SearchGame_TextChanged(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == ModernWpf.Controls.AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ((MainWindowViewModel)this.DataContext).SearchGameString.Value = sender.Text;
            }
        }

        private void SearchGame_QuerySubmitted(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                ((MainWindowViewModel)this.DataContext).SearchGameString.Value = sender.Text;
            }
        }
        private void GameDataGrid_Row_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            (sender as DataGridCell).ContextMenu = GameDataGrid.Resources["DataGridContextMenu"] as ContextMenu;
        }
    }
}

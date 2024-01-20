using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
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

using ADVNow.ViewModels;
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
using System.Windows.Shapes;

namespace ADVNow.Views
{
    /// <summary>
    /// AddGameView.xaml の相互作用ロジック
    /// </summary>
    public partial class AddGameView : Window
    {
        public AddGameView()
        {
            InitializeComponent();
        }

        // ModernWpf の AutoSuggestBox は View から ViewModel への変更が通知されないので，Event で処理
        private void gameFind_TextChanged(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == ModernWpf.Controls.AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ((AddGameViewModel)this.DataContext).SearchGameString.Value = sender.Text;
            }
        }

        private void gameFind_QuerySubmitted(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                ((AddGameViewModel)this.DataContext).SearchGameString.Value = sender.Text;
            }
        }
    }
}

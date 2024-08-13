using StarRail_Launcher.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace StarRail_Launcher.Views
{
    /// <summary>
    /// UpdatePage.xaml 的交互逻辑
    /// </summary>
    public partial class UpdatePage : UserControl
    {
        public UpdatePage()
        {
            InitializeComponent();
            DataContext = new UpdatePageViewModel(DialogCoordinator.Instance);
        }
        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            ((Grid)Parent).Children.Remove(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

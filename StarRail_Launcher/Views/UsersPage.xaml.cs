using System.Windows;
using System.Windows.Controls;
using StarRail_Launcher.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace StarRail_Launcher.Views
{
    /// <summary>
    /// AddUsersPage.xaml 的交互逻辑
    /// </summary>
    public partial class UsersPage : UserControl
    {
        public UsersPage()
        {
            InitializeComponent();
            DataContext = new UsersPageViewModel(DialogCoordinator.Instance);
        }
    }
}

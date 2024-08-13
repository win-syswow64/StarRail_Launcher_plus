using StarRail_Launcher.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls;

namespace StarRail_Launcher.Views
{
    /// <summary>
    /// GuidePage.xaml 的交互逻辑
    /// </summary>
    public partial class GuidePage : UserControl
    {
        public GuidePage()
        {
            InitializeComponent();
            DataContext = new GuidePageViewModel(DialogCoordinator.Instance);
        }
    }
}

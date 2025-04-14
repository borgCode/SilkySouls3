using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkySouls3.ViewModels;

namespace SilkySouls3.Views
{
    public partial class UtilityTab
    {
        private readonly UtilityViewModel _utilityViewModel;

        public UtilityTab(UtilityViewModel utilityViewModel)
        {
            InitializeComponent();
            _utilityViewModel = utilityViewModel;
            DataContext = utilityViewModel;
        }

        private void Warp_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.Warp();
        }

        private void UnlockBonfires_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.UnlockBonfires();
        }

        private void UnlockMidir_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.UnlockMidir();
        }

        private void MovePatches_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.MovePatchesToFirelink();
        }
        
        private void NoClipInfoBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Vertical movement with Ctrl/Space or L2/R2 on controller", "Info", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
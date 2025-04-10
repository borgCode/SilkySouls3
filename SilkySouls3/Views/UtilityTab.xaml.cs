using System.Windows;
using System.Windows.Controls;
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
    }
}
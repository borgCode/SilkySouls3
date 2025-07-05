using System.Windows;
using SilkySouls3.ViewModels;

namespace SilkySouls3.Views
{
    public partial class TravelTab
    {
        private readonly TravelViewModel _travelViewModel;
        public TravelTab(TravelViewModel travelViewModel)
        {
            InitializeComponent();
            _travelViewModel = travelViewModel;
            DataContext = _travelViewModel;
        }

        private void WarpButton_Click(object sender, RoutedEventArgs e)
        {
            _travelViewModel.Warp();
        }

        private void UnlockAllWarps_Click(object sender, RoutedEventArgs e) => _travelViewModel.UnlockBonfires();
    }
}
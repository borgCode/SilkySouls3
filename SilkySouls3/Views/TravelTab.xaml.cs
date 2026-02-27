using System.Windows.Input;
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

        private void WarpLocation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_travelViewModel.AreOptionsEnabled)
                _travelViewModel.WarpCommand.Execute(null);
        }
    }
}
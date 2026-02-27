using System.Windows;
using System.Windows.Input;
using SilkySouls3.ViewModels;
using Xceed.Wpf.Toolkit;

namespace SilkySouls3.Views
{
    public partial class TargetTab
    {
        private readonly TargetViewModel _targetViewModel;

        public TargetTab(TargetViewModel targetViewModel)
        {
            InitializeComponent();
            _targetViewModel = targetViewModel;
            DataContext = _targetViewModel;
        }

        private void SpeedUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;
            var upDown = sender as DoubleUpDown;
            if (upDown?.Value.HasValue == true)
                _targetViewModel.SetSpeed((float)upDown.Value);
            Focus();
            e.Handled = true;
        }

        private void SpeedUpDown_LostFocus(object sender, RoutedEventArgs e)
        {
            var upDown = sender as DoubleUpDown;
            if (upDown?.Value.HasValue == true)
                _targetViewModel.SetSpeed((float)upDown.Value);
        }

    }
}

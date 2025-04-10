using System.Windows;
using System.Windows.Controls;
using SilkySouls3.ViewModels;

namespace SilkySouls3.Views
{
    public partial class PlayerTab
    
    {
        private readonly PlayerViewModel _playerViewModel;
        
        public PlayerTab(PlayerViewModel playerViewModel)
        {
            InitializeComponent();
            _playerViewModel = playerViewModel;
            DataContext = _playerViewModel;
        }

        private void SavePos_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string parameter = button.CommandParameter.ToString();
            int index = int.Parse(parameter);
            _playerViewModel.SavePos(index);
        }
        

        private void RestorePos_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string parameter = button.CommandParameter.ToString();
            int index = int.Parse(parameter);
            _playerViewModel.RestorePos(index);
        }
        
        private void SetRtsrClick(object sender, RoutedEventArgs e)
        {
            _playerViewModel.SetHp(1);
        }

        private void SetMaxHpClick(object sender, RoutedEventArgs e)
        {
            _playerViewModel.SetMaxHp();
        }
    }
}
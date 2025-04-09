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

        private void RestorePos2_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void SavePos2_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void RestorePos1_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void SetMaxHpClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void SetRtsrClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void SavePos1_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
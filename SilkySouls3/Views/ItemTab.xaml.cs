using System.Windows;
using SilkySouls3.ViewModels;

namespace SilkySouls3.Views
{
    public partial class ItemTab
    {
        private readonly ItemViewModel _itemViewModel;
        public ItemTab(ItemViewModel itemViewModel)
        {
            InitializeComponent();
            _itemViewModel = itemViewModel;
            DataContext = _itemViewModel;
        }

        private void SpawnButton_Click(object sender, RoutedEventArgs e)
        {
            _itemViewModel.SpawnItem();
        }

        private void MassSpawn_Click(object sender, RoutedEventArgs e)
        {
            _itemViewModel.MassSpawn();
        }

        private void LoadPreset_Click(object sender, RoutedEventArgs e)
        {
            _itemViewModel.SpawnLoadout();
        }
        
    }
}
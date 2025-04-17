using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkySouls3.Memory;
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
        
        private void ShrineHandmaid_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenRegularShop(GameIds.ShopParams.ShrineMaiden);
        }

        private void Greirat_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenRegularShop(GameIds.ShopParams.Greirat);
        }

        private void Patches_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenRegularShop(GameIds.ShopParams.Patches);
        }

        private void Orbeck_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenRegularShop(GameIds.ShopParams.Orbeck);
        }

        private void Cornyx_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenRegularShop(GameIds.ShopParams.Cornyx);
        }

        private void Karla_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenRegularShop(GameIds.ShopParams.Karla);
        }

        private void Transpose_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenRegularShop(GameIds.ShopParams.Transpose);
        }

        private void Travel_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenMenu(Offsets.Funcs.Travel);
        }

        private void LevelUp_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenMenu(Offsets.Funcs.LevelUp);
        }

        private void ReinforceWeapon_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenMenuWithEvent(Offsets.Funcs.ReinforceWeapon, GameIds.EventFlags.ReinforceWeaponFlagRange);
        }

        private void InfuseWeapon_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenMenuWithEvent(Offsets.Funcs.InfuseWeapon, GameIds.EventFlags.InfuseWeaponFlagRange);
        }

        private void Repair_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenMenu(Offsets.Funcs.Repair);
        }

        private void Attunement_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenMenu(Offsets.Funcs.Attunement);
        }

        private void AllotEstus_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenMenu(Offsets.Funcs.AllotEstus);
        }
    }
}
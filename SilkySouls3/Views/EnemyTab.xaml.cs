using System.Windows;
using System.Windows.Controls;
using SilkySouls3.ViewModels;

namespace SilkySouls3.Views
{
    public partial class EnemyTab : UserControl
    {
        private readonly EnemyViewModel _enemyViewModel;

        public EnemyTab(EnemyViewModel enemyViewModel)
        {
            InitializeComponent();
            _enemyViewModel = enemyViewModel;
            DataContext = _enemyViewModel;
        }

        private void OnHealthButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string parameter = button.CommandParameter.ToString();
            int healthPercentage = int.Parse(parameter);
            _enemyViewModel.SetTargetHealth(healthPercentage);
        }
    }
}
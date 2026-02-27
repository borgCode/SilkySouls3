using System.Windows;
using SilkySouls3.Utilities;
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

            InitializeUpDownHelpers();
        }

        private void InitializeUpDownHelpers()
        {
            _ = new UpDownHelper<int>(
                HealthUpDown,
                _playerViewModel.SetHp,
                _playerViewModel.PauseUpdates,
                _playerViewModel.ResumeUpdates
            );
            
            var statControls = new[]
            {
                VigorUpDown, AttunementUpDown, EnduranceUpDown, StrengthUpDown, DexterityUpDown,
                IntelligenceUpDown, FaithUpDown, LuckUpDown, VitalityUpDown, SoulsUpDown
            };

            foreach (var control in statControls)
            {
                var statName = control.Tag?.ToString();
                if (string.IsNullOrEmpty(statName)) continue;

                _ = new UpDownHelper<int>(
                    control,
                    value => _playerViewModel.SetStat(statName, value),
                    _playerViewModel.PauseUpdates,
                    _playerViewModel.ResumeUpdates
                );
            }
        }


    }
}

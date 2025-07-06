﻿using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using SilkySouls3.ViewModels;
using Xceed.Wpf.Toolkit;

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

        private void SetRtsrClick(object sender, RoutedEventArgs e)
        {
            _playerViewModel.SetHp(1);
        }

        private void SetMaxHpClick(object sender, RoutedEventArgs e)
        {
            _playerViewModel.SetMaxHp();
        }

        private void HealthUpDown_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is IntegerUpDown upDown)) return;
            if (upDown.Template.FindName("PART_TextBox", upDown) is TextBox textBox)
            {
                textBox.GotFocus += PauseUpdates_GotFocus;
            }

            var spinner = upDown.Template.FindName("PART_Spinner", upDown);
            if (spinner == null) return;

            var type = spinner.GetType();

            var incField = type.GetField("_increaseButton", BindingFlags.Instance | BindingFlags.NonPublic);
            var decField = type.GetField("_decreaseButton", BindingFlags.Instance | BindingFlags.NonPublic);

            if (incField?.GetValue(spinner) is ButtonBase incBtn)
                incBtn.Click += SpinnerSetHp;

            if (decField?.GetValue(spinner) is ButtonBase decBtn)
                decBtn.Click += SpinnerSetHp;
        }


        private void PauseUpdates_GotFocus(object sender, RoutedEventArgs e)
        {
            _playerViewModel.PauseUpdates();
        }

        private void SpinnerSetHp(object sender, RoutedEventArgs e)
        {
            _playerViewModel.PauseUpdates();
            if (HealthUpDown.Value.HasValue)
            {
                _playerViewModel.SetHp(HealthUpDown.Value.Value);
            }

            _playerViewModel.ResumeUpdates();
        }

        private void HealthUpDown_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HealthUpDown.Value.HasValue)
            {
                _playerViewModel.SetHp(HealthUpDown.Value.Value);
            }

            _playerViewModel.ResumeUpdates();
        }

        private void HealthUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;
            if (HealthUpDown.Value.HasValue)
            {
                _playerViewModel.SetHp(HealthUpDown.Value.Value);
            }

            Focus();

            e.Handled = true;
        }


        private void CoordinateUpDown_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is DoubleUpDown upDown)) return;
            if (upDown.Template.FindName("PART_TextBox", upDown) is TextBox textBox)
            {
                textBox.GotFocus += PauseUpdates_GotFocus;
            }

            var spinner = upDown.Template.FindName("PART_Spinner", upDown);
            if (spinner == null) return;

            var type = spinner.GetType();
            var incField = type.GetField("_increaseButton", BindingFlags.Instance | BindingFlags.NonPublic);
            var decField = type.GetField("_decreaseButton", BindingFlags.Instance | BindingFlags.NonPublic);

            if (incField?.GetValue(spinner) is ButtonBase incBtn)
            {
                if (upDown == PosXUpDown)
                    incBtn.Click += SpinnerSetPosX;
                else if (upDown == PosYUpDown)
                    incBtn.Click += SpinnerSetPosY;
                else if (upDown == PosZUpDown)
                    incBtn.Click += SpinnerSetPosZ;
            }

            if (decField?.GetValue(spinner) is ButtonBase decBtn)
            {
                if (upDown == PosXUpDown)
                    decBtn.Click += SpinnerSetPosX;
                else if (upDown == PosYUpDown)
                    decBtn.Click += SpinnerSetPosY;
                else if (upDown == PosZUpDown)
                    decBtn.Click += SpinnerSetPosZ;
            }
        }

        private void SpinnerSetPosX(object sender, RoutedEventArgs e)
        {
            _playerViewModel.PauseUpdates();
            if (PosXUpDown.Value.HasValue)
            {
                _playerViewModel.SetPosX((float)PosXUpDown.Value.Value);
            }

            _playerViewModel.ResumeUpdates();
        }

        private void SpinnerSetPosY(object sender, RoutedEventArgs e)
        {
            _playerViewModel.PauseUpdates();
            if (PosYUpDown.Value.HasValue)
            {
                _playerViewModel.SetPosY((float)PosYUpDown.Value.Value);
            }

            _playerViewModel.ResumeUpdates();
        }

        private void SpinnerSetPosZ(object sender, RoutedEventArgs e)
        {
            _playerViewModel.PauseUpdates();
            if (PosZUpDown.Value.HasValue)
            {
                _playerViewModel.SetPosZ((float)PosZUpDown.Value.Value);
            }

            _playerViewModel.ResumeUpdates();
        }

        private void PosX_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PosXUpDown.Value.HasValue)
            {
                _playerViewModel.SetPosX((float)PosXUpDown.Value.Value);
            }

            _playerViewModel.ResumeUpdates();
        }

        private void PosY_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PosYUpDown.Value.HasValue)
            {
                _playerViewModel.SetPosY((float)PosYUpDown.Value.Value);
            }

            _playerViewModel.ResumeUpdates();
        }

        private void PosZ_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PosZUpDown.Value.HasValue)
            {
                _playerViewModel.SetPosZ((float)PosZUpDown.Value.Value);
            }

            _playerViewModel.ResumeUpdates();
        }

        private void PosX_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;
            if (PosXUpDown.Value.HasValue)
            {
                _playerViewModel.SetPosX((float)PosXUpDown.Value.Value);
            }

            Focus();
            e.Handled = true;
        }

        private void PosY_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;
            if (PosYUpDown.Value.HasValue)
            {
                _playerViewModel.SetPosY((float)PosYUpDown.Value.Value);
            }

            Focus();
            e.Handled = true;
        }

        private void PosZ_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;
            if (PosZUpDown.Value.HasValue)
            {
                _playerViewModel.SetPosZ((float)PosZUpDown.Value.Value);
            }

            Focus();
            e.Handled = true;
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

        private void StatUpDowns_Loaded(object sender, RoutedEventArgs e)
        {
            HookIntegerUpDownSpinner(VigorUpDown, "Vigor");
            HookIntegerUpDownSpinner(AttunementUpDown, "Attunement");
            HookIntegerUpDownSpinner(EnduranceUpDown, "Endurance");
            HookIntegerUpDownSpinner(StrengthUpDown, "Strength");
            HookIntegerUpDownSpinner(DexterityUpDown, "Dexterity");
            HookIntegerUpDownSpinner(IntelligenceUpDown, "Intelligence");
            HookIntegerUpDownSpinner(FaithUpDown, "Faith");
            HookIntegerUpDownSpinner(LuckUpDown, "Luck");
            HookIntegerUpDownSpinner(VitalityUpDown, "Vitality");
            HookIntegerUpDownSpinner(SoulsUpDown, "Souls");
        }

        private void HookIntegerUpDownSpinner(IntegerUpDown upDown, string stat)
        {
            upDown.ApplyTemplate();

            var spinner = upDown.Template.FindName("PART_Spinner", upDown);
            if (spinner == null) return;

            var type = spinner.GetType();
            var incField = type.GetField("_increaseButton", BindingFlags.Instance | BindingFlags.NonPublic);
            var decField = type.GetField("_decreaseButton", BindingFlags.Instance | BindingFlags.NonPublic);

            if (incField?.GetValue(spinner) is ButtonBase incBtn)
                incBtn.Click += (s, e) => SpinnerSetStat(stat, upDown);

            if (decField?.GetValue(spinner) is ButtonBase decBtn)
                decBtn.Click += (s, e) => SpinnerSetStat(stat, upDown);
        }

        private void SpinnerSetStat(string statName, IntegerUpDown upDown)
        {
            _playerViewModel.PauseUpdates();

            if (upDown.Value.HasValue)
                _playerViewModel.SetStat(statName, upDown.Value.Value);

            _playerViewModel.ResumeUpdates();
        }

        private void StatUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;

            if (sender is IntegerUpDown upDown && upDown.Tag is string statName && upDown.Value.HasValue)
            {
                _playerViewModel.SetStat(statName, upDown.Value.Value);
            }

            Focus();
            e.Handled = true;
        }

        private void Stat_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is IntegerUpDown upDown && upDown.Tag is string statName && upDown.Value.HasValue)
            {
                _playerViewModel.SetStat(statName, upDown.Value.Value);
            }

            _playerViewModel.ResumeUpdates();
        }

        private void GiveSouls_Click(object sender, RoutedEventArgs e)
        {
            _playerViewModel.GiveSouls();
        }

        private void Ember_Click(object sender, RoutedEventArgs e)
        {
            _playerViewModel.Ember();
        }
    }
}
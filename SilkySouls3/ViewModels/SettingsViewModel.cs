using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using H.Hooks;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Utilities;
using Key = H.Hooks.Key;

namespace SilkySouls3.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly HotkeyManager _hotkeyManager;

        private readonly Dictionary<string, HotkeyBindingViewModel> _hotkeyLookup;

        private string _currentSettingHotkeyId;
        private LowLevelKeyboardHook _tempHook;
        private Keys _currentKeys;

        private bool _isLoaded;
        private bool _isAttached;

        public Dictionary<string, List<HotkeyBindingViewModel>> Hotkeys { get; }

        #region Properties

        private bool _isEnableHotkeysEnabled;

        public bool IsEnableHotkeysEnabled
        {
            get => _isEnableHotkeysEnabled;
            set
            {
                if (SetProperty(ref _isEnableHotkeysEnabled, value))
                {
                    SettingsManager.Default.EnableHotkeys = value;
                    SettingsManager.Default.Save();
                    if (_isEnableHotkeysEnabled) _hotkeyManager.Start();
                    else _hotkeyManager.Stop();
                }
            }
        }

        private bool _isStutterFixEnabled;

        public bool IsStutterFixEnabled
        {
            get => _isStutterFixEnabled;
            set
            {
                if (!SetProperty(ref _isStutterFixEnabled, value)) return;
                SettingsManager.Default.StutterFix = value;
                SettingsManager.Default.Save();
                if (_isLoaded) _settingsService.ToggleStutterFix(_isStutterFixEnabled);
            }
        }

        private bool _isAlwaysOnTopEnabled;

        public bool IsAlwaysOnTopEnabled
        {
            get => _isAlwaysOnTopEnabled;
            set
            {
                if (!SetProperty(ref _isAlwaysOnTopEnabled, value)) return;
                SettingsManager.Default.AlwaysOnTop = value;
                SettingsManager.Default.Save();
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null) mainWindow.Topmost = _isAlwaysOnTopEnabled;
            }
        }

        private bool _isDisableMenuMusicEnabled;

        public bool IsDisableMenuMusicEnabled
        {
            get => _isDisableMenuMusicEnabled;
            set
            {
                if (!SetProperty(ref _isDisableMenuMusicEnabled, value)) return;
                SettingsManager.Default.DisableMenuMusic = value;
                SettingsManager.Default.Save();
                if (_isAttached) _settingsService.ToggleDisableMusic(_isDisableMenuMusicEnabled);
            }
        }

        private bool _isHotkeyReminderEnabled;

        public bool IsHotkeyReminderEnabled
        {
            get => _isHotkeyReminderEnabled;
            set
            {
                if (!SetProperty(ref _isHotkeyReminderEnabled, value)) return;
                SettingsManager.Default.HotkeyReminder = value;
                SettingsManager.Default.Save();
            }
        }

        private bool _isDefaultSoundChangeEnabled;

        public bool IsDefaultSoundChangeEnabled
        {
            get => _isDefaultSoundChangeEnabled;
            set
            {
                if (!SetProperty(ref _isDefaultSoundChangeEnabled, value)) return;
                SettingsManager.Default.DefaultSoundChangeEnabled = value;
                SettingsManager.Default.Save();
            }
        }

        private int _defaultSoundVolume;

        public int DefaultSoundVolume
        {
            get => _defaultSoundVolume;
            set
            {
                if (!SetProperty(ref _defaultSoundVolume, value)) return;
                if (!IsDefaultSoundChangeEnabled) return;
                SettingsManager.Default.DefaultSoundVolume = value;
                SettingsManager.Default.Save();
            }
        }

        #endregion

        public SettingsViewModel(ISettingsService settingsService, HotkeyManager hotkeyManager,
            IStateService stateService)
        {
            _settingsService = settingsService;
            _hotkeyManager = hotkeyManager;

            Hotkeys = new Dictionary<string, List<HotkeyBindingViewModel>>
            {
                ["Player"] =
                [
                    new("RTSR Setup", HotkeyActions.Rtsr),
                    new("Max HP", HotkeyActions.MaxHp),
                    new("Die", HotkeyActions.Die),
                    new("Set Custom HP", HotkeyActions.SetCustomHp),
                    new("Save Position 1", HotkeyActions.SavePos1),
                    new("Save Position 2", HotkeyActions.SavePos2),
                    new("Restore Position 1", HotkeyActions.RestorePos1),
                    new("Restore Position 2", HotkeyActions.RestorePos2),
                    new("No Death", HotkeyActions.NoDeath),
                    new("One Shot", HotkeyActions.OneShot),
                    new("No Damage", HotkeyActions.PlayerNoDamage),
                    new("Infinite Stamina", HotkeyActions.InfiniteStamina),
                    new("No Goods Consume", HotkeyActions.NoGoodsConsume),
                    new("Infinite FP", HotkeyActions.InfiniteFp),
                    new("Infinite Durability", HotkeyActions.InfiniteDurability),
                    new("Invisible", HotkeyActions.Invisible),
                    new("Silent", HotkeyActions.Silent),
                    new("No Ammo Consume", HotkeyActions.NoAmmoConsume),
                    new("Infinite Poise", HotkeyActions.InfinitePoise),
                    new("No Hit", HotkeyActions.NoHit),
                    new("Heal Over Time", HotkeyActions.HealOverTime),
                    new("FP Regen", HotkeyActions.FpRegen),
                    new("Ember", HotkeyActions.Ember),
                    new("Rest", HotkeyActions.Rest),
                    new("Apply SpEffect", HotkeyActions.ApplySpEffect),
                    new("Remove SpEffect", HotkeyActions.RemoveSpEffect),
                    new("Show Active SpEffects", HotkeyActions.ShowActiveSpEffects),
                    new("Toggle Speed", HotkeyActions.TogglePlayerSpeed),
                    new("Increase Speed", HotkeyActions.IncreasePlayerSpeed),
                    new("Decrease Speed", HotkeyActions.DecreasePlayerSpeed),
                ],
                ["Enemies"] =
                [
                    new("Disable All AI", HotkeyActions.DisableAi),
                    new("All No Death", HotkeyActions.AllNoDeath),
                    new("All No Damage", HotkeyActions.AllNoDamage),
                    new("All No Move", HotkeyActions.AllNoMove),
                    new("All No Attack", HotkeyActions.AllNoAttack),
                    new("All Repeat Action", HotkeyActions.AllRepeatAct),
                    new("Enemy Targeting View", HotkeyActions.EnemyTargetingView),
                    new("Draw Navigation Route", HotkeyActions.DrawNavigation),
                    new("Cinder Sword Phase", HotkeyActions.SetSwordPhase),
                    new("Cinder Lance Phase", HotkeyActions.SetLancePhase),
                    new("Cinder Curved Phase", HotkeyActions.SetCurvedPhase),
                    new("Cinder Staff Phase", HotkeyActions.SetStaffPhase),
                    new("Cinder Gwyn Phase", HotkeyActions.SetGwynPhase),
                    new("Phase Lock", HotkeyActions.PhaseLock),
                    new("Cast Soulmass", HotkeyActions.CastSoulmass),
                    new("Remove Soulmass", HotkeyActions.RemoveSoulmass),
                    new("Endless Soulmass", HotkeyActions.EndlessSoulmass),
                    new("No Stagger", HotkeyActions.CinderNoStagger),
                    new("Place Prism Stones", HotkeyActions.PlacePrismStones),
                    new("Pontiff No Clone", HotkeyActions.PontiffNoClone),
                    new("Oceiros Phase 2", HotkeyActions.OceirosPhaseTwo),
                    new("Skip King of the Storm", HotkeyActions.SkipKingOfTheStorm),
                    new("Deacons Phase 2", HotkeyActions.DeaconsPhaseTwo),
                    new("Deacons Phase 2 (With Move)", HotkeyActions.DeaconsPhaseTwoWithMove),
                    new("Drop 10 Skulls at Deacons Fog", HotkeyActions.DropDeaconSkulls),
                    new("DSA Cycle Left Animation", HotkeyActions.CycleLeftButterflyAnimation),
                    new("DSA Cycle Right Animation", HotkeyActions.CycleRightButterflyAnimation),
                ],
                ["Target"] =
                [
                    new("Enable Target Options", HotkeyActions.EnableTargetOptions),
                    new("Disable Target AI", HotkeyActions.DisableTargetAi),
                    new("Freeze HP", HotkeyActions.FreezeHp),
                    new("Kill Target", HotkeyActions.KillTarget),
                    new("Target View", HotkeyActions.TargetView),
                    new("Target Custom HP", HotkeyActions.TargetCustomHp),
                    new("Repeat Action", HotkeyActions.TargetRepeatAct),
                    new("Increase Speed", HotkeyActions.IncreaseTargetSpeed),
                    new("Decrease Speed", HotkeyActions.DecreaseTargetSpeed),
                    new("Toggle Speed", HotkeyActions.ToggleTargetSpeed),
                    new("Show All Resistances", HotkeyActions.ShowAllResistances),
                ],
                ["Utility"] =
                [
                    new("Quitout", HotkeyActions.Quitout),
                    new("No Clip", HotkeyActions.NoClip),
                    new("Death Cam", HotkeyActions.EnableDeathCam),
                    new("Increase NoClip Speed", HotkeyActions.IncreaseNoClipSpeed),
                    new("Decrease NoClip Speed", HotkeyActions.DecreaseNoClipSpeed),
                    new("Increase Game Speed", HotkeyActions.IncreaseGameSpeed),
                    new("Decrease Game Speed", HotkeyActions.DecreaseGameSpeed),
                    new("Toggle Game Speed", HotkeyActions.ToggleGameSpeed),
                    new("Enable Free Cam", HotkeyActions.EnableFreeCam),
                    new("Move Cam To Player", HotkeyActions.MoveCamToPlayer),
                ],
            };

            _hotkeyLookup = Hotkeys.Values
                .SelectMany(x => x)
                .ToDictionary(h => h.ActionId);
            
            stateService.Subscribe(State.Loaded, OnGameLoaded);
            stateService.Subscribe(State.OnNewGameStart, OnNewGameStart);
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);
            stateService.Subscribe(State.Detached, OnGameDetached);
            stateService.Subscribe(State.Attached, OnAttached);

            RegisterHotkeys();
            LoadHotkeyDisplays();
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.Quitout, () => _settingsService.Quitout());
        }

        #region Hotkey Binding

        private void LoadHotkeyDisplays()
        {
            foreach (var hotkey in _hotkeyLookup.Values)
            {
                hotkey.HotkeyText = GetHotkeyDisplayText(hotkey.ActionId);
            }
        }

        private string GetHotkeyDisplayText(string actionId)
        {
            Keys keys = _hotkeyManager.GetHotkey(actionId);
            return keys != null && keys.Values.ToArray().Length > 0 ? string.Join(" + ", keys) : "None";
        }

        public void StartSettingHotkey(string actionId)
        {
            if (_currentSettingHotkeyId != null &&
                _hotkeyLookup.TryGetValue(_currentSettingHotkeyId, out var prev))
            {
                prev.HotkeyText = GetHotkeyDisplayText(_currentSettingHotkeyId);
            }

            _currentSettingHotkeyId = actionId;

            if (_hotkeyLookup.TryGetValue(actionId, out var current))
            {
                current.HotkeyText = "Press keys...";
            }

            _tempHook = new LowLevelKeyboardHook();
            _tempHook.IsExtendedMode = true;
            _tempHook.Down += TempHook_Down;
            _tempHook.Start();
        }

        private void TempHook_Down(object sender, KeyboardEventArgs e)
        {
            if (_currentSettingHotkeyId == null || e.Keys.IsEmpty)
                return;

            try
            {
                bool containsEnter = e.Keys.Values.Contains(Key.Enter) || e.Keys.Values.Contains(Key.Return);

                if (containsEnter && _currentKeys != null)
                {
                    _hotkeyManager.SetHotkey(_currentSettingHotkeyId, _currentKeys);
                    StopSettingHotkey();
                    e.IsHandled = true;
                    return;
                }

                if (e.Keys.Values.Contains(Key.Escape))
                {
                    CancelSettingHotkey();
                    e.IsHandled = true;
                    return;
                }

                if (containsEnter)
                {
                    e.IsHandled = true;
                    return;
                }

                if (e.Keys.IsEmpty)
                    return;

                _currentKeys = e.Keys;

                if (_hotkeyLookup.TryGetValue(_currentSettingHotkeyId, out var binding))
                {
                    binding.HotkeyText = e.Keys.ToString();
                }
            }
            catch (Exception)
            {
                if (_hotkeyLookup.TryGetValue(_currentSettingHotkeyId, out var binding))
                {
                    binding.HotkeyText = "Error: Invalid key combination";
                }
            }

            e.IsHandled = true;
        }

        private void StopSettingHotkey()
        {
            if (_tempHook != null)
            {
                _tempHook.Down -= TempHook_Down;
                _tempHook.Dispose();
                _tempHook = null;
            }

            _currentSettingHotkeyId = null;
            _currentKeys = null;
        }

        public void ConfirmHotkey()
        {
            var currentSettingHotkeyId = _currentSettingHotkeyId;
            var currentKeys = _currentKeys;
            if (currentSettingHotkeyId == null || currentKeys == null || currentKeys.IsEmpty)
            {
                CancelSettingHotkey();
                return;
            }

            HandleExistingHotkey(currentKeys);
            SetNewHotkey(currentSettingHotkeyId, currentKeys);
            StopSettingHotkey();
        }

        private void HandleExistingHotkey(Keys currentKeys)
        {
            string existingHotkeyId = _hotkeyManager.GetActionIdByKeys(currentKeys);
            if (string.IsNullOrEmpty(existingHotkeyId)) return;

            _hotkeyManager.ClearHotkey(existingHotkeyId);
            if (_hotkeyLookup.TryGetValue(existingHotkeyId, out var binding))
            {
                binding.HotkeyText = "None";
            }
        }

        private void SetNewHotkey(string currentSettingHotkeyId, Keys currentKeys)
        {
            _hotkeyManager.SetHotkey(currentSettingHotkeyId, currentKeys);

            if (_hotkeyLookup.TryGetValue(currentSettingHotkeyId, out var binding))
            {
                binding.HotkeyText = new Keys(currentKeys.Values.ToArray()).ToString();
            }
        }

        public void CancelSettingHotkey()
        {
            var actionId = _currentSettingHotkeyId;

            if (actionId != null && _hotkeyLookup.TryGetValue(actionId, out var binding))
            {
                binding.HotkeyText = "None";
                _hotkeyManager.SetHotkey(actionId, new Keys());
            }

            StopSettingHotkey();
        }

        #endregion

        public void OnGameLoaded()
        {
            _isLoaded = true;
            if (IsStutterFixEnabled) _settingsService.ToggleStutterFix(true);
        }

        public void ApplyStartUpOptions()
        {
            _isEnableHotkeysEnabled = SettingsManager.Default.EnableHotkeys;
            if (_isEnableHotkeysEnabled) _hotkeyManager.Start();
            else _hotkeyManager.Stop();
            OnPropertyChanged(nameof(IsEnableHotkeysEnabled));

            _isStutterFixEnabled = SettingsManager.Default.StutterFix;
            OnPropertyChanged(nameof(IsStutterFixEnabled));

            IsAlwaysOnTopEnabled = SettingsManager.Default.AlwaysOnTop;

            _isDisableMenuMusicEnabled = SettingsManager.Default.DisableMenuMusic;
            OnPropertyChanged(nameof(IsDisableMenuMusicEnabled));

            _isDefaultSoundChangeEnabled = SettingsManager.Default.DefaultSoundChangeEnabled;
            OnPropertyChanged(nameof(IsDefaultSoundChangeEnabled));

            _defaultSoundVolume = SettingsManager.Default.DefaultSoundVolume;
            OnPropertyChanged(nameof(DefaultSoundVolume));

            _isHotkeyReminderEnabled = SettingsManager.Default.HotkeyReminder;
            OnPropertyChanged(nameof(IsHotkeyReminderEnabled));
        }

        private void OnGameNotLoaded() => _isLoaded = false;

        private void OnGameDetached() => _isAttached = false;

        private void OnNewGameStart()
        {
            if (!IsHotkeyReminderEnabled) return;
            if (!IsEnableHotkeysEnabled) return;

            MsgBox.Show("Hotkeys are enabled");
        }

        public void OnAttached()
        {
            _isAttached = true;
            if (IsDefaultSoundChangeEnabled) _settingsService.PatchDefaultSound(DefaultSoundVolume);
            if (IsDisableMenuMusicEnabled) _settingsService.ToggleDisableMusic(true);
        }
    }
}

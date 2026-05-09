using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using SilkySouls3.Core;
using SilkySouls3.Enums;
using SilkySouls3.GameIds;
using SilkySouls3.Interfaces;
using SilkySouls3.Models;
using SilkySouls3.Utilities;
using SilkySouls3.Views.Windows;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        private int _currentSoulLevel;
        private bool _customHpHasBeenSet = !string.IsNullOrWhiteSpace(SettingsManager.Default.SaveCustomHp);

        private readonly CharacterState _saveState1 = new();
        private readonly CharacterState _saveState2 = new();

        private float _playerDesiredSpeed = -1f;
        private const float DefaultSpeed = 1f;
        private const float Epsilon = 0.0001f;

        private bool _pauseUpdates;
        private int _currentBlockId;
        private int _currentBossGaugeId;

        private readonly IPlayerService _playerService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly IStateService _stateService;
        private readonly IGameTickService _gameTickService;
        private readonly ISpEffectService _spEffectService;

        private readonly SpEffectViewModel _spEffectViewModel = new();
        private SpEffectsWindow _spEffectsWindow;

        public PlayerViewModel(IPlayerService playerService, HotkeyManager hotkeyManager, IStateService stateService,
            IGameTickService gameTickService, ISpEffectService spEffectService)
        {
            _playerService = playerService;
            _hotkeyManager = hotkeyManager;
            _stateService = stateService;
            _gameTickService = gameTickService;
            _spEffectService = spEffectService;

            RegisterHotkeys();

            stateService.Subscribe(State.Loaded, OnGameLoaded);
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);
            stateService.Subscribe(State.OnNewGameStart, OnNewGameStart);

            SetRtsrCommand = new DelegateCommand(SetRtsr);
            SetMaxHpCommand = new DelegateCommand(SetMaxHp);
            SetCustomHpCommand = new DelegateCommand(SetCustomHp);
            DieCommand = new DelegateCommand(Die);
            SavePositionCommand = new DelegateCommand(SavePosition);
            RestorePositionCommand = new DelegateCommand(RestorePosition);
            GiveSoulsCommand = new DelegateCommand(GiveSouls);
            EmberCommand = new DelegateCommand(Ember);
            RestCommand = new DelegateCommand(Rest);
            BreakWeaponCommand = new DelegateCommand(BreakWeapon);
            ApplySpEffectCommand = new DelegateCommand(ApplySpEffect);
            RemoveSpEffectCommand = new DelegateCommand(RemoveSpEffect);

            ApplyPrefs();
        }

        private void ApplyPrefs()
        {
            _isRememberSpeedEnabled = SettingsManager.Default.RememberPlayerSpeed;
            OnPropertyChanged(nameof(IsRememberSpeedEnabled));
            if (_isRememberSpeedEnabled) _playerDesiredSpeed = SettingsManager.Default.PlayerSpeed;
        }

        #region Commands

        public ICommand SetRtsrCommand { get; set; }
        public ICommand SetMaxHpCommand { get; set; }
        public ICommand SetCustomHpCommand { get; set; }
        public ICommand DieCommand { get; set; }

        public ICommand SavePositionCommand { get; set; }
        public ICommand RestorePositionCommand { get; set; }
        public ICommand GiveSoulsCommand { get; set; }
        public ICommand EmberCommand { get; set; }
        public ICommand RestCommand { get; set; }
        public ICommand BreakWeaponCommand { get; set; }
        public ICommand ApplySpEffectCommand { get; set; }
        public ICommand RemoveSpEffectCommand { get; set; }

        #endregion

        #region Properties

        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        private int _currentHp;

        public int CurrentHp
        {
            get => _currentHp;
            set => SetProperty(ref _currentHp, value);
        }

        private int _currentMaxHp;

        public int CurrentMaxHp
        {
            get => _currentMaxHp;
            set => SetProperty(ref _currentMaxHp, value);
        }

        private string _customHp = SettingsManager.Default.SaveCustomHp;

        public string CustomHp
        {
            get => _customHp;
            set
            {
                if (SetProperty(ref _customHp, value))
                {
                    _customHpHasBeenSet = true;
                }
            }
        }

        private bool _isHotEnabled;

        public bool IsHotEnabled
        {
            get => _isHotEnabled;
            set => SetProperty(ref _isHotEnabled, value);
        }

        private bool _isFpRegenEnabled;

        public bool IsFpRegenEnabled
        {
            get => _isFpRegenEnabled;
            set => SetProperty(ref _isFpRegenEnabled, value);
        }

        private bool _isRememberSpeedEnabled;

        public bool IsRememberSpeedEnabled
        {
            get => _isRememberSpeedEnabled;
            set
            {
                if (!SetProperty(ref _isRememberSpeedEnabled, value)) return;
                if (_isRememberSpeedEnabled)
                {
                    SettingsManager.Default.RememberPlayerSpeed = true;
                    if (Math.Abs(PlayerSpeed - DefaultSpeed) > Epsilon)
                        SettingsManager.Default.PlayerSpeed = PlayerSpeed;
                }
                else
                {
                    SettingsManager.Default.PlayerSpeed = DefaultSpeed;
                    SettingsManager.Default.RememberPlayerSpeed = false;
                }
            }
        }

        private bool _isPos1Saved;

        public bool IsPos1Saved
        {
            get => _isPos1Saved;
            set => SetProperty(ref _isPos1Saved, value);
        }

        private bool _isPos2Saved;

        public bool IsPos2Saved
        {
            get => _isPos2Saved;
            set => SetProperty(ref _isPos2Saved, value);
        }

        private bool _isStateIncluded;

        public bool IsStateIncluded
        {
            get => _isStateIncluded;
            set => SetProperty(ref _isStateIncluded, value);
        }

        private float _posX;

        public float PosX
        {
            get => _posX;
            set => SetProperty(ref _posX, value);
        }

        private float _posY;

        public float PosY
        {
            get => _posY;
            set => SetProperty(ref _posY, value);
        }

        private float _posZ;

        public float PosZ
        {
            get => _posZ;
            set => SetProperty(ref _posZ, value);
        }

        private bool _isNoDeathEnabled;

        public bool IsNoDeathEnabled
        {
            get => _isNoDeathEnabled;
            set
            {
                if (SetProperty(ref _isNoDeathEnabled, value))
                {
                    _playerService.ToggleDebugFlag(DebugFlags.NoDeath, _isNoDeathEnabled ? 1 : 0);
                }
            }
        }

        private bool _isNoDamageEnabled;

        public bool IsNoDamageEnabled
        {
            get => _isNoDamageEnabled;
            set
            {
                if (SetProperty(ref _isNoDamageEnabled, value))
                {
                    _playerService.ToggleNoDamage(_isNoDamageEnabled);
                }
            }
        }

        private bool _isInfiniteStaminaEnabled;

        public bool IsInfiniteStaminaEnabled
        {
            get => _isInfiniteStaminaEnabled;
            set
            {
                if (SetProperty(ref _isInfiniteStaminaEnabled, value))
                {
                    _playerService.ToggleDebugFlag(DebugFlags.InfiniteStam, _isInfiniteStaminaEnabled ? 1 : 0);
                }
            }
        }

        private bool _isNoGoodsConsumeEnabled;

        public bool IsNoGoodsConsumeEnabled
        {
            get => _isNoGoodsConsumeEnabled;
            set
            {
                if (SetProperty(ref _isNoGoodsConsumeEnabled, value))
                {
                    _playerService.ToggleNoGoodsConsume(_isNoGoodsConsumeEnabled);
                }
            }
        }

        private bool _isInfiniteDurabilityEnabled;

        public bool IsInfiniteDurabilityEnabled
        {
            get => _isInfiniteDurabilityEnabled;
            set
            {
                if (SetProperty(ref _isInfiniteDurabilityEnabled, value))
                {
                    _playerService.ToggleInfiniteDurability(_isInfiniteDurabilityEnabled);
                }
            }
        }

        private bool _isInfiniteFpEnabled;

        public bool IsInfiniteFpEnabled
        {
            get => _isInfiniteFpEnabled;
            set
            {
                if (SetProperty(ref _isInfiniteFpEnabled, value))
                {
                    _playerService.ToggleDebugFlag(DebugFlags.InfiniteFp, _isInfiniteFpEnabled ? 1 : 0);
                }
            }
        }

        private bool _isOneShotEnabled;

        public bool IsOneShotEnabled
        {
            get => _isOneShotEnabled;
            set
            {
                if (SetProperty(ref _isOneShotEnabled, value))
                {
                    _playerService.ToggleDebugFlag(DebugFlags.OneShot, _isOneShotEnabled ? 1 : 0);
                }
            }
        }

        private bool _isInvisibleEnabled;

        public bool IsInvisibleEnabled
        {
            get => _isInvisibleEnabled;
            set
            {
                if (SetProperty(ref _isInvisibleEnabled, value))
                {
                    _playerService.ToggleInvisible(_isInvisibleEnabled);
                }
            }
        }

        private bool _isSilentEnabled;

        public bool IsSilentEnabled
        {
            get => _isSilentEnabled;
            set
            {
                if (SetProperty(ref _isSilentEnabled, value))
                {
                    _playerService.ToggleSilent(_isSilentEnabled);
                }
            }
        }

        private bool _isNoAmmoConsumeEnabled;

        public bool IsNoAmmoConsumeEnabled
        {
            get => _isNoAmmoConsumeEnabled;
            set
            {
                if (SetProperty(ref _isNoAmmoConsumeEnabled, value))
                {
                    _playerService.ToggleDebugFlag(DebugFlags.InfiniteArrows, _isNoAmmoConsumeEnabled ? 1 : 0);
                }
            }
        }

        private bool _isInfinitePoiseEnabled;

        public bool IsInfinitePoiseEnabled
        {
            get => _isInfinitePoiseEnabled;
            set
            {
                if (SetProperty(ref _isInfinitePoiseEnabled, value))
                {
                    _playerService.ToggleInfinitePoise(_isInfinitePoiseEnabled);
                }
            }
        }

        private bool _isNoHitEnabled;

        public bool IsNoHitEnabled
        {
            get => _isNoHitEnabled;
            set
            {
                if (SetProperty(ref _isNoHitEnabled, value))
                {
                    _playerService.ToggleNoHit(_isNoHitEnabled);
                }
            }
        }

        private bool _isAutoSetNewGameSevenEnabled;

        public bool IsAutoSetNewGameSevenEnabled
        {
            get => _isAutoSetNewGameSevenEnabled;
            set => SetProperty(ref _isAutoSetNewGameSevenEnabled, value);
        }

        private bool _isNoRollEnabled;

        public bool IsNoRollEnabled
        {
            get => _isNoRollEnabled;
            set
            {
                if (!SetProperty(ref _isNoRollEnabled, value)) return;
                _playerService.ToggleNoRoll(_isNoRollEnabled);
            }
        }

        private int _vigor;

        public int Vigor
        {
            get => _vigor;
            set => SetProperty(ref _vigor, value);
        }

        private int _attunement;

        public int Attunement
        {
            get => _attunement;
            set => SetProperty(ref _attunement, value);
        }

        private int _endurance;

        public int Endurance
        {
            get => _endurance;
            set => SetProperty(ref _endurance, value);
        }

        private int _strength;

        public int Strength
        {
            get => _strength;
            set => SetProperty(ref _strength, value);
        }

        private int _dexterity;

        public int Dexterity
        {
            get => _dexterity;
            set => SetProperty(ref _dexterity, value);
        }

        private int _intelligence;

        public int Intelligence
        {
            get => _intelligence;
            set => SetProperty(ref _intelligence, value);
        }

        private int _faith;

        public int Faith
        {
            get => _faith;
            set => SetProperty(ref _faith, value);
        }

        private int _luck;

        public int Luck
        {
            get => _luck;
            set => SetProperty(ref _luck, value);
        }

        private int _vitality;

        public int Vitality
        {
            get => _vitality;
            set => SetProperty(ref _vitality, value);
        }

        private int _soulLevel;

        public int SoulLevel
        {
            get => _soulLevel;
            private set => SetProperty(ref _soulLevel, value);
        }

        private int _souls;

        public int Souls
        {
            get => _souls;
            set => SetProperty(ref _souls, value);
        }

        private int _newGame;

        public int NewGame
        {
            get => _newGame;
            set
            {
                if (SetProperty(ref _newGame, value))
                {
                    _playerService.SetNewGame(value);
                }
            }
        }

        private int _currentAnimation;

        public int CurrentAnimation
        {
            get => _currentAnimation;
            set => SetProperty(ref _currentAnimation, value);
        }

        private string _applySpEffectInput;

        public string ApplySpEffectInput
        {
            get => _applySpEffectInput;
            set => SetProperty(ref _applySpEffectInput, value);
        }

        private string _removeSpEffectInput;

        public string RemoveSpEffectInput
        {
            get => _removeSpEffectInput;
            set => SetProperty(ref _removeSpEffectInput, value);
        }

        private bool _isShowActiveSpEffectsEnabled;

        public bool IsShowActiveSpEffectsEnabled
        {
            get => _isShowActiveSpEffectsEnabled;
            set
            {
                if (SetProperty(ref _isShowActiveSpEffectsEnabled, value))
                {
                    if (_isShowActiveSpEffectsEnabled) OpenSpEffectsWindow();
                    else CloseSpEffectsWindow();
                }
            }
        }

        private float _playerSpeed;

        public float PlayerSpeed
        {
            get => _playerSpeed;
            set
            {
                if (SetProperty(ref _playerSpeed, value))
                {
                    _playerService.SetPlayerSpeed(value);
                    if (IsRememberSpeedEnabled && Math.Abs(value - DefaultSpeed) > Epsilon)
                    {
                        SettingsManager.Default.PlayerSpeed = value;
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void PauseUpdates() => _pauseUpdates = true;
        public void ResumeUpdates() => _pauseUpdates = false;
        public void SetHp(int hp) => _playerService.SetHp(hp);

        public void SetStat(string statName, int value)
        {
            if (Enum.TryParse<GameDataMan.PlayerGameDataOffsets.Stats>(statName, out var stat))
                _playerService.SetPlayerStat(stat, value);
        }

        public void SetSpeed(float value) => PlayerSpeed = value;

        #endregion

        #region Private Methods

        private void OnGameLoaded()
        {
            if (IsNoDeathEnabled)
                _playerService.ToggleDebugFlag(DebugFlags.NoDeath, 1);
            if (IsNoDamageEnabled)
                _playerService.ToggleNoDamage(true);
            if (IsInfiniteStaminaEnabled)
                _playerService.ToggleDebugFlag(DebugFlags.InfiniteStam, 1);
            if (IsNoGoodsConsumeEnabled)
                _playerService.ToggleNoGoodsConsume(true);
            if (_isInfiniteFpEnabled)
                _playerService.ToggleDebugFlag(DebugFlags.InfiniteFp, 1);
            if (IsOneShotEnabled)
                _playerService.ToggleDebugFlag(DebugFlags.OneShot, 1);
            if (IsInvisibleEnabled)
                _playerService.ToggleDebugFlag(DebugFlags.Invisible, 1);
            if (IsSilentEnabled)
                _playerService.ToggleDebugFlag(DebugFlags.Silent, 1);
            if (IsNoAmmoConsumeEnabled)
                _playerService.ToggleDebugFlag(DebugFlags.InfiniteArrows, 1);
            if (IsInfinitePoiseEnabled)
                _playerService.ToggleInfinitePoise(true);
            if (IsInfiniteDurabilityEnabled)
                _playerService.ToggleInfiniteDurability(true);
            if (IsNoRollEnabled)
                _playerService.ToggleNoRoll(true);
            if (IsNoHitEnabled) _playerService.ToggleNoHit(true);
            AreOptionsEnabled = true;
            LoadStats();
            _gameTickService.Subscribe(PlayerTick);
        }

        private void OnGameNotLoaded()
        {
            AreOptionsEnabled = false;
            _gameTickService.Unsubscribe(PlayerTick);
            _currentBlockId = -1;
            _currentBossGaugeId = -1;
            IsShowActiveSpEffectsEnabled = false;
        }

        private void OnNewGameStart()
        {
            if (!IsAutoSetNewGameSevenEnabled) return;
            _playerService.SetNewGame(8);
            NewGame = _playerService.GetNewGame();
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.SavePos1, () => SavePosition(0));
            _hotkeyManager.RegisterAction(HotkeyActions.SavePos2, () => SavePosition(1));
            _hotkeyManager.RegisterAction(HotkeyActions.RestorePos1, () => RestorePosition(0));
            _hotkeyManager.RegisterAction(HotkeyActions.RestorePos2, () => RestorePosition(1));
            _hotkeyManager.RegisterAction(HotkeyActions.Rtsr, SetRtsr);
            _hotkeyManager.RegisterAction(HotkeyActions.MaxHp, () => SetHp(CurrentMaxHp));
            _hotkeyManager.RegisterAction(HotkeyActions.Die, Die);
            _hotkeyManager.RegisterAction(HotkeyActions.SetCustomHp, SetCustomHp);
            _hotkeyManager.RegisterAction(HotkeyActions.NoDeath, () => { IsNoDeathEnabled = !IsNoDeathEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.OneShot, () => { IsOneShotEnabled = !IsOneShotEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.PlayerNoDamage, () => { IsNoDamageEnabled = !IsNoDamageEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.InfiniteStamina, () => { IsInfiniteStaminaEnabled = !IsInfiniteStaminaEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.NoGoodsConsume, () => { IsNoGoodsConsumeEnabled = !IsNoGoodsConsumeEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.InfiniteFp, () => { IsInfiniteFpEnabled = !IsInfiniteFpEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.InfiniteDurability, () => { IsInfiniteDurabilityEnabled = !IsInfiniteDurabilityEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.Invisible, () => { IsInvisibleEnabled = !IsInvisibleEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.Silent, () => { IsSilentEnabled = !IsSilentEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.NoAmmoConsume, () => { IsNoAmmoConsumeEnabled = !IsNoAmmoConsumeEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.InfinitePoise, () => { IsInfinitePoiseEnabled = !IsInfinitePoiseEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.NoHit, () => { IsNoHitEnabled = !IsNoHitEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.HealOverTime, () => { IsHotEnabled = !IsHotEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.FpRegen, () => { IsFpRegenEnabled = !IsFpRegenEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.Ember, () => { if (AreOptionsEnabled) Ember(); });
            _hotkeyManager.RegisterAction(HotkeyActions.Rest, () => { if (AreOptionsEnabled) Rest(); });
            _hotkeyManager.RegisterAction(HotkeyActions.ApplySpEffect, () => { if (AreOptionsEnabled) ApplySpEffect(); });
            _hotkeyManager.RegisterAction(HotkeyActions.RemoveSpEffect, () => { if (AreOptionsEnabled) RemoveSpEffect(); });
            _hotkeyManager.RegisterAction(HotkeyActions.ShowActiveSpEffects, () => { if (AreOptionsEnabled) IsShowActiveSpEffectsEnabled = !IsShowActiveSpEffectsEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.TogglePlayerSpeed, ToggleSpeed);
            _hotkeyManager.RegisterAction(HotkeyActions.IncreasePlayerSpeed,
                () => SetSpeed(Math.Min(10, PlayerSpeed + 0.25f)));
            _hotkeyManager.RegisterAction(HotkeyActions.DecreasePlayerSpeed,
                () => SetSpeed(Math.Max(0, PlayerSpeed - 0.25f)));
        }

        private void PlayerTick()
        {
            if (_pauseUpdates) return;

            if (IsHotEnabled) TryApplyHot();
            if (IsFpRegenEnabled) TryApplyFpRegen();

            CurrentHp = _playerService.GetHp();
            CurrentMaxHp = _playerService.GetMaxHp();
            CurrentAnimation = _playerService.GetCurrentAnimationId();
            Souls = _playerService.GetPlayerStat(GameDataMan.PlayerGameDataOffsets.Stats.Souls);
            PlayerSpeed = _playerService.GetPlayerSpeed();
            int newSoulLevel = _playerService.GetPlayerStat(GameDataMan.PlayerGameDataOffsets.Stats.SoulLevel);
            var pos = _playerService.GetPosition();
            PosX = pos.X;
            PosY = pos.Y;
            PosZ = pos.Z;

            var newBlockId = _playerService.GetCurrentBlockId();
            if (newBlockId != _currentBlockId)
            {
#if DEBUG
                Console.WriteLine($"{newBlockId:X}");
#endif
                _currentBlockId = newBlockId;
                _stateService.Publish(State.BlockChanged, _currentBlockId);
            }

            var newBossGaugeId = _playerService.GetBossGaugeId();
            if (newBossGaugeId != _currentBossGaugeId)
            {
#if DEBUG
                Console.WriteLine(newBossGaugeId);
#endif
                _currentBossGaugeId = newBossGaugeId;
                _stateService.Publish(State.BossFight, _currentBossGaugeId);
            }


            if (_currentSoulLevel != newSoulLevel)
            {
                SoulLevel = newSoulLevel;
                _currentSoulLevel = newSoulLevel;
                LoadStats();
            }

            if (IsShowActiveSpEffectsEnabled)
                _spEffectViewModel.RefreshEffects(_spEffectService.GetActiveSpEffectList(_playerService.GetPlayerIns()));
        }

        private void LoadStats()
        {
            var stats = _playerService.GetStats();
            Vigor = stats.Vigor;
            Attunement = stats.Attunement;
            Endurance = stats.Endurance;
            Vitality = stats.Vitality;
            Strength = stats.Strength;
            Dexterity = stats.Dexterity;
            Intelligence = stats.Intelligence;
            Faith = stats.Faith;
            Luck = stats.Luck;
            SoulLevel = stats.SoulLevel;
            Souls = stats.Souls;
            NewGame = _playerService.GetNewGame();
            PlayerSpeed = _playerService.GetPlayerSpeed();
        }

        private void TryApplyHot()
        {
            int currentHp = _playerService.GetHp();
            int maxHp = _playerService.GetMaxHp();

            if (currentHp >= maxHp) return;
            int hpToSet = Math.Min(currentHp + (int)(maxHp * 0.033), maxHp);
            _playerService.SetHp(hpToSet);
        }

        private void TryApplyFpRegen()
        {
            int currentFp = _playerService.GetMp();
            int maxFp = _playerService.GetMaxMp();

            if (currentFp >= maxFp) return;
            int fpToSet = Math.Min(currentFp + (int)(maxFp * 0.033), maxFp);
            _playerService.SetMp(fpToSet);
        }

        private void SetCustomHp()
        {
            if (!_customHpHasBeenSet) return;
            var (customHp, error) = ParseCustomHp();
            if (customHp == null)
            {
                MsgBox.Show(error, "Invalid Input");
                return;
            }

            if (customHp > CurrentMaxHp)
                customHp = CurrentMaxHp;

            _playerService.SetHp(customHp.Value);
            SettingsManager.Default.SaveCustomHp = CustomHp;
            SettingsManager.Default.Save();
        }

        private (int? value, string error) ParseCustomHp()
        {
            var input = CustomHp?.Trim();
            if (string.IsNullOrEmpty(input))
                return (null, "Please enter a value");

            if (input.EndsWith("%"))
            {
                if (double.TryParse(input.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture,
                        out var percent))
                    return ((int)(percent / 100.0 * CurrentMaxHp), null);
                return (null, "Invalid percentage format");
            }

            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var absolute))
                return (absolute, null);

            return (null, "Enter a number or percentage (e.g. 545 or 40%)");
        }

        private void SetMaxHp() => _playerService.SetHp(CurrentMaxHp);
        private void Die() => _playerService.SetHp(0);
        private void SetRtsr() => _playerService.SetHp(CurrentMaxHp / 5 - 1);
        private void GiveSouls() => _playerService.GiveSouls();
        private void Ember() => _spEffectService.ApplySpEffect(_playerService.GetPlayerIns(), SpEffect.Ember);
        private void Rest() => _playerService.Rest();
        private void BreakWeapon(object parameter) => _playerService.BreakWeapon(Convert.ToInt32(parameter));

        private void ApplySpEffect()
        {
            if (!uint.TryParse(ApplySpEffectInput, out var id))
            {
                MsgBox.Show("SpEffect ID must be a number");
                return;
            }
            _spEffectService.ApplySpEffect(_playerService.GetPlayerIns(), id);
        }

        private void RemoveSpEffect()
        {
            if (!uint.TryParse(RemoveSpEffectInput, out var id))
            {
                MsgBox.Show("SpEffect ID must be a number");
                return;
            }
            _spEffectService.RemoveSpEffect(_playerService.GetPlayerIns(), id);
        }

        private void OpenSpEffectsWindow()
        {
            _spEffectsWindow = new SpEffectsWindow
            {
                DataContext = _spEffectViewModel,
                Title = "Player Active Special Effects"
            };
            _spEffectsWindow.Closed += (s, e) =>
            {
                _spEffectsWindow = null;
                IsShowActiveSpEffectsEnabled = false;
            };
            _spEffectsWindow.Show();
        }

        private void CloseSpEffectsWindow()
        {
            if (_spEffectsWindow == null || !_spEffectsWindow.IsVisible) return;
            _spEffectsWindow.Close();
            _spEffectsWindow = null;
        }

        private void SavePosition(object parameter)
        {
            int index = Convert.ToInt32(parameter);
            var state = index == 0 ? _saveState1 : _saveState2;
            if (index == 0) IsPos1Saved = true;
            else IsPos2Saved = true;

            state.IncludesState = IsStateIncluded;
            if (IsStateIncluded)
            {
                state.Hp = CurrentHp;
                state.Mp = _playerService.GetMp();
                state.Sp = _playerService.GetSp();
            }

            _playerService.SavePosition(index);
        }

        private void RestorePosition(object parameter)
        {
            int index = Convert.ToInt32(parameter);
            if (index == 0 && !IsPos1Saved) return;
            if (index == 1 && !IsPos2Saved) return;

            _ = Task.Run(() =>
            {
                _playerService.RestorePosition(index);
                if (!IsStateIncluded) return;
                var state = index == 0 ? _saveState1 : _saveState2;
                if (state.IncludesState)
                {
                    _playerService.SetHp(state.Hp);
                    _playerService.SetMp(state.Mp);
                    _playerService.SetSp(state.Sp);
                }
            });
        }

        private void ToggleSpeed()
        {
            if (!AreOptionsEnabled) return;

            if (!IsApproximately(PlayerSpeed, DefaultSpeed))
            {
                _playerDesiredSpeed = PlayerSpeed;
                SetSpeed(DefaultSpeed);
            }
            else if (_playerDesiredSpeed >= 0)
            {
                SetSpeed(_playerDesiredSpeed);
            }
        }

        private bool IsApproximately(float a, float b)
        {
            return Math.Abs(a - b) < Epsilon;
        }

        #endregion
    }
}
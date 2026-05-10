using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using SilkySouls3.Core;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Models;
using SilkySouls3.Utilities;
using SilkySouls3.Views;
using SilkySouls3.Views.Windows;

namespace SilkySouls3.ViewModels
{
    public class TargetViewModel : BaseViewModel
    {
        private readonly ITargetService _targetService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly IDebugDrawService _debugDrawService;
        private readonly IGameTickService _gameTickService;
        private readonly ISpEffectService _spEffectService;

        private nint _currentTargetChrIns;
        private DateTime _moveTargetInstallTime;
        private ResistancesWindow _resistancesWindowWindow;

        private float _targetDesiredSpeed = -1f;
        private const float DefaultSpeed = 1f;
        private const float Epsilon = 0.0001f;

        private readonly SpEffectViewModel _spEffectViewModel = new();
        private SpEffectsWindow _spEffectsWindow;

        private DefensesWindow _defensesWindow;

        public TargetViewModel(ITargetService targetService, HotkeyManager hotkeyManager,
            IDebugDrawService debugDrawService, IStateService stateService, IGameTickService gameTickService,
            ISpEffectService spEffectService)
        {
            _targetService = targetService;
            _hotkeyManager = hotkeyManager;
            _debugDrawService = debugDrawService;
            _gameTickService = gameTickService;
            _spEffectService = spEffectService;

            stateService.Subscribe(State.Loaded, OnGameLoaded);
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);

            ShowCombatInfo = SettingsManager.Default.ResistancesShowCombatInfo;

            RegisterHotkeys();

            SetHpCommand = new DelegateCommand(SetHp);
            SetHpPercentageCommand = new DelegateCommand(SetHpPercentage);
            SetCustomHpCommand = new DelegateCommand(() => ExecuteTargetAction(SetCustomHp));
            MoveTargetToPlayerCommand = new DelegateCommand(TryMoveTargetToPlayer);
        }

        #region Commands

        public ICommand SetHpCommand { get; }
        public ICommand SetHpPercentageCommand { get; }
        public ICommand SetCustomHpCommand { get; }
        public ICommand MoveTargetToPlayerCommand { get; }

        #endregion

        #region Properties
        
        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        private bool _isValidTarget;

        public bool IsValidTarget
        {
            get => _isValidTarget;
            set => SetProperty(ref _isValidTarget, value);
        }
        
        private bool _isTargetOptionsEnabled;

        public bool IsTargetOptionsEnabled
        {
            get => _isTargetOptionsEnabled;
            set
            {
                if (!SetProperty(ref _isTargetOptionsEnabled, value)) return;
                if (value)
                {
                    _targetService.ToggleTargetHook(true);
                    _gameTickService.Subscribe(TargetTick);
                    ShowAllResistances = true;
                }
                else
                {
                    _gameTickService.Unsubscribe(TargetTick);
                    IsRepeatActEnabled = false;
                    IsShowSpEffectEnabled = false;
                    IsShowDefensesEnabled = false;
                    IsDisableAllExceptTargetEnabled = false;
                    ShowAllResistances = false;
                    IsResistancesWindowOpen = false;
                    IsFreezeHealthEnabled = false;
                    ShowPoise = false;
                    ShowBleed = false;
                    ShowPoison = false;
                    ShowFrost = false;
                    ShowToxic = false;
                    if (IsMoveTargetInFlight)
                    {
                        _targetService.UninstallMoveTargetHook();
                        IsMoveTargetInFlight = false;
                    }
                    _targetService.ToggleTargetHook(false);
                }
                
                RefreshResistancesWindow();
            }
        }

        private string _customHp = "1";

        public string CustomHp
        {
            get => _customHp;
            set => SetProperty(ref _customHp, value);
        }

        private int _currentHealth;

        public int CurrentHealth
        {
            get => _currentHealth;
            set
            {
                SetProperty(ref _currentHealth, value);
                OnPropertyChanged(nameof(HealthPercentage));
            }
        }

        private int _maxHealth;

        public int MaxHealth
        {
            get => _maxHealth;
            set
            {
                SetProperty(ref _maxHealth, value);
                OnPropertyChanged(nameof(HealthPercentage));
            }
        }

        public string HealthPercentage => MaxHealth > 0
            ? (CurrentHealth / (double)MaxHealth * 100).ToString("F1")
            : "0.0";

        private bool _showCombatInfo;

        public bool ShowCombatInfo
        {
            get => _showCombatInfo;
            set
            {
                if (SetProperty(ref _showCombatInfo, value))
                {
                    SettingsManager.Default.ResistancesShowCombatInfo = value;
                    SettingsManager.Default.Save();
                }
            }
        }

        private bool _isFreezeHealthEnabled;

        public bool IsFreezeHealthEnabled
        {
            get => _isFreezeHealthEnabled;
            set
            {
                SetProperty(ref _isFreezeHealthEnabled, value);
                _targetService.ToggleNoDamage(_isFreezeHealthEnabled);
            }
        }

        private float _targetSpeed;

        public float TargetSpeed
        {
            get => _targetSpeed;
            set
            {
                if (SetProperty(ref _targetSpeed, value))
                    _targetService.SetSpeed(value);
            }
        }

        private bool _isFreezeAiEnabled;

        public bool IsFreezeAiEnabled
        {
            get => _isFreezeAiEnabled;
            set
            {
                if (SetProperty(ref _isFreezeAiEnabled, value))
                    _targetService.ToggleFreezeAi(_isFreezeAiEnabled);
            }
        }

        private bool _isTargetingViewEnabled;

        public bool IsTargetingViewEnabled
        {
            get => _isTargetingViewEnabled;
            set
            {
                if (!SetProperty(ref _isTargetingViewEnabled, value)) return;
                if (value) _debugDrawService.RequestDebugDraw();
                _targetService.ToggleTargetingView(_isTargetingViewEnabled);
            }
        }
        
        private bool _isNoAttackEnabled;

        public bool IsNoAttackEnabled
        {
            get => _isNoAttackEnabled;
            set
            {
                if (SetProperty(ref _isNoAttackEnabled, value))
                    _targetService.ToggleNoAttack(_isNoAttackEnabled);
            }
        }
        
        private bool _isNoMoveEnabled;

        public bool IsNoMoveEnabled
        {
            get => _isNoMoveEnabled;
            set
            {
                if (SetProperty(ref _isNoMoveEnabled, value))
                    _targetService.ToggleNoMove(_isNoMoveEnabled);
            }
        }

        private bool _isDisableAllExceptTargetEnabled;

        public bool IsDisableAllExceptTargetEnabled
        {
            get => _isDisableAllExceptTargetEnabled;
            set
            {
                if (!SetProperty(ref _isDisableAllExceptTargetEnabled, value)) return;
                _targetService.ToggleDisableAllExceptTarget(_isDisableAllExceptTargetEnabled);
            }
        }

        private bool _isRepeatActEnabled;

        public bool IsRepeatActEnabled
        {
            get => _isRepeatActEnabled;
            set
            {
                if (!SetProperty(ref _isRepeatActEnabled, value)) return;

                bool isRepeating = _targetService.IsRepeatingAct();

                switch (value)
                {
                    case true when !isRepeating:
                        _targetService.ToggleRepeatAct(true);
                        ForceAct = _targetService.GetLastAct();
                        break;
                    case false when isRepeating:
                        _targetService.ToggleRepeatAct(false);
                        ForceAct = 0;
                        break;
                }
            }
        }
        
        private float _dist;

        public float Dist
        {
            get => _dist;
            set => SetProperty(ref _dist, value);
        }

        private int _lastAct;

        public int LastAct
        {
            get => _lastAct;
            set => SetProperty(ref _lastAct, value);
        }

        private int _currentAnimation;

        public int CurrentAnimation
        {
            get => _currentAnimation;
            set => SetProperty(ref _currentAnimation, value);
        }

        private int _forceAct;

        public int ForceAct
        {
            get => _forceAct;
            set
            {
                if (!SetProperty(ref _forceAct, value)) return;
                _targetService.ForceAct(_forceAct);
                if (_forceAct == 0) IsRepeatActEnabled = false;
            }
        }

        private bool _isResistancesWindowOpen;

        public bool IsResistancesWindowOpen
        {
            get => _isResistancesWindowOpen;
            set
            {
                if (!SetProperty(ref _isResistancesWindowOpen, value)) return;
                if (value)
                    OpenResistancesWindow();
                else
                    CloseResistancesWindow();
            }
        }

        private bool _isShowSpEffectEnabled;

        public bool IsShowSpEffectEnabled
        {
            get => _isShowSpEffectEnabled;
            set
            {
                if (SetProperty(ref _isShowSpEffectEnabled, value))
                {
                    if (_isShowSpEffectEnabled) OpenSpEffectsWindow();
                    else CloseSpEffectsWindow();
                }
            }
        }

        private bool _isShowDefensesEnabled;

        public bool IsShowDefensesEnabled
        {
            get => _isShowDefensesEnabled;
            set
            {
                if (!SetProperty(ref _isShowDefensesEnabled, value)) return;
                if (value) OpenDefensesWindow();
                else CloseDefensesWindow();
            }
        }

        private float _standardDefense;
        public float StandardDefense
        {
            get => _standardDefense;
            set => SetProperty(ref _standardDefense, value);
        }

        private float _slashDefense;
        public float SlashDefense
        {
            get => _slashDefense;
            set => SetProperty(ref _slashDefense, value);
        }

        private float _strikeDefense;
        public float StrikeDefense
        {
            get => _strikeDefense;
            set => SetProperty(ref _strikeDefense, value);
        }

        private float _thrustDefense;
        public float ThrustDefense
        {
            get => _thrustDefense;
            set => SetProperty(ref _thrustDefense, value);
        }

        private float _magicDefense;
        public float MagicDefense
        {
            get => _magicDefense;
            set => SetProperty(ref _magicDefense, value);
        }

        private float _fireDefense;
        public float FireDefense
        {
            get => _fireDefense;
            set => SetProperty(ref _fireDefense, value);
        }

        private float _lightningDefense;
        public float LightningDefense
        {
            get => _lightningDefense;
            set => SetProperty(ref _lightningDefense, value);
        }

        private float _darkDefense;
        public float DarkDefense
        {
            get => _darkDefense;
            set => SetProperty(ref _darkDefense, value);
        }

        private bool _showAllResistances;

        public bool ShowAllResistances
        {
            get => _showAllResistances;
            set
            {
                if (SetProperty(ref _showAllResistances, value))
                    UpdateResistancesDisplay();
            }
        }

        private float _currentPoise;

        public float CurrentPoise
        {
            get => _currentPoise;
            set => SetProperty(ref _currentPoise, value);
        }

        private float _maxPoise;

        public float MaxPoise
        {
            get => _maxPoise;
            set => SetProperty(ref _maxPoise, value);
        }

        private float _poiseTimer;

        public float PoiseTimer
        {
            get => _poiseTimer;
            set => SetProperty(ref _poiseTimer, value);
        }

        private bool _showPoise;

        public bool ShowPoise
        {
            get => _showPoise;
            set
            {
                SetProperty(ref _showPoise, value);
                RefreshResistancesWindow();
            }
        }

        private int _currentBleed;

        public int CurrentBleed
        {
            get => _currentBleed;
            set => SetProperty(ref _currentBleed, value);
        }

        private int _maxBleed;

        public int MaxBleed
        {
            get => _maxBleed;
            set => SetProperty(ref _maxBleed, value);
        }

        private bool _showBleed;

        public bool ShowBleed
        {
            get => _showBleed;
            set
            {
                SetProperty(ref _showBleed, value);
                RefreshResistancesWindow();
            }
        }

        private bool _isBleedImmune;

        public bool IsBleedImmune
        {
            get => _isBleedImmune;
            set => SetProperty(ref _isBleedImmune, value);
        }

        private int _currentPoison;

        public int CurrentPoison
        {
            get => _currentPoison;
            set => SetProperty(ref _currentPoison, value);
        }

        private int _maxPoison;

        public int MaxPoison
        {
            get => _maxPoison;
            set => SetProperty(ref _maxPoison, value);
        }

        private bool _showPoison;

        public bool ShowPoison
        {
            get => _showPoison;
            set
            {
                SetProperty(ref _showPoison, value);
                RefreshResistancesWindow();
            }
        }

        private bool _isPoisonImmune;

        public bool IsPoisonImmune
        {
            get => _isPoisonImmune;
            set => SetProperty(ref _isPoisonImmune, value);
        }

        private int _currentToxic;

        public int CurrentToxic
        {
            get => _currentToxic;
            set => SetProperty(ref _currentToxic, value);
        }

        private int _maxToxic;

        public int MaxToxic
        {
            get => _maxToxic;
            set => SetProperty(ref _maxToxic, value);
        }

        private bool _showToxic;

        public bool ShowToxic
        {
            get => _showToxic;
            set
            {
                SetProperty(ref _showToxic, value);
                RefreshResistancesWindow();
            }
        }

        private bool _isToxicImmune;

        public bool IsToxicImmune
        {
            get => _isToxicImmune;
            set => SetProperty(ref _isToxicImmune, value);
        }

        private int _currentFrost;

        public int CurrentFrost
        {
            get => _currentFrost;
            set => SetProperty(ref _currentFrost, value);
        }

        private int _maxFrost;

        public int MaxFrost
        {
            get => _maxFrost;
            set => SetProperty(ref _maxFrost, value);
        }

        private bool _showFrost;

        public bool ShowFrost
        {
            get => _showFrost;
            set
            {
                SetProperty(ref _showFrost, value);
                RefreshResistancesWindow();
            }
        }

        private bool _isFrostImmune;

        public bool IsFrostImmune
        {
            get => _isFrostImmune;
            set => SetProperty(ref _isFrostImmune, value);
        }

        private bool _isMoveTargetInFlight;

        public bool IsMoveTargetInFlight
        {
            get => _isMoveTargetInFlight;
            set => SetProperty(ref _isMoveTargetInFlight, value);
        }

        public bool ShowBleedAndNotImmune => ShowBleed && !IsBleedImmune;
        public bool ShowPoisonAndNotImmune => ShowPoison && !IsPoisonImmune;
        public bool ShowToxicAndNotImmune => ShowToxic && !IsToxicImmune;
        public bool ShowFrostAndNotImmune => ShowFrost && !IsFrostImmune;

        #endregion

        #region Public Methods

        public void SetSpeed(double value) => TargetSpeed = (float)value;

        private void ToggleTargetSpeed()
        {
            if (!AreOptionsEnabled) return;

            if (!IsApproximately(TargetSpeed, DefaultSpeed))
            {
                _targetDesiredSpeed = TargetSpeed;
                SetSpeed(DefaultSpeed);
            }
            else if (_targetDesiredSpeed >= 0)
            {
                SetSpeed(_targetDesiredSpeed);
            }
        }

        private static bool IsApproximately(float a, float b) => Math.Abs(a - b) < Epsilon;

        #endregion

        #region Private Methods

        private void SetHp(object parameter) => _targetService.SetHp(Convert.ToInt32(parameter));

        private void SetHpPercentage(object parameter)
        {
            int percentage = Convert.ToInt32(parameter);
            _targetService.SetHp(MaxHealth * percentage / 100);
        }

        private void SetCustomHp()
        {
            var (hp, error) = ParseCustomHp();
            if (hp == null)
            {
                MsgBox.Show(error, "Invalid Input");
                return;
            }
            int health = hp.Value;
            if (health > MaxHealth) health = MaxHealth;
            _targetService.SetHp(health);
        }

        private (int? value, string error) ParseCustomHp()
        {
            var input = CustomHp?.Trim();
            if (string.IsNullOrEmpty(input))
                return (null, "Please enter a value");

            if (input.EndsWith("%"))
            {
                if (double.TryParse(input.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out var pct))
                    return ((int)(pct / 100.0 * MaxHealth), null);
                return (null, "Invalid percentage format");
            }

            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var abs))
                return (abs, null);

            return (null, "Enter a number or percentage (e.g. 545 or 40%)");
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.EnableTargetOptions,
                () => { IsTargetOptionsEnabled = !IsTargetOptionsEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.ShowAllResistances, () =>
            {
                if (!IsTargetOptionsEnabled) IsTargetOptionsEnabled = true;
                _showAllResistances = !_showAllResistances;
                UpdateResistancesDisplay();
            });
            _hotkeyManager.RegisterAction(HotkeyActions.KillTarget, () =>
                ExecuteTargetAction(() => SetHp(0)));
            _hotkeyManager.RegisterAction(HotkeyActions.TargetCustomHp, () =>
                ExecuteTargetAction(SetCustomHp));
            _hotkeyManager.RegisterAction(HotkeyActions.TargetView, () =>
                ExecuteTargetAction(() => IsTargetingViewEnabled = !IsTargetingViewEnabled));
            _hotkeyManager.RegisterAction(HotkeyActions.FreezeHp, () =>
                ExecuteTargetAction(() => IsFreezeHealthEnabled = !IsFreezeHealthEnabled));
            _hotkeyManager.RegisterAction(HotkeyActions.DisableTargetAi, () =>
                ExecuteTargetAction(() => IsFreezeAiEnabled = !IsFreezeAiEnabled));
            _hotkeyManager.RegisterAction(HotkeyActions.IncreaseTargetSpeed, () =>
                ExecuteTargetAction(() => SetSpeed(Math.Min(5, TargetSpeed + 0.25f))));
            _hotkeyManager.RegisterAction(HotkeyActions.DecreaseTargetSpeed, () =>
                ExecuteTargetAction(() => SetSpeed(Math.Max(0, TargetSpeed - 0.25f))));
            _hotkeyManager.RegisterAction(HotkeyActions.ToggleTargetSpeed, () =>
                ExecuteTargetAction(ToggleTargetSpeed));
            _hotkeyManager.RegisterAction(HotkeyActions.TargetRepeatAct, () =>
                ExecuteTargetAction(() => IsRepeatActEnabled = !IsRepeatActEnabled));
            _hotkeyManager.RegisterAction(HotkeyActions.ShowDefenses, () =>
                ExecuteTargetAction(() => IsShowDefensesEnabled = !IsShowDefensesEnabled));
            _hotkeyManager.RegisterAction(HotkeyActions.DisableAllExceptTargetAi, () =>
                ExecuteTargetAction(() => IsDisableAllExceptTargetEnabled = !IsDisableAllExceptTargetEnabled));
            _hotkeyManager.RegisterAction(HotkeyActions.MoveTargetToPlayer, TryMoveTargetToPlayer);
            _hotkeyManager.RegisterAction(HotkeyActions.TargetNoAttack, () =>
                ExecuteTargetAction(() => IsNoAttackEnabled = !IsNoAttackEnabled));
            _hotkeyManager.RegisterAction(HotkeyActions.TargetNoMove, () =>
                ExecuteTargetAction(() => IsNoMoveEnabled = !IsNoMoveEnabled));
            _hotkeyManager.RegisterAction(HotkeyActions.TargetShowSpEffects, () =>
                ExecuteTargetAction(() => IsShowSpEffectEnabled = !IsShowSpEffectEnabled));
        }

        private void TryMoveTargetToPlayer()
        {
            if (IsMoveTargetInFlight) return;
            ExecuteTargetAction(() =>
            {
                _moveTargetInstallTime = DateTime.UtcNow;
                IsMoveTargetInFlight = true;
                _targetService.MoveTargetToPlayer();
            });
        }

        private void PollMoveTarget()
        {
            if (!IsMoveTargetInFlight) return;
            if (_targetService.GetMoveTargetStatus() == 1 ||
                (DateTime.UtcNow - _moveTargetInstallTime).TotalMilliseconds >= 500)
            {
                _targetService.UninstallMoveTargetHook();
                IsMoveTargetInFlight = false;
            }
        }

        private void OnGameLoaded()
        {
            if (IsTargetOptionsEnabled)
            {
                _targetService.ToggleTargetHook(true);
                _gameTickService.Subscribe(TargetTick);
            }

            AreOptionsEnabled = true;
        }

        private void OnGameNotLoaded()
        {
            _gameTickService.Unsubscribe(TargetTick);
            IsFreezeHealthEnabled = false;
            LastAct = 0;
            ForceAct = 0;
            AreOptionsEnabled = false;
            IsMoveTargetInFlight = false;
        }

        private void ExecuteTargetAction(Action action)
        {
            if (!IsTargetOptionsEnabled)
            {
                IsTargetOptionsEnabled = true;
                Task.Run(async () =>
                {
                    await Task.Delay(100);
                    if (EnsureValidTarget()) action();
                });
                return;
            }

            if (!IsValidTarget) return;
            action();
        }

        private bool EnsureValidTarget() => IsValidTarget || IsTargetValid();

        private bool IsTargetValid()
        {
            nint targetChrIns = _targetService.GetChrIns();
            if (targetChrIns == 0) return false;

            float health = _targetService.GetHp();
            float maxHealth = _targetService.GetMaxHp();
            if (health < 0 || maxHealth <= 0 || health > 10000000 || maxHealth > 10000000) return false;
            if (health > maxHealth * 1.5) return false;

            var position = _targetService.GetPosition();
            if (float.IsNaN(position.X) || float.IsNaN(position.Y) || float.IsNaN(position.Z)) return false;
            if (Math.Abs(position.X) > 10000 || Math.Abs(position.Y) > 10000 || Math.Abs(position.Z) > 10000)
                return false;

            return true;
        }

        private void TargetTick()
        {
            PollMoveTarget();

            if (!IsTargetValid())
            {
                IsValidTarget = false;
                return;
            }

            IsValidTarget = true;
            CurrentHealth = _targetService.GetHp();
            MaxHealth = _targetService.GetMaxHp();

            nint targetChrIns = _targetService.GetChrIns();

            Resistances resistances = _targetService.GetResistances();
            Poise poise = _targetService.GetPoise();

            if (targetChrIns != _currentTargetChrIns)
            {
#if DEBUG
                Console.WriteLine($@"enemyIns: {(long)targetChrIns:X}");
                Console.WriteLine($@"eventId: {_targetService.GetEventId()}");
#endif
                IsFreezeAiEnabled = _targetService.IsAiFrozen();
                IsTargetingViewEnabled = _targetService.IsTargetViewEnabled();
                IsNoAttackEnabled = _targetService.IsNoAttackEnabled();
                IsNoMoveEnabled = _targetService.IsNoMoveEnabled();
                int forceActValue = _targetService.GetForceAct();
                if (forceActValue != 0)
                {
                    IsRepeatActEnabled = true;
                    ForceAct = forceActValue;
                }
                else
                {
                    ForceAct = 0;
                    IsRepeatActEnabled = false;
                }

                IsFreezeHealthEnabled = _targetService.IsNoDamageEnabled();
                _currentTargetChrIns = targetChrIns;
                MaxPoise = poise.Max;
                var immunities = _targetService.GetImmunities();
                IsPoisonImmune = immunities.Poison;
                IsToxicImmune = immunities.Toxic;
                IsBleedImmune = immunities.Bleed;
                IsFrostImmune = immunities.Frost;
                MaxPoison = IsPoisonImmune ? 0 : resistances.PoisonMax;
                MaxToxic = IsToxicImmune ? 0 : resistances.ToxicMax;
                MaxBleed = IsBleedImmune ? 0 : resistances.BleedMax;
                MaxFrost = IsFrostImmune ? 0 : resistances.FrostMax;
                UpdateDefenses();
                if (!IsResistancesWindowOpen || _resistancesWindowWindow == null) return;
                _resistancesWindowWindow.DataContext = null;
                _resistancesWindowWindow.DataContext = this;
            }

            Dist = _targetService.GetDist();
            
            TargetSpeed = _targetService.GetSpeed();
            CurrentPoise = poise.Current;
            PoiseTimer = poise.Timer;
            CurrentPoison = IsPoisonImmune ? 0 : resistances.PoisonCurrent;
            CurrentToxic = IsToxicImmune ? 0 : resistances.ToxicCurrent;
            CurrentBleed = IsBleedImmune ? 0 : resistances.BleedCurrent;
            CurrentFrost = IsFrostImmune ? 0 : resistances.FrostCurrent;
            LastAct = _targetService.GetLastAct();
            CurrentAnimation = _targetService.GetCurrentAnimation();

            if (IsShowSpEffectEnabled)
            {
                var spEffects = _spEffectService.GetActiveSpEffectList(_targetService.GetChrIns());
                _spEffectViewModel.RefreshEffects(spEffects);
            }
        }

        private void UpdateResistancesDisplay()
        {
            if (!IsTargetOptionsEnabled) return;
            if (_showAllResistances)
            {
                ShowBleed = true;
                ShowPoise = true;
                ShowPoison = true;
                ShowFrost = true;
                ShowToxic = true;
            }
            else
            {
                ShowBleed = false;
                ShowPoise = false;
                ShowPoison = false;
                ShowFrost = false;
                ShowToxic = false;
            }

            RefreshResistancesWindow();
        }

        private void OpenResistancesWindow()
        {
            if (_resistancesWindowWindow != null && _resistancesWindowWindow.IsVisible) return;
            _resistancesWindowWindow = new ResistancesWindow
            {
                DataContext = this
            };
            _resistancesWindowWindow.Closed += (s, e) => _isResistancesWindowOpen = false;
            _resistancesWindowWindow.Show();
        }

        private void CloseResistancesWindow()
        {
            if (_resistancesWindowWindow == null || !_resistancesWindowWindow.IsVisible) return;
            _resistancesWindowWindow.Close();
            _resistancesWindowWindow = null;
        }
        
        private void OpenSpEffectsWindow()
        {
            _spEffectsWindow = new SpEffectsWindow
            {
                DataContext = _spEffectViewModel,
                Title = "Target Active Special Effects"
            };
            _spEffectsWindow.Closed += (s, e) =>
            {
                _spEffectsWindow = null;
                IsShowSpEffectEnabled = false;
            };
            _spEffectsWindow.Show();
        }

        private void CloseSpEffectsWindow()
        {
            if (_spEffectsWindow == null || !_spEffectsWindow.IsVisible) return;
            _spEffectsWindow.Close();
            _spEffectsWindow = null;
        }

        private void OpenDefensesWindow()
        {
            if (_defensesWindow != null && _defensesWindow.IsVisible) return;
            _defensesWindow = new DefensesWindow
            {
                DataContext = this
            };
            _defensesWindow.Closed += (s, e) =>
            {
                _defensesWindow = null;
                _isShowDefensesEnabled = false;
            };
            _defensesWindow.Show();
        }

        private void CloseDefensesWindow()
        {
            if (_defensesWindow == null || !_defensesWindow.IsVisible) return;
            _defensesWindow.Close();
            _defensesWindow = null;
        }

        private void UpdateDefenses()
        {
            var d = _targetService.GetDefenses();
            StandardDefense  = (1 - d.Standard)  * 100;
            SlashDefense     = (1 - d.Slash)     * 100;
            StrikeDefense    = (1 - d.Strike)    * 100;
            ThrustDefense    = (1 - d.Thrust)    * 100;
            MagicDefense     = (1 - d.Magic)     * 100;
            FireDefense      = (1 - d.Fire)      * 100;
            LightningDefense = (1 - d.Lightning) * 100;
            DarkDefense      = (1 - d.Dark)      * 100;
        }

        private void RefreshResistancesWindow()
        {
            if (!IsResistancesWindowOpen || _resistancesWindowWindow == null) return;
            _resistancesWindowWindow.DataContext = null;
            _resistancesWindowWindow.DataContext = this;
        }

        #endregion
    }
}
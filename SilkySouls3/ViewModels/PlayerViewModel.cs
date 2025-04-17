using System;
using System.Windows.Threading;
using SilkySouls3.Services;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;
using static SilkySouls3.Memory.Offsets.GameDataMan;

namespace SilkySouls3.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        private int _currentHp;
        private int _currentMaxHp;

        private bool _isPos1Saved;
        private bool _isPos2Saved;

        private bool _isNoDeathEnabled;
        private bool _isNoDamageEnabled;
        private bool _isInfiniteStaminaEnabled;
        private bool _isNoGoodsConsumeEnabled;
        private bool _isInfiniteFpEnabled;
        private bool _isInfiniteDurabilityEnabled;
        private bool _isOneShotEnabled;
        private bool _isInvisibleEnabled;
        private bool _isSilentEnabled;
        private bool _isNoAmmoConsumeEnabled;
        private bool _isInfinitePoiseEnabled;
        private bool _isAutoSetNewGameSevenEnabled;
        private bool _isNoRollEnabled;

        private int _vigor;
        private int _attunement;
        private int _endurance;
        private int _strength;
        private int _dexterity;
        private int _intelligence;
        private int _faith;
        private int _luck;
        private int _vitality;
        private int _soulLevel;
        private int _souls;
        private int _newGame;
        private float _playerSpeed;
        private int _currentSoulLevel;

        private float _playerDesiredSpeed = -1f;
        private const float DefaultSpeed = 1f;
        private const float Epsilon = 0.0001f;

        private bool _pauseUpdates;
        private bool _areOptionsEnabled;
        private readonly DispatcherTimer _timer;

        private readonly PlayerService _playerService;
        private readonly HotkeyManager _hotkeyManager;

        public PlayerViewModel(PlayerService playerService, HotkeyManager hotkeyManager)
        {
            _playerService = playerService;
            _hotkeyManager = hotkeyManager;

            RegisterHotkeys();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += (s, e) =>
            {
                if (_pauseUpdates) return;

                CurrentHp = _playerService.GetHp();
                CurrentMaxHp = _playerService.GetMaxHp();
                Souls = _playerService.GetPlayerStat(Stats.Souls);
                PlayerSpeed = _playerService.GetPlayerSpeed();
                int newSoulLevel = _playerService.GetPlayerStat(Stats.SoulLevel);
                if (_currentSoulLevel != newSoulLevel)
                {
                    SoulLevel = newSoulLevel;
                    _currentSoulLevel = newSoulLevel;
                    LoadStats();
                }
            };
        }
        public bool IsHealthTyping { get; set; } = false;
        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction("SavePos1", () => SavePos(0));
            _hotkeyManager.RegisterAction("SavePos2", () => SavePos(1));
            _hotkeyManager.RegisterAction("RestorePos1", () => RestorePos(0));
            _hotkeyManager.RegisterAction("RestorePos2", () => RestorePos(1));
            _hotkeyManager.RegisterAction("RTSR", () => SetHp(1));
            _hotkeyManager.RegisterAction("NoDeath", () => { IsNoDeathEnabled = !IsNoDeathEnabled; });
            _hotkeyManager.RegisterAction("OneShot", () => { IsOneShotEnabled = !IsOneShotEnabled; });
            _hotkeyManager.RegisterAction("PlayerNoDamage", () => { IsNoDamageEnabled = !IsNoDamageEnabled; });
            _hotkeyManager.RegisterAction("TogglePlayerSpeed", ToggleSpeed);
            _hotkeyManager.RegisterAction("IncreasePlayerSpeed", () => SetSpeed(Math.Min(10, PlayerSpeed + 0.25f)));
            _hotkeyManager.RegisterAction("DecreasePlayerSpeed", () => SetSpeed(Math.Max(0, PlayerSpeed - 0.25f)));
        }

        private void LoadStats()
        {
            Vigor = _playerService.GetPlayerStat(Stats.Vigor);
            Attunement = _playerService.GetPlayerStat(Stats.Attunement);
            Endurance = _playerService.GetPlayerStat(Stats.Endurance);
            Vitality = _playerService.GetPlayerStat(Stats.Vitality);
            Strength = _playerService.GetPlayerStat(Stats.Strength);
            Dexterity = _playerService.GetPlayerStat(Stats.Dexterity);
            Intelligence = _playerService.GetPlayerStat(Stats.Intelligence);
            Faith = _playerService.GetPlayerStat(Stats.Faith);
            Luck = _playerService.GetPlayerStat(Stats.Luck);
            SoulLevel = _playerService.GetPlayerStat(Stats.SoulLevel);
            Souls = _playerService.GetPlayerStat(Stats.Souls);
            NewGame = _playerService.GetNewGame();
            PlayerSpeed = _playerService.GetPlayerSpeed();
        }


        public void PauseUpdates()
        {
            _pauseUpdates = true;
        }

        public void ResumeUpdates()
        {
            _pauseUpdates = false;
        }


        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        public int CurrentHp
        {
            get => _currentHp;
            set => SetProperty(ref _currentHp, value);
        }

        public int CurrentMaxHp
        {
            get => _currentMaxHp;
            set => SetProperty(ref _currentMaxHp, value);
        }

        public void SetHp(int hp)
        {
            _playerService.SetHp(hp);
            CurrentHp = hp;
        }

        public void SetMaxHp()
        {
            _playerService.SetHp(CurrentMaxHp);
        }


        public bool IsPos1Saved
        {
            get => _isPos1Saved;
            set => SetProperty(ref _isPos1Saved, value);
        }

        public bool IsPos2Saved
        {
            get => _isPos2Saved;
            set => SetProperty(ref _isPos2Saved, value);
        }

        public void SavePos(int index)
        {
            if (index == 0) IsPos1Saved = true;
            else IsPos2Saved = true;
            _playerService.SavePos(index);
        }

        public void RestorePos(int index)
        {
            _playerService.RestorePos(index);
        }

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

        public bool IsInvisibleEnabled
        {
            get => _isInvisibleEnabled;
            set
            {
                if (SetProperty(ref _isInvisibleEnabled, value))
                {
                    _playerService.ToggleDebugFlag(DebugFlags.Invisible, _isInvisibleEnabled ? 1 : 0);
                }
            }
        }

        public bool IsSilentEnabled
        {
            get => _isSilentEnabled;
            set
            {
                if (SetProperty(ref _isSilentEnabled, value))
                {
                    _playerService.ToggleDebugFlag(DebugFlags.Silent, _isSilentEnabled ? 1 : 0);
                }
            }
        }

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

        public bool IsAutoSetNewGameSevenEnabled
        {
            get => _isAutoSetNewGameSevenEnabled;
            set => SetProperty(ref _isAutoSetNewGameSevenEnabled, value);
        }

        public bool IsNoRollEnabled
        {
            get => _isNoRollEnabled;
            set
            {
                if (!SetProperty(ref _isNoRollEnabled, value)) return;
                _playerService.ToggleNoRoll(_isNoRollEnabled);
            }
        }

        public void TryEnableFeatures()
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
            AreOptionsEnabled = true;
            LoadStats();
            _timer.Start();
        }

        public void DisableFeatures()
        {
            AreOptionsEnabled = false;
            _timer.Stop();
        }

        public int Vigor
        {
            get => _vigor;
            set
            {
                if (SetProperty(ref _vigor, value))
                {
                    _playerService.SetPlayerStat(Stats.Vigor, value);
                }
            }
        }

        public int Attunement
        {
            get => _attunement;
            set
            {
                if (SetProperty(ref _attunement, value))
                {
                    _playerService.SetPlayerStat(Stats.Attunement, value);
                }
            }
        }

        public int Endurance
        {
            get => _endurance;
            set
            {
                if (SetProperty(ref _endurance, value))
                {
                    _playerService.SetPlayerStat(Stats.Endurance, value);
                }
            }
        }

        public int Strength
        {
            get => _strength;
            set
            {
                if (SetProperty(ref _strength, value))
                {
                    _playerService.SetPlayerStat(Stats.Strength, value);
                }
            }
        }

        public int Dexterity
        {
            get => _dexterity;
            set
            {
                if (SetProperty(ref _dexterity, value))
                {
                    _playerService.SetPlayerStat(Stats.Dexterity, value);
                }
            }
        }

        public int Intelligence
        {
            get => _intelligence;
            set
            {
                if (SetProperty(ref _intelligence, value))
                {
                    _playerService.SetPlayerStat(Stats.Intelligence, value);
                }
            }
        }

        public int Faith
        {
            get => _faith;
            set
            {
                if (SetProperty(ref _faith, value))
                {
                    _playerService.SetPlayerStat(Stats.Faith, value);
                }
            }
        }

        public int Luck
        {
            get => _luck;
            set
            {
                if (SetProperty(ref _luck, value))
                {
                    _playerService.SetPlayerStat(Stats.Luck, value);
                }
            }
        }

        public int Vitality
        {
            get => _vitality;
            set
            {
                if (SetProperty(ref _vitality, value))
                {
                    _playerService.SetPlayerStat(Stats.Vitality, value);
                }
            }
        }

        public int SoulLevel
        {
            get => _soulLevel;
            private set => SetProperty(ref _soulLevel, value);
        }

        public int Souls
        {
            get => _souls;
            set
            {
                if (SetProperty(ref _souls, value))
                {
                    _playerService.SetPlayerStat(Stats.Souls, value);
                }
            }
        }

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

        public float PlayerSpeed
        {
            get => _playerSpeed;
            set
            {
                if (SetProperty(ref _playerSpeed, value))
                {
                    _playerService.SetPlayerSpeed(value);
                }
            }
        }

        public void SetNewGame(int value) => NewGame = value;
        public void SetSpeed(float value) => PlayerSpeed = value;

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

        public void TrySetNgPref()
        {
            if (!IsAutoSetNewGameSevenEnabled) return;
            _playerService.SetNewGame(8);
            NewGame = _playerService.GetNewGame();
        }
    }
}
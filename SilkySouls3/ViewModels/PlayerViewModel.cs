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
        private bool _isAutoSetNewGameSixEnabled;

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
                // PlayerSpeed = _playerService.GetSetPlayerSpeed(null);
                int newSoulLevel = _playerService.GetPlayerStat(Stats.SoulLevel);
                if (_currentSoulLevel != newSoulLevel)
                {
                    SoulLevel = newSoulLevel;
                    _currentSoulLevel = newSoulLevel;
                    LoadStats();
                }
            };
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction("SavePos1", () => SavePos(0));
            _hotkeyManager.RegisterAction("SavePos2", () => SavePos(1));
            _hotkeyManager.RegisterAction("RestorePos1", () => RestorePos(0));
            _hotkeyManager.RegisterAction("RestorePos2", () => RestorePos(1));
            _hotkeyManager.RegisterAction("RTSR", () => SetHp(1));
            _hotkeyManager.RegisterAction("NoDeath", () => { IsNoDeathEnabled = !IsNoDeathEnabled; });
            _hotkeyManager.RegisterAction("OneShot", () => { IsOneShotEnabled = !IsOneShotEnabled; });
            // _hotkeyManager.RegisterAction("ToggleSpeed", ToggleSpeed);
            // _hotkeyManager.RegisterAction("IncreaseSpeed", () => SetSpeed(PlayerSpeed.HasValue ? Math.Min(10, PlayerSpeed.Value + 0.25f) : 0.25f));
            // _hotkeyManager.RegisterAction("DecreaseSpeed", () =>
            // {
            //     if (PlayerSpeed != null) SetSpeed(Math.Max(0, PlayerSpeed.Value - 0.25f));
            // });
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
            // PlayerSpeed = _playerService.GetSetPlayerSpeed(null);
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

        public bool IsAutoSetNewGameSixEnabled
        {
            get => _isAutoSetNewGameSixEnabled;
            set
            {
                if (SetProperty(ref _isAutoSetNewGameSixEnabled, value))
                {
                    if (_isAutoSetNewGameSixEnabled && AreOptionsEnabled) SetNewGame(8);
                }
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
            AreOptionsEnabled = true;
            _timer.Start();
        }

        public void DisableFeatures()
        {
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

        public void SetVigor(int value) => Vigor = value;

        public void SetAttunement(int value) => Attunement = value;

        public void SetEndurance(int value) => Endurance = value;

        public void SetStrength(int value) => Strength = value;

        public void SetDexterity(int value) => Dexterity = value;

        public void SetIntelligence(int value) => Intelligence = value;

        public void SetFaith(int value) => Faith = value;

        public void SetLuck(int value) => Luck = value;

        public void SetVitality(int value) => Vitality = value;

        public void SetSoulLevel(int value) => SoulLevel = value;

        public void SetSouls(int value) => Souls = value;

        public void SetNewGame(int value) => NewGame = value;
    }
}
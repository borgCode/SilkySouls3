using System;
using System.Windows.Threading;
using SilkySouls3.Memory;
using SilkySouls3.Services;
using SilkySouls3.Utilities;

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
        
        
        private bool _pauseUpdates;
        private bool _areOptionsEnabled;
        private readonly DispatcherTimer _timer;
        
        private readonly PlayerService _playerService;
        public PlayerViewModel(PlayerService playerService, HotkeyManager hotkeyManager)
        {
            _playerService = playerService;
            
            
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += (s, e) =>
            {
                if (_pauseUpdates) return;
                
                CurrentHp = _playerService.GetHp();
                CurrentMaxHp = _playerService.GetMaxHp();
                // Souls = _playerService.GetSetPlayerStat(Offsets.GameDataMan.PlayerGameData.Souls);
                // PlayerSpeed = _playerService.GetSetPlayerSpeed(null);
                // int? newSoulLevel = _playerService.GetSoulLevel();
                // if (_currentSoulLevel != newSoulLevel)
                // {
                //     SoulLevel = newSoulLevel;
                //     _currentSoulLevel = newSoulLevel;
                //     LoadStats();
                // }

            };
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
                    _playerService.ToggleDebugFlag(Offsets.DebugFlags.NoDeath, _isNoDeathEnabled ? 1 : 0);
                }
            }
        }
        //
        // public bool IsNoDamageEnabled
        // {
        //     get => _isNoDamageEnabled;
        //     set
        //     {
        //         if (SetProperty(ref _isNoDamageEnabled, value))
        //         {
        //             _playerService.ToggleNoDamage(_isNoDamageEnabled);
        //         }
        //     }
        // }
        //
        public bool IsInfiniteStaminaEnabled
        {
            get => _isInfiniteStaminaEnabled;
            set
            {
                if (SetProperty(ref _isInfiniteStaminaEnabled, value))
                {
                    _playerService.ToggleDebugFlag(Offsets.DebugFlags.InfiniteStam, _isInfiniteStaminaEnabled ? 1 : 0);
                }
            }
        }
        //
        // public bool IsNoGoodsConsumeEnabled
        // {
        //     get => _isNoGoodsConsumeEnabled;
        //     set
        //     {
        //         if (SetProperty(ref _isNoGoodsConsumeEnabled, value))
        //         {
        //             _playerService.ToggleNoGoodsConsume(_isNoGoodsConsumeEnabled);
        //         }
        //     }
        // }
        //
        // public bool IsInfiniteDurabilityEnabled
        // {
        //     get => _isInfiniteDurabilityEnabled;
        //     set
        //     {
        //         if (SetProperty(ref _isInfiniteDurabilityEnabled, value))
        //         {

        //             _playerService.ToggleInfiniteDurability(_isInfiniteDurabilityEnabled);

        //         }

        //     }
        // }

        //

        public bool IsInfiniteFpEnabled
        {
            get => _isInfiniteFpEnabled;
            set
            {
                if (SetProperty(ref _isInfiniteFpEnabled, value))
                {
                    _playerService.ToggleDebugFlag(Offsets.DebugFlags.InfiniteFp, _isInfiniteFpEnabled ? 1 : 0);
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
                    _playerService.ToggleDebugFlag(Offsets.DebugFlags.OneShot, _isOneShotEnabled ? 1 : 0);
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
                    _playerService.ToggleDebugFlag(Offsets.DebugFlags.Invisible, _isInvisibleEnabled ? 1 : 0);
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
                    _playerService.ToggleDebugFlag(Offsets.DebugFlags.Silent, _isSilentEnabled ? 1 : 0);
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
                    _playerService.ToggleDebugFlag(Offsets.DebugFlags.InfiniteArrows, _isNoAmmoConsumeEnabled ? 1 : 0);
                }
            }
        }

        //

        // public bool IsInfinitePoiseEnabled

        // {

        //     get => _isInfinitePoiseEnabled;

        //     set

        //     {

        //         if (SetProperty(ref _isInfinitePoiseEnabled, value))

        //         {

        //             _playerService.ToggleInfinitePoise(_isInfinitePoiseEnabled);

        //         }

        //     }

        // }

        //

        // public bool IsAutoSetNewGameSixEnabled

        // {

        //     get => _isAutoSetNewGameSixEnabled;

        //     set

        //     {

        //         if (SetProperty(ref _isAutoSetNewGameSixEnabled, value))

        //         {

        //             if (_isAutoSetNewGameSixEnabled && AreOptionsEnabled)

        //             {

        //                 NewGame = _playerService.GetSetNewGame(7);

        //             }

        //         }

        //     }

        // }


        public void TryEnableFeatures()
        {
            if (IsNoDeathEnabled)
                _playerService.ToggleDebugFlag(Offsets.DebugFlags.NoDeath, 1);
            // if (IsNoDamageEnabled)
            //     _playerService.ToggleNoDamage(true);
            if (IsInfiniteStaminaEnabled)
                _playerService.ToggleDebugFlag(Offsets.DebugFlags.InfiniteStam, 1);
            // if (IsNoGoodsConsumeEnabled)
            //     _playerService.ToggleNoGoodsConsume(true);
            if (_isInfiniteFpEnabled)
                _playerService.ToggleDebugFlag(Offsets.DebugFlags.InfiniteFp, 1);
            if (IsOneShotEnabled)
                _playerService.ToggleDebugFlag(Offsets.DebugFlags.OneShot, 1);
            if (IsInvisibleEnabled)
                _playerService.ToggleDebugFlag(Offsets.DebugFlags.Invisible, 1);
            if (IsSilentEnabled)
                _playerService.ToggleDebugFlag(Offsets.DebugFlags.Silent, 1);
            if (IsNoAmmoConsumeEnabled)
                _playerService.ToggleDebugFlag(Offsets.DebugFlags.InfiniteArrows, 1);
            // if (IsInfinitePoiseEnabled)
            //     _playerService.ToggleInfinitePoise(true);
            // if (IsInfiniteDurabilityEnabled)
            //     _playerService.ToggleInfiniteDurability(true);
            AreOptionsEnabled = true;
            _timer.Start();
        }

        public void DisableFeatures()
        {
            
        }
    }
}
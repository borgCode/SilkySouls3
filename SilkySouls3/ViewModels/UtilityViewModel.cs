using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SilkySouls3.Memory;
using SilkySouls3.Models;
using SilkySouls3.Services;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.ViewModels
{
    public class UtilityViewModel : BaseViewModel
    {
        private readonly HotkeyManager _hotkeyManager;
        private readonly UtilityService _utilityService;
        
        private bool _isHitboxEnabled;
        private bool _isSoundViewEnabled;
        private bool _isDrawEventEnabled;
        private bool _isTargetingViewEnabled;
        private bool _isHideMapEnabled;
        private bool _isHideObjectsEnabled;
        private bool _isHideCharactersEnabled;
        private bool _isHideSfxEnabled;
        private bool _isDisableEventEnabled;
        private bool _isFreeCamEnabled;
        private int _freeCamMode = 1;
        private bool _isCamVertIncreaseEnabled;

        
        private const float DefaultNoclipMultiplier = 0.25f;
        private const uint BaseXSpeedHex = 0x3e4ccccd;
        private const uint BaseYSpeedHex = 0x3e19999a;
        private float _noClipSpeedMultiplier = DefaultNoclipMultiplier;
        private float _gameSpeed;

        private bool _isNoClipEnabled;
        private bool _areButtonsEnabled;

        private bool _wasNoDeathEnabled;

        private readonly Dictionary<string, WarpEntry> _warpLocations;
        private KeyValuePair<string, string> _selectedWarp;
        private readonly PlayerViewModel _playerViewModel;

        public UtilityViewModel(UtilityService utilityService, HotkeyManager hotkeyManager,
            PlayerViewModel playerViewModel)
        {
            _playerViewModel = playerViewModel;
            _utilityService = utilityService;
            _hotkeyManager = hotkeyManager;
            _warpLocations = DataLoader.GetWarpEntryDict();
            
            if (_warpLocations.Any())
            {
                var firstLocation = _warpLocations.First();
                _selectedWarp = new KeyValuePair<string, string>(firstLocation.Key, firstLocation.Value.Name);
            }
            
            RegisterHotkeys();
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction("NoClip", () => { IsNoClipEnabled = !IsNoClipEnabled; });
            _hotkeyManager.RegisterAction("IncreaseNoClipSpeed", () => 
            { 
                if (IsNoClipEnabled)
                    NoClipSpeed = Math.Min(5, NoClipSpeed + 0.50f);
            });
    
            _hotkeyManager.RegisterAction("DecreaseNoClipSpeed", () => 
            { 
                if (IsNoClipEnabled)
                    NoClipSpeed = Math.Max(0.05f, NoClipSpeed - 0.50f);
            });
            _hotkeyManager.RegisterAction("IncreaseGameSpeed", () => SetSpeed(Math.Min(10, GameSpeed + 0.50f)));
            _hotkeyManager.RegisterAction("DecreaseGameSpeed", () => SetSpeed(Math.Max(0, GameSpeed - 0.50f)));
        }

        public IEnumerable<KeyValuePair<string, string>> WarpLocations =>
            _warpLocations.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.Name));

        public KeyValuePair<string, string> SelectedWarp
        
        {
            get => _selectedWarp;
            set => SetProperty(ref _selectedWarp, value);
        }


        public bool AreButtonsEnabled
        {
            get => _areButtonsEnabled;
            set => SetProperty(ref _areButtonsEnabled, value);
        }

        public bool IsHitboxEnabled
        {
            get => _isHitboxEnabled;
            set
            {
                if (!SetProperty(ref _isHitboxEnabled, value)) return;
                _utilityService.ToggleHitboxView(_isHitboxEnabled);
            }
        }

        public bool IsSoundViewEnabled
        {
            get => _isSoundViewEnabled;
            set
            {
                if (!SetProperty(ref _isSoundViewEnabled, value)) return;
                  _utilityService.ToggleSoundView(_isSoundViewEnabled);
            }
        }
        
        public bool IsDrawEventEnabled
        {
            get => _isDrawEventEnabled;
            set
            {
                if (!SetProperty(ref _isDrawEventEnabled, value)) return;
                _utilityService.ToggleEventDraw(_isDrawEventEnabled);
            }
        }

        public bool IsTargetingViewEnabled
        {
            get => _isTargetingViewEnabled;
            set
            {
                if (!SetProperty(ref _isTargetingViewEnabled, value)) return;
                _utilityService.ToggleTargetingView(_isTargetingViewEnabled);
            }
        }

        public bool IsHideMapEnabled
        {
            get => _isHideMapEnabled;
            set
            {
                if (!SetProperty(ref _isHideMapEnabled, value)) return;
                _utilityService.ToggleGroupMask(GroupMask.Map, _isHideMapEnabled);
            }
        }

        public bool IsHideObjectsEnabled
        {
            get => _isHideObjectsEnabled;
            set
            {
                if (!SetProperty(ref _isHideObjectsEnabled, value)) return;
                _utilityService.ToggleGroupMask(GroupMask.Obj, _isHideObjectsEnabled);
            }
        }

        public bool IsHideCharactersEnabled
        {
            get => _isHideCharactersEnabled;
            set
            {
                if (!SetProperty(ref _isHideCharactersEnabled, value)) return;
                _utilityService.ToggleGroupMask(GroupMask.Chr, _isHideCharactersEnabled);
            }
        }

        public bool IsHideSfxEnabled
        {
            get => _isHideSfxEnabled;
            set
            {
                if (!SetProperty(ref _isHideSfxEnabled, value)) return;
                _utilityService.ToggleGroupMask(GroupMask.Sfx, _isHideSfxEnabled);
            }
        }
        
        public bool IsDisableEventEnabled
        {
            get => _isDisableEventEnabled;
            set
            {
                if (!SetProperty(ref _isDisableEventEnabled, value)) return;
                _utilityService.ToggleDisableEvent(_isDisableEventEnabled);
            }
        }
        
        public bool IsFreeCamEnabled
        {
            get => _isFreeCamEnabled;
            set
            {
                if (!SetProperty(ref _isFreeCamEnabled, value)) return;
                if (_isFreeCamEnabled)
                {
                    int modeNumber = IsFreeCamMode1Selected ? 1 : 2;
                    _utilityService.SetFreeCamState(true, modeNumber);
                }
                else _utilityService.SetFreeCamState(false, 0);
            }
        }

        

        public int FreeCamMode
        {
            get => _freeCamMode;
            set
            {
                if (SetProperty(ref _freeCamMode, value) && IsFreeCamEnabled)
                {
                    _utilityService.SetFreeCamState(true, value);
                }
            }
        }

        public bool IsFreeCamMode1Selected
        {
            get => _freeCamMode == 1;
            set
            {
                if (value) FreeCamMode = 1;
         
            }
        }

        public bool IsFreeCamMode2Selected
        {
            get => _freeCamMode == 2;
            set
            {
                if (value) FreeCamMode = 2;
                
            }
        }
        
        public bool IsCamVertIncreaseEnabled
        {
            get => _isCamVertIncreaseEnabled;
            set
            {
                if (!SetProperty(ref _isCamVertIncreaseEnabled, value)) return;
                _utilityService.ToggleCamVertIncrease(_isCamVertIncreaseEnabled);
            }
        }

        public bool IsNoClipEnabled
        {
            get => _isNoClipEnabled;
            set
            {
                if (!SetProperty(ref _isNoClipEnabled, value)) return;
                
                if (_isNoClipEnabled)
                {
                    _utilityService.ToggleNoClip(_isNoClipEnabled);
                    _wasNoDeathEnabled = _playerViewModel.IsNoDeathEnabled;
                    _playerViewModel.IsNoDeathEnabled = true;
                    _playerViewModel.IsSilentEnabled = true;
                    _playerViewModel.IsInvisibleEnabled = true;
                }
                else
                {
                    _utilityService.ToggleNoClip(_isNoClipEnabled);
                    _playerViewModel.IsNoDeathEnabled = _wasNoDeathEnabled;
                    _playerViewModel.IsSilentEnabled = false;
                    _playerViewModel.IsInvisibleEnabled = false;
                    NoClipSpeed = DefaultNoclipMultiplier;

                }
            }
        }

        public float NoClipSpeed
        {
            get => _noClipSpeedMultiplier;
            set
            {
                if (SetProperty(ref _noClipSpeedMultiplier, value))
                {
                    SetNoClipSpeed(value);
                }
            }
        }

        public void SetNoClipSpeed(float multiplier)
        {
            if (!IsNoClipEnabled) return;
            if (multiplier < 0.05f) multiplier = 0.05f;
            else if (multiplier > 5.0f) multiplier = 5.0f;
            
            SetProperty(ref _noClipSpeedMultiplier, multiplier);
            
            float baseXFloat = BitConverter.ToSingle(BitConverter.GetBytes(BaseXSpeedHex), 0);
            float baseYFloat = BitConverter.ToSingle(BitConverter.GetBytes(BaseYSpeedHex), 0);
            
            float newXFloat = baseXFloat * multiplier;
            float newYFloat = baseYFloat * multiplier;
            
            byte[] xBytes = BitConverter.GetBytes(newXFloat);
            byte[] yBytes = BitConverter.GetBytes(newYFloat);
            
            _utilityService.SetNoClipSpeed(xBytes, yBytes);
        }


        public float GameSpeed
        {
            get => _gameSpeed;
            set
            {
                if (SetProperty(ref _gameSpeed, value))
                {
                    _utilityService.SetGameSpeed(value);
                }
            }
        }

        public void SetSpeed(float value) => GameSpeed = value;

        public void Warp()
        {
            
            _ = Task.Run(() => _utilityService.Warp(_warpLocations[SelectedWarp.Key]));
        }

        public void OpenMenu(long funcAddr)
        {
            _utilityService.OpenMenu(funcAddr);
        }

        public void OpenMenuWithEvent(long funcAddr, int[] eventIds)
        {
            _utilityService.OpenMenuWithEvent(funcAddr, eventIds);
        }

        public void OpenRegularShop(ulong[] shopParams)
        {
            _utilityService.OpenRegularShop(shopParams);
        }

        public void UnlockBonfires()
        {
            _utilityService.UnlockBonfires(_warpLocations.Values.Where(w => w.IsStandardWarp));
        }


        public void UnlockMidir()
        {
            _utilityService.SetEvent(GameIds.EventFlags.UnlockMidir);
        }

        public void MovePatchesToFirelink()
        {
            _utilityService.SetMultipleEvents(GameIds.EventFlags.Patches);
        }

        public void TryEnableFeatures()
        {
            if (IsHitboxEnabled) _utilityService.ToggleHitboxView(true);
            if (IsSoundViewEnabled) _utilityService.ToggleSoundView(true);
            if (IsDrawEventEnabled) _utilityService.ToggleEventDraw(true);
            if (IsTargetingViewEnabled) _utilityService.ToggleTargetingView(true);
            if (IsHideMapEnabled) _utilityService.ToggleGroupMask(GroupMask.Map,true);
            if (IsHideObjectsEnabled) _utilityService.ToggleGroupMask(GroupMask.Obj,true);
            if (IsHideCharactersEnabled) _utilityService.ToggleGroupMask(GroupMask.Chr,true);
            if (IsHideSfxEnabled) _utilityService.ToggleGroupMask(GroupMask.Sfx,true);
            GameSpeed = _utilityService.GetGameSpeed();
            AreButtonsEnabled = true;
        }

        public void DisableFeatures()
        {
            IsNoClipEnabled = false;
            AreButtonsEnabled = false;
        }

        public void TryApplyOneTimeFeatures()
        {
            if (IsCamVertIncreaseEnabled) _utilityService.ToggleCamVertIncrease(true);
        }
    }
}
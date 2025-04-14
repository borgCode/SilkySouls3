using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SilkySouls3.Memory;
using SilkySouls3.Models;
using SilkySouls3.Services;
using SilkySouls3.Utilities;

namespace SilkySouls3.ViewModels
{
    public class UtilityViewModel : BaseViewModel
    {
        
        private readonly UtilityService _utilityService;
        
        private bool _isHitboxEnabled;
        private bool _isSoundViewEnabled;
        private bool _isDrawEventEnabled;
        private bool _isTargetingViewEnabled;
        private bool _isNoClipEnabled;
        private bool _areButtonsEnabled;
        private bool _areAttachedOptionsEnabled;
        private bool _areAttachedOptionsRestored;
        
        private bool _wasNoDeathEnabled;
        
        private readonly Dictionary<string, WarpEntry> _warpLocations;
        private KeyValuePair<string, string> _selectedWarp;
        
        public UtilityViewModel(UtilityService utilityService, HotkeyManager hotkeyManager)
        {
            _utilityService = utilityService;
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
          
        }

        public IEnumerable<KeyValuePair<string, string>> WarpLocations =>
            _warpLocations.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.Name));
        
        public KeyValuePair<string, string> SelectedWarp
        
        {
            get => _selectedWarp;
            set => SetProperty(ref _selectedWarp, value);
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
                    // if (_playerService.IsNoDeathOn()) _wasNoDeathEnabled = true;
                    // else _playerService.ToggleNoDeath(1);
                }
                else
                {
                    _utilityService.ToggleNoClip(_isNoClipEnabled);
                    // _utilityService.DisableNoClip();
                    // if (_wasNoDeathEnabled) _wasNoDeathEnabled = false;
                    // else _playerService.ToggleNoDeath(0);
                }
            }
        }
        
        // public bool AreButtonsEnabled
        // {
        //     get => _areButtonsEnabled;
        //     set => SetProperty(ref _areButtonsEnabled, value);
        // }
        //
        // public bool AreAttachedOptionsEnabled
        // {
        //     get => _areAttachedOptionsEnabled;
        //     set => SetProperty(ref _areAttachedOptionsEnabled, value);
        // }

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
        
        //
        // public bool IsDrawEventEnabled
        // {
        //     get => _isDrawEventEnabled;
        //     set
        //     {
        //         if (!SetProperty(ref _isDrawEventEnabled, value)) return;
        //         if (_isDrawEventEnabled)
        //         {
        //             _utilityService.EnableDrawEvent();
        //         }
        //         else
        //         {
        //             _utilityService.DisableDrawEvent();
        //         }
        //     }
        // }
        //
        // public bool IsTargetingViewEnabled
        // {
        //     get => _isTargetingViewEnabled;
        //     set
        //     {
        //         if (!SetProperty(ref _isTargetingViewEnabled, value)) return;
        //         if (_isTargetingViewEnabled)
        //         {
        //             _utilityService.EnableTargetingView();
        //         }
        //         else
        //         {
        //             _utilityService.DisableTargetingView();
        //         }
        //     }
        // }
        
        public void Warp()
        {
            
            _ = Task.Run(() => _utilityService.Warp(_warpLocations[SelectedWarp.Key]));
        }

        public void UnlockBonfires()
        {
            _utilityService.UnlockBonfires(_warpLocations.Values.Where(w => w.IsStandardWarp));
        }


        public void UnlockMidir()
        {
            _utilityService.SetEvent(EventFlags.UnlockMidir);
        }

        public void MovePatchesToFirelink()
        {
            _utilityService.SetMultipleEvents(EventFlags.Patches);
        }
    }
}
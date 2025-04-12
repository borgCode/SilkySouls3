using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SilkySouls3.Models;
using SilkySouls3.Services;
using SilkySouls3.Utilities;

namespace SilkySouls3.ViewModels
{
    public class UtilityViewModel : BaseViewModel
    {
        
        private readonly UtilityService _utilityService;
        
        private bool _isNoClipEnabled;
        
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
                    _utilityService.EnableNoClip();
                    // if (_playerService.IsNoDeathOn()) _wasNoDeathEnabled = true;
                    // else _playerService.ToggleNoDeath(1);
                }
                else
                {
                    // _utilityService.DisableNoClip();
                    // if (_wasNoDeathEnabled) _wasNoDeathEnabled = false;
                    // else _playerService.ToggleNoDeath(0);
                }
            }
        }
        
        public void Warp()
        {
            
            _ = Task.Run(() => _utilityService.Warp(_warpLocations[SelectedWarp.Key]));
        }

        public void UnlockBonfires()
        {
            _utilityService.UnlockBonfires(_warpLocations.Values.Where(w => w.IsStandardWarp));
        }
    }
}
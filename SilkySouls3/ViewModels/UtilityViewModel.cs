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
        
        private readonly Dictionary<string, WarpEntry> _warpLocations;
        private KeyValuePair<string, string> _selectedWarp;
        
        public UtilityViewModel(UtilityService utilityService)
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
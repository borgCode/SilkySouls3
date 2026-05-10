using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SilkySouls3.Core;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Models;
using SilkySouls3.Utilities;
using SilkySouls3.Views;

namespace SilkySouls3.ViewModels
{
    public class TravelViewModel : BaseViewModel
    {
        private readonly ITravelService _travelService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly IDlcService _dlcService;
        private readonly IPlayerService _playerService;

        private bool _areOptionsEnabled;

        private Dictionary<string, List<CustomWarp>> _customWarpDict;
        private ObservableCollection<string> _customMainAreas;
        private ObservableCollection<CustomWarp> _customAreaLocations;
        private string _selectedCustomMainArea;
        private CustomWarp _selectedCustomWarp;

        private ObservableCollection<string> _mainAreas;
        private ObservableCollection<WarpLocation> _areaLocations;

        private string _selectedMainArea;
        private WarpLocation _selectedWarpLocation;

        private Dictionary<string, List<WarpLocation>> _locationDict;
        private List<WarpLocation> _allLocations;

        private string _searchText = string.Empty;
        private bool _isSearchActive;
        private string _preSearchMainArea;

        private readonly ObservableCollection<WarpLocation> _searchResultsCollection = new();

        private string _customSearchText = string.Empty;
        private bool _isCustomSearchActive;
        private string _preSearchCustomMainArea;

        public TravelViewModel(ITravelService travelService, HotkeyManager hotkeyManager, IStateService stateService,
            IDlcService dlcService, IPlayerService playerService)
        {
            _travelService = travelService;
            _hotkeyManager = hotkeyManager;
            _dlcService = dlcService;
            _playerService = playerService;

            _mainAreas = new ObservableCollection<string>();
            _areaLocations = new ObservableCollection<WarpLocation>();
            _customMainAreas = new ObservableCollection<string>();
            _customAreaLocations = new ObservableCollection<CustomWarp>();

            WarpCommand = new DelegateCommand(Warp);
            UnlockBonfiresCommand = new DelegateCommand(UnlockBonfires);
            CustomWarpCommand = new DelegateCommand(CustomWarp);
            OpenCreateCustomWarpCommand = new DelegateCommand(OpenCreateCustomWarp);

            stateService.Subscribe(State.Loaded, OnGameLoaded);
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);

            LoadLocations();
            LoadCustomWarps();
            RegisterHotkeys();
        }

        #region Commands

        public ICommand WarpCommand { get; }
        public ICommand UnlockBonfiresCommand { get; }
        public ICommand CustomWarpCommand { get; }
        public ICommand OpenCreateCustomWarpCommand { get; }

        #endregion

        #region Properties

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        public ObservableCollection<string> MainAreas
        {
            get => _mainAreas;
            private set => SetProperty(ref _mainAreas, value);
        }

        public ObservableCollection<WarpLocation> AreaLocations
        {
            get => _areaLocations;
            set => SetProperty(ref _areaLocations, value);
        }

        public string SelectedMainArea
        {
            get => _selectedMainArea;
            set
            {
                if (!SetProperty(ref _selectedMainArea, value) || value == null) return;

                if (_isSearchActive)
                {
                    IsSearchActive = false;
                    _searchText = string.Empty;
                    OnPropertyChanged(nameof(SearchText));
                    _preSearchMainArea = null;
                }

                UpdateLocationsList();
            }
        }

        public WarpLocation SelectedWarpLocation
        {
            get => _selectedWarpLocation;
            set => SetProperty(ref _selectedWarpLocation, value);
        }

        public ObservableCollection<string> CustomMainAreas
        {
            get => _customMainAreas;
            private set => SetProperty(ref _customMainAreas, value);
        }

        public ObservableCollection<CustomWarp> CustomAreaLocations
        {
            get => _customAreaLocations;
            set => SetProperty(ref _customAreaLocations, value);
        }

        public string SelectedCustomMainArea
        {
            get => _selectedCustomMainArea;
            set
            {
                if (!SetProperty(ref _selectedCustomMainArea, value) || value == null) return;

                if (_isCustomSearchActive)
                {
                    IsCustomSearchActive = false;
                    _customSearchText = string.Empty;
                    OnPropertyChanged(nameof(CustomSearchText));
                    _preSearchCustomMainArea = null;
                }

                UpdateCustomLocationsList();
            }
        }

        public CustomWarp SelectedCustomWarp
        {
            get => _selectedCustomWarp;
            set => SetProperty(ref _selectedCustomWarp, value);
        }

        public bool IsCustomSearchActive
        {
            get => _isCustomSearchActive;
            private set => SetProperty(ref _isCustomSearchActive, value);
        }

        public string CustomSearchText
        {
            get => _customSearchText;
            set
            {
                if (!SetProperty(ref _customSearchText, value)) return;

                if (string.IsNullOrEmpty(value))
                {
                    _isCustomSearchActive = false;

                    if (_preSearchCustomMainArea != null)
                    {
                        _selectedCustomMainArea = _preSearchCustomMainArea;
                        OnPropertyChanged(nameof(SelectedCustomMainArea));
                        UpdateCustomLocationsList();
                        _preSearchCustomMainArea = null;
                    }
                }
                else
                {
                    if (!_isCustomSearchActive)
                    {
                        _preSearchCustomMainArea = SelectedCustomMainArea;
                        _isCustomSearchActive = true;
                    }

                    ApplyCustomFilter();
                }
            }
        }

        public bool IsSearchActive
        {
            get => _isSearchActive;
            private set => SetProperty(ref _isSearchActive, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (!SetProperty(ref _searchText, value)) return;

                if (string.IsNullOrEmpty(value))
                {
                    _isSearchActive = false;

                    if (_preSearchMainArea != null)
                    {
                        _selectedMainArea = _preSearchMainArea;
                        UpdateLocationsList();
                        _preSearchMainArea = null;
                    }
                }
                else
                {
                    if (!_isSearchActive)
                    {
                        _preSearchMainArea = SelectedMainArea;
                        _isSearchActive = true;
                    }

                    ApplyFilter();
                }
            }
        }

        #endregion
        
        #region Private Methods

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.Warp, Warp);
        }

        private void LoadLocations()
        {
            _locationDict = DataLoader.GetWarpLocations();

            _allLocations = _locationDict.Values.SelectMany(x => x).ToList();

            foreach (var area in _locationDict.Keys)
            {
                _mainAreas.Add(area);
            }

            SelectedMainArea = _mainAreas.FirstOrDefault();
        }

        private void UpdateLocationsList()
        {
            if (string.IsNullOrEmpty(SelectedMainArea) || !_locationDict.ContainsKey(SelectedMainArea))
            {
                AreaLocations = new ObservableCollection<WarpLocation>();
                return;
            }

            AreaLocations = new ObservableCollection<WarpLocation>(_locationDict[SelectedMainArea]);
            SelectedWarpLocation = AreaLocations.FirstOrDefault();
        }

        private void ApplyFilter()
        {
            _searchResultsCollection.Clear();
            var searchTextLower = SearchText.ToLower();

            foreach (var location in _allLocations)
            {
                if (location.Name.ToLower().Contains(searchTextLower) ||
                    location.MainArea.ToLower().Contains(searchTextLower))
                {
                    _searchResultsCollection.Add(location);
                }
            }

            AreaLocations = new ObservableCollection<WarpLocation>(_searchResultsCollection);
            SelectedWarpLocation = AreaLocations.FirstOrDefault();
        }
        
        private void Warp()
        {
            if (!_dlcService.MeetsRequirement(SelectedWarpLocation.DlcRequirement)) return;

            if (SelectedWarpLocation.HasCoordinates)
            {
                // ReSharper disable once PossibleInvalidOperationException
                _ = Task.Run(() => _travelService.WarpWithCoords(SelectedWarpLocation.Coords.Value,
                    SelectedWarpLocation.Angle, SelectedWarpLocation.BonfireId));
            }
            else
            {
                _travelService.Warp(SelectedWarpLocation.BonfireId);
            }
        }

        private void UnlockBonfires()
        {
            _travelService.UnlockBonfires(_allLocations.Where(w => w.IsStandardWarp));
        }

        private void LoadCustomWarps()
        {
            _customWarpDict = DataLoader.LoadCustomWarps();
            RebuildCustomMainAreas();
        }

        private void RebuildCustomMainAreas()
        {
            var previousSelection = _selectedCustomMainArea;
            _customMainAreas.Clear();
            foreach (var area in _customWarpDict.Keys)
            {
                _customMainAreas.Add(area);
            }

            if (previousSelection != null && _customWarpDict.ContainsKey(previousSelection))
            {
                SelectedCustomMainArea = previousSelection;
            }
            else
            {
                SelectedCustomMainArea = _customMainAreas.FirstOrDefault();
                if (SelectedCustomMainArea == null) UpdateCustomLocationsList();
            }
        }

        private void UpdateCustomLocationsList()
        {
            if (string.IsNullOrEmpty(SelectedCustomMainArea) || !_customWarpDict.ContainsKey(SelectedCustomMainArea))
            {
                CustomAreaLocations = new ObservableCollection<CustomWarp>();
                SelectedCustomWarp = null;
                return;
            }

            CustomAreaLocations = new ObservableCollection<CustomWarp>(_customWarpDict[SelectedCustomMainArea]);
            SelectedCustomWarp = CustomAreaLocations.FirstOrDefault();
        }

        private void ApplyCustomFilter()
        {
            var searchTextLower = CustomSearchText.ToLower();
            var matches = _customWarpDict
                .SelectMany(kv => kv.Value)
                .Where(w => (w.Name != null && w.Name.ToLower().Contains(searchTextLower)) ||
                            (w.MainArea != null && w.MainArea.ToLower().Contains(searchTextLower)));

            CustomAreaLocations = new ObservableCollection<CustomWarp>(matches);
            SelectedCustomWarp = CustomAreaLocations.FirstOrDefault();
        }

        private void CustomWarp()
        {
            if (SelectedCustomWarp == null) return;
            if (!_dlcService.MeetsRequirement(SelectedCustomWarp.DlcRequirement)) return;

            var warp = SelectedCustomWarp;
            _ = Task.Run(() => _travelService.WarpWithCoords(
                warp.Position.Coords, warp.Position.Angle, warp.BonfireId));
        }

        private void OpenCreateCustomWarp()
        {
            var window = new CreateCustomWarpWindow(
                _customWarpDict,
                AreOptionsEnabled,
                _playerService,
                _travelService,
                OnCustomWarpChanged);
            window.ShowDialog();
        }

        private void OnCustomWarpChanged(CustomWarpChange change)
        {
            switch (change)
            {
                case WarpAdded added:
                    if (!_customWarpDict.TryGetValue(added.Warp.MainArea, out var addList))
                    {
                        addList = new List<CustomWarp>();
                        _customWarpDict[added.Warp.MainArea] = addList;
                    }
                    if (!addList.Contains(added.Warp)) addList.Add(added.Warp);
                    break;

                case WarpDeleted deleted:
                    if (_customWarpDict.TryGetValue(deleted.Category, out var delList))
                    {
                        delList.Remove(deleted.Warp);
                        if (delList.Count == 0) _customWarpDict.Remove(deleted.Category);
                    }
                    break;

                case CategoryDeleted catDeleted:
                    _customWarpDict.Remove(catDeleted.Category);
                    break;
            }

            RebuildCustomMainAreas();
            DataLoader.SaveCustomWarps(_customWarpDict);
        }

        private void OnGameLoaded()
        {
            AreOptionsEnabled = true;
        }

        private void OnGameNotLoaded()
        {
            AreOptionsEnabled = false;
        }

        #endregion
    }
}
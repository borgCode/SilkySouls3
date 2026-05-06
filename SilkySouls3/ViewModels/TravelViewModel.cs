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

namespace SilkySouls3.ViewModels
{
    public class TravelViewModel : BaseViewModel
    {
        private readonly ITravelService _travelService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly IDlcService _dlcService;

        private bool _areOptionsEnabled;

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

        public TravelViewModel(ITravelService travelService, HotkeyManager hotkeyManager, IStateService stateService,
            IDlcService dlcService)
        {
            _travelService = travelService;
            _hotkeyManager = hotkeyManager;
            _dlcService = dlcService;

            _mainAreas = new ObservableCollection<string>();
            _areaLocations = new ObservableCollection<WarpLocation>();

            WarpCommand = new DelegateCommand(Warp);
            UnlockBonfiresCommand = new DelegateCommand(UnlockBonfires);

            stateService.Subscribe(State.Loaded, OnGameLoaded);
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);

            LoadLocations();
            RegisterHotkeys();
        }

        #region Commands

        public ICommand WarpCommand { get; }
        public ICommand UnlockBonfiresCommand { get; }

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
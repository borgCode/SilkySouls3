using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SilkySouls3.Core;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Models;
using SilkySouls3.Utilities;
using SilkySouls3.Views;

namespace SilkySouls3.ViewModels
{
    public class ItemViewModel : BaseViewModel
    {
        private readonly IItemService _itemService;

        private readonly Dictionary<string, ObservableCollection<Item>> _itemsByCategory = new();

        private ILookup<string, Item> _allItems;

        private readonly ObservableCollection<Item> _searchResultsCollection = new();
        private string _preSearchCategory;

        private Dictionary<string, LoadoutTemplate> _loadoutTemplatesByName = new();
        private Dictionary<string, LoadoutTemplate> _customLoadoutTemplates = new();
        
        public ItemViewModel(IItemService itemService, IStateService stateService)
        {
            _itemService = itemService;

            stateService.Subscribe(State.Loaded, OnGameLoaded);
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);
            stateService.Subscribe(State.OnNewGameStart, OnNewGameStart);

            SpawnItemCommand = new DelegateCommand(SpawnItem);
            MassSpawnCommand = new DelegateCommand(MassSpawn);
            SpawnLoadoutCommand = new DelegateCommand(SpawnLoadout);
            ShowCreateLoadoutCommand = new DelegateCommand(ShowCreateLoadoutWindow);

            LoadData();
        }

        #region Commands

        public ICommand SpawnItemCommand { get; }
        public ICommand MassSpawnCommand { get; }
        public ICommand SpawnLoadoutCommand { get; }
        public ICommand ShowCreateLoadoutCommand { get; }

        #endregion

        #region Properties

        public Dictionary<string, int> InfusionTypes { get; } = new()
        {
            { "Normal", 0 }, { "Heavy", 100 }, { "Sharp", 200 },
            { "Refined", 300 }, { "Simple", 400 }, { "Crystal", 500 },
            { "Fire", 600 }, { "Chaos", 700 }, { "Lightning", 800 },
            { "Deep", 900 }, { "Dark", 1000 }, { "Poison", 1100 },
            { "Blood", 1200 }, { "Raw", 1300 }, { "Blessed", 1400 },
            { "Hollow", 1500 }
        };

        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        private ObservableCollection<string> _categories = new();

        public ObservableCollection<string> Categories
        {
            get => _categories;
            private set => SetProperty(ref _categories, value);
        }

        private ObservableCollection<Item> _items = new();

        public ObservableCollection<Item> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private ObservableCollection<string> _loadouts;

        public ObservableCollection<string> Loadouts
        {
            get => _loadouts;
            private set => SetProperty(ref _loadouts, value);
        }

        private string _selectedLoadoutName;

        public string SelectedLoadoutName
        {
            get => _selectedLoadoutName;
            set => SetProperty(ref _selectedLoadoutName, value);
        }

        private string _selectedCategory;

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (!SetProperty(ref _selectedCategory, value) || value == null) return;
                if (_selectedCategory == null) return;

                if (_isSearchActive)
                {
                    IsSearchActive = false;
                    _searchText = string.Empty;
                    OnPropertyChanged(nameof(SearchText));
                    _preSearchCategory = null;
                }

                Items = _itemsByCategory[_selectedCategory];
                SelectedItem = Items.FirstOrDefault();
            }
        }

        private bool _canUpgrade;

        public bool CanUpgrade
        {
            get => _canUpgrade;
            private set => SetProperty(ref _canUpgrade, value);
        }

        private bool _canInfuse;

        public bool CanInfuse
        {
            get => _canInfuse;
            private set => SetProperty(ref _canInfuse, value);
        }

        private int _maxUpgradeLevel = 10;

        public int MaxUpgradeLevel
        {
            get => _maxUpgradeLevel;
            private set => SetProperty(ref _maxUpgradeLevel, value);
        }

        private bool _quantityEnabled;

        public bool QuantityEnabled
        {
            get => _quantityEnabled;
            private set => SetProperty(ref _quantityEnabled, value);
        }

        private int _maxQuantity;

        public int MaxQuantity
        {
            get => _maxQuantity;
            private set => SetProperty(ref _maxQuantity, value);
        }

        private bool _isSearchActive;

        public bool IsSearchActive
        {
            get => _isSearchActive;
            private set => SetProperty(ref _isSearchActive, value);
        }

        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (!SetProperty(ref _searchText, value))
                {
                    return;
                }

                if (string.IsNullOrEmpty(value))
                {
                    _isSearchActive = false;

                    if (_preSearchCategory != null)
                    {
                        _selectedCategory = _preSearchCategory;
                        Items = _itemsByCategory[_selectedCategory];
                        SelectedItem = Items.FirstOrDefault();
                        _preSearchCategory = null;
                    }
                }
                else
                {
                    if (!_isSearchActive)
                    {
                        _preSearchCategory = SelectedCategory;
                        _isSearchActive = true;
                    }

                    ApplyFilter();
                }
            }
        }

        public List<Item> SelectedItems { get; set; } = new List<Item>();

        private Item _selectedItem;

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                if (_selectedItem == null) return;
                QuantityEnabled = _selectedItem.StackSize > 1;
                MaxQuantity = _selectedItem.MaxStorage > 0
                    ? _selectedItem.MaxStorage + _selectedItem.StackSize
                    : _selectedItem.StackSize;
                SelectedQuantity = _selectedItem.StackSize;
                CanInfuse = _selectedItem.Infusable;
                if (!CanInfuse) SelectedInfusionType = "Normal";
                CanUpgrade = _selectedItem.UpgradeType > 0;
                if (!CanUpgrade) SelectedUpgrade = 0;
                else MaxUpgradeLevel = _selectedItem.UpgradeType == 1 ? 10 : 5;
                if (SelectedUpgrade > MaxUpgradeLevel) SelectedUpgrade = MaxUpgradeLevel;
            }
        }

        private int _selectedQuantity = 1;

        public int SelectedQuantity
        {
            get => _selectedQuantity;
            set
            {
                int clampedValue = Math.Max(1, Math.Min(value, MaxQuantity));
                SetProperty(ref _selectedQuantity, clampedValue);
            }
        }

        private int _selectedUpgrade;

        public int SelectedUpgrade
        {
            get => _selectedUpgrade;
            set => SetProperty(ref _selectedUpgrade, Math.Max(0, Math.Min(value, MaxUpgradeLevel)));
        }

        private string _selectedInfusionType = "Normal";

        public string SelectedInfusionType
        {
            get => _selectedInfusionType;
            set => SetProperty(ref _selectedInfusionType, value);
        }

        private string _selectedMassSpawnCategory;

        public string SelectedMassSpawnCategory
        {
            get => _selectedMassSpawnCategory;
            set => SetProperty(ref _selectedMassSpawnCategory, value);
        }

        private bool _autoSpawnEnabled;

        public bool AutoSpawnEnabled
        {
            get => _autoSpawnEnabled;
            set => SetProperty(ref _autoSpawnEnabled, value);
        }

        private Item _selectedAutoSpawnWeapon;

        public Item SelectedAutoSpawnWeapon
        {
            get => _selectedAutoSpawnWeapon;
            set => SetProperty(ref _selectedAutoSpawnWeapon, value);
        }

        public ObservableCollection<Item> WeaponList => new ObservableCollection<Item>(_itemsByCategory["Weapons"]);

        #endregion
        

        #region Private Methods

        private void LoadData()
        {
            Categories.Add("Ammo");
            Categories.Add("Armor");
            Categories.Add("Consumables");
            Categories.Add("Key Items");
            Categories.Add("Rings");
            Categories.Add("Spells");
            Categories.Add("Upgrade Materials");
            Categories.Add("Weapons");

            _itemsByCategory.Add("Ammo", new ObservableCollection<Item>(DataLoader.GetItemList("Ammo")));
            _itemsByCategory.Add("Armor", new ObservableCollection<Item>(DataLoader.GetItemList("Armor")));
            _itemsByCategory.Add("Consumables", new ObservableCollection<Item>(DataLoader.GetItemList("Consumables")));
            _itemsByCategory.Add("Key Items", new ObservableCollection<Item>(DataLoader.GetItemList("KeyItems")));
            _itemsByCategory.Add("Rings", new ObservableCollection<Item>(DataLoader.GetItemList("Rings")));
            _itemsByCategory.Add("Spells", new ObservableCollection<Item>(DataLoader.GetItemList("Spells")));
            _itemsByCategory.Add("Upgrade Materials",
                new ObservableCollection<Item>(DataLoader.GetItemList("UpgradeMaterials")));
            _itemsByCategory.Add("Weapons", new ObservableCollection<Item>(DataLoader.GetItemList("Weapons")));

            _allItems = _itemsByCategory.Values.SelectMany(x => x).ToLookup(i => i.Name);


            _loadoutTemplatesByName = LoadoutTemplates.All.ToDictionary(lt => lt.Name);

            LoadCustomLoadouts();

            _loadouts = new ObservableCollection<string>(_loadoutTemplatesByName.Keys);

            SelectedLoadoutName = Loadouts.FirstOrDefault();
            SelectedCategory = Categories.FirstOrDefault();
            SelectedMassSpawnCategory = Categories.FirstOrDefault();
            SelectedAutoSpawnWeapon = WeaponList.FirstOrDefault();
        }

        private void ApplyFilter()
        {
            _searchResultsCollection.Clear();
            var searchTextLower = SearchText.ToLower();

            foreach (var category in _itemsByCategory)
            {
                foreach (var item in category.Value)
                {
                    if (item.Name.ToLower().Contains(searchTextLower))
                    {
                        item.CategoryName = category.Key;
                        _searchResultsCollection.Add(item);
                    }
                }
            }

            Items = _searchResultsCollection;
        }

        private void SpawnItem()
        {
            if (SelectedItems.Count > 1)
            {
                foreach (var item in SelectedItems)
                    _itemService.SpawnItem(item.Id, item.StackSize, item.StackSize > 1,
                        item.MaxStorage > 0 ? item.MaxStorage + item.StackSize : item.StackSize);
                return;
            }

            if (SelectedItem == null) return;

            int itemId = SelectedItem.Id;
            if (CanInfuse) itemId += InfusionTypes[SelectedInfusionType];
            if (CanUpgrade) itemId += SelectedUpgrade;
            _itemService.SpawnItem(itemId, SelectedQuantity, SelectedItem.StackSize > 1, MaxQuantity);
        }

        private void SpawnLoadout()
        {
            if (string.IsNullOrEmpty(SelectedLoadoutName) || !_loadoutTemplatesByName.ContainsKey(SelectedLoadoutName))
                return;

            var selectedTemplate = _loadoutTemplatesByName[SelectedLoadoutName];

            foreach (var template in selectedTemplate.Items)
            {
                foreach (var item in _allItems[template.ItemName])
                {
                    int itemId = item.Id;
                    itemId += InfusionTypes[template.Infusion];
                    itemId += template.Upgrade;

                    int quantity = template.Quantity > 0 ? template.Quantity : item.StackSize;
                    _itemService.SpawnItem(itemId, quantity, item.StackSize > 1,
                        item.MaxStorage > 0 ? item.MaxStorage + item.StackSize : item.StackSize);
                }
            }
        }

        private void MassSpawn()
        {
            foreach (var item in _itemsByCategory[SelectedMassSpawnCategory])
            {
                _itemService.SpawnItem(item.Id, item.StackSize, item.StackSize > 1,
                    item.MaxStorage > 0 ? item.MaxStorage + item.StackSize : item.StackSize);
            }
        }
        
        private void ShowCreateLoadoutWindow()
        {
            var createLoadoutWindow = new CreateLoadoutWindow(_categories, _itemsByCategory, _loadoutTemplatesByName,
                _customLoadoutTemplates, InfusionTypes);


            if (createLoadoutWindow.ShowDialog() == true)
            {
                RefreshLoadouts();
            }
        }

        private void RefreshLoadouts()
        {
            _loadoutTemplatesByName = LoadoutTemplates.All.ToDictionary(lt => lt.Name);

            foreach (var loadout in _customLoadoutTemplates.Values)
            {
                if (!string.IsNullOrEmpty(loadout.Name))
                {
                    _loadoutTemplatesByName[loadout.Name] = loadout;
                }
            }

            _loadouts.Clear();
            foreach (var entry in _loadoutTemplatesByName)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    _loadouts.Add(entry.Key);
                }
            }

            if (string.IsNullOrEmpty(SelectedLoadoutName) || !_loadoutTemplatesByName.ContainsKey(SelectedLoadoutName))
            {
                SelectedLoadoutName = _loadouts.FirstOrDefault();
            }

            SaveCustomLoadouts();
        }

        private void SaveCustomLoadouts() => DataLoader.SaveCustomLoadouts(_customLoadoutTemplates);

        private void LoadCustomLoadouts()
        {
            _customLoadoutTemplates = DataLoader.LoadCustomLoadouts();
            foreach (var loadout in _customLoadoutTemplates.Values)
            {
                _loadoutTemplatesByName[loadout.Name] = loadout;
            }
        }

        private void OnGameLoaded()
        {
            AreOptionsEnabled = true;
        }

        private void OnGameNotLoaded()
        {
            AreOptionsEnabled = false;
        }

        private void OnNewGameStart()
        {
            if (!AutoSpawnEnabled || SelectedAutoSpawnWeapon == null) return;
            _itemService.SpawnItem(SelectedAutoSpawnWeapon.Id, 1, false, 1);
        }

        #endregion
    }
}
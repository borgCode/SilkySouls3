using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SilkySouls3.Models;
using SilkySouls3.Services;
using SilkySouls3.Utilities;
using SilkySouls3.Views;

namespace SilkySouls3.ViewModels
{
    public class ItemViewModel : BaseViewModel
    {
        private readonly ItemService _itemService;
        private string _selectedCategory;
        private Item _selectedItem;
        private int _selectedQuantity = 1;
        private int _selectedUpgrade;
        private string _selectedInfusionType = "Normal";
        private int _maxUpgradeLevel = 10;
        private bool _quantityEnabled;
        private int _maxQuantity;
        private bool _canUpgrade;
        private bool _canInfuse;

        private bool _areOptionsEnabled;
        private string _searchText = string.Empty;

        private readonly Dictionary<string, ObservableCollection<Item>> _itemsByCategory =
            new Dictionary<string, ObservableCollection<Item>>();

        private ILookup<string, Item> _allItems;

        private ObservableCollection<string> _categories = new ObservableCollection<string>();
        private ObservableCollection<Item> _items = new ObservableCollection<Item>();

        private string _preSearchCategory;
        private bool _isSearchActive;
        private readonly ObservableCollection<Item> _searchResultsCollection = new ObservableCollection<Item>();

        private ObservableCollection<string> _loadouts;
        private string _selectedLoadoutName;
        private Dictionary<string, LoadoutTemplate> _loadoutTemplatesByName = new Dictionary<string, LoadoutTemplate>();

        private string _selectedMassSpawnCategory;

        private bool _autoSpawnEnabled;
        private Item _selectedAutoSpawnWeapon;

        public Dictionary<string, int> InfusionTypes { get; } = new Dictionary<string, int>
        {
            { "Normal", 0 }, { "Heavy", 100 }, { "Sharp", 200 },
            { "Refined", 300 }, { "Simple", 400 }, { "Crystal", 500 },
            { "Fire", 600 }, { "Chaos", 700 }, { "Lightning", 800 },
            { "Deep", 900 }, { "Dark", 1000 }, { "Poison", 1100 },
            { "Blood", 1200 }, { "Raw", 1300 }, { "Blessed", 1400 },
            { "Hollow", 1500 }
        };


        public ItemViewModel(ItemService itemService)
        {
            _itemService = itemService;
            LoadData();
        }

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

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            private set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<Item> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public ObservableCollection<string> Loadouts
        {
            get => _loadouts;
            private set => SetProperty(ref _loadouts, value);
        }

        public string SelectedLoadoutName
        {
            get => _selectedLoadoutName;
            set => SetProperty(ref _selectedLoadoutName, value);
        }

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

        public bool CanUpgrade
        {
            get => _canUpgrade;
            private set => SetProperty(ref _canUpgrade, value);
        }

        public bool CanInfuse
        {
            get => _canInfuse;
            private set => SetProperty(ref _canInfuse, value);
        }

        public int MaxUpgradeLevel
        {
            get => _maxUpgradeLevel;
            private set => SetProperty(ref _maxUpgradeLevel, value);
        }

        public bool QuantityEnabled
        {
            get => _quantityEnabled;
            private set => SetProperty(ref _quantityEnabled, value);
        }

        public int MaxQuantity
        {
            get => _maxQuantity;
            private set => SetProperty(ref _maxQuantity, value);
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

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                if (_selectedItem == null) return;
                QuantityEnabled = _selectedItem.StackSize > 1;
                MaxQuantity = _selectedItem.StackSize;
                SelectedQuantity = _selectedItem.StackSize;
                CanInfuse = _selectedItem.Infusable;
                if (!CanInfuse) SelectedInfusionType = "Normal";
                CanUpgrade = _selectedItem.UpgradeType > 0;
                if (!CanUpgrade) SelectedUpgrade = 0;
                else MaxUpgradeLevel = _selectedItem.UpgradeType == 1 ? 10 : 5;
                if (SelectedUpgrade > MaxUpgradeLevel) SelectedUpgrade = MaxUpgradeLevel;
            }
        }

        public int SelectedQuantity
        {
            get => _selectedQuantity;
            set
            {
                int clampedValue = Math.Max(1, Math.Min(value, MaxQuantity));
                SetProperty(ref _selectedQuantity, clampedValue);
            }
        }

        public int SelectedUpgrade
        {
            get => _selectedUpgrade;
            set => SetProperty(ref _selectedUpgrade, Math.Max(0, Math.Min(value, MaxUpgradeLevel)));
        }

        public string SelectedInfusionType
        {
            get => _selectedInfusionType;
            set => SetProperty(ref _selectedInfusionType, value);
        }

        public void SpawnItem()
        {
            if (_selectedItem == null) return;

            int itemId = _selectedItem.Id;
            if (CanInfuse) itemId += InfusionTypes[SelectedInfusionType];
            if (CanUpgrade) itemId += SelectedUpgrade;
            _itemService.SpawnItem(itemId, SelectedQuantity);
        }

        public void SpawnLoadout()
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
                    _itemService.SpawnItem(itemId, item.StackSize);
                }
            }
        }


        public string SelectedMassSpawnCategory
        {
            get => _selectedMassSpawnCategory;
            set => SetProperty(ref _selectedMassSpawnCategory, value);
        }

        public bool AutoSpawnEnabled
        {
            get => _autoSpawnEnabled;
            set => SetProperty(ref _autoSpawnEnabled, value);
        }

        public Item SelectedAutoSpawnWeapon
        {
            get => _selectedAutoSpawnWeapon;
            set => SetProperty(ref _selectedAutoSpawnWeapon, value);
        }

        public ObservableCollection<Item> WeaponList => new ObservableCollection<Item>(_itemsByCategory["Weapons"]);

        private readonly string[] _sensitiveCategories = { "Ammo", "Consumables", "Upgrade Materials" };

        public void MassSpawn()
        {
            bool isSensitiveCategory = _sensitiveCategories.Contains(SelectedMassSpawnCategory);

            if (isSensitiveCategory && !ConfirmSpawn()) return;
            foreach (var item in _itemsByCategory[SelectedMassSpawnCategory])
            {
                _itemService.SpawnItem(item.Id, item.StackSize);
            }
        }

        private bool ConfirmSpawn()
        {
            var result = MessageBox.Show(
                $"Are you sure you want to spawn all items in the {SelectedMassSpawnCategory} category?\n\n" +
                "WARNING: If you've already mass spawned this category, spawning it again may crash the game.",
                "Confirm Mass Spawn",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }

        public void TryEnableFeatures()
        {
            AreOptionsEnabled = true;
        }

        public void DisableFeatures()
        {
            AreOptionsEnabled = false;
        }

        public void TrySpawnWeaponPref()
        {
            if (!AutoSpawnEnabled || SelectedAutoSpawnWeapon == null) return;
            _itemService.SpawnItem(SelectedAutoSpawnWeapon.Id, SelectedAutoSpawnWeapon.StackSize);
        }

        public void ShowCreateLoadoutWindow()
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
        private Dictionary<string, LoadoutTemplate> _customLoadoutTemplates = new Dictionary<string, LoadoutTemplate>();

      

        private void SaveCustomLoadouts() => DataLoader.SaveCustomLoadouts(_customLoadoutTemplates);

        private void LoadCustomLoadouts()
        {
            _customLoadoutTemplates = DataLoader.LoadCustomLoadouts();
            foreach (var loadout in _customLoadoutTemplates.Values)
            {
                _loadoutTemplatesByName[loadout.Name] = loadout;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using SilkySouls3.Models;
using SilkySouls3.Services;
using SilkySouls3.Utilities;

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

        private ObservableCollection<string> _categories = new ObservableCollection<string>();
        private ObservableCollection<Item> _items = new ObservableCollection<Item>();

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

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value) && value != null)
                {
                    if (_selectedCategory != null)
                    {
                        Items = _itemsByCategory[_selectedCategory];
                        SelectedItem = Items.FirstOrDefault();

                        if (!string.IsNullOrEmpty(SearchText))
                        {
                            ApplyFilter();
                        }
                    }
                }
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

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        private void ApplyFilter()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(Items);

            if (string.IsNullOrEmpty(SearchText))
            {
                view.Filter = null;
            }
            else
            {
                view.Filter = item => ((Item)item).Name.ToLower().Contains(SearchText.ToLower());
            }
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                if (_selectedItem != null)
                {
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

        public void MassSpawn()
        {
            foreach (var item in _itemsByCategory[SelectedCategory])
            {
                _itemService.SpawnItem(item.Id, item.StackSize);
            }
        }

        public void TryEnableFeatures()
        {
            AreOptionsEnabled = true;
        }
    }
}
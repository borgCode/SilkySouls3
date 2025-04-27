using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkySouls3.Models;

namespace SilkySouls3.Views
{
    public sealed partial class CreateLoadoutWindow : INotifyPropertyChanged
    {
        private readonly ObservableCollection<string> _categories;
        private readonly Dictionary<string, ObservableCollection<Item>> _itemsByCategory;
        private readonly Dictionary<string, int> _infusionTypes;

        private ObservableCollection<LoadoutTemplate> _customLoadouts = new ObservableCollection<LoadoutTemplate>();
        private LoadoutTemplate _selectedLoadout;
        private readonly ObservableCollection<ItemTemplate> _currentLoadoutItems = new ObservableCollection<ItemTemplate>();

        private string _selectedCategory;
        private ObservableCollection<Item> _items = new ObservableCollection<Item>();
        private Item _selectedItem;
        private string _selectedInfusionType = "Normal";
        private int _selectedUpgrade;
        private int _maxUpgradeLevel = 10;
        private bool _canUpgrade;
        private bool _canInfuse;
        private int _selectedQuantity = 1;
        private bool _quantityEnabled;
        private int _maxQuantity = 99;

        private string _searchText = string.Empty;
        private bool _isSearchActive;
        private string _preSearchCategory;
        private ObservableCollection<Item> _searchResultsCollection = new ObservableCollection<Item>();

        private readonly Dictionary<string, LoadoutTemplate> _loadoutTemplatesByName;
        private readonly Dictionary<string, LoadoutTemplate> _customLoadoutTemplates;

        public CreateLoadoutWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public CreateLoadoutWindow(
            ObservableCollection<string> categories,
            Dictionary<string, ObservableCollection<Item>> itemsByCategory,
            Dictionary<string, LoadoutTemplate> loadoutTemplatesByName,
            Dictionary<string, LoadoutTemplate> customLoadoutTemplates,
            Dictionary<string, int> infusionTypes)
        {
            InitializeComponent();

            _categories = categories;
            _itemsByCategory = itemsByCategory;
            _loadoutTemplatesByName = loadoutTemplatesByName;
            _customLoadoutTemplates = customLoadoutTemplates;
            _infusionTypes = infusionTypes;

            _customLoadouts = new ObservableCollection<LoadoutTemplate>(customLoadoutTemplates.Values);
            
            SelectedCategory = categories[0];
     

            if (_customLoadouts.Count > 0)
            {
                SelectedLoadout = _customLoadouts[0];
            }

            DataContext = this;
        }

        public ObservableCollection<LoadoutTemplate> CustomLoadouts
        {
            get => _customLoadouts;
            set
            {
                _customLoadouts = value;
                OnPropertyChanged(nameof(CustomLoadouts));
            }
        }

        public LoadoutTemplate SelectedLoadout
        {
            get => _selectedLoadout;
            set
            {
                _selectedLoadout = value;

                _currentLoadoutItems.Clear();
                if (_selectedLoadout?.Items != null)
                {
                    foreach (var item in _selectedLoadout.Items)
                    {
                        _currentLoadoutItems.Add(item);
                    }
                }

                OnPropertyChanged(nameof(SelectedLoadout));
                OnPropertyChanged(nameof(CurrentLoadoutItems));
            }
        }

        public ObservableCollection<ItemTemplate> CurrentLoadoutItems => _currentLoadoutItems;

        public ObservableCollection<string> Categories => _categories;

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;

                if (_selectedCategory != null)
                {
                    if (_isSearchActive)
                    {
                        IsSearchActive = false;
                        _searchText = string.Empty;
                        OnPropertyChanged(nameof(SearchText));
                        OnPropertyChanged(nameof(IsSearchActive));
                        _preSearchCategory = null;
                    }

                    Items = _itemsByCategory[_selectedCategory];
                    if (Items.Count > 0)
                    {
                        SelectedItem = Items[0];
                    }
                }

                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        public ObservableCollection<Item> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;

                if (_selectedItem != null)
                {
                    CanInfuse = _selectedItem.Infusable;
                    if (!CanInfuse) SelectedInfusionType = "Normal";

                    CanUpgrade = _selectedItem.UpgradeType > 0;
                    if (!CanUpgrade) SelectedUpgrade = 0;
                    else MaxUpgradeLevel = _selectedItem.UpgradeType == 1 ? 10 : 5;

                    if (SelectedUpgrade > MaxUpgradeLevel)
                    {
                        SelectedUpgrade = MaxUpgradeLevel;
                    }
                    
                    QuantityEnabled = _selectedItem.StackSize > 1;
                    MaxQuantity = _selectedItem.StackSize;
                    SelectedQuantity = _selectedItem.StackSize;
                }

                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public string SelectedInfusionType
        {
            get => _selectedInfusionType;
            set
            {
                _selectedInfusionType = value;
                OnPropertyChanged(nameof(SelectedInfusionType));
            }
        }

        public int SelectedUpgrade
        {
            get => _selectedUpgrade;
            set
            {
                _selectedUpgrade = Math.Max(0, Math.Min(value, MaxUpgradeLevel));
                OnPropertyChanged(nameof(SelectedUpgrade));
            }
        }

        public bool CanInfuse
        {
            get => _canInfuse;
            set
            {
                _canInfuse = value;
                OnPropertyChanged(nameof(CanInfuse));
            }
        }
        
        public int SelectedQuantity
        {
            get => _selectedQuantity;
            set
            {
                _selectedQuantity = Math.Max(1, Math.Min(value, MaxQuantity));
                OnPropertyChanged(nameof(SelectedQuantity));
            }
        }

        public bool QuantityEnabled
        {
            get => _quantityEnabled;
            set
            {
                _quantityEnabled = value;
                OnPropertyChanged(nameof(QuantityEnabled));
            }
        }

        public int MaxQuantity
        {
            get => _maxQuantity;
            set
            {
                _maxQuantity = value;
                OnPropertyChanged(nameof(MaxQuantity));
            }
        }

        public bool CanUpgrade
        {
            get => _canUpgrade;
            set
            {
                _canUpgrade = value;
                OnPropertyChanged(nameof(CanUpgrade));
            }
        }

        public int MaxUpgradeLevel
        {
            get => _maxUpgradeLevel;
            set
            {
                _maxUpgradeLevel = value;
                OnPropertyChanged(nameof(MaxUpgradeLevel));
            }
        }

        public bool IsSearchActive
        {
            get => _isSearchActive;
            set
            {
                _isSearchActive = value;
                OnPropertyChanged(nameof(IsSearchActive));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;

                if (string.IsNullOrEmpty(value))
                {
                    _isSearchActive = false;

                    if (_preSearchCategory != null)
                    {
                        SelectedCategory = _preSearchCategory;
                        _preSearchCategory = null;
                        
                        foreach (var item in Items)
                        {
                            item.CategoryName = null;
                        }
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
                
                OnPropertyChanged(nameof(SearchText));
                OnPropertyChanged(nameof(IsSearchActive));
            }
        }

        public Dictionary<string, int> InfusionTypes => _infusionTypes;


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

        private void CreateLoadout_Click(object sender, RoutedEventArgs e)
        {
            string newName = ShowInputDialog("Enter name for new loadout:");

            if (string.IsNullOrWhiteSpace(newName))
                return;
            
            if (_loadoutTemplatesByName.ContainsKey(newName))
            {
                MessageBox.Show("A loadout with this name already exists.", "Duplicate Name",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var newLoadout = new LoadoutTemplate
            {
                Name = newName,
                Items = new List<ItemTemplate>()
            };
            
            _customLoadoutTemplates[newName] = newLoadout;
            _customLoadouts.Add(newLoadout);
            
            SelectedLoadout = newLoadout;
        }

        private string ShowInputDialog(string prompt, string defaultValue = "")
        {
            Window inputDialog = new Window
            {
                Title = "Input",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
    
           
            StackPanel panel = new StackPanel { Margin = new Thickness(10) };
            panel.Children.Add(new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 10) });
    
            TextBox textBox = new TextBox { Text = defaultValue, Margin = new Thickness(0, 0, 0, 10) };
            panel.Children.Add(textBox);
    
            Button okButton = new Button { Content = "OK", Width = 60, IsDefault = true };
            okButton.Click += (s, e) => { inputDialog.DialogResult = true; };
            panel.Children.Add(okButton);
    
            inputDialog.Content = panel;
            
            return inputDialog.ShowDialog() == true ? textBox.Text : string.Empty;
        }

        private void RenameLoadout_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLoadout == null)
            {
                MessageBox.Show("Please select a loadout to rename.", "No Loadout Selected",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            string newName = ShowInputDialog("Enter new name for loadout:", SelectedLoadout.Name);

            if (string.IsNullOrWhiteSpace(newName))
                return;
            
            if (newName == SelectedLoadout.Name)
                return;
            
            if (_loadoutTemplatesByName.ContainsKey(newName))
            {
                MessageBox.Show("A loadout with this name already exists.", "Duplicate Name",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            _customLoadoutTemplates.Remove(SelectedLoadout.Name);
            
            var currentItems = SelectedLoadout.Items;

            var renamedLoadout = new LoadoutTemplate
            {
                Name = newName,
                Items = currentItems
            };
            
            int index = _customLoadouts.IndexOf(SelectedLoadout);
            _customLoadouts.RemoveAt(index);
            _customLoadouts.Insert(index, renamedLoadout);
            _customLoadoutTemplates[newName] = renamedLoadout;
            
            SelectedLoadout = renamedLoadout;
        }

        private void DeleteLoadout_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLoadout == null)
            {
                MessageBox.Show("Please select a loadout to delete.", "No Loadout Selected",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var result = MessageBox.Show($"Are you sure you want to delete the loadout '{SelectedLoadout.Name}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
        
                _customLoadoutTemplates.Remove(SelectedLoadout.Name);
                _customLoadouts.Remove(SelectedLoadout);
                
                _currentLoadoutItems.Clear();
                
                SelectedLoadout = _customLoadouts.FirstOrDefault();
            }
        }

        private void AddItemToLoadout_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLoadout == null)
            {
                MessageBox.Show("Please select or create a loadout first.", "No Loadout Selected",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedItem == null)
            {
                MessageBox.Show("Please select an item to add.", "No Item Selected",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var itemTemplate = new ItemTemplate
            {
                ItemName = SelectedItem.Name,
                Infusion = SelectedInfusionType,
                Upgrade = SelectedUpgrade,
                Quantity = SelectedQuantity
            };
            
            SelectedLoadout.Items.Add(itemTemplate);
            _currentLoadoutItems.Add(itemTemplate);

            OnPropertyChanged(nameof(CurrentLoadoutItems));
        }

        private void RemoveItemFromLoadout_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLoadout == null || LoadoutItemsList.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to remove.", "No Item Selected",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedItemTemplate = (ItemTemplate)LoadoutItemsList.SelectedItem;
            
            SelectedLoadout.Items.Remove(selectedItemTemplate);
            _currentLoadoutItems.Remove(selectedItemTemplate);

            OnPropertyChanged(nameof(CurrentLoadoutItems));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var loadout in _customLoadouts)
            {
                if (!string.IsNullOrEmpty(loadout.Name))  // Add this check
                {
                    _customLoadoutTemplates[loadout.Name] = loadout;
                }
            }
            
            DialogResult = true;
            Close();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
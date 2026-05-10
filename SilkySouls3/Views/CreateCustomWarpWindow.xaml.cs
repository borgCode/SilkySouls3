using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SilkySouls3.Interfaces;
using SilkySouls3.Models;
using SilkySouls3.Utilities;

namespace SilkySouls3.Views
{
    public partial class CreateCustomWarpWindow : Window, INotifyPropertyChanged
    {
        private readonly Dictionary<string, List<CustomWarp>> _warpDict;
        private readonly IPlayerService _playerService;
        private readonly ITravelService _travelService;
        private readonly Action<CustomWarpChange> _onChanged;

        private readonly ObservableCollection<string> _categories = new();
        private readonly ObservableCollection<CustomWarp> _warps = new();
        private string _selectedCategory;
        private readonly bool _canCapture;

        public CreateCustomWarpWindow(
            Dictionary<string, List<CustomWarp>> warpDict,
            bool canCapture,
            IPlayerService playerService,
            ITravelService travelService,
            Action<CustomWarpChange> onChanged)
        {
            InitializeComponent();

            _warpDict = warpDict;
            _playerService = playerService;
            _travelService = travelService;
            _onChanged = onChanged;
            _canCapture = canCapture;

            foreach (var key in _warpDict.Keys)
                _categories.Add(key);

            DataContext = this;
            SelectedCategory = _categories.FirstOrDefault();
        }

        public ObservableCollection<string> Categories => _categories;
        public ObservableCollection<CustomWarp> Warps => _warps;
        public bool CanCapture => _canCapture;

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory == value) return;
                _selectedCategory = value;
                RefreshWarps();
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        private void RefreshWarps()
        {
            _warps.Clear();
            if (_selectedCategory != null && _warpDict.TryGetValue(_selectedCategory, out var list))
            {
                foreach (var w in list) _warps.Add(w);
            }
        }

        private void AddWarp_Click(object sender, RoutedEventArgs e)
        {
            if (!_canCapture)
            {
                MsgBox.Show("Game must be loaded to capture a position.", "Custom Warp");
                return;
            }

            Position currentPos;
            try
            {
                currentPos = _playerService.GetCurrentPosition();
            }
            catch (Exception ex)
            {
                MsgBox.Show($"Failed to read player position: {ex.Message}", "Custom Warp");
                return;
            }

            if (!_travelService.TryResolveBonfire(currentPos.BlockId, currentPos.CeremonyId,
                    out var bonfireId, out var dlcRequirement))
            {
                MsgBox.Show($"No bonfire mapped for current block (BlockId: {currentPos.BlockId}). Cannot save warp.",
                    "Custom Warp");
                return;
            }

            var values = MsgBox.ShowInputs(new[]
            {
                new InputField("category", "Category:", _selectedCategory ?? string.Empty),
                new InputField("name", "Warp Name:")
            }, "New Custom Warp");

            if (values == null) return;
            string category = values["category"]?.Trim();
            string name = values["name"]?.Trim();

            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(name))
            {
                MsgBox.Show("Category and name are required.", "Custom Warp");
                return;
            }

            var warp = new CustomWarp
            {
                MainArea = category,
                Name = name,
                Position = currentPos,
                BonfireId = bonfireId,
                DlcRequirement = dlcRequirement
            };

            if (!_warpDict.ContainsKey(category))
            {
                _warpDict[category] = new List<CustomWarp>();
                _categories.Add(category);
            }
            _warpDict[category].Add(warp);

            _onChanged?.Invoke(new WarpAdded(warp));

            SelectedCategory = category;
            RefreshWarps();
        }

        private void WarpsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            if (_selectedCategory == null) return;

            var selected = WarpsList.SelectedItems.Cast<CustomWarp>().ToList();
            if (selected.Count == 0) return;

            foreach (var warp in selected)
            {
                if (_warpDict.TryGetValue(_selectedCategory, out var list))
                {
                    list.Remove(warp);
                    if (list.Count == 0) _warpDict.Remove(_selectedCategory);
                }
                _onChanged?.Invoke(new WarpDeleted(_selectedCategory, warp));
            }

            if (!_warpDict.ContainsKey(_selectedCategory))
            {
                _categories.Remove(_selectedCategory);
                SelectedCategory = _categories.FirstOrDefault();
            }
            else
            {
                RefreshWarps();
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCategory == null) return;

            if (!MsgBox.ShowYesNo(
                    $"Delete the entire '{_selectedCategory}' category and all its warps?",
                    "Confirm Delete"))
                return;

            string category = _selectedCategory;
            _warpDict.Remove(category);
            _categories.Remove(category);
            _onChanged?.Invoke(new CategoryDeleted(category));

            SelectedCategory = _categories.FirstOrDefault();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

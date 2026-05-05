using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkySouls3.Models;
using SilkySouls3.ViewModels;

namespace SilkySouls3.Views
{
    public partial class ItemTab
    {
        private readonly ItemViewModel _itemViewModel;
        private string _lastValidText;
        public ItemTab(ItemViewModel itemViewModel)
        {
            InitializeComponent();
            _itemViewModel = itemViewModel;
            DataContext = _itemViewModel;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ItemViewModel vm && sender is ListView lv)
                vm.SelectedItems = lv.SelectedItems.Cast<Item>().ToList();
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ItemViewModel vm && vm.SpawnItemCommand.CanExecute(null))
            {
                vm.SpawnItemCommand.Execute(null);
            }
        }

        private void AutoSpawn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ComboBox combo)) return;
            _lastValidText = combo.Text;
            
            combo.PreviewMouseDown -= AutoSpawn_PreviewMouseDown;
            combo.DropDownClosed += AutoSpawn_DropDownClosed;
                
            combo.Dispatcher.BeginInvoke(new Action(() =>
            {
                combo.IsEditable = true;
                combo.Focus();
                combo.IsDropDownOpen = true;
            }), System.Windows.Threading.DispatcherPriority.Input);
        }
        
        private void AutoSpawn_DropDownClosed(object sender, EventArgs e)
        {
            if (!(sender is ComboBox combo)) return;
            
            if (string.IsNullOrWhiteSpace(combo.Text))
            {
                combo.Text = _lastValidText;
            }
            combo.IsEditable = false;
            combo.DropDownClosed -= AutoSpawn_DropDownClosed;
            combo.PreviewMouseDown += AutoSpawn_PreviewMouseDown;
        }

    }
}
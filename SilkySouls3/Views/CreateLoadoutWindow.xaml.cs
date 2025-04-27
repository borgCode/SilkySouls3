using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using SilkySouls3.Models;

namespace SilkySouls3.Views
{
    public partial class CreateLoadoutWindow : Window
    {
        public CreateLoadoutWindow()
        {
            InitializeComponent();
        }

        public CreateLoadoutWindow(ObservableCollection<string> categories, Dictionary<string,ObservableCollection<Item>> itemsByCategory, Dictionary<string, LoadoutTemplate> loadoutTemplatesByName, Dictionary<string, LoadoutTemplate> customLoadoutTemplates, Dictionary<string, int> infusionTypes)
        {
            throw new System.NotImplementedException();
        }
    }
}
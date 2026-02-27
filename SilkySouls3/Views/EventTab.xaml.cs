using System.Windows.Controls;
using SilkySouls3.Utilities;
using SilkySouls3.ViewModels;

namespace SilkySouls3.Views
{
    public partial class EventTab : UserControl
    {
        public EventTab(EventViewModel eventViewModel)
        {
            InitializeComponent();
            DataContext = eventViewModel;

            if (!GameLauncher.IsDlc2Available)
            {
                UnlockMidirButton.IsEnabled = false;
                UnlockMidirButton.ToolTip = "Requires DLC2";
            }
        }
    }
}

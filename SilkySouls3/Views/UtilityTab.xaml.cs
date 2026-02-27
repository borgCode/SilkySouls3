using SilkySouls3.ViewModels;

namespace SilkySouls3.Views
{
    public partial class UtilityTab
    {
        public UtilityTab(UtilityViewModel utilityViewModel)
        {
            InitializeComponent();
            DataContext = utilityViewModel;
        }
    }
}

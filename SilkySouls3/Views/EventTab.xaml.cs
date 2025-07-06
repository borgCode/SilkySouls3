using System.Windows;
using System.Windows.Controls;
using SilkySouls3.Utilities;
using SilkySouls3.ViewModels;

namespace SilkySouls3.Views
{
    public partial class EventTab : UserControl
    {
        private readonly EventViewModel _eventViewModel;
        public EventTab(EventViewModel eventViewModel)
        {
            InitializeComponent();
            _eventViewModel = eventViewModel;
            DataContext = eventViewModel;
            
            if (!GameLauncher.IsDlc2Available)
            {
                UnlockMidirButton.IsEnabled = false;
                UnlockMidirButton.ToolTip = "Requires DLC2";
            }
        }

        private void SetFlag_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.SetFlag();
        }

        private void GetEvent_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.GetEvent();
        }
        
        private void UnlockMidir_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.UnlockMidir();
        }
        
        private void MovePatches_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.MovePatchesToFirelink();
        }

        private void MoveGreirat_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.MoveGreiratToFirelink();
        }

        private void MoveKarla_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.MoveKarlaToFirelink();
        }

        private void MoveCornyx_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.MoveCornyxToFirelink();
        }

        private void MoveIrina_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.MoveIrinaToFirelink();
        }

        private void MoveOrbeck_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.MoveOrbeckToFirelink();
        }
    }
}
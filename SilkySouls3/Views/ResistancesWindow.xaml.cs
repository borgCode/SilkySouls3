using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using SilkySouls3.Utilities;
using SilkySouls3.ViewModels;

namespace SilkySouls3.Views
{
    public partial class ResistancesWindow
    {
        public ResistancesWindow()
        {
            InitializeComponent();

            MouseLeftButtonDown += (s, e) => DragMove();
            Background = new SolidColorBrush(
                Color.FromArgb(128, 0, 0, 0));


            Loaded += (s, e) =>
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                User32.SetTopmost(hwnd);

                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Closing += (sender, args) => { Close(); };
                }
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EnemyViewModel viewModel)
            {
                viewModel.IsResistancesWindowOpen = false;
            }

            Close();
        }


        private void DecreaseSize_Click(object sender, RoutedEventArgs e)
        {
            ContentScale.ScaleX -= 0.2;
            ContentScale.ScaleY -= 0.2;
            Width *= ContentScale.ScaleX / (ContentScale.ScaleX + 0.2);
            Height *= ContentScale.ScaleY / (ContentScale.ScaleY + 0.2);
        }

        private void IncreaseSize_Click(object sender, RoutedEventArgs e)
        {
            ContentScale.ScaleX += 0.2;
            ContentScale.ScaleY += 0.2;
            Width *= ContentScale.ScaleX / (ContentScale.ScaleX - 0.2);
            Height *= ContentScale.ScaleY / (ContentScale.ScaleY - 0.2);
        }
    }
}
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
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
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(128, 0, 0, 0));


            Loaded += (s, e) =>
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                User32.SetTopmost(hwnd);
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
    }
}
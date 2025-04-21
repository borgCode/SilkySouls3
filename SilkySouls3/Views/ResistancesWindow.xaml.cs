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
                
                if (SettingsManager.Default.ResistancesWindowLeft > 0)
                    Left = SettingsManager.Default.ResistancesWindowLeft;
                
                if (SettingsManager.Default.ResistancesWindowTop > 0)
                    Top = SettingsManager.Default.ResistancesWindowTop;
                
                if (SettingsManager.Default.ResistancesWindowScaleX > 0)
                {
                    ContentScale.ScaleX = SettingsManager.Default.ResistancesWindowScaleX;
                    ContentScale.ScaleY = SettingsManager.Default.ResistancesWindowScaleY;
                }
                
                if (SettingsManager.Default.ResistancesWindowWidth > 0)
                    Width = SettingsManager.Default.ResistancesWindowWidth;
    
                if (SettingsManager.Default.ResistancesWindowHeight > 0)
                    Height = SettingsManager.Default.ResistancesWindowHeight;
                
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
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            
            SettingsManager.Default.ResistancesWindowScaleX = ContentScale.ScaleX;
            SettingsManager.Default.ResistancesWindowScaleY = ContentScale.ScaleY;
            SettingsManager.Default.ResistancesWindowWidth = Width;
            SettingsManager.Default.ResistancesWindowHeight = Height;
            SettingsManager.Default.ResistancesWindowLeft = Left;
            SettingsManager.Default.ResistancesWindowTop = Top;
            SettingsManager.Default.Save();
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
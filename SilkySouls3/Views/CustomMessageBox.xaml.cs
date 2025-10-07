using System;
using System.Windows;
using System.Windows.Interop;
using SilkySouls3.Utilities;

namespace SilkySouls3.Views
{
    public partial class CustomMessageBox : Window
    {
        
        public MessageBoxResult Result { get; private set; }
        private readonly bool _topMost;
        
        
        public CustomMessageBox(string message, string title, MessageBoxButton button, bool topMost)
        {
            InitializeComponent();
            
            Title = title;
            MessageText.Text = message;
            
            SetButtons(button);
            
            _topMost = topMost;
            
            SetButtons(button);
            
            if (_topMost)
            {
                SourceInitialized += OnSourceInitialized;
            }

            if (Application.Current.MainWindow != null)
            {
                Owner = Application.Current.MainWindow;
                Application.Current.MainWindow.Closing += (sender, args) => { Close(); };
            }
        }
        
        private void OnSourceInitialized(object sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            User32.SetTopmost(hwnd);
        }
        
        private void SetButtons(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OK:
                    CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    CancelButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    OkButton.Content = "Yes";
                    CancelButton.Content = "No";
                    CancelButton.Visibility = Visibility.Visible;
                    break;
            }
        }
        
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Result = OkButton.Content.ToString() == "Yes" ? MessageBoxResult.Yes : MessageBoxResult.OK;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = CancelButton.Content.ToString() == "No" ? MessageBoxResult.No : MessageBoxResult.Cancel;
            DialogResult = false;
        }
        
        public static MessageBoxResult Show(string message, string title = "", 
            MessageBoxButton button = MessageBoxButton.OK, bool topMost = false)
        {
            var msgBox = new CustomMessageBox(message, title, button, topMost);
            msgBox.ShowDialog();
            return msgBox.Result;
        }
    }
}
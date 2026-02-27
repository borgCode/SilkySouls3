//

using System;
using System.Windows;
using System.Windows.Interop;
using SilkySouls3.Utilities;

namespace SilkySouls3.Views.Windows;

public class TopmostWindow : Window
{
    protected void AlwaysOnTopCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        User32.SetTopmost(hwnd);
    }

    protected void AlwaysOnTopCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        User32.RemoveTopmost(hwnd);
    }
}

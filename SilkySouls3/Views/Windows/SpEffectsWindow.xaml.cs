//

using System.Windows;
using SilkySouls3.Utilities;

namespace SilkySouls3.Views.Windows;

public partial class SpEffectsWindow : TopmostWindow
{
    public SpEffectsWindow()
    {
        InitializeComponent();

        if (Application.Current.MainWindow != null)
        {
            Application.Current.MainWindow.Closing += (sender, args) => { Close(); };
        }

        Loaded += (s, e) =>
        {
            if (SettingsManager.Default.SpEffectWindowLeft > 0)
                Left = SettingsManager.Default.SpEffectWindowLeft;

            if (SettingsManager.Default.SpEffectWindowTop > 0)
                Top = SettingsManager.Default.SpEffectWindowTop;

            AlwaysOnTopCheckBox.IsChecked = SettingsManager.Default.SpEffectAlwaysOnTop;
        };
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);

        SettingsManager.Default.SpEffectWindowLeft = Left;
        SettingsManager.Default.SpEffectWindowTop = Top;
        SettingsManager.Default.SpEffectAlwaysOnTop = AlwaysOnTopCheckBox.IsChecked ?? false;
        SettingsManager.Default.Save();
    }
}

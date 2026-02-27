//

using System;
using System.Collections.Generic;
using System.Windows;
using SilkySouls3.Views;

namespace SilkySouls3.Utilities;

/// <summary>
/// Static helper class to show custom message boxes from anywhere in the application.
/// </summary>
public static class MsgBox
{
    private static T OnUiThread<T>(Func<T> func)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess()) return func();
        return dispatcher.Invoke(func);
    }

    private static void OnUiThread(Action action) => OnUiThread<object?>(() => { action(); return null; });

    /// <summary>
    /// Shows a message box with only an OK button.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title"></param>
    public static void Show(string message, string title = "Message") => OnUiThread(() =>
    {
        var box = new CustomMessageBox(message, showCancel: false, title);
        box.ShowDialog();
    });

    /// <summary>
    /// Shows a message box with OK and Cancel buttons.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title"></param>
    /// <returns>True if OK was clicked, false if Cancel was clicked.</returns>
    public static bool ShowOkCancel(string message, string title = "Message") => OnUiThread(() =>
    {
        var box = new CustomMessageBox(message, showCancel: true, title);
        box.ShowDialog();
        return box.Result;
    });

    /// <summary>
    /// Shows a single input dialog.
    /// </summary>
    public static string ShowInput(string prompt, string defaultValue = "", string title = "Input") => OnUiThread(() =>
    {
        var box = new InputBox(prompt, defaultValue, title);
        box.ShowDialog();
        return box.Result ? box.InputValue : string.Empty;
    });

    /// <summary>
    /// Shows a multi-input dialog.
    /// </summary>
    /// <returns>Dictionary of key-value pairs, or null if cancelled.</returns>
    public static Dictionary<string, string>? ShowInputs(InputField[] fields, string title = "Input") => OnUiThread(() =>
    {
        var box = new InputBox(fields, title);
        box.ShowDialog();
        return box.Result ? box.GetValues() : null;
    });

    /// <summary>
    /// Shows a message box with Yes and No buttons.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">The title of the message box.</param>
    /// <returns>True if Yes was clicked, false if No was clicked.</returns>
    public static bool ShowYesNo(string message, string title = "Message") => OnUiThread(() =>
    {
        var box = new CustomMessageBox(message, title, showYesNo: true);
        box.ShowDialog();
        return box.Result;
    });
}

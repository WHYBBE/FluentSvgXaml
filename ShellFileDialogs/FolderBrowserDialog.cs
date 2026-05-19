using System;
using System.Windows;
using System.Windows.Interop;

namespace ShellFileDialogs;

public static class FolderBrowserDialog
{
    public static string? ShowDialog(IntPtr windowHandle, string description, string initialPath)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = description,
            InitialDirectory = initialPath
        };

        var owner = HwndSource.FromHwnd(windowHandle)?.RootVisual as Window;
        bool? result = dialog.ShowDialog(owner);

        if (result == true && dialog.FolderName != null)
        {
            return dialog.FolderName;
        }

        return null;
    }
}

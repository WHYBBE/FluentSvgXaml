// The codes by Jake Ginnivan and licensed under MIT.
// Web Link: http://jake.ginnivan.net/remembering-wpf-window-positions
//

using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FluentSvgXaml.Core;

/// <summary>
/// Persists a Window's Size, Location and WindowState to UserScopeSettings 
/// </summary>
public sealed partial class MainWindowSettings(Window window)
{
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

    // ReSharper disable InconsistentNaming
    private const int SW_SHOWNORMAL = 1;
    private const int SW_SHOWMINIMIZED = 2;
    // ReSharper restore InconsistentNaming

    private Window? _window = window;

    private WindowApplicationSettings? _windowApplicationSettings;

    /// <summary>
    /// Register the "Save" attached property and the "OnSaveInvalidated" callback 
    /// </summary>
    public static readonly DependencyProperty SaveProperty
        = DependencyProperty.RegisterAttached("Save", typeof(bool), typeof(MainWindowSettings),
                                              new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSaveInvalidated)));

    public static void SetSave(DependencyObject dependencyObject, bool enabled)
    {
        dependencyObject.SetValue(SaveProperty, enabled);
    }

    /// <summary>
    /// Called when Save is changed on an object.
    /// </summary>
    private static void OnSaveInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not Window window || !((bool)e.NewValue))
            return;
        var settings = new MainWindowSettings(window);
        settings.Attach();
    }

    /// <summary>
    /// Load the Window Size Location and State from the settings object
    /// </summary>
    private void LoadWindowState()
    {
        Settings.Reload();

        if (Settings.Placement == null)
            return;
        try
        {
            // Load window placement details for previous application session from application settings
            // if window was closed on a monitor that is now disconnected from the computer,
            // SetWindowPlacement will place the window onto a visible monitor.
            var wp = Settings.Placement.Value;

            wp.length = Marshal.SizeOf<WINDOWPLACEMENT>();
            wp.flags = 0;
            wp.showCmd = (wp.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : wp.showCmd);
            var hwnd = new WindowInteropHelper(_window).Handle;
            SetWindowPlacement(hwnd, ref wp);
        }
        catch (Exception ex)
        {
            Trace.Write(ex);
        }
    }

    /// <summary>
    /// Save the Window Size, Location and State to the settings object
    /// </summary>
    private void SaveWindowState()
    {
        var hwnd = new WindowInteropHelper(_window).Handle;
        GetWindowPlacement(hwnd, out WINDOWPLACEMENT wp);
        Settings.Placement = wp;
        Settings.Save();
    }

    private void Attach()
    {
        if (_window == null)
            return;
        _window.Closing += WindowClosing;
        _window.SourceInitialized += WindowSourceInitialized;
    }

    private void WindowSourceInitialized(object? sender, EventArgs e)
    {
        LoadWindowState();
    }

    private void WindowClosing(object? sender, CancelEventArgs e)
    {
        SaveWindowState();
        _window!.Closing -= WindowClosing!;
        _window!.SourceInitialized -= WindowSourceInitialized!;
        _window = null;
    }

    internal WindowApplicationSettings CreateWindowApplicationSettingsInstance()
    {
        return new WindowApplicationSettings(this);
    }

    [Browsable(false)]
    internal WindowApplicationSettings Settings
    {
        get
        {
            _windowApplicationSettings ??= CreateWindowApplicationSettingsInstance();
            return _windowApplicationSettings;
        }
    }

    internal class WindowApplicationSettings(MainWindowSettings windowSettings) : ApplicationSettingsBase(windowSettings._window!.GetType().FullName)
    {
        [UserScopedSetting]
        public WINDOWPLACEMENT? Placement
        {
            get
            {
                if (this[nameof(Placement)] != null)
                {
                    return ((WINDOWPLACEMENT)this[nameof(Placement)]);
                }
                return null;
            }
            set
            {
                this[nameof(Placement)] = value;
            }
        }
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct RECT(int left, int top, int right, int bottom)
{
    private int _left = left;
    private int _top = top;
    private int _right = right;
    private int _bottom = bottom;

    public override readonly bool Equals(object? obj)
    {
        if (obj is RECT rect)
        {
            return rect._bottom == _bottom &&
                   rect._left == _left &&
                   rect._right == _right &&
                   rect._top == _top;
        }
        return base.Equals(obj);
    }

    public override readonly int GetHashCode()
    {
        return _bottom.GetHashCode() ^
               _left.GetHashCode() ^
               _right.GetHashCode() ^
               _top.GetHashCode();
    }

    public static bool operator ==(RECT a, RECT b)
    {
        return a._bottom == b._bottom &&
               a._left == b._left &&
               a._right == b._right &&
               a._top == b._top;
    }

    public static bool operator !=(RECT a, RECT b)
    {
        return !(a == b);
    }

    public int Left
    {
        readonly get => _left;
        set => _left = value;
    }

    public int Top
    {
        readonly get => _top;
        set => _top = value;
    }

    public int Right
    {
        readonly get => _right;
        set => _right = value;
    }

    public int Bottom
    {
        readonly get => _bottom;
        set => _bottom = value;
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct POINT(int x, int y)
{
    private int _x = x;
    private int _y = y;

    public int X
    {
        readonly get => _x;
        set => _x = value;
    }

    public int Y
    {
        readonly get => _y;
        set => _y = value;
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is POINT point)
        {
            return point._x == _x && point._y == _y;
        }
        return base.Equals(obj);
    }
    public override readonly int GetHashCode()
    {
        return _x.GetHashCode() ^ _y.GetHashCode();
    }

    public static bool operator ==(POINT a, POINT b)
    {
        return a._x == b._x && a._y == b._y;
    }

    public static bool operator !=(POINT a, POINT b)
    {
        return !(a == b);
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct WINDOWPLACEMENT
{
    public int length;
    public int flags;
    public int showCmd;
    public POINT minPosition;
    public POINT maxPosition;
    public RECT normalPosition;
}

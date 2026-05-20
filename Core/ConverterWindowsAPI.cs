using System.Runtime.InteropServices;

namespace FluentSvgXaml.Core;

public static partial class ConverterWindowsAPI
{
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AllocConsole();

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool FreeConsole();

    [LibraryImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AttachConsole(int dwProcessId);

#pragma warning disable CA1401 // P/Invokes 应该是不可见的
    [LibraryImport("kernel32.dll")]
    public static partial IntPtr GetConsoleWindow();

    /// <summary>
    /// The GetForegroundWindow function returns a handle to the foreground window.
    /// </summary>
    [LibraryImport("user32.dll")]
    public static partial IntPtr GetForegroundWindow();

    [LibraryImport("kernel32.dll")]
    public static partial uint GetCurrentThreadId();

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AttachThreadInput(uint idAttach, uint idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool fAttach);

    [LibraryImport("kernel32.dll")]
    public static partial void ExitProcess(uint uExitCode);
#pragma warning restore CA1401 // P/Invokes 应该是不可见的
}

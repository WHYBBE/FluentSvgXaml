using FluentSvgXaml.Cli;
using FluentSvgXaml.Core;

namespace FluentSvgXaml;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            var commandLines = new ConverterCommandLines(args);
            bool parseOk = commandLines.Parse(true);

            if (commandLines.ShowHelp)
            {
                ConverterWindowsAPI.AttachConsole(-1);
                Console.WriteLine(commandLines.Usage);
                ConverterWindowsAPI.FreeConsole();
                Environment.Exit(0);
                return;
            }

            if (commandLines.ShowGuiHelp)
            {
                var app = new App();
                app.InitializeComponent();
                app.CommandLines = commandLines;
                var window = new MainWindow
                {
                    StartTabIndex = 5
                };
                app.Run(window);
                return;
            }

            var ui = commandLines.Ui;
            var isQuiet = (ui == ConverterUIOption.None);

            if (parseOk && (ui == ConverterUIOption.Console || ui == ConverterUIOption.None))
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var consoleApp = new ConsoleApplication(process)
                {
                    CommandLines = commandLines
                };
                consoleApp.InitializeComponent(true, isQuiet);
                var exitCode = consoleApp.Run();
                Environment.Exit(exitCode);
                return;
            }
        }

        var guiApp = new App();
        guiApp.InitializeComponent();
        guiApp.CommandLines = (args.Length > 0) ? new ConverterCommandLines(args) : null;
        guiApp.CommandLines?.Parse(false);
        guiApp.Run(new MainWindow());
    }
}

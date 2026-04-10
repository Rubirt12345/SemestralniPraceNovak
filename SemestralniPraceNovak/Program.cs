using Avalonia;
using SemestralniPraceNovak.Database;
using System;

namespace SemestralniPraceNovak
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            
            DatabaseInitializer.InitializeAsync().GetAwaiter().GetResult();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
#if DEBUG
                .WithDeveloperTools()
#endif
                .WithInterFont()
                .LogToTrace();
    }
}
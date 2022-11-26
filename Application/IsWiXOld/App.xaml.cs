using System;
using System.Windows;
using System.Windows.Media.Imaging;
using FireworksFramework.Managers;

namespace IsWiX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            var fireworksManager = FireworksManager.FireworksManagerInstance;
            fireworksManager.BrandingBitMap = new BitmapImage(new Uri(@"/IsWiX.bmp", UriKind.Relative));

            if (e.Args.Length > 0)
            {
                fireworksManager.FilePath = e.Args[0];
            }

            fireworksManager.ProductName = "IsWiX";
            fireworksManager.Start();
        }

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            MessageBox.Show($"An unhandled error occurred. {e.Message}", "IsWiX Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

    }
}

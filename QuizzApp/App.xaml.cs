using System;
using System.Threading;
using System.Windows;

namespace QuizzApp
{
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var splashScreen = new SplashScreen("./././Images/SplashScreen.png");
            splashScreen.Show(true);

            Thread.Sleep(TimeSpan.FromMilliseconds(2000.0));
            var mainWindow = new UserInfo();
            mainWindow.Show();
        }
    }
}
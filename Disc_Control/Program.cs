using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Disc_Control
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var config = new Config();
            var driveconfig = new DrivesConfig();
            int interval = config.Interval;

            var serverTask = WebServer.StartServerAsync();
            WebServer.RestoreConsoleOutput();
            while (true)
            {
              //  Console.Clear();
                ReloadDrives();
                await Task.Delay(interval * 1000);

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.X)
                    {
                        break;
                    }
                }
            }
        }

        static void ReloadDrives()
        {
            string skeleton = Drive.ToConsole();
            Console.WriteLine(skeleton);
            Console.WriteLine("Press X to quit.");
        }

        
    }
}

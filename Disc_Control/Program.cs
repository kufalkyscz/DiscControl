using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Disc_Control
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var serverTask = WebServer.StartServerAsync();
            WebServer.RestoreConsoleOutput();
            int interval = 4;
            while (true)
            {
                Console.Clear();
                ReloadDrives();
                await Task.Delay(interval * 250);

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
            string skeleton = Drives();
            Console.WriteLine(skeleton);
            Console.WriteLine("Press X to quit.");
        }

        public static string Drives()
        {
            string information = "Drive         FreeSpace (GB)       %FreeSpace       UsedSpace (GB)       TotalSpace (GB)     FileSystem     DriveType          Name       Serial Number\n";
            information += "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -\r\n";
            try
            {
                var drives = Drive.GetDrives();
                foreach (var drive in drives.Values)
                {
                    string driveInfo;
                    if (drive.FreeSpacePercentage != 0)
                    {
                        driveInfo = $"{drive.Name,-10} {drive.FreeSpace,15:F2}  {drive.FreeSpacePercentage,15:F2}% {drive.UsedSpace,20:F2} {drive.TotalSpace,15:F2}  {drive.FileSystem,15}  {drive.DriveType,15}  {drive.VolumeLabel,15}  {drive.SerialNumber,15}";

                        if (drive.FreeSpacePercentage <= 10)
                        {
                            Notification.Show(drive.Name, drive.FreeSpacePercentage);
                        }
                    }
                    else
                    {
                        driveInfo = $"{drive.Name,-10} {"N/A",15}  {"N/A",15} {"N/A",20} {"N/A",15}  {"N/A",15}  {drive.DriveType,15}  {"Not Found",15}  {drive.SerialNumber,15}";
                    }
                    information += driveInfo + "\n";
                }
            }
            catch (Exception ex)
            {
                information += $"An error occurred: {ex.Message}\n";
            }
            return information;
        }
    }
}

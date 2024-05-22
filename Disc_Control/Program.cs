using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.IO;
using System.Management;
using System.Threading.Tasks;

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
                DriveInfo[] drives = DriveInfo.GetDrives();
                if (drives.Length == 0)
                {
                    information += "No drives found.\n";
                }
                foreach (DriveInfo drive in drives)
                {
                    string driveInfo;
                    string serialNumber = GetDriveSerialNumber(drive.Name);
                    if (drive.IsReady)
                    {
                        double freeSpace = drive.AvailableFreeSpace / (1024 * 1024 * 1024.0);
                        double totalSpace = drive.TotalSize / (1024 * 1024 * 1024.0);
                        double usedSpace = totalSpace - freeSpace;
                        double fsPercentage = (freeSpace / totalSpace) * 100;
                        string fileSystem = drive.DriveFormat;
                        string driveType = drive.DriveType.ToString();
                        string driveName = string.IsNullOrEmpty(drive.VolumeLabel) ? "Unnamed" : drive.VolumeLabel;

                        driveInfo = $"{drive.Name,-10} {freeSpace,15:F2}  {fsPercentage,15:F2}% {usedSpace,20:F2} {totalSpace,15:F2}  {fileSystem,15}  {driveType,15}  {driveName,15}  {serialNumber,15}";

                        
                        if (fsPercentage <= 10)
                        {
                            Notification.Show(drive.Name, fsPercentage);
                        }
                    }
                    else
                    {
                        driveInfo = $"{drive.Name,-10} {"N/A",15}  {"N/A",15} {"N/A",20} {"N/A",15}  {"N/A",15}  {drive.DriveType.ToString(),15}  {"Not Found",15}  {serialNumber,15}";
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

        static string GetDriveSerialNumber(string driveName)
        {
            try
            {
                if (!driveName.EndsWith("\\"))
                {
                    driveName += "\\";
                }

                ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID = '{driveName.Substring(0, 2)}'");
                foreach (ManagementObject vol in searcher.Get())
                {
                    return vol["VolumeSerialNumber"]?.ToString() ?? "Unknown";
                }
            }
            catch (Exception)
            {
                return "Error";
            }
            return "Unknown";
        }

    }
}

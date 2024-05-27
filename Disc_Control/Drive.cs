using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Xml.Schema;

namespace Disc_Control
{
    internal class Drive
    {
        public string Name { get; set; }
        public double FreeSpace { get; set; }
        public double TotalSpace { get; set; }
        public double UsedSpace { get; set; }
        public double FreeSpacePercentage { get; set; }
        public string FileSystem { get; set; }
        public string DriveType { get; set; }
        public string VolumeLabel { get; set; }
        public string SerialNumber { get; set; }

        public static Dictionary<string, Drive> GetDrives()
        {
            var config = new Config();
            bool ShowUnreadyDrives = config.ShowUnreadyDrives;

            var drivesDict = new Dictionary<string, Drive>();
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (var driveInfo in drives)
            {
                if (driveInfo.IsReady)
                {
                    string serialNumber = GetDriveSerialNumber(driveInfo.Name);
                    double freeSpace = driveInfo.AvailableFreeSpace / (1024 * 1024 * 1024.0);
                    double totalSpace = driveInfo.TotalSize / (1024 * 1024 * 1024.0);
                    double usedSpace = totalSpace - freeSpace;
                    double fsPercentage = (freeSpace / totalSpace) * 100;
                    string fileSystem = driveInfo.DriveFormat;
                    string driveType = driveInfo.DriveType.ToString();
                    string volumeLabel = string.IsNullOrEmpty(driveInfo.VolumeLabel) ? "Unnamed" : driveInfo.VolumeLabel;

                    var drive = new Drive
                    {
                        Name = driveInfo.Name,
                        FreeSpace = freeSpace,
                        TotalSpace = totalSpace,
                        UsedSpace = usedSpace,
                        FreeSpacePercentage = fsPercentage,
                        FileSystem = fileSystem,
                        DriveType = driveType,
                        VolumeLabel = volumeLabel,
                        SerialNumber = serialNumber
                    };

                    string uniqueKey = driveInfo.Name;
                    drivesDict[uniqueKey] = drive;

                    if (fsPercentage <= 10)
                    {
                        Notification.Show(drive.Name, fsPercentage);
                    }
                }
                else if (ShowUnreadyDrives == true)
                {
                    string serialNumber = GetDriveSerialNumber(driveInfo.Name);
                    var drive = new Drive
                    {
                        Name = driveInfo.Name,
                        FreeSpace = 0,
                        TotalSpace = 0,
                        UsedSpace = 0,
                        FreeSpacePercentage = 0,
                        FileSystem = "N/A",
                        DriveType = driveInfo.DriveType.ToString(),
                        VolumeLabel = "Not Found",
                        SerialNumber = serialNumber
                    };

                    string uniqueKey = driveInfo.Name; 
                    drivesDict[uniqueKey] = drive;
                }
            }
            return drivesDict;
        }

        private static string GetDriveSerialNumber(string driveName)
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

        public string ToHTML()
        {
            var config = new Config();
            int critical_threshold = config.CriticalThreshold;
            int warning_threshold = config.WarningThreshold;

            string color = FreeSpacePercentage <= critical_threshold ? "red" :
                           FreeSpacePercentage <= warning_threshold ? "orange" : "lightgreen";

            return $@"
            <div style=""background-color:{color}; margin-bottom: 10px; padding: 10px; border: 1px solid black;"">
                <strong>Drive Name:</strong> {Name}<br>
                <strong>Free Space (GB):</strong> {FreeSpace:F2}<br>
                <strong>Free Space Percentage:</strong> {FreeSpacePercentage:F2}%<br>
                <strong>Used Space (GB):</strong> {UsedSpace:F2}<br>
                <strong>Total Space (GB):</strong> {TotalSpace:F2}<br>
                <strong>File System:</strong> {FileSystem}<br>
                <strong>Drive Type:</strong> {DriveType}<br>
                <strong>Volume Label:</strong> {VolumeLabel}<br>
                <strong>Serial Number:</strong> {SerialNumber}<br>
            </div>";
        }

        public static string ToConsole()
        {
            var config = new Config();
            var critical_threshold = config.CriticalThreshold;

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

                        if (drive.FreeSpacePercentage <= critical_threshold)
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
        
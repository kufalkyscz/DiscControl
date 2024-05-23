using System;
using System.Collections.Generic;
using System.IO;
using System.Management;

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
            var drivesDict = new Dictionary<string, Drive>();
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (var driveInfo in drives)
            {
                string serialNumber = GetDriveSerialNumber(driveInfo.Name);
                if (driveInfo.IsReady)
                {
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

                    drivesDict[serialNumber] = drive;

                    if (fsPercentage <= 10)
                    {
                        Notification.Show(drive.Name, fsPercentage);
                    }
                }
                else
                {
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

                    drivesDict[serialNumber] = drive;
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
            int critical_threshold = config.critical_threshold;
            int warning_threshold = config.warning_threshold;

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
    }
}
        
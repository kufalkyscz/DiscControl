using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Disc_Control
{
    internal class DrivesConfig
    {

        public DrivesConfig()
        {
            CheckConfig();
        }
        private void CheckConfig()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.FullName;
            string configPath = Path.Combine(projectDirectory, "drivesconfig.json");

            Dictionary<string, int> drivesConfig = new Dictionary<string, int>();
            if (File.Exists(configPath))
            {
                string configJson = File.ReadAllText(configPath);
                if (!string.IsNullOrEmpty(configJson))
                {
                    drivesConfig = JsonSerializer.Deserialize<Dictionary<string, int>>(configJson);
                }
            }
            if (!File.Exists(configPath))
            {
                File.Create(configPath).Close();
            }

            string configJsonPath = Path.Combine(projectDirectory, "config.json");
            int basicValue = 0;
            if (File.Exists(configJsonPath))
            {
                string configJson = File.ReadAllText(configJsonPath);
                var configObject = JsonSerializer.Deserialize<Dictionary<string, int>>(configJson);
                if (configObject.ContainsKey("CriticalThreshold"))
                {
                    basicValue = configObject["CriticalThreshold"];
                }
            }

            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                string driveIdentifier = $"{drive.Name.TrimEnd('\\')}"; 
                if (!drivesConfig.ContainsKey(driveIdentifier))
                {
                    drivesConfig.Add(driveIdentifier, basicValue);
                }
            }

            string json = JsonSerializer.Serialize(drivesConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);

            Console.WriteLine("Configuration updated successfully.");
        }
    }
}

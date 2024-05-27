using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Disc_Control
{
    internal class DrivesConfig
    {
        private int BasicValue { get; set; }

        public DrivesConfig()
        {
            LoadBasicValue();
            CheckConfig();
        }

        private void LoadBasicValue()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.FullName;
            string configPath = Path.Combine(projectDirectory, "config.json");

            if (File.Exists(configPath))
            {
                string configJson = File.ReadAllText(configPath);
                var configObject = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson);
                if (configObject != null && configObject.ContainsKey("CriticalThreshold"))
                {
                    if (configObject["CriticalThreshold"] is JsonElement criticalThresholdElement && criticalThresholdElement.ValueKind == JsonValueKind.Number)
                    {
                        BasicValue = criticalThresholdElement.GetInt32();
                        Console.WriteLine($"BasicValue assigned: {BasicValue}");
                    }
                    else
                    {
                        Console.WriteLine("CriticalThreshold value is not a valid integer.");
                    }
                }
                else
                {
                    Console.WriteLine("CriticalThreshold not found in config.json");
                }
            }
            else
            {
                Console.WriteLine($"Config file not found at: {configPath}");
            }
        }

        private void CheckConfig()
        {
            try
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
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(configPath));

                    try
                    {
                        using (var fs = File.Create(configPath))
                        {
                            fs.Close();
                        }
                        Console.WriteLine($"Created new config file at: {configPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating config file: {ex.Message}");
                        return; 
                    }
                }

                DriveInfo[] drives = DriveInfo.GetDrives();

                foreach (DriveInfo drive in drives)
                {
                    string driveIdentifier = $"{drive.Name}";
                    if (!drivesConfig.ContainsKey(driveIdentifier))
                    {
                        drivesConfig.Add(driveIdentifier, BasicValue);
                    }
                }

                string json = JsonSerializer.Serialize(drivesConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);

                Console.WriteLine($"Drives configuration saved to: {configPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking config: {ex.Message}");
            }
        }
    }
}

using System;
using System.IO;
using System.Text.Json;

namespace Disc_Control
{
    internal class Config
    {
        public int Interval { get; private set; }
        public int CriticalThreshold { get; private set; }
        public int WarningThreshold { get; private set; }
        public int Port { get; private set; }
        public bool GlobalDriveConfig { get; private set; }
        public bool ShowNetworkDrives { get; private set; }

        public Config()
        {
            CheckConfig();
            LoadConfig();
        }

        private void CheckConfig()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.FullName;
            string configPath = Path.Combine(projectDirectory, "config.json");

            if (!File.Exists(configPath))
            {
                var defaultConfig = new ConfigData
                {
                    Interval = 1,
                    CriticalThreshold = 10,
                    WarningThreshold = 25,
                    Port = 8000,
                    GlobalDriveConfig = true,
                    ShowNetworkDrives = true,
                };

                string jsonString = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, jsonString);

                Console.WriteLine("config.json not found. Created with default values.");
            }
        }

        internal void LoadConfig()
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.FullName;
                string configPath = Path.Combine(projectDirectory, "config.json");

                string jsonString = File.ReadAllText(configPath);
                var configData = JsonSerializer.Deserialize<ConfigData>(jsonString);

                if (configData != null)
                {
                    Interval = configData.Interval;
                    CriticalThreshold = configData.CriticalThreshold;
                    WarningThreshold = configData.WarningThreshold;
                    Port = configData.Port;
                    GlobalDriveConfig = configData.GlobalDriveConfig;
                    ShowNetworkDrives = configData.ShowNetworkDrives;
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("config.json not found.");
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Error parsing config.json: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private class ConfigData
        {
            public int Interval { get; set; }
            public int CriticalThreshold { get; set; }
            public int WarningThreshold { get; set; }
            public int Port { get; set; }
            public bool GlobalDriveConfig { get; set; }
            public bool ShowNetworkDrives { get; set; }
        }
    }
}

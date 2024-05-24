using System;
using System.IO;
using System.Text.Json;

namespace Disc_Control
{
    internal class Config
    {
        public int interval { get; private set; }
        public int critical_threshold { get; private set; }
        public int warning_threshold { get; private set; }

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
                    interval = 1,
                    critical_threshold = 10,
                    warning_threshold = 25
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
                    interval = configData.interval;
                    critical_threshold = configData.critical_threshold;
                    warning_threshold = configData.warning_threshold;
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
            public int interval { get; set; }
            public int critical_threshold { get; set; }
            public int warning_threshold { get; set; }
        }
    }
}

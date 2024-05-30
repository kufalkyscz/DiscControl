using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Disc_Control
{
    internal static class Notification
    {
        private static readonly Dictionary<string, double> LastNotifiedPercentages = new Dictionary<string, double>();
        private static Dictionary<string, int> DriveCriticalThresholds = new Dictionary<string, int>();
        private const string EventSourceName = "DiscControlApp";
        private const string EventLogName = "Application";

        static Notification()
        {
            LoadConfigurations();
            try
            {
                if (!EventLog.SourceExists(EventSourceName))
                {
                    EventLog.CreateEventSource(EventSourceName, EventLogName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing event source: {ex.Message}");
            }
        }

        public static void Show(string driveName, double fsPercentage)
        {
            if (ShouldNotify(driveName, fsPercentage))
            {
                EventLogEntryType? logType = DetermineLogType(driveName, fsPercentage);

                if (logType.HasValue)
                {
                    string message = $"Drive '{driveName}' has reached {fsPercentage:F2}% of free space.";
                    new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddArgument("conversationId", 9813)
                        .AddText(message)
                        .Show();

                    LogToEventViewer(message, logType.Value);

                    LastNotifiedPercentages[driveName] = fsPercentage;
                }
            }
        }

        private static bool ShouldNotify(string driveName, double fsPercentage)
        {
            if (!LastNotifiedPercentages.ContainsKey(driveName))
            {
                LastNotifiedPercentages[driveName] = fsPercentage;
                return true;
            }

            double lastPercentage = LastNotifiedPercentages[driveName];
            int warningThreshold = GetWarningThreshold(driveName);
            int criticalThreshold = GetCriticalThreshold(driveName); 

            bool shouldNotify = (lastPercentage > warningThreshold && fsPercentage <= warningThreshold) ||
                                (lastPercentage > criticalThreshold && fsPercentage <= criticalThreshold) ||
                                (lastPercentage <= criticalThreshold && fsPercentage > criticalThreshold) ||
                                (lastPercentage <= warningThreshold && fsPercentage > warningThreshold && fsPercentage > criticalThreshold) ||
                                (lastPercentage > warningThreshold && fsPercentage <= criticalThreshold);

            return shouldNotify;
        }

        private static EventLogEntryType? DetermineLogType(string driveName, double fsPercentage)
        {
            int warningThreshold = GetWarningThreshold(driveName);
            int criticalThreshold = GetCriticalThreshold(driveName);

            if (fsPercentage <= criticalThreshold)
            {
                return EventLogEntryType.Warning;
            }
            if (fsPercentage <= warningThreshold)
            {
                return EventLogEntryType.Warning;
            }
            if (fsPercentage > warningThreshold)
            {
                return EventLogEntryType.Information;
            }

            return null;
        }

        static void LogToEventViewer(string message, EventLogEntryType logType)
        {
            try
            {
                EventLog.WriteEntry(EventSourceName, message, logType);
                Console.WriteLine($"Logged to event viewer: {message} as {logType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to event log: {ex.Message}");
            }
        }

        private static void LoadConfigurations()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).FullName;
            string configPath = Path.Combine(projectDirectory, "drivesconfig.json");

            try
            {
                var configJson = File.ReadAllText(configPath);
                DriveCriticalThresholds = JsonSerializer.Deserialize<Dictionary<string, int>>(configJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configurations: {ex.Message}");
            }
        }

        public static int GetCriticalThreshold(string driveName)
        {
            var config = new Config();
            bool globalDriveConfig = config.GlobalDriveConfig;
            int defaultCriticalThreshold = config.CriticalThreshold;

            if (globalDriveConfig  == false && DriveCriticalThresholds.TryGetValue(driveName, out int specificThreshold))
            {
                return specificThreshold;
            }

            return defaultCriticalThreshold;
        }

        public static int GetWarningThreshold(string driveName)
        {
            var config = new Config();
            bool globalDriveConfig = config.GlobalDriveConfig;
            int defaultWarningThreshold = config.WarningThreshold;
            if (globalDriveConfig == false)
            {
                int warningThreshold = GetCriticalThreshold(driveName) + 15;
                return warningThreshold;
            }
            return defaultWarningThreshold;
        }
    }
}

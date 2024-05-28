using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Disc_Control
{
    internal static class Notification
    {
        private static readonly Dictionary<string, double> LastNotifiedPercentages = new Dictionary<string, double>();
        private const string EventSourceName = "DiscControlApp";
        private const string EventLogName = "Application";

        static Notification()
        {
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

        public static void Show(string volumeLabel, double fsPercentage)
        {
            if (ShouldNotify(volumeLabel, fsPercentage))
            {
                EventLogEntryType? logType = DetermineLogType(volumeLabel, fsPercentage);

                if (logType.HasValue)
                {
                    string message = $"Drive '{volumeLabel}' has reached {fsPercentage:F2}% of free space.";
                    new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddArgument("conversationId", 9813)
                        .AddText(message)
                        .Show();

                    LogToEventViewer(message, logType.Value);

                    LastNotifiedPercentages[volumeLabel] = fsPercentage;
                }
            }
        }

        private static bool ShouldNotify(string volumeLabel, double fsPercentage)
        {
            if (!LastNotifiedPercentages.ContainsKey(volumeLabel))
            {
                return true;
            }

            double lastPercentage = LastNotifiedPercentages[volumeLabel];
            var config = new Config();
            int WarningThreshold = config.WarningThreshold;
            int CriticalThreshold = config.CriticalThreshold;

            return (lastPercentage > WarningThreshold && fsPercentage <= WarningThreshold) ||
                   (lastPercentage > CriticalThreshold && fsPercentage <= CriticalThreshold) ||
                   (lastPercentage <= CriticalThreshold && fsPercentage > CriticalThreshold) ||
                   (lastPercentage <= WarningThreshold && fsPercentage > WarningThreshold) ||
                   (lastPercentage <= CriticalThreshold && fsPercentage > WarningThreshold);
        }

        private static EventLogEntryType? DetermineLogType(string volumeLabel, double fsPercentage)
        {
            var config = new Config();
            int WarningThreshold = config.WarningThreshold;
            int CriticalThreshold = config.CriticalThreshold;

            double lastPercentage = LastNotifiedPercentages.ContainsKey(volumeLabel) ? LastNotifiedPercentages[volumeLabel] : double.MaxValue;

            if (fsPercentage <= CriticalThreshold)
            {
                return EventLogEntryType.Warning;
            }
            if (fsPercentage <= WarningThreshold)
            {
                if (lastPercentage <= CriticalThreshold && fsPercentage > CriticalThreshold)
                {
                    return EventLogEntryType.Information; 
                }
                return EventLogEntryType.Warning;
            }
            if (fsPercentage > WarningThreshold)
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
    }    
}

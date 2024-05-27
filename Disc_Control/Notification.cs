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
            if (ShouldNotify(volumeLabel))
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

        private static bool ShouldNotify(string volumeLabel)
        {
            return !LastNotifiedPercentages.ContainsKey(volumeLabel);
        }

        private static EventLogEntryType? DetermineLogType(string volumeLabel, double fsPercentage)
        {
            var config = new Config();
            int WarningThreshold = config.WarningThreshold;
            int CriticalThreshold = config.CriticalThreshold;
            if (!LastNotifiedPercentages.ContainsKey(volumeLabel))
            {
                if (fsPercentage > WarningThreshold)
                {
                    LastNotifiedPercentages[volumeLabel] = fsPercentage;
                    return EventLogEntryType.Information;
                }
                else if (fsPercentage <= WarningThreshold && fsPercentage > CriticalThreshold)
                {
                    LastNotifiedPercentages[volumeLabel] = fsPercentage;
                    return EventLogEntryType.Warning;
                }
            }

            double lastPercentage = LastNotifiedPercentages[volumeLabel];

            EventLogEntryType logType;

            if (lastPercentage > WarningThreshold && fsPercentage <= WarningThreshold && fsPercentage > CriticalThreshold)
            {
                logType = EventLogEntryType.Warning;
            }
            else if (lastPercentage > CriticalThreshold && fsPercentage <= CriticalThreshold)
            {
                logType = EventLogEntryType.Warning;
            }
            else if (lastPercentage <= CriticalThreshold && fsPercentage > CriticalThreshold)
            {
                logType = EventLogEntryType.Information;
            }
            else if (lastPercentage <= WarningThreshold && fsPercentage > WarningThreshold)
            {
                logType = EventLogEntryType.Information;
            }
            else
            {
                logType = EventLogEntryType.Information;
            }

            LastNotifiedPercentages[volumeLabel] = fsPercentage;

            return logType;
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

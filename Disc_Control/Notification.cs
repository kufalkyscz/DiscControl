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
                string message = $"Drive '{volumeLabel}' has reached a critical threshold of {fsPercentage:F2}% free space.";

                new ToastContentBuilder()
                    .AddArgument("action", "viewConversation")
                    .AddArgument("conversationId", 9813)
                    .AddText(message)
                    .Show();

                LogToEventViewer(message);

                LastNotifiedPercentages[volumeLabel] = fsPercentage;
            }
        }

        private static bool ShouldNotify(string volumeLabel, double fsPercentage)
        {
            if (!LastNotifiedPercentages.ContainsKey(volumeLabel))
            {
                return true;
            }

            return false;
        }
        /* private static bool ResetNotify(string driveName, double fsPercentage)
        {
            if (LastNotifiedPercentages.ContainsKey(driveName) && LastNotifiedPercentages[driveName] < fsPercentage)
            {
                LastNotifiedPercentages.Remove(driveName);
                return true;
            }
            return false;
        }*/
        static void LogToEventViewer(string message)
        {
            try
            {
                EventLog.WriteEntry(EventSourceName, message, EventLogEntryType.Warning);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to event log: {ex.Message}");
            }
        }
    }
}

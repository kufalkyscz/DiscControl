using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;

namespace Disc_Control
{
    internal static class Notification
    {
        private static readonly Dictionary<string, double> LastNotifiedPercentages = new Dictionary<string, double>();

        public static void Show(string driveName, double fsPercentage)
        {
            
            if (ShouldNotify(driveName, fsPercentage))
            {
                new ToastContentBuilder()
                    .AddArgument("action", "viewConversation")
                    .AddArgument("conversationId", 9813)
                    .AddText($"Drive '{driveName}' has reached a critical threshold of {fsPercentage:F2}% free space.")
                    .Show();

                LastNotifiedPercentages[driveName] = fsPercentage;
            }
        }

        private static bool ShouldNotify(string driveName, double fsPercentage)
        {
            if (!LastNotifiedPercentages.ContainsKey(driveName) || LastNotifiedPercentages[driveName] != fsPercentage)
            {
                return true;
            }
            return false;
        }
    }
}

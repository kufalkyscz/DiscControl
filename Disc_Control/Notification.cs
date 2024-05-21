using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disc_Control
{
    internal static class Notification
    {
        public static void Show(string driveName, double fsPercentage)
        {
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 9813)
                .AddText($"Drive '{driveName}' has reached a critical threshold of {fsPercentage:F2}% free space.")
                .Show();
        }
    }
}

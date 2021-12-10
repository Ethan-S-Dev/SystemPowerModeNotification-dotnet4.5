using System;
using System.PowerNotifications;

namespace SystemPowerNotificationsTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SystemPowerNotifications.ServiceName = "SimpleTestService";
            SystemPowerNotifications.PowerModeChanged += SystemPowerNotifications_PowerModeChanged;
            Console.ReadLine();
            SystemPowerNotifications.PowerModeChanged -= SystemPowerNotifications_PowerModeChanged;
        }

        private static void SystemPowerNotifications_PowerModeChanged(object sender, PowerNotificationArgs args)
        {
            Console.WriteLine($"Event Fired type: {Enum.GetName(typeof(PowerBroadcastType), args.Mode)}");
        }

    }
}

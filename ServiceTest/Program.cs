using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.PowerNotifications;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceTest
{
    public static class FilePaths
    {
        public const string LogFile = @"C:\Users\EthanShoham\source\cs\SystemPowerNotifications\ServiceTest\bin\x64\Release\serviceLogs.txt";
    }

    public class SimpleTestService : ServiceBase
    {
        private Thread _mainLoop;
        static void Main(string[] args)
        {
            ServiceBase.Run(new SimpleTestService());
        }

        protected override void OnStart(string[] args)
        {
            File.AppendAllText(FilePaths.LogFile, "Starting SimpleTest Service\n");
            _mainLoop = new Thread(RunMessagePump);
            _mainLoop.Start();
        }

        void RunMessagePump()
        {
            File.AppendAllText(FilePaths.LogFile, "Starting SimpleTest Application\n");
            BackgroundApp.Run();
        }

        protected override void OnStop()
        {
            File.AppendAllText(FilePaths.LogFile, "Stopping SimpleTest Application\n");
            BackgroundApp.Exit();
            File.AppendAllText(FilePaths.LogFile, "Stopping SimpleTest Service\n");
        }
    }

    public static class BackgroundApp
    {
        private static SystemPowerNotificationEventHandler _function;
        private static CancellationTokenSource _cancellationTokenSource;
        private static CancellationToken _token;
        public static void Run()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _token = _cancellationTokenSource.Token;
            _function = SystemPowerNotifications_PowerModeChanged;
            try
            {
                SystemPowerNotifications.ServiceName = "SimpleTestService";
                SystemPowerNotifications.PowerModeChanged += _function;
                while (true)
                {
                    File.AppendAllText(FilePaths.LogFile, DateTime.Now.ToString() + "\n");
                    Task.Delay(5000, _token).Wait();
                    if (_token.IsCancellationRequested)
                        return;
                }
            }
            catch (Exception e)
            {
                PrintException(e, 0);
            }
        }

        private static void PrintException(Exception e,int t)
        {
            if (e == null)
                return;
            File.AppendAllText(FilePaths.LogFile, $"{new string('\t',t)}Exception of type: {e.GetType().Name}\n{new string('\t', t)}Message: {e.Message}\n");
            PrintException(e.InnerException, t + 1);
        }

        public static void Exit()
        {
            try
            {
                _cancellationTokenSource.Cancel();
                SystemPowerNotifications.PowerModeChanged -= _function;
            }
            catch (Exception e)
            {
                File.AppendAllText(FilePaths.LogFile, e.Message);
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _function = null;
            }
        }

        private static void SystemPowerNotifications_PowerModeChanged(object sender, PowerNotificationArgs args)
        {
            if(args.Mode == PowerBroadcastType.PBT_POWERSETTINGCHANGE)
                File.AppendAllText(FilePaths.LogFile, $"Event Fired type: {Enum.GetName(typeof(PowerBroadcastType), args.Mode)}\n\tIsMonitorOn: {args.IsMonitorOn}\n");
            else
                File.AppendAllText(FilePaths.LogFile, $"Event Fired type: {Enum.GetName(typeof(PowerBroadcastType), args.Mode)}\n");
        }
    }

    [RunInstaller(true)]
    public class SimpleInstaller : Installer
    {
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        public SimpleInstaller()
        {
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            // Service will run under system account
            processInstaller.Account = ServiceAccount.LocalSystem;

            // Service will have Start Type of Manual
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            serviceInstaller.ServiceName = "SimpleTestService";

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }
    }
}

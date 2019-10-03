using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using TelemetryLib;

namespace locationmodule
{
    sealed class HubConnection
    {
        private static ModuleClient _ioTHubModuleClient;
        private static ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;

        private HubConnection() { }
        private static readonly Lazy<HubConnection> lazy = new Lazy<HubConnection>(() => new HubConnection());

        public static HubConnection Instance
        {
            get { return lazy.Value; }
        }

        public async Task Connect()
        {
            if (_connectionStatus == ConnectionStatus.Connected)
            {
                return;
            }
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            _ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            _ioTHubModuleClient.SetConnectionStatusChangesHandler(ConnectionStatusChangeHandler);
         
            await _ioTHubModuleClient.OpenAsync();
       
            Console.WriteLine("Location module initialized.");
        }

        private void ConnectionStatusChangeHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            _connectionStatus = status;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Status {status} changed: {reason}");
            Console.ResetColor();
        }

        public ModuleClient GetClient()
        {
            return _ioTHubModuleClient;
        }

        public ConnectionStatus Status()
        {
            return _connectionStatus;
        }
    }
}

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using TelemetryLib;

namespace locationmodule
{
    class HubConnection
    {
        private static ModuleClient _ioTHubModuleClient;

        public async Task Connect()
        {
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            _ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            _ioTHubModuleClient.SetConnectionStatusChangesHandler(ConnectionStatusChangeHandler);
            await _ioTHubModuleClient.OpenAsync();
       
            Console.WriteLine("Location module initialized.");
        }

        private void ConnectionStatusChangeHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Status {status} changed: {reason}");
            Console.ResetColor();
        }

        public ModuleClient GetClient()
        {
            return _ioTHubModuleClient;
        }
    }
}

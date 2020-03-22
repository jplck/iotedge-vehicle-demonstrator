using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VehicleDemonstrator.Shared.Connectivity;
using VehicleDemonstrator.Shared.Util;
using VehicleDemonstrator.Shared.Telemetry;
using VehicleDemonstrator.Shared.Telemetry.Odometry;
using System.Net.Http;
using System.Globalization;
using AzureMapsToolkit.Common;
using AzureMapsToolkit.Search;
using Microsoft.Azure.Devices.Shared;
using LocationModule;

namespace VehicleDemonstrator.Module.Location
{
    class DriveSimulationHost : SimulationHost
    {
        private const string OdometerInputName = "odometerInput";
        private const string OutputName = "locationModuleOutput";
        private HttpClient _httpClient = new HttpClient();
        private Maps _maps;
        private string _locStart = "";
        private string _locEnd = "";

        public async Task SetupAsync()
        {
            await SetupConnectionAsync();

            var mapsKey = Environment.GetEnvironmentVariable("AZURE_MAPS_KEY");
            _maps = new Maps(mapsKey);

            _httpClient.BaseAddress = new Uri("https://atlas.microsoft.com");

            await AddInputHandlerAsync(OdometerInputName, OdometerMessageReceivedAsync);
        }

        public override async Task RunAsync()
        {
            try
            {
                var route = await _maps.GetRoute(GetTwin().LocStart, GetTwin().LocEnd);
                Helper.WriteLine("Route calculated.", ConsoleColor.White, ConsoleColor.DarkYellow);
                ConnectSimulation(new DriveSimulation(route, GetTwin().UpdateInterval, this));
                SimulationStatus = true;
                Helper.WriteLine("Simulation setup completed.", ConsoleColor.White, ConsoleColor.DarkYellow);
                GetTwin().SimulationStatus = true;
                await GetTwin().SendReportedProperties();
                Helper.WriteLine("Twin reported properties sent.", ConsoleColor.White, ConsoleColor.DarkYellow);
                await RunSimulationAsync();
            }
            catch (OperationCanceledException)
            {
                SimulationStatus = false;
                GetTwin().SimulationStatus = false;
                await GetTwin().SendReportedProperties();

                Helper.WriteLine("Simulation run cancelled or stopped on request.", ConsoleColor.White, ConsoleColor.DarkRed);
            }
        }

        public override async Task DeviceTwinUpdateReceivedAsync(VehicleModuleTwin twin)
        {
            if (twin.UpdateInterval != GetSimulation().UpdateInterval)
            {
                GetSimulation().UpdateInterval = twin.UpdateInterval;
                Helper.WriteLine($"Updated UpdateInterval {twin.UpdateInterval} received.", ConsoleColor.White, ConsoleColor.DarkYellow);
            }

            if (twin.LocEnd != _locEnd || twin.LocStart != _locStart)
            {
                _locEnd = twin.LocEnd;
                _locStart = twin.LocStart;
                Helper.WriteLine($"Updated location {twin.LocStart} -> {twin.LocEnd}.", ConsoleColor.White, ConsoleColor.DarkYellow);
                await Stop();
                _ = RunAsync();
            }
        }

        private async Task<MessageResponse> OdometerMessageReceivedAsync(Message message, object userContext)
        {
            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine(messageString);
            Odometry odometry = TelemetrySegmentFactory<Odometry>.FromJsonString(messageString);
            GetSimulation().InputExternalTelemetry(odometry);

            return await Task.FromResult(MessageResponse.Completed);
        }

        public override async Task DispatchToOutputs(TelemetrySegment telemetry)
        {
            await SendIntoOutputAsync(telemetry, OutputName);
        }
    }
}

using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using VehicleDemonstrator.Shared.Util;
using VehicleDemonstrator.Shared.Telemetry;
using VehicleDemonstrator.Shared.Telemetry.Odometry;
using System.Net.Http;
using LocationModule;
using VehicleDemonstrator.Shared.SimulationEnvironment;
using Microsoft.Azure.Devices.Shared;

namespace VehicleDemonstrator.Module.Location
{
    class DriveSimulationHost : SimulationHost
    {
        private const string OdometerInputName = "odometerInput";
        private const string OutputName = "locationModuleOutput";
        private HttpClient _httpClient = new HttpClient();
        public Maps _maps;
        private string _locStart = "Bremen";
        private string _locEnd = "Hamburg";
        private int _updateInterval = 1000;

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
            if (_locStart != string.Empty && _locEnd != String.Empty)
            {
                try
                {
                    var route = await _maps.GetRoute(_locStart, _locEnd);
                    Helper.WriteLine("Route calculated.", ConsoleColor.White, ConsoleColor.DarkYellow);
                    ConnectSimulation(new DriveSimulation(route, _updateInterval, this));
                    SimulationStatus = true;
                    Helper.WriteLine("Simulation setup completed.", ConsoleColor.White, ConsoleColor.DarkYellow);

                    await SetTwinSimulationStatus(true);

                    Helper.WriteLine("Twin reported properties sent.", ConsoleColor.White, ConsoleColor.DarkYellow);
                    await RunSimulationAsync();
                }
                catch (OperationCanceledException)
                {
                    SimulationStatus = false;
                    await SetTwinSimulationStatus(false);
                    Helper.WriteLine("Simulation run cancelled or stopped on request.", ConsoleColor.White, ConsoleColor.DarkRed);
                }
            }
        }

        private async Task SetTwinSimulationStatus(bool status)
        {
            GetTwin().AddReportedProperties(new TwinCollection()
            {
                ["SimulationStatus"] = status
            });
            await GetTwin().SendReportedProperties();
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

        public override async Task UpdatedDesiredPropertiesReceivedAsync(TwinCollection desiredProperties)
        {
            if (desiredProperties.Contains("UpdateInterval"))
            {
                if (GetSimulation() != null)
                {
                    GetSimulation().UpdateInterval = desiredProperties["UpdateInterval"];
                } else
                {
                    _updateInterval = desiredProperties["UpdateInterval"];
                }
                
                Helper.WriteLine($"Updated UpdateInterval {desiredProperties["UpdateInterval"]} received.", ConsoleColor.White, ConsoleColor.DarkYellow);
            }

            if (desiredProperties.Contains("LocEnd") && desiredProperties["LocEnd"] != _locEnd && desiredProperties["LocEnd"] != String.Empty || desiredProperties.Contains("LocStart") && desiredProperties["LocStart"] != _locStart && desiredProperties["LocStart"] != String.Empty)
            {
                _locEnd = desiredProperties["LocEnd"];
                _locStart = desiredProperties["LocStart"];
                Helper.WriteLine($"Updated location {_locStart} -> {_locEnd}.", ConsoleColor.White, ConsoleColor.DarkYellow);
                await Stop();
                _ = RunAsync();
            }
        }
    }
}

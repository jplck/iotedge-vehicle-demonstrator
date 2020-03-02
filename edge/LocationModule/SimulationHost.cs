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

namespace VehicleDemonstrator.Module.Location
{
    class SimulationHost : IModuleTwin, ISimulationHost
    {
        private DriveSimulation _sim;
        private VehicleModuleTwin _twin;
        private CancellationTokenSource _cts;
        private bool _simStatus = false;
        private const string OdometerInputName = "odometerInput";
        private const string OutputName = "locationModuleOutput";
        private HttpClient _httpClient = new HttpClient();
        private string _subscriptionKey = "";

        public async Task Setup()
        {
            _subscriptionKey = Environment.GetEnvironmentVariable("AZURE_MAPS_KEY");
            _httpClient.BaseAddress = new Uri("https://atlas.microsoft.com");

            var hub = HubConnection.Instance;
            await hub.Connect();

            var client = hub.GetClient();

            _twin = new VehicleModuleTwin(this);
            await _twin.Init();

            await client.SetMethodHandlerAsync("Stop", OnStopRequest, null);
            await client.SetMethodHandlerAsync("Reset", OnResetRequest, null);
            await client.SetInputMessageHandlerAsync(OdometerInputName, OdometerMessageReceivedAsync, null);
        }

        public async Task Run()
        {
            try
            {
                _cts = new CancellationTokenSource();
                var route = await GetRoute();
                _sim = new DriveSimulation(route, _twin.UpdateInterval, this, _cts.Token);
                _simStatus = true;
                _twin.SimulationStatus = true;
                await _twin.SendReportedProperties();
                await _sim.Run(SimulationRunType.Track);
            }
            catch (OperationCanceledException)
            {
                _simStatus = false;
                _twin.SimulationStatus = false;
                await _twin.SendReportedProperties();

                Helper.WriteLine("Simulation run cancelled or stopped on request.", ConsoleColor.White, ConsoleColor.DarkRed);
            }
        }

        public async Task Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _sim = null;
                _cts = null;

                while (_simStatus)
                {
                    Helper.WriteLine("Waiting for simulation to stop...", ConsoleColor.White, ConsoleColor.DarkYellow);
                    await Task.Delay(250);
                }
            }
        }

        public async void DeviceTwinUpdateReceived(VehicleModuleTwin twin)
        {
            if (_sim != null)
            {
                if (twin.UpdateInterval != _sim.UpdateInterval)
                {
                    _sim.UpdateInterval = twin.UpdateInterval;
                    Helper.WriteLine($"Updated UpdateInterval {twin.UpdateInterval} received.", ConsoleColor.White, ConsoleColor.DarkYellow);
                }

                if (twin.LocEnd != _twin.LocEnd)
                {
                    _twin.LocEnd = twin.LocEnd;
                    Helper.WriteLine($"Updated end location {twin.LocEnd}.", ConsoleColor.White, ConsoleColor.DarkYellow);
                }

                if (twin.LocStart != _twin.LocStart)
                {
                    _twin.LocEnd = twin.LocEnd;
                    Helper.WriteLine($"Updated start location {twin.LocStart}.", ConsoleColor.White, ConsoleColor.DarkYellow);
                }

                if (twin.LocEnd != _twin.LocEnd || twin.LocStart != _twin.LocStart)
                {
                    await Stop();
                    _ = Run();
                }
            }

        }

        private async Task<MethodResponse> OnStopRequest(MethodRequest request, object context)
        {
            await Stop();
            return await Task.FromResult(new MethodResponse(200));
        }

        private async Task<MethodResponse> OnResetRequest(MethodRequest request, object context)
        {
            Helper.WriteLine("Resetting simulation...", ConsoleColor.White, ConsoleColor.DarkGreen);
            await Stop();
            _ = Run();
            return await Task.FromResult(new MethodResponse(200));
        }

        public void SendSimulatedTelemetry(TelemetrySegment telemetry)
        {
            Console.WriteLine(telemetry.ToJson());
            Message coordMsg = new Message(telemetry.ToJsonByteArray());
            HubConnection.Instance.GetClient().SendEventAsync(OutputName, coordMsg);
        }

        private async Task<MessageResponse> OdometerMessageReceivedAsync(Message message, object userContext)
        {
            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine(messageString);
            Odometry odometry = TelemetrySegmentFactory<Odometry>.FromJsonString(messageString);
            _sim.InputTelemetry(odometry);

            return await Task.FromResult(MessageResponse.Completed);
        }

        private async Task<RouteDirectionsResult> GetRoute()
        {
            var am = new AzureMapsToolkit.AzureMapsServices(_subscriptionKey);
      
            SearchAddressRequest startLocRequest = new SearchAddressRequest();
            startLocRequest.Query = _twin.LocStart;

            SearchAddressRequest endLocRequest = new SearchAddressRequest();
            endLocRequest.Query = _twin.LocEnd;
            
            var startLoc = await am.GetSearchAddress(startLocRequest);
            var endLoc = await am.GetSearchAddress(endLocRequest);

            var start = startLoc.Result.Results[0].Position;
            var end = endLoc.Result.Results[0].Position;

            var lat1 = start.Lat.ToString("0.00", new CultureInfo("en-us", false));
            var lon1 = start.Lon.ToString("0.00", new CultureInfo("en-us", false));
            var lat2 = end.Lat.ToString("0.00", new CultureInfo("en-us", false));
            var lon2 = end.Lon.ToString("0.00", new CultureInfo("en-us", false));

            RouteRequestDirections directions = new RouteRequestDirections();
            directions.Query = $"{lat1},{lon1}:{lat2},{lon2}";

            var route = await am.GetRouteDirections(directions);
            var routeContent = route.Result.Routes[0];

            return routeContent;
        }
    }
}

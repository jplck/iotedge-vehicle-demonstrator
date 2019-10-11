using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VehicleDemonstrator.Shared.Connectivity;
using VehicleDemonstrator.Shared.Util;
using VehicleDemonstrator.Shared.GPX;
using VehicleDemonstrator.Shared.Telemetry;
using VehicleDemonstrator.Shared.Telemetry.Odometry;

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

        public async Task Setup()
        {
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
                GPX gpx = GPXReader.Read(_twin.RouteFile);
                _sim = new DriveSimulation(gpx, _twin.UpdateInterval, this, _cts.Token);
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

        public void DeviceTwinUpdateReceived(VehicleModuleTwin twin)
        {
            if (_sim != null)
            {
                _sim.UpdateInterval = twin.UpdateInterval;
                Helper.WriteLine($"Updated UpdateInterval {twin.UpdateInterval} received.", ConsoleColor.White, ConsoleColor.DarkYellow);
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
    }
}

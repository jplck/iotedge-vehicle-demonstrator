using GPXParserLib;
using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelemetryLib;

namespace locationmodule
{
    class SimulationHost : HubConnection, IModuleTwin, ISimulationHost
    {
        private DriveSimulation _sim;
        private ModuleTwin _twin;
        private CancellationTokenSource _cts;
        private const string OdometerInputName = "odometerInput";
        private const string OutputName = "locationModuleOutput";

        public async Task Setup()
        {
            await Connect();

            var client = GetClient();

            if (client != null)
            {
                _twin = new ModuleTwin(client, this);
                await _twin.Init();

                await client.SetMethodHandlerAsync("Stop", OnStopRequest, null);
                await client.SetMethodHandlerAsync("Reset", OnResetRequest, null);
                await client.SetInputMessageHandlerAsync(OdometerInputName, OdometerMessageReceivedAsync, client);
            }
            else
            {
                Console.WriteLine("Error: No client set.");
            }
        }

        public async Task Run()
        {
            try
            {
                _cts = new CancellationTokenSource();
                GPX gpx = GPXReader.Read(_twin.RouteFile);
                _sim = new DriveSimulation(gpx, _twin.UpdateInterval, this, _cts.Token);
                _twin.SimulationStatus = true;
                await _twin.SendReportedProperties();
                await _sim.Run(SimulationRunType.Track);
            }
            catch (Exception cancellationException)
            {
                Console.WriteLine(cancellationException.Message);
                Console.WriteLine("Simulation run cancelled or stopped on request.");
                _twin.SimulationStatus = false;
                await _twin.SendReportedProperties();
            }
        }

        public void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _sim = null;
                _cts = null;
            }
        }

        public void UpdatedDesiredPropertiesReceived(ModuleTwin twin)
        {
            if (_sim != null)
            {
                _sim.UpdateInterval = twin.UpdateInterval;
            }

        }

        private async Task<MethodResponse> OnStopRequest(MethodRequest request, object context)
        {
            Stop();
            return await Task.FromResult(new MethodResponse(200));
        }

        private async Task<MethodResponse> OnResetRequest(MethodRequest request, object context)
        {
            Stop();
            _ = Run();
            return await Task.FromResult(new MethodResponse(200));
        }

        public void SendSimulatedTelemetry(TelemetrySegment telemetry)
        {
            Console.WriteLine(telemetry.ToJson());
            Message coordMsg = new Message(telemetry.ToJsonByteArray());
            GetClient().SendEventAsync(OutputName, coordMsg);
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

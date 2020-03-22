using System;
using System.Threading;
using System.Threading.Tasks;
using VehicleDemonstrator.Shared.Util;
using VehicleDemonstrator.Shared.Connectivity;
using Microsoft.Azure.Devices.Client;
using VehicleDemonstrator.Shared.Telemetry;
using Microsoft.Azure.Devices.Shared;

namespace VehicleDemonstrator.Shared.SimulationEnvironment
{
    public abstract class SimulationHost: ITwinUpdateReceiver
    {
        private ModuleTwin _twin;
        private CancellationTokenSource _cts;
        private Simulation sim;
        public bool SimulationStatus = false;
        private ModuleClient client;

        public async Task SetupConnectionAsync()
        {
            var hub = HubConnection.Instance;
            await hub.Connect();

            client = hub.GetClient();

            _twin = new ModuleTwin(this);
            await _twin.Init();

            await client.SetMethodHandlerAsync("Stop", OnStopRequest, null);
            await client.SetMethodHandlerAsync("Reset", OnResetRequest, null);
        }

        public void ConnectSimulation(Simulation simulation)
        {
            _cts = new CancellationTokenSource();
            sim = simulation;
            sim.SetCancellationToken(_cts.Token);
        }

        public async Task RunSimulationAsync() => await sim.RunAsync();

        public ModuleTwin GetTwin() => _twin;

        public Simulation GetSimulation() => sim;

        public abstract Task RunAsync();

        public async Task AddInputHandlerAsync(string inputName, MessageHandler handlerMethod, object userContext = null) 
            => await client.SetInputMessageHandlerAsync(inputName, handlerMethod, userContext);

        public async Task AddDirectMethodHandler(string methodName, MethodCallback callbackMethod, object userContext = null) 
            => await client.SetMethodHandlerAsync(methodName, callbackMethod, userContext);

        private async Task<MethodResponse> OnStopRequest(MethodRequest request, object context)
        {
            await Stop();
            return await Task.FromResult(new MethodResponse(200));
        }

        private async Task<MethodResponse> OnResetRequest(MethodRequest request, object context)
        {
            Helper.WriteLine("Resetting simulation...", ConsoleColor.White, ConsoleColor.DarkGreen);
            await Stop();
            _ = RunAsync();
            return await Task.FromResult(new MethodResponse(200));
        }

        public async Task Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                sim = null;
                _cts = null;

                while (SimulationStatus)
                {
                    Helper.WriteLine("Waiting for simulation to stop...", ConsoleColor.White, ConsoleColor.DarkYellow);
                    await Task.Delay(250);
                }
            }
        }

        public abstract Task DispatchToOutputs(TelemetrySegment telemetry);

        public async Task SendIntoOutputAsync(TelemetrySegment telemetry, string outputName)
        {
            Console.WriteLine(telemetry.ToJson());
            Message message = new Message(telemetry.ToJsonByteArray());
            await HubConnection.Instance.GetClient().SendEventAsync(outputName, message);
        }

        public abstract Task UpdatedDesiredPropertiesReceivedAsync(TwinCollection desiredProperties);
    }
}

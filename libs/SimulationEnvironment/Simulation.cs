using System;
using System.Threading;
using System.Threading.Tasks;
using VehicleDemonstrator.Shared.Telemetry;

namespace VehicleDemonstrator.Shared.SimulationEnvironment
{
    public abstract class Simulation
    {
        private SimulationHost simulationHost;

        public CancellationToken CT { get; set; }

        public abstract void InputExternalTelemetry(TelemetrySegment telemetry);

        public abstract Task RunAsync();

        public Simulation(SimulationHost host) 
        {
            _ = host ?? throw new ArgumentNullException("host", "SimulationHost cannot be null");
            simulationHost = host;
        }

        public void PushTelemetryToHost(TelemetrySegment telemetry) => simulationHost.DispatchToOutputs(telemetry);

        public void ListenToCancellationRequests() => CT.ThrowIfCancellationRequested();
    }
}

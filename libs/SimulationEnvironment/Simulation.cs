using System.Threading;
using System.Threading.Tasks;
using VehicleDemonstrator.Shared.Telemetry;

namespace VehicleDemonstrator.Shared.SimulationEnvironment
{
    public abstract class Simulation
    {
        private SimulationHost simulationHost;

        private CancellationToken cancellationToken;

        public int UpdateInterval = 1000;

        public abstract void InputExternalTelemetry(TelemetrySegment telemetry);

        public abstract Task RunAsync();

        public Simulation( int updateInterval, SimulationHost host) 
        {
            UpdateInterval = updateInterval;
            simulationHost = host;
        }

        public void SetCancellationToken(CancellationToken ct)
        {
            cancellationToken = ct;
        }

        public void PushTelemetryToHost(TelemetrySegment telemetry) => simulationHost.DispatchToOutputs(telemetry);

        public void ListenToCancellationRequests() => cancellationToken.ThrowIfCancellationRequested();
    }
}

using AzureMapsToolkit.Common;
using LocationModule;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VehicleDemonstrator.Shared.Telemetry;
using VehicleDemonstrator.Shared.Telemetry.Location;
using VehicleDemonstrator.Shared.Telemetry.Odometry;

namespace VehicleDemonstrator.Module.Location
{
    class DriveSimulation: Simulation
    {
        private int currentSpeed = 50;
        private RouteDirectionsResult gpx;
        private double accTripDistance;
        private Stopwatch tripStopwatch;

        public DriveSimulation(RouteDirectionsResult gpx, int updateInterval, SimulationHost simulationHost): base(updateInterval, simulationHost)
        {
            this.gpx = gpx;
        }

        public override async Task RunAsync()
        {
            for (int l = 0; l < gpx.Legs.Length; l++)
            {
                var leg = gpx.Legs[l];
                await RunItemsAsync(leg);
            }
        }

        public async Task RunItemsAsync(RouteResultLeg leg)
        {
            tripStopwatch = new Stopwatch();
            accTripDistance = 0;

            string tripGuid = Guid.NewGuid().ToString();

            tripStopwatch.Start();

            Coordinate previousPoint = null;
            for (int p = 0; p < leg.Points.Length; p++)
            {
                var point = leg.Points[p];

                Console.WriteLine("Read coordinates...");
                if (previousPoint != null)
                {
                    await DriveAsync(previousPoint, point, tripGuid);
                }
                previousPoint = point;
                ListenToCancellationRequests();
            }
        }

        private async Task DriveAsync(Coordinate startPoint, Coordinate endPoint, string tripGuid)
        {
            var start = new Point(startPoint);
            var end = new Point(endPoint);

            double fullDistance = Util.CalculateDistance(start, end);
            double percentage = 0;

            while (percentage < 1)
            {
                /*
                 * Calculate the distance the vehicle drives in [m/s] based on an update interval factor in [s] and the current speed.
                 * */
                double distanceInTimeInterval = ((currentSpeed * 1000) / 3600) * TimeSpan.FromMilliseconds(UpdateInterval).TotalSeconds;

                /*
                 * Calculate the overall distance between the start and end point of the drive. Multiply the percentage 
                 * of the already driven parts of the route
                 * 
                 * */
                double distance = fullDistance * (percentage > 0 ? 1 - percentage : 1.0);
                accTripDistance += distance;
                /*
                 * Calculate how much of percent the vehicle drives off of the overall distance
                 * */
                double percentageInTimeInterval = distanceInTimeInterval / distance;

                /* 
                 * Add the previous percentage to the calculated percentage in this time interval
                 */
                percentage = Math.Min(percentageInTimeInterval + percentage, 1);

                Console.WriteLine($"Percentage driven: {100*Math.Round(percentage, 2)}%");

                var timeSinceStart = tripStopwatch.Elapsed.TotalSeconds;

                if (percentage == 1)
                {
                    var coords = end;
                    var trip = new Trip(tripGuid, coords, accTripDistance, timeSinceStart);
                    PushTelemetryToHost(trip);
                    await Task.Delay(UpdateInterval);
                }
                else
                {
                    Point newCoord = Util.NewCoordinates(start, end, percentage);
                    var trip = new Trip(tripGuid, newCoord, accTripDistance, timeSinceStart);
                    PushTelemetryToHost(trip);
                    await Task.Delay(UpdateInterval);
                }

                ListenToCancellationRequests();
            }
        }

        public override void InputExternalTelemetry(TelemetrySegment telemetry)
        {
            if (telemetry.GetTelemetryType() == TelemetryType.Odometry)
            {
                var odometry = TelemetrySegmentFactory<Odometry>.FromJsonString(telemetry.ToJson());
                currentSpeed = odometry.GetSpeed();

                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Received updated speed of {odometry.GetSpeed()}km/h");
                Console.ResetColor();
            }
        }
    }
}

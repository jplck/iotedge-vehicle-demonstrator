using AzureMapsToolkit.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VehicleDemonstrator.Shared.Telemetry;
using VehicleDemonstrator.Shared.Telemetry.Location;
using VehicleDemonstrator.Shared.Telemetry.Odometry;

namespace VehicleDemonstrator.Module.Location
{
    interface ISimulationReceiver
    {
        void TelemetryReceived(TelemetrySegment telemetry);
    }

    interface ISimulationHost
    {
        void SendSimulatedTelemetry(TelemetrySegment telemetry);
    }

    enum SimulationRunType
    {
        Route,
        Track,
        Waypoint
    }

    class DriveSimulation
    {
        private int _currentSpeed = 50;
        private ISimulationHost _simulationHost;
        private RouteDirectionsResult _gpx;
        public int UpdateInterval = 1000;
        private CancellationToken _cancellationToken;
        private double _accTripDistance;
        private Stopwatch _tripStopwatch;

        public DriveSimulation(RouteDirectionsResult gpx, int updateInterval, ISimulationHost simulationHost, CancellationToken cancellationToken)
        {
            _gpx = gpx;
            UpdateInterval = updateInterval;
            _simulationHost = simulationHost;
            _cancellationToken = cancellationToken;
        }

        public async Task Run(SimulationRunType runType)
        {
            for (int l = 0; l < _gpx.Legs.Length; l++)
            {
                var leg = _gpx.Legs[l];
                await RunItems(leg);
            }
        }

        public async Task RunItems(RouteResultLeg leg)
        {
            _tripStopwatch = new Stopwatch();
            _accTripDistance = 0;

            string tripGuid = Guid.NewGuid().ToString();

            _tripStopwatch.Start();

            AzureMapsToolkit.Common.Coordinate previousPoint = null;
            for (int p = 0; p < leg.Points.Length; p++)
            {
                var point = leg.Points[p];

                Console.WriteLine("Read coordinates...");
                if (previousPoint != null)
                {
                    await Drive(previousPoint, point, tripGuid);
                }
                previousPoint = point;
                _cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private async Task Drive(Coordinate startPoint, Coordinate endPoint, string tripGuid)
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
                double distanceInTimeInterval = ((_currentSpeed * 1000) / 3600) * TimeSpan.FromMilliseconds(UpdateInterval).TotalSeconds;

                /*
                 * Calculate the overall distance between the start and end point of the drive. Multiply the percentage 
                 * of the already driven parts of the route
                 * 
                 * */
                double distance = fullDistance * (percentage > 0 ? 1 - percentage : 1.0);
                _accTripDistance += distance;
                /*
                 * Calculate how much of percent the vehicle drives off of the overall distance
                 * */
                double percentageInTimeInterval = distanceInTimeInterval / distance;

                /* 
                 * Add the previous percentage to the calculated percentage in this time interval
                 */
                percentage = Math.Min(percentageInTimeInterval + percentage, 1);

                Console.WriteLine($"Percentage driven: {100*Math.Round(percentage, 2)}%");

                var timeSinceStart = _tripStopwatch.Elapsed.TotalSeconds;

                if (percentage == 1)
                {
                    var coords = end;
                    var trip = new Trip(tripGuid, coords, _accTripDistance, timeSinceStart);
                    _simulationHost.SendSimulatedTelemetry(trip);
                    await Task.Delay(UpdateInterval);
                }
                else
                {
                    Point newCoord = Util.NewCoordinates(start, end, percentage);
                    var trip = new Trip(tripGuid, newCoord, _accTripDistance, timeSinceStart);
                    _simulationHost.SendSimulatedTelemetry(trip);
                    await Task.Delay(UpdateInterval);
                }

                _cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public void InputTelemetry(TelemetrySegment telemetry)
        {
            if (telemetry.GetTelemetryType() == TelemetryType.Odometry)
            {
                var odometry = TelemetrySegmentFactory<Odometry>.FromJsonString(telemetry.ToJson());
                _currentSpeed = odometry.GetSpeed();

                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Received updated speed of {odometry.GetSpeed()}km/h");
                Console.ResetColor();
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VehicleDemonstrator.Shared.GPX;
using VehicleDemonstrator.Shared.GPX.Route;
using VehicleDemonstrator.Shared.GPX.Track;
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
        private GPX _gpx;
        public int UpdateInterval = 1000;
        private CancellationToken _cancellationToken;
        private double _accTripDistance;
        private Stopwatch _tripStopwatch;

        public DriveSimulation(GPX gpx, int updateInterval, ISimulationHost simulationHost, CancellationToken cancellationToken)
        {
            _gpx = gpx;
            UpdateInterval = updateInterval;
            _simulationHost = simulationHost;
            _cancellationToken = cancellationToken;
        }

        public async Task Run(SimulationRunType runType)
        {
            if (runType == SimulationRunType.Route && _gpx.Routes.Count > 0 && _gpx.Routes[0].RoutePoints.Count > 0)
            {
                foreach (GPXRoute route in _gpx.Routes)
                {
                    await RunItems(route.RoutePoints);
                }
            } else if (runType == SimulationRunType.Track && _gpx.Tracks.Count > 0 && _gpx.Tracks[0].TrackSegments.Count > 0)
            {
                foreach (GPXTrack track in _gpx.Tracks)
                {
                    foreach (GPXTrackSegment segment in track.TrackSegments)
                    {
                        await RunItems(segment.TrackPoints);
                    }
                }
            }
        }

        public async Task RunItems(IEnumerable<GPXGenericItem> items)
        {
            _tripStopwatch = new Stopwatch();
            _accTripDistance = 0;

            string tripGuid = Guid.NewGuid().ToString();

            _tripStopwatch.Start();

            GPXGenericItem previousPoint = null;
            foreach (GPXGenericItem item in items)
            {
                Console.WriteLine("Read coordinates...");
                if (previousPoint != null)
                {
                    await Drive(previousPoint, item, tripGuid);
                }
                previousPoint = item;
                _cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private async Task Drive(GPXGenericItem startPoint, GPXGenericItem endPoint, string tripGuid)
        {
            Coordinate start = new Coordinate(startPoint);
            Coordinate end = new Coordinate(endPoint);

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
                    var coords = new Coordinate(endPoint);
                    var trip = new Trip(tripGuid, coords, _accTripDistance, timeSinceStart);
                    _simulationHost.SendSimulatedTelemetry(trip);
                    await Task.Delay(UpdateInterval);
                }
                else
                {
                    Coordinate newCoord = Util.NewCoordinates(start, end, percentage);
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

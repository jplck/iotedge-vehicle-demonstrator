using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using VehicleDemonstrator.Shared.Telemetry;
using VehicleDemonstrator.Shared.Telemetry.Odometry;

namespace VehicleServices
{
    public static class ServiceDataDispatcher
    {
        [FunctionName("ServiceDataDispatcher")]
        public static async Task Run(
            [EventHubTrigger("iotedge-telemetry-demo-hub", Connection = "IotEdgeEventHubConnection")] EventData[] events,
            [EventHub("iotedge-trip-event-hub", Connection = "TripHubConnectionWriter")] ICollector<EventData> tripDataEvents,
            [EventHub("iotedge-odometry-event-hub", Connection = "OdometerHubConnectionWriter")] ICollector<EventData> odometerDataEvents,
            ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    TelemetryContainer<TelemetrySegment> telemetryContainer = TelemetrySegmentFactory<TelemetryContainer<TelemetrySegment>>.FromJsonString(messageBody);

                    switch (telemetryContainer.GetPayload().GetTelemetryType())
                    {
                        case TelemetryType.Trip:
                            DispatchTripTelemetry(eventData.Body.Array, tripDataEvents, log);
                            break;
                        case TelemetryType.Odometry:
                            DispatchOdometerTelemetry(eventData.Body.Array, odometerDataEvents, log);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }

        private static void DispatchTripTelemetry(byte[] msgBytes, ICollector<EventData> tripDataEvents, ILogger log)
        {
            log.LogInformation("Trip data has been dispatched.");
            tripDataEvents.Add(new EventData(msgBytes));
        }
        private static void DispatchOdometerTelemetry(byte[] msgBytes, ICollector<EventData> odometerDataEvents, ILogger log)
        {
            log.LogInformation("Odometer data has been dispatched.");
            odometerDataEvents.Add(new EventData(msgBytes));
        }
    }
}

namespace VehicleDemonstrator.Module.TelemetryDispatcher
{
    using System;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using VehicleDemonstrator.Shared.Telemetry;
    using VehicleDemonstrator.Shared.Telemetry.Location;
    using VehicleDemonstrator.Shared.Telemetry.Odometry;

    class Program
    {
        static string _deviceId;
        static string InputName = "telemetryInput";
        static string OutputName = "output1";

        static void Main(string[] args)
        {
            Init().Wait();

            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        static async Task Init()
        {
            _deviceId = Environment.GetEnvironmentVariable("IOTEDGE_DEVICEID");
            if (_deviceId == null)
            {
                _deviceId = Guid.NewGuid().ToString();
            }

            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("Dispatcher active.");

            await ioTHubModuleClient.SetInputMessageHandlerAsync(InputName, PipeMessage, ioTHubModuleClient);
        }

        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            try
            {


                var moduleClient = userContext as ModuleClient;
                if (moduleClient == null)
                {
                    throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
                }

                byte[] messageBytes = message.GetBytes();
                string messageString = Encoding.UTF8.GetString(messageBytes);

                if (!string.IsNullOrEmpty(messageString))
                {

                    var telemetry = TelemetrySegmentFactory<TelemetrySegment>.FromJsonString(messageString);

                    switch (telemetry.GetTelemetryType())
                    {
                        case TelemetryType.Location:
                            telemetry = TelemetrySegmentFactory<Coordinate>.FromJsonString(messageString);
                            Console.WriteLine($"Received coordinates.");
                            break;
                        default:
                            telemetry = TelemetrySegmentFactory<Odometry>.FromJsonString(messageString); ;
                            Console.WriteLine($"Received odometry.");
                            break;
                    }

                    TelemetryContainer container = new TelemetryContainer(_deviceId, telemetry);
                    Console.WriteLine(container.ToJson());
                    var pipeMessage = new Message(container.ToJsonByteArray());
                    await moduleClient.SendEventAsync(OutputName, pipeMessage);
                    Console.WriteLine("Received message sent");
                }

            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return MessageResponse.Completed;
        }
    }
}

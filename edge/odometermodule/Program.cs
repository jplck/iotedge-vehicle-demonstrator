namespace odometermodule
{
    using System;
    using System.Runtime.Loader;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using TelemetryLib;

    class Program
    {
        static int UpdateInterval = 1000;
        static string OdometerOutputName = "odometerOutput";

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
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("Odometer initialized.");

            while (true)
            {
                Random random = new Random();
                int rndSpeed = random.Next(50, 100);
                Odometry odometry = new Odometry(rndSpeed);
                byte[] msg = odometry.ToJsonByteArray();
                await ioTHubModuleClient.SendEventAsync(OdometerOutputName, new Message(msg));
                Console.WriteLine(odometry.ToJson());
                await Task.Delay(UpdateInterval);
            }
        }
    }
}

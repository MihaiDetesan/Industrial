using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.IO;
using System.Threading;

namespace MiDe.TimeSender
{
    class Program
    {
        private static System.Timers.Timer? timer;
        static ILogger logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
        static ManualResetEventSlim waitHandle = new ManualResetEventSlim(false);
        static ManualResetEventSlim connectHandle = new ManualResetEventSlim(false);

        static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true, reloadOnChange: true).Build();

            var interval = configuration.GetValue<double?>("Interval") ?? throw new ArgumentNullException("Interval", "Missing refresh time interval setting");
            var address = configuration.GetValue<string>("Ip") ?? throw new ArgumentNullException("Interval", "Missing IP address setting ");
            var port = configuration.GetValue<int?>("port") ?? throw new ArgumentNullException("Interval", "Missing port setting");

            SocketStringSender client = new SocketStringSender(address, port, logger);
            bool connected = false;
            bool connectionFailed = false;
            
            while (true)
            {
                try
                {
                    client = new SocketStringSender(address, port, logger);
                    if (client.Connect())
                    {
                        logger.Info($"Connected to {address}:{port}");
                        connectHandle.Reset();
                        timer = new System.Timers.Timer()
                        {
                            AutoReset = true,
                            Interval = interval * 1000
                        };

                        timer.Elapsed += (s, e) => OnTimer(client);
                        timer.Start();
                        connectHandle.Wait();
                        timer.Stop();
                    }
                    else
                    {
                        if (connectionFailed == false)
                        {
                            logger.Error($"Connection to {address}:{port} failed");
                        }

                        connectionFailed = true;
                        Thread.Sleep(5000);
                    }
                }
                catch (Exception)
                {
                    if (connected)
                    {
                        connectionFailed = true;
                        connected = false;
                        logger.Error($"Connection to {address}:{port} was reset");
                    }

                    client?.Close();
                }
            }
        }

        private static void OnTimer(SocketStringSender client)
        {
            var triggerTime = DateTime.UtcNow;

            try
            {
                client.Send($"{triggerTime.ToString()}/r/n");
            }
            catch(Exception ex)
            {
                logger.Error($"WTF {ex} ");
                connectHandle.Set();
            }
        }
    }
}

using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Timers;

namespace MiDe.TimeReceiver
{
    class Program
    {
        static ILogger logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
        static ManualResetEventSlim waitHandle = new ManualResetEventSlim(false);

        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true, reloadOnChange: true).Build();

            var port = configuration.GetValue<int>("port");
            var ethernetListenr = new EthernetNotificationListener(GetLocalIPAddress().FirstOrDefault("127.0.0.1"), port, logger); ;
            await ethernetListenr.StartListenerAsync();
            waitHandle.Wait();
        }

        public static IList<string> GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var validAdresses = new List<string>();

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    validAdresses.Add(ip.ToString());
                }
            }

            return validAdresses;
        }
    }
}


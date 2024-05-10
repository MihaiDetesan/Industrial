using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MiDe.TimeReceiver
{
    public class EthernetNotificationListener
    {
        TcpListener server;
        private readonly ILogger logger;

        public EthernetNotificationListener(string ip, int port, ILogger logger)
        {
            this.logger = logger;
            IPAddress localAddr;


            localAddr = IPAddress.Parse(ip);

            server = new TcpListener(localAddr, port);
            server.Start();

            var localEndpoint = (IPEndPoint)server.LocalEndpoint;
            logger.Info($"Waiting connections at {localEndpoint.Address}:{localEndpoint.Port} ");
        }

        public async Task StartListenerAsync()
        {
            try
            {
                while (true)
                {
                    var client = await server.AcceptTcpClientAsync();
                    var remoteEndPoint = (IPEndPoint?)client.Client.RemoteEndPoint;

                    logger.Info($"Connected to {remoteEndPoint?.Address}:{remoteEndPoint?.Port}");

                    if (client != null)
                    {
                        await HandleDeviceAsync(client);
                    }
                }
            }
            catch (SocketException e)
            {
                logger.Info("SocketException: {0}", e);
                server?.Stop();
            }
        }

        public async Task HandleDeviceAsync(TcpClient client)
        {
            var stream = client.GetStream();
            string imei = String.Empty;

            string? data = null;
            Byte[] bytes = new Byte[256];
            int i;
            try
            {
                while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, i);
                    logger.Info(data);
                }
            }
            catch (Exception e)
            {
                logger.Error("Exception: {0}", e.ToString());
                client.Close();
            }
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MiDe.TimeSender
{
    class SocketStringSender
    {
        private TcpClient? client;
        private string server;
        private int port;
        NLog.ILogger logger;

        public SocketStringSender(String server, int port, NLog.ILogger logger)
        {
            this.server = server;
            this.port = port;
            this.logger = logger;
        }

        public void Send(string message)
        {
            if (client?.Connected ?? false)
            {
                var data = Encoding.ASCII.GetBytes(message);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);

                var remoteEndpoint = (IPEndPoint?)client?.Client?.RemoteEndPoint;
                logger.Info($"Sent: {message} to {remoteEndpoint?.Address}:{remoteEndpoint?.Port}");
            }
        }

        public void Close()
        {
            if (client?.Connected ?? false)
            {
                NetworkStream stream = client.GetStream();
                stream.Close();
                client.Close();
            }
        }

        public bool Connect()
        {
            bool connectionState = false;

            try
            {
                client = new TcpClient(server, port);
                connectionState = client.Connected;
            }
            catch (SocketException ex)
            {
                logger.Trace($"Could not connect to {server}:{port}", ex);
            }
            catch (ArgumentNullException ex)
            {
                logger.Trace($"Server address can't be null", ex);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                logger.Trace($"Port outside of range {port}", ex);
            }

            return connectionState;
        }
    }
}

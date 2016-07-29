using FluentHub.Hub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.TCP
{
    public class TcpClientFactory : INativeIOFactory<TcpClient>
    {
        private int port;
        private string host;
        private TcpClient connectedClient;
        private bool isDisposed;

        public TcpClientFactory(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public void Dispose()
        {
            this.isDisposed = true;
        }

        
        public TcpClient Make()
        {
            // 接続済だったら接続しない
            if (this.connectedClient != null && this.connectedClient.Connected)
            {
                Thread.Sleep(1000);
                return null;
            }
            this.connectedClient = null;

            var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);

            while (this.isDisposed == false && connectTask.IsEnd() == false)
            {
                Thread.Sleep(10);
            }

            if (this.isDisposed)
            {
                return null;
            }

            if (connectTask.IsFaulted)
            {
                Thread.Sleep(1000);
                return null;
            }

            this.connectedClient = client;
            return client;
        }
    }
}

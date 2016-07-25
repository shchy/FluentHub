using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.TCP
{
    public class TcpClientFactory : TcpFactory
    {
        private int port;
        private string host;
        private TcpClient connectedClient;

        public TcpClientFactory(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        // todo 接続したのに何度もいっちゃうよね
        protected override TcpClient GetTcpClient()
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

            while (this.isDisporsed == false && IsEnd(connectTask) == false)
            {
                Thread.Sleep(10);
            }

            if (isDisporsed || connectTask.IsFaulted || connectTask.Exception != null)
            {
                Thread.Sleep(1000);
                return null;
            }

            this.connectedClient = client;
            return client;
        }
    }
}

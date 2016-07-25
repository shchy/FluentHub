using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.Hub.TCP
{
    public class TcpServerFactory : TcpFactory
    {
        private TcpListener listener;
        private int port;

        // todo 複数ポートいけるようにしたい
        public TcpServerFactory(int port)
        {
            this.port = port;

            this.listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }

        public override void Dispose()
        {
            base.Dispose();
            listener.Stop();
        }

        protected override TcpClient GetTcpClient()
        {
            var acceptTask = listener.AcceptTcpClientAsync();

            while (this.isDisporsed == false && IsEnd(acceptTask) == false)
            {
                Thread.Sleep(10);
            }

            if (isDisporsed || acceptTask.IsCompleted == false)
            {
                return null;
            }

            return acceptTask.Result;
        }
    }

}

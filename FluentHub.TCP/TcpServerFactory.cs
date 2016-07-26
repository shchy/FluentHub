using FluentHub.Hub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.TCP
{
    public class TcpServerFactory : INativeIOFactory<TcpClient>
    {
        private bool isDisposed;
        private TcpListener listener;
        private int port;

        // todo 複数ポートいけるようにしたい
        public TcpServerFactory(int port)
        {
            this.port = port;
            this.listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }

        public void Dispose()
        {
            listener.Stop();
            this.isDisposed = true;
        }

        public TcpClient Make()
        {
            if (this.isDisposed)
            {
                return null;
            }
            var acceptTask = listener.AcceptTcpClientAsync();

            while (this.isDisposed == false && acceptTask.IsEnd() == false)
            {
                Thread.Sleep(10);
            }

            if (this.isDisposed)
            {
                return null;
            }

            if (acceptTask.IsFaulted)
            {
                Thread.Sleep(1000);
                return null;
            }

            return acceptTask.Result;
        }
    }

}

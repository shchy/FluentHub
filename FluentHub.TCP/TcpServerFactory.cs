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
        private TcpListener[] listeners;

        // todo 複数ポートいけるようにしたい
        public TcpServerFactory(params int[] ports)
        {
            this.listeners = ports.Select(port => new TcpListener(IPAddress.Any, port)).ToArray();
            foreach (var listener in listeners)
            {
                listener.Start();
            }

        }

        public void Dispose()
        {
            foreach (var listener in listeners)
            {
                listener.Stop();
            }
            this.isDisposed = true;
        }

        public TcpClient Make()
        {
            if (this.isDisposed)
            {
                return null;
            }
            var listener = this.listeners.Where(x => x.Pending()).FirstOrDefault();
            if (listener == null)
            {
                Thread.Sleep(10);
                return null;
            }
            var client = listener.AcceptTcpClient();
            
            if (this.isDisposed)
            {
                return null;
            }

            return client;
        }
    }

}

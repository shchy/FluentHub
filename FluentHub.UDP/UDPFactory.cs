using System;
using System.IO.Ports;
using FluentHub.IO;
using FluentHub.Hub;
using System.Threading;
using FluentHub.IO.Extension;
using FluentHub.UDP;
using System.IO;

namespace FluentHub.UDP
{
    public class UDPFactory : INativeIOFactory<Stream>
    {
        private Stream connected;
        private bool isDisposed;
        private int recvPort;
        private string sendHost;
        private int sendPort;

        public UDPFactory(string sendHost, int sendPort, int recvPort)
        {
            this.sendHost = sendHost;
            this.sendPort = sendPort;
            this.recvPort = recvPort;
        }

        public void Dispose()
        {
            this.isDisposed = true;
            if (this.connected != null)
            {
                this.connected.Dispose();
            }
        }

        public Stream Make()
        {
            if (this.isDisposed)
            {
                return null;
            }

            // 接続済だったら接続しない
            if (this.connected != null 
                && connected.CanRead 
                && connected.CanWrite)
            {
                Thread.Sleep(1000);
                return null;
            }

            try
            {
                var stream = new FakeStream(new UDPIO(sendHost, sendPort, recvPort));
                this.connected = stream;
                return stream;
            }
            catch (Exception )
            {
                Thread.Sleep(1000);
                return null;
            }
            
            
        }
    }
}
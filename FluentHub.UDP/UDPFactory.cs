using System;
using System.IO.Ports;
using FluentHub.IO;
using FluentHub.Hub;
using System.Threading;
using FluentHub.IO.Extension;
using FluentHub.UDP;
using System.IO;
using System.Net;

namespace FluentHub.UDP
{
    public class UDPFactory : INativeIOFactory<FakeStream>
    {
        private Stream connected;
        private bool isDisposed;
        private int recvPort;
        private IPAddress sendHost;
        private int sendPort;

        public UDPFactory(IPAddress host, int sendPort, int recvPort)
        {
            this.sendHost = host;
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

        public bool IsAlreadyEnough()
        {
            return this.connected != null
                && connected.CanRead
                && connected.CanWrite;
        }

        public FakeStream Make()
        {
            if (this.isDisposed)
            {
                return null;
            }

            // 接続済だったら接続しない
            if (IsAlreadyEnough())
            {
                Thread.Sleep(1000);
                return null;
            }

            try
            {
                var localPoint = new IPEndPoint(IPAddress.Any, recvPort);
                var remotePoint = new IPEndPoint(sendHost, sendPort);
                var stream = new FakeStream(
                    new UDPIO(localPoint, remotePoint)
                    , localPoint
                    , remotePoint);
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
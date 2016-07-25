using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.UDP
{
    public class UDPContextFactory : IRunnableFactory<IIOContext<byte>>
    {
        private ManualResetEvent waithandle;
        private Func<UdpStream> make;

        public Action<IIOContext<byte>> Maked { get; set; }

        public UDPContextFactory(string host, int sendPort, int recvPort)
        {
            this.waithandle = new ManualResetEvent(false);
            this.make = () => new UdpStream(host, sendPort, recvPort);
        }

        public void Dispose()
        {
            this.waithandle.Set();
        }

        public void Run()
        {
            Maked(make().BuildContextByUdp());
            this.waithandle.WaitOne();
        }
    }
}

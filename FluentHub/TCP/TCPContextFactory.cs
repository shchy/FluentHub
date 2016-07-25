using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.TCP
{
    public class TCPContextFactory : IRunnableFactory<IIOContext<byte>>
    {
        private IRunnableFactory<TcpClient> factory;

        public Action<IIOContext<byte>> Maked { get; set; }

        public TCPContextFactory(
            IRunnableFactory<TcpClient> factory)
        {
            this.factory = factory;
            factory.Maked = MakedClient;
        }

        private void MakedClient(TcpClient client)
        {
            var context = client.BuildContextByTcp();
            if (Maked == null)
            {
                context.Dispose();
                return;
            }
            Maked(context);
        }

        public void Dispose()
        {
            factory.Dispose();
        }

        public void Run()
        {
            factory.Run();
        }
    }
}

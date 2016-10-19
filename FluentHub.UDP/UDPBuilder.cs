using FluentHub.Hub;
using FluentHub.IO;
using FluentHub.IO.Extension;
using FluentHub.Logger;
using FluentHub.UDP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub
{
    public static class UDPBuilder
    {
        public static IAppBuilder<T, FakeStream> MakeAppByUdp<T>(
            this ContainerBootstrap @this
            , string host
            , int sendPort
            , int recvPort)
        {
            return
                @this.MakeApp<T, FakeStream>(
                    new UDPFactory(IPAddress.Parse(host), sendPort, recvPort)
                    , x => x.BuildContextByStream());
        }

        public static IAppBuilder<T, FakeStream> MakeAppByUdps<T>(
           this ContainerBootstrap @this
           , IEnumerable<Tuple<string, int, int>> serverInfos)
        {
            return
                @this.MakeApp<T, FakeStream>(
                    serverInfos.Select(ci => 
                        new UDPFactory(IPAddress.Parse(ci.Item1), ci.Item2, ci.Item3)).ToArray()
                    , client => client.BuildContextByStream());
        }
    }
}

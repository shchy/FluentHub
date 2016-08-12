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
        public static IAppBuilder<T> MakeAppByUdp<T>(
            this ContainerBootstrap @this
            , string host
            , int sendPort
            , int recvPort)
        {
            var appBuilder = new AppBuilder<T, Stream>();
            appBuilder.Logger = @this.Logger;
            appBuilder.DependencyContainer = @this.DependencyContainer;
            @this.Builders.Add(appBuilder);

            appBuilder.NativeIOFactory = new UDPFactory(host, sendPort, recvPort);
            appBuilder.NativeToStreamContext = (Stream x) => x.BuildContextByStream();

            return
                appBuilder;
        }        
    }
}

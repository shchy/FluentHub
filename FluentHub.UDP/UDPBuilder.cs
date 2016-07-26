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

namespace FluentHub.Hub
{
    public static class UDPBuilder
    {
        public static IContextApplication<T> MakeAppByUdp<T>(
            this IApplicationContainer @this
            , IEnumerable<IModelConverter<T>> converters
            , string host
            , int sendPort
            , int recvPort)
        {
            return
                @this.MakeApp(
                    converters
                    , new NativeIOToContextMaker<Stream>(
                        new UDPFactory(host, sendPort, recvPort)
                        , (Stream x) => x.BuildContextByStream()
                        , c => c.Close()));
        }        
    }
}

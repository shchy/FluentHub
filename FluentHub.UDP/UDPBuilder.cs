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
            , string host
            , int sendPort
            , int recvPort
            , Func<object, ISession> makeSession = null)
        {
            return
                @this.MakeApp<T>(
                    new ModelContextFactory<T,Stream>(
                        new UDPFactory(host, sendPort, recvPort)
                        , (Stream x) => x.BuildContextByStream()
                        , new SuspendedDisposalSource(1000) // todo 変更方法を考える
                        , @this.Logger), makeSession);
        }        
    }
}

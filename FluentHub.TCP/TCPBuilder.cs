using FluentHub.Hub;
using FluentHub.IO;
using FluentHub.IO.Extension;
using FluentHub.Logger;
using FluentHub.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public static class TCPBuilder
    {
        /// <summary>
        /// TCPServerアプリケーションを生成して追加する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static IContextApplication<T> MakeAppByTcpServer<T>(
            this IApplicationContainer @this
            , Func<object, ISession> makeSession
            , params int[] ports)
        {
            return
                @this.MakeApp<T>(
                    new ModelContextFactory<T,TcpClient>(
                        new TcpServerFactory(ports)
                        , (TcpClient client) => client.BuildContextByTcp()
                        , new SuspendedDisposalSource(1000) // todo 変更方法を考える
                        , @this.Logger
                        ), makeSession);
        }

        public static IContextApplication<T> MakeAppByTcpServer<T>(
            this IApplicationContainer @this
            , params int[] ports)
        {
            return
                @this.MakeAppByTcpServer<T>(null, ports);
        }

        public static IContextApplication<T> MakeAppByTcpClient<T>(
            this IApplicationContainer @this
            , string host
            , int port
            , Func<object, ISession> makeSession = null)
        {
            return
               @this.MakeApp<T>(
                   new ModelContextFactory<T,TcpClient>(
                        new TcpClientFactory(host, port)
                        , (TcpClient client) => client.BuildContextByTcp()
                        , new SuspendedDisposalSource(1000) // todo 変更方法を考える
                        , @this.Logger
                        ), makeSession);
        }

        public static IIOContext<byte[]> BuildContextByTcp(
           this TcpClient @this)
        {
            return
                new StreamContext(
                    @this.GetStream()
                    , () => @this.Close());
        }

    }
}

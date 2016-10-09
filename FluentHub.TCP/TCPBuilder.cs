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

namespace FluentHub
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
        public static IAppBuilder<T, TcpClient> MakeAppByTcpServer<T>(
            this ContainerBootstrap @this
            , params int[] ports)
        {
            return
                @this.MakeApp<T, TcpClient>(
                    new TcpServerFactory(ports)
                    , (TcpClient client) => client.BuildContextByTcp());
        }

        public static IAppBuilder<T, TcpClient> MakeAppByTcpClient<T>(
            this ContainerBootstrap @this
            , string host
            , int port)
        {
            return
                @this.MakeApp<T, TcpClient>(
                    new TcpClientFactory(host, port)
                    , client => client.BuildContextByTcp());
        }

        public static IAppBuilder<T, TcpClient> MakeAppByTcpClientDual<T>(
            this ContainerBootstrap @this
            , string host
            , int port
            , string secondaryHost
            , int secondaryPort
            , int switchMillisecond)
        {
            var primaryFactory = new TcpClientFactory(host, port);
            var secondaryFactory = new TcpClientFactory(secondaryHost, secondaryPort);
            return
                @this.MakeApp<T, TcpClient>(
                    primaryFactory
                    , secondaryFactory
                    , switchMillisecond
                    , client => client.BuildContextByTcp());
        }

        public static IAppBuilder<T, TcpClient> MakeAppByTcpClients<T>(
            this ContainerBootstrap @this
            , IEnumerable<Tuple<string, int>> serverInfos)
        {
            return
                @this.MakeApp<T, TcpClient>(
                    serverInfos.Select(ci => new TcpClientFactory(ci.Item1, ci.Item2)).ToArray()
                    , client => client.BuildContextByTcp());
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

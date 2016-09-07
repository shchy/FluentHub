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
        public static IAppBuilder<T> MakeAppByTcpServer<T>(
            this ContainerBootstrap @this
            , params int[] ports)
        {
            var appBuilder = new AppBuilder<T, TcpClient>();
            appBuilder.Logger = @this.Logger;
            appBuilder.DependencyContainer = @this.DependencyContainer;
            @this.Builders.Add(appBuilder);

            appBuilder.NativeIOFactory = new TcpServerFactory(ports);
            appBuilder.NativeToStreamContext = (TcpClient client) => client.BuildContextByTcp();

            return
                appBuilder;
        }

        public static IAppBuilder<T> MakeAppByTcpClient<T>(
            this ContainerBootstrap @this
            , string host
            , int port)
        {
            var appBuilder = new AppBuilder<T, TcpClient>();
            appBuilder.Logger = @this.Logger;
            appBuilder.DependencyContainer = @this.DependencyContainer;
            @this.Builders.Add(appBuilder);

            appBuilder.NativeIOFactory = new TcpClientFactory(host, port);
            appBuilder.NativeToStreamContext = (TcpClient client) => client.BuildContextByTcp();

            return
                appBuilder;
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

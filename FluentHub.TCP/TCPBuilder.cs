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
            var appBuilder = 
                new AppBuilder<T, TcpClient>(
                    @this.Logger
                    , @this.DependencyContainer
                    , new TcpServerFactory(ports));
            appBuilder.NativeToStreamContext = (TcpClient client) => client.BuildContextByTcp();
            @this.AppBuilders.Add(appBuilder);
            return
                appBuilder;
        }

        public static IAppBuilder<T, TcpClient> MakeAppByTcpClient<T>(
            this ContainerBootstrap @this
            , string host
            , int port)
        {
            var factory = new TcpClientFactory(host, port);
            return
                MakeAppByTcpClient<T>(@this, factory );
        }

        public static IAppBuilder<T, TcpClient> MakeAppByTcpClient<T>(
            this ContainerBootstrap @this
            , string host
            , int port
            , string secondaryHost
            , int secondaryPort
            , int switchMillisecond)
        {
            var primaryFactory = new TcpClientFactory(host, port);
            var secondaryFactory = new TcpClientFactory(secondaryHost, secondaryPort);
            var dual = new DualNativeIOFactory<TcpClient>(primaryFactory, secondaryFactory, switchMillisecond);
            return
                MakeAppByTcpClient<T>(@this, dual);
        }

        static IAppBuilder<T, TcpClient> MakeAppByTcpClient<T>(
            ContainerBootstrap @this
            , INativeIOFactory<TcpClient> nativeFactory)
        {
            var appBuilder = 
                new AppBuilder<T, TcpClient>(
                    @this.Logger
                    , @this.DependencyContainer
                    , nativeFactory);
            appBuilder.NativeToStreamContext = (TcpClient client) => client.BuildContextByTcp();
            @this.AppBuilders.Add(appBuilder);

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

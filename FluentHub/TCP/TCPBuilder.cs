using FluentHub.Hub;
using FluentHub.IO;
using FluentHub.IO.Extension;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.TCP
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
            , IEnumerable<IModelConverter<T>> converters
            , int port)
        {
            return
                @this.MakeApp(
                    converters
                    , new TCPContextFactory(new TcpServerFactory(port)));
        }

        public static IContextApplication<T> MakeAppByTcpClient<T>(
            this IApplicationContainer @this
            , IEnumerable<IModelConverter<T>> converters
            , string host
            , int port)
        {
            return
               @this.MakeApp(
                   converters
                   , new TCPContextFactory(new TcpClientFactory(host, port)));
        }

        public static IIOContext<byte> BuildContextByTcp(
           this TcpClient @this)
        {
            return
                new StreamContext(
                    @this.GetStream()
                    , () => @this.Close());
        }

    }
}

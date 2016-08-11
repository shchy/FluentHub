using FluentHub;
using FluentHub.Hub;
using FluentHub.IO;
using FluentHub.ModelConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Test00
{
    public class Test
    {
        private bool isServer;

        public Test(bool isServer)
        {
            this.isServer = isServer;
        }

        public void Run(string[] args)
        {
            var appContainer = MakeApps(this.isServer);

            Task.Run((Action)appContainer.Run);

            Controller(appContainer.Container);
        }

        public static ContainerBootstrap MakeApps(bool isServer)
        {
            // アプリケーションコンテナの生成
            var bootstrap = new ContainerBootstrap();// as IApplicationContainer;
            var logger = bootstrap.Logger;

            // TCPサーバーアプリケーションAを立てる
            var appA =
                MakeAppUDP<IModelMessageA>(bootstrap, isServer, 1244)
                .RegisterConverter(new AMessage0Converter())
                .RegisterConverter(new AMessage1Converter())
                .RegisterConverter<IModelMessageA, AMessage2>();
            
            // TCPサーバーアプリケーションBを立てる
            var appB =
                MakeAppTCP<IModelMessageB>(bootstrap, isServer, 1245)
                .RegisterConverter<IModelMessageB, BMessage0>()
                .RegisterConverter<IModelMessageB, BMessage1>();
            
            // シーケンスの登録
            bootstrap
                .RegisterSequence((IIOContext<IModelMessageA> context) => logger.Info($"Aから何かを受信"))
                // サーバーとクライアントの1:1シーケンスの登録(型指定)
                .RegisterSequence((IIOContext<IModelMessageA> context, AMessage0 model) =>
                {
                    logger.Info($"{model.GetType().Name}を受信");
                    context.Write(new AMessage1());
                })
                .RegisterSequence((IIOContext<IModelMessageB> context) => logger.Info($"Bから何かを受信"))
                // サーバーとクライアントの1:1シーケンスの登録(型指定)
                .RegisterSequence((IIOContext<IModelMessageB> context, BMessage0 model) =>
                {
                    logger.Info($"{model.GetType().Name}を受信");
                    context.Write(new BMessage1());
                })
                // シーケンスの登録
                .RegisterSequence((IIOContext<IModelMessageA> context, AMessage2 _, IEnumerable<IIOContext<IModelMessageB>> ts) =>
                {
                    foreach (var t in ts)
                    {
                        t.Write(new BMessage0());
                        var res = t.Read(m => m is BMessage1, 50 * 1000);
                        logger.Info($"{res.GetType().Name}を受信");
                    }
                });
            return bootstrap;
        }

        private static IAppBuilder<T> MakeAppTCP<T>(ContainerBootstrap container, bool isServer, int v)
        {
            Func<IAppBuilder<T>> f = null;
            // TCP
            f =
                isServer
                ? (Func<IAppBuilder<T>>)(() => container.MakeAppByTcpServer<T>(v) as IAppBuilder<T>)
                : () => container.MakeAppByTcpClient<T>("localhost", v);
            return f();

        }

        private static IAppBuilder<T> MakeAppUDP<T>(ContainerBootstrap container, bool isServer, int v)
        {
            Func<IAppBuilder<T>> f = null;
            // UDP
            var p1 = v;
            var p2 = v - 100;
            if (!isServer)
            {
                var a = p1;
                p1 = p2;
                p2 = a;
            }
            f = () => container.MakeAppByUdp<T>("localhost", p1, p2);
            return f();
        }
        private static IAppBuilder<T> MakeAppSerial<T>(ContainerBootstrap container, bool isServer, int v)
        {
            Func<IAppBuilder<T>> f = null;
            // SerialPort
            var comName = isServer ? $"COM{v}" : $"COM{v + 1}";
            f = () => container.MakeAppBySerialPort<T>(comName, 2400, Parity.None, 8, StopBits.One);
            return f();
        }

        public static void Controller(IApplicationContainer appContainer)
        {
            var logger = appContainer.Logger;
            // Ctrl+Cで終了
            while (true)
            {
                try
                {
                    var line = Console.ReadLine();
                    switch (line)
                    {
                        // Aクライアントにメッセージ0を送信
                        case "a":
                            appContainer
                                .GetApp<IModelMessageA>()
                                .InstantSequence((IEnumerable<IIOContext<IModelMessageA>> cx) =>
                                {
                                    foreach (var c in cx)
                                    {
                                        c.Write(new AMessage0());
                                        var res = c.Read(m => m is AMessage1,  1000);
                                        logger.Info($"{res.GetType().Name}を受信");
                                    }
                                });
                            break;
                        // Bクライアントにメッセージ0を送信
                        case "b":
                            appContainer
                                .GetApp<IModelMessageB>()
                                .InstantSequence((IEnumerable<IIOContext<IModelMessageB>> cx) =>
                                {
                                    foreach (var c in cx)
                                    {
                                        c.Write(new BMessage0());
                                        var res = c.Read(m => m is BMessage1, 1000);
                                        logger.Info($"{res.GetType().Name}を受信");
                                    }
                                });
                            break;
                        // Aクライアントにメッセージ2を送信
                        case "ac":
                            appContainer
                                .GetApp<IModelMessageA>()
                                .InstantSequence((IEnumerable<IIOContext<IModelMessageA>> cx) =>
                                {
                                    foreach (var c in cx)
                                    {
                                        c.Write(new AMessage2());
                                    }
                                });
                            break;
                        case "x":
                            appContainer.Dispose();
                            return;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                }
            }
        }

    }


    #region 通信相手A用の電文モデル

    public interface IModelMessageA
    {
        int ID { get; set; }
    }
    [Serializable]
    public class AMessage0 : IModelMessageA
    {
        public int ID { get; set; } = 0x00;
        public int Foo { get; set; }
    }
    [Serializable]
    public class AMessage1 : IModelMessageA
    {
        public int ID { get; set; } = 0x01;
        public int Bar { get; set; }
    }
    [Serializable]
    public class AMessage2 : IModelMessageA
    {
        public int ID { get; set; } = 0x02;
        public int Bar { get; set; }
    }

    public class AMessage0Converter : WrapperModelConverter<IModelMessageA, AMessage0>
    {
        protected override IModelConverter<AMessage0> MakeConverter()
        {
            return
                new AMessage0().ToModelBuilder()
                .ToBigEndian()
                .Constant(0x00)
                .Property(m => m.Foo)
                .ToConverter();
        }
    }
    public class AMessage1Converter : WrapperModelConverter<IModelMessageA, AMessage1>
    {
        protected override IModelConverter<AMessage1> MakeConverter()
        {
            return
                new AMessage1().ToModelBuilder()
                .ToBigEndian()
                .Constant(0x01)
                .Property(m => m.Bar)
                .ToConverter();

        }
    }
    public class AMessage2Converter : IModelConverter<IModelMessageA>
    {
        public bool CanBytesToModel(IEnumerable<byte> bytes)
        {
            var header = bytes.Take(MESSAGESIZE).ToArray();
            if (header.Length != MESSAGESIZE)
            {
                return false;
            }

            var id = header[0];
            return id == 0x02;
        }

        public bool CanModelToBytes(object model)
        {
            return model is AMessage2;
        }

        const int MESSAGESIZE = 8;
        public byte[] ToBytes(IModelMessageA model)
        {
            var m = model as AMessage2;
            using (var ms = new MemoryStream(MESSAGESIZE))
            using (var w = new BinaryWriter(ms))
            {
                w.Write(m.ID);
                w.Write(m.Bar);
                w.Flush();
                return ms.ToArray();
            }
        }

        public Tuple<IModelMessageA, int> ToModel(IEnumerable<byte> bytes)
        {
            var data = bytes.ToArray();
            if (data.Length < MESSAGESIZE)
            {
                return Tuple.Create(default(IModelMessageA), 0);
            }

            using (var ms = new MemoryStream(data))
            using (var r = new BinaryReader(ms))
            {
                var model = new AMessage2();
                model.ID = r.ReadInt32();
                model.Bar = r.ReadInt32();
                return
                    Tuple.Create(model as IModelMessageA, MESSAGESIZE);
            }
        }

    }
    #endregion

    #region 通信相手B用の電文モデル
    public interface IModelMessageB
    {
        int ID { get; set; }
    }
    [Serializable]
    public class BMessage0 : IModelMessageB
    {
        public int ID { get; set; } = 0x00;
        public int Hoge { get; set; }
    }
    [Serializable]
    public class BMessage1 : IModelMessageB
    {
        public int ID { get; set; } = 0x01;
        public int Fuga { get; set; }
    }

    public class BMessage0Converter : IModelConverter<IModelMessageB>
    {
        public bool CanBytesToModel(IEnumerable<byte> bytes)
        {
            var header = bytes.Take(MESSAGESIZE).ToArray();
            if (header.Length != MESSAGESIZE)
            {
                return false;
            }

            var id = header[0];
            return id == 0x00;
        }

        public bool CanModelToBytes(object model)
        {
            return model is BMessage0;
        }

        const int MESSAGESIZE = 8;
        public byte[] ToBytes(IModelMessageB model)
        {
            var m = model as BMessage0;
            using (var ms = new MemoryStream(MESSAGESIZE))
            using (var w = new BinaryWriter(ms))
            {
                w.Write(m.ID);
                w.Write(m.Hoge);
                w.Flush();
                return ms.ToArray();
            }
        }

        public Tuple<IModelMessageB, int> ToModel(IEnumerable<byte> bytes)
        {
            var data = bytes.ToArray();
            if (data.Length < MESSAGESIZE)
            {
                return Tuple.Create(default(IModelMessageB), 0);
            }

            using (var ms = new MemoryStream(data))
            using (var r = new BinaryReader(ms))
            {
                var model = new BMessage0();
                model.ID = r.ReadInt32();
                model.Hoge = r.ReadInt32();
                return
                    Tuple.Create(model as IModelMessageB, MESSAGESIZE);
            }
        }
    }

    public class BMessage1Converter : IModelConverter<IModelMessageB>
    {
        public bool CanBytesToModel(IEnumerable<byte> bytes)
        {
            var header = bytes.Take(MESSAGESIZE).ToArray();
            if (header.Length != MESSAGESIZE)
            {
                return false;
            }

            var id = header[0];
            return id == 0x01;
        }
        public bool CanModelToBytes(object model)
        {
            return model is BMessage1;
        }

        const int MESSAGESIZE = 8;
        public byte[] ToBytes(IModelMessageB model)
        {
            var m = model as BMessage1;
            using (var ms = new MemoryStream(MESSAGESIZE))
            using (var w = new BinaryWriter(ms))
            {
                w.Write(m.ID);
                w.Write(m.Fuga);
                w.Flush();
                return ms.ToArray();
            }
        }

        public Tuple<IModelMessageB, int> ToModel(IEnumerable<byte> bytes)
        {
            var data = bytes.ToArray();
            if (data.Length < MESSAGESIZE)
            {
                return Tuple.Create(default(IModelMessageB), 0);
            }

            using (var ms = new MemoryStream(data))
            using (var r = new BinaryReader(ms))
            {
                var model = new BMessage1();
                model.ID = r.ReadInt32();
                model.Fuga = r.ReadInt32();
                return
                    Tuple.Create(model as IModelMessageB, MESSAGESIZE);
            }
        }
    }
    #endregion

}

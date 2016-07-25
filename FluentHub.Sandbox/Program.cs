using FluentHub.Hub;
using FluentHub.Hub.TCP;
using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.Sandbox
{
    public class Program
    {
        static void Main(string[] args)
        {

            var logger = new DebugLogger();

            var appContainer = MakeApps(logger, true);

            Task.Run((Action)appContainer.Run);

            //// サーバーから開始するシーケンスの実行（3者間）
            //var result = appContainer
            //    .InstantSequence((IEnumerable<IIOContext<IModelMessageA>> xs, IEnumerable<IIOContext<IModelMessageB>> ys) =>
            //    {
            //        return 1;
            //    });

            Controller(appContainer,logger);
        }

        public static void Controller(IApplicationContainer appContainer, ILogger logger)
        {
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
                                .InstantSequence(cx =>
                                {
                                    foreach (var c in cx)
                                    {
                                        c.Write(new AMessage0());
                                        var res = c.Read(m => m is AMessage1, 50 * 1000);
                                        logger.Info($"{res.GetType().Name}を受信");
                                    }
                                });
                            break;
                        // Bクライアントにメッセージ0を送信
                        case "b":
                            appContainer
                                .GetApp<IModelMessageB>()
                                .InstantSequence(cx =>
                                {
                                    foreach (var c in cx)
                                    {
                                        c.Write(new BMessage0());
                                        var res = c.Read(m => m is BMessage1, 50 * 1000);
                                        logger.Info($"{res.GetType().Name}を受信");
                                    }
                                });
                            break;
                        // Aクライアントにメッセージ2を送信
                        case "ac":
                            appContainer
                                .GetApp<IModelMessageA>()
                                .InstantSequence(cx =>
                                {
                                    foreach (var c in cx)
                                    {
                                        c.Write(new AMessage2());
                                    }
                                });
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception )
                {
                }
            }
        }

        public static IApplicationContainer MakeApps(ILogger logger, bool isServer)
        {
            // アプリケーションコンテナの生成
            var appContainer = new ApplicationContainer(logger) as IApplicationContainer;

            var messageConvertersA = new IModelConverter<IModelMessageA>[] {
                    new AMessage0Converter(), new AMessage1Converter(), new AMessage2Converter()
                };


            // TCPサーバーアプリケーションAを立てる
            var appA = MakeApp(appContainer, isServer, 1244, messageConvertersA);
                
                
            // サーバーとクライアントの1:1シーケンスの登録
            appA.RegisterSequence((IIOContext<IModelMessageA> context) => logger.Info($"Aから何かを受信"))
                // サーバーとクライアントの1:1シーケンスの登録(型指定)
                .RegisterSequence((IIOContext<IModelMessageA> context, AMessage0 model) =>
                {
                    logger.Info($"{model.GetType().Name}を受信");
                    context.Write(new AMessage1());
                });


            var messageConvertersB = new IModelConverter<IModelMessageB>[] {
                    new BMessage0Converter(), new BMessage1Converter()
                };
            // TCPサーバーアプリケーションBを立てる
            var appB = MakeApp(appContainer, isServer, 1245, messageConvertersB);

            // サーバーとクライアントの1:1シーケンスの登録
            appB.RegisterSequence((IIOContext<IModelMessageB> context) => logger.Info($"Bから何かを受信"))
                // サーバーとクライアントの1:1シーケンスの登録(型指定)
                .RegisterSequence((IIOContext<IModelMessageB> context, BMessage0 model) =>
                {
                    logger.Info($"{model.GetType().Name}を受信");
                    context.Write(new BMessage1());
                });

            // 3者間シーケンスの登録
            appContainer
                // シーケンスの登録
                .RegisterSequence((IModelMessageA _, IEnumerable<IIOContext<IModelMessageB>> ts) =>
                {
                    foreach (var t in ts)
                    {
                        t.Write(new BMessage0());
                        var res = t.Read(m => m is BMessage1, 50*1000);
                        logger.Info($"{res.GetType().Name}を受信");
                    }
                });
            return appContainer;
        }

        private static IContextApplication<T> MakeApp<T>(IApplicationContainer container, bool isServer, int v, IModelConverter<T>[] messageConverters)
        {
            var f = 
                isServer
                ? (Func<IContextApplication<T>>)(() => container.MakeAppByTcpServer(v, messageConverters) as IContextApplication<T>)
                : () => container.MakeAppByTcpClient("localhost", v, messageConverters);

            return f();

        }
    }


    #region 通信相手A用の電文モデル

    public interface IModelMessageA
    {
        int ID { get; set; }
    }

    public class AMessage0 : IModelMessageA
    {
        public int ID { get; set; } = 0x00;
        public int Foo { get; set; }
    }

    public class AMessage1 : IModelMessageA
    {
        public int ID { get; set; } = 0x01;
        public int Bar { get; set; }
    }
    public class AMessage2 : IModelMessageA
    {
        public int ID { get; set; } = 0x02;
        public int Bar { get; set; }
    }
    public class AMessage0Converter : IModelConverter<IModelMessageA>
    {
        const int MESSAGESIZE = 8;

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
            return model is AMessage0;
        }

        public byte[] ToBytes(IModelMessageA model)
        {
            var m = model as AMessage0;
            using (var ms = new MemoryStream(MESSAGESIZE))
            using (var w = new BinaryWriter(ms))
            {
                w.Write(m.ID);
                w.Write(m.Foo);
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
                var model = new AMessage0();
                model.ID = r.ReadInt32();
                model.Foo = r.ReadInt32();
                return 
                    Tuple.Create(model as IModelMessageA, MESSAGESIZE);
            }
        }
    }
    public class AMessage1Converter : IModelConverter<IModelMessageA>
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
            return model is AMessage1;
        }

        const int MESSAGESIZE = 8;
        public byte[] ToBytes(IModelMessageA model)
        {
            var m = model as AMessage1;
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
                var model = new AMessage1();
                model.ID = r.ReadInt32();
                model.Bar = r.ReadInt32();
                return
                    Tuple.Create(model as IModelMessageA, MESSAGESIZE);
            }
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

    public class BMessage0 : IModelMessageB
    {
        public int ID { get; set; } = 0x00;
        public int Hoge { get; set; }
    }

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

    public class DebugLogger : ILogger
    {
        public void Debug(string message)
        {
            Log("D", message);
        }

        public void Exception(Exception ex)
        {
            Log("E", ex.Message);
        }

        public void Info(string message)
        {
            Log("I", message);
        }

        public void Warn(string message)
        {
            Log("W", message);
        }

        private void Log(string type, string message)
        {
            Console.WriteLine($"{type} {message}");
        }
    }
}

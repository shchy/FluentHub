using FluentHub.Hub;
using FluentHub.TCP;
using FluentHub.Serial;
using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentHub.UDP;
using System.IO.Ports;
using FluentHub.ModelConverter;
using System.Linq.Expressions;

namespace FluentHub.Sandbox
{
    public class Program
    {
        static void Main(string[] args)
        {
            var a = new TestModel()
                .ToModelBuilder()
                // byte -> model の時に生成するモデルのメンバがnullだと困るのでここで初期化などしてもらう
                .Init(m => m.InnerModel = new InnerModel())
                // 定数値電文の識別子など
                .Constant(0x03) 
                .Constant(0x99)
                // padding
                .Constant(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })
                // メンバアクセス
                .Property(m => m.Value) 
                // メンバアクセスがチェーンになっていても復元するよ
                .Property(m => m.InnerModel.A)
                // 配列の数が電文に含まれてたりするよね。書き込むときはメンバの値を書けばいいけど復元する時は読むだけでいいよね。っていうときはGetProperty。
                // そして読んだ値を覚えておきたいよねって時にAsTag
                .GetProperty(m => m.Array.Count()).AsTag("InnerCount")
                // さらにN個分配列を復元するときのNに割り当てたいよね
                .Array("InnerCount", m => m.Array, b => b.Property(mi => mi.A))
                // 固定長の配列もあるよね
                .FixedArray(5, m => m.FixedArray, b => b.Property(mi => mi.A))
                // InnerClass Builder
                .Property(m => m.InnerModel2, b => b.Property(mi=>mi.A))
                .ToConverter();
            var t = new TestModel();
            t.Value = 0x07;
            t.InnerModel = new InnerModel();
            t.InnerModel.A = 0x08;
            t.Array = new List<InnerModel>
            {
                new InnerModel { A = 0x09 },
                new InnerModel { A = 0x0A },
                new InnerModel { A = 0x0B },
                new InnerModel { A = 0x0C },
            };
            t.InnerModel2 = new InnerModel() { A = 0x0D };

            var data = a.ToBytes(t);
            var tt = a.ToModel(data);


            var appContainer = MakeApps(true);

            Task.Run((Action)appContainer.Run);
            
            Controller(appContainer);
        }

        public static Expression<Func<T1, TR>> Expression<T1, TR>(Expression<Func<T1, TR>> e)
        {
            return e;
        }


        public static IApplicationContainer MakeApps(bool isServer)
        {
            //var messageConvertersA = new IModelConverter<IModelMessageA>[] {
            //        new AMessage0Converter(), new AMessage1Converter(), new AMessage2Converter()
            //    };
            //var messageConvertersB = new IModelConverter<IModelMessageB>[] {
            //        new BMessage0Converter(), new BMessage1Converter()
            //    };
            
            // アプリケーションコンテナの生成
            var appContainer = new ApplicationContainer() as IApplicationContainer;
            var logger = appContainer.Logger;

            // TCPサーバーアプリケーションAを立てる
            var appA = 
                MakeAppUDP<IModelMessageA>(appContainer, isServer, 1244)
                .RegisterConverter( new AMessage0Converter())
                .RegisterConverter( new AMessage1Converter())
                .RegisterConverter<IModelMessageA, AMessage2>()
                .RegisterSequence((IIOContext<IModelMessageA> context) => logger.Info($"Aから何かを受信"))
                // サーバーとクライアントの1:1シーケンスの登録(型指定)
                .RegisterSequence((IIOContext<IModelMessageA> context, AMessage0 model) =>
                {
                    logger.Info($"{model.GetType().Name}を受信");
                    context.Write(new AMessage1());
                });

            // TCPサーバーアプリケーションBを立てる
            var appB = 
                MakeAppTCP<IModelMessageB>(appContainer, isServer, 1245)
                .RegisterConverter<IModelMessageB, BMessage0>()
                .RegisterConverter<IModelMessageB, BMessage1>()
                .RegisterSequence((IIOContext<IModelMessageB> context) => logger.Info($"Bから何かを受信"))
                // サーバーとクライアントの1:1シーケンスの登録(型指定)
                .RegisterSequence((IIOContext<IModelMessageB> context, BMessage0 model) =>
                {
                    logger.Info($"{model.GetType().Name}を受信");
                    context.Write(new BMessage1());
                });

            // 3者間シーケンスの登録
            appContainer
                // シーケンスの登録
                .RegisterSequence((IIOContext<IModelMessageA> context, AMessage2 _, IEnumerable<IIOContext<IModelMessageB>> ts) =>
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

        private static IContextApplication<T> MakeAppTCP<T>(IApplicationContainer container, bool isServer, int v)
        {
            Func<IContextApplication<T>> f = null;
            // TCP
            f =
                isServer
                ? (Func<IContextApplication<T>>)(() => container.MakeAppByTcpServer<T>(v) as IContextApplication<T>)
                : () => container.MakeAppByTcpClient<T>("localhost", v);
            return f();

        }

        private static IContextApplication<T> MakeAppUDP<T>(IApplicationContainer container, bool isServer, int v)
        {
            Func<IContextApplication<T>> f = null;
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
        private static IContextApplication<T> MakeAppSerial<T>(IApplicationContainer container, bool isServer, int v)
        {
            Func<IContextApplication<T>> f = null;
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

    public class TestModel : IModelMessageA
    {
        public int ID { get; set; } = 0x03;
        public int GetOnly { get; } = 0x99;
        public int Value { get; set; }
        public IInnerModel InnerModel { get; set; }
        public IEnumerable<InnerModel> Array { get; set; }
        public InnerModel[] FixedArray { get; set; }
        public InnerModel InnerModel2 { get; set; }
    }

    public interface IInnerModel
    {
        int A { get; set; }
    }

    public class InnerModel : IInnerModel
    {
        public int A { get; set; }

    }

    public class AMessage0Converter : WrapperModelConverter<IModelMessageA>
    {
        protected override IModelConverter<IModelMessageA> MakeConverter()
        {
            return
                new AMessage0().ToModelBuilder()
                .ToBigEndian()
                .Constant(0x00)
                .Property(m => m.Foo)
                .ToConverter()
                .ToBaseTypeConverter<AMessage0, IModelMessageA>();

        }
    }
    public class AMessage1Converter : WrapperModelConverter<IModelMessageA>
    {
        protected override IModelConverter<IModelMessageA> MakeConverter()
        {
            return
                new AMessage1().ToModelBuilder()
                .ToBigEndian()
                .Constant(0x01)
                .Property(m => m.Bar)
                .ToConverter()
                .ToBaseTypeConverter<AMessage1, IModelMessageA>();

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

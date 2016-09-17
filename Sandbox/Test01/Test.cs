using FluentHub;
using FluentHub.Hub;
using FluentHub.IO;
using FluentHub.IO.Extension;
using FluentHub.Logger;
using FluentHub.ModelConverter;
using FluentHub.Unity;
using FluentHub.Validation;
using FluentValidation;
using Microsoft.Practices.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.Test01
{
    public class DebugSession : ISession
    {
        public object NativeIO { get; set; }
        public string Test { get; set; }
    }

    class PingValidator : ModelValidator<Ping, IPingPongAppMessage>
    {
        public PingValidator()
        {
            RuleFor(m => m.ID).Equal((byte)0x01);
        }
    }

    public class TestServer
    {
        public void Run(string[] args)
        {
            using (var lifeTime = new ContainerControlledLifetimeManager())
            {
                // Unity使う版
                var unityContainer = new UnityContainer();
                // ModuleをUnityに登録しておく
                unityContainer.RegisterType<ServerApp>(lifeTime);

                var bootstrap = new ContainerBootstrap();
                bootstrap.DependencyContainer = new UnityModuleDependencyContainer(unityContainer);
                var app =
                    // 待ち受けポートは8089
                    bootstrap.MakeAppByTcpServer<IPingPongAppMessage>(8089,8090)
                    // Ping電文のbyte[] <=> Model変換定義
                    .RegisterConverter(new PingModelConverter())
                    // Pong電文のbyte[] <=> Model変換定義
                    .RegisterConverter(new PongModelConverter())
                    // Tunnel電文のbyte[] <=> Model変換定義
                    .RegisterConverter(new TunnelModelConverter())
                    .RegisterSession(nativeIO => new DebugSession { NativeIO = nativeIO })
                    .RegisterValidator(new PingValidator());

                // 異なるプロトコルを持つ第3者通信相手を定義
                var thirdApp =
                    bootstrap.MakeAppByTcpServer<IThirdAppMessage>(8099)
                    .RegisterConverter(new PangModelConverter())
                    .RegisterSession(x => new DebugSession { NativeIO = x });

                // シーケンスモジュールを直接登録するスタイル
                bootstrap.RegisterModule<ServerApp>();

                bootstrap.Run();
            }
        }
    }

    public class TestClient
    {
        public void Run(string[] args)
        {
            // アプリケーションコンテナ
            var bootstrap = new ContainerBootstrap();
            // IPingPongAppMessage型の電文をやり取りするサーバーアプリケーションを生成
            var app =
                // 待ち受けポートは8089
                bootstrap.MakeAppByTcpClient<IPingPongAppMessage>("localhost", 8089, "localhost", 8090, 1000 * 60)
                // Ping電文のbyte[] <=> Model変換定義
                .RegisterConverter(new PingModelConverter())
                // Pong電文のbyte[] <=> Model変換定義
                .RegisterConverter(new PongModelConverter())
                // Tunnel電文のbyte[] <=> Model変換定義
                .RegisterConverter(new TunnelModelConverter())
                .RegisterConverter(new GomiModelConverter())
                .RegisterInitializeSequence(( IIOContext<IPingPongAppMessage> c) => PingPongSequence(c, bootstrap.Logger));


            Task.Run((Action)bootstrap.Run);

            while (true)
            {
                // Enter to send Ping
                var line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    // サーバーにPingメッセージを送信
                    bootstrap.Container.GetApp<IPingPongAppMessage>().InstantSequence((IEnumerable<IIOContext<IPingPongAppMessage>> contexts) =>
                    {
                        PingPongSequence(contexts.First(), bootstrap.Logger);
                    });
                }
                else
                {
                    // サーバーにPingメッセージを送信
                    bootstrap.Container.GetApp<IPingPongAppMessage>().InstantSequence(((IEnumerable<IIOContext<IPingPongAppMessage>> contexts) =>
                    {
                        var server = contexts.FirstOrDefault();
                        if (server == null)
                        {
                            return;
                        }
                        // 送信
                        server.Write(new Gomi());
                    }));
                }
            }
        }

        private void PingPongSequence(IIOContext<IPingPongAppMessage> server, ILogger logger)
        {
            if (server == null)
            {
                return;
            }
            // 送信
            server.Write(new Ping());
            // Pongを受信するまで10秒待機
            var pong = server.Read(m => m is Pong, 1000 * 10);

            if (pong == null)
            {
                logger.Debug("pong not recv");
                return;
            }

            // send Tunnel
            server.Write(new Tunnel());

            // Pongを受信するまで10秒待機
            var pong2 = server.Read(m => m is Pong, 1000 * 10);

            if (pong2 == null)
            {
                logger.Debug("pong2 not recv");
                return;
            }
        }
    }

    public class TestOtherClient
    {
        public void Run(string[] args)
        {
            // アプリケーションコンテナ
            var bootstrap = new ContainerBootstrap();
            // IPingPongAppMessage型の電文をやり取りするサーバーアプリケーションを生成
            var app =
                // 待ち受けポートは8089
                bootstrap.MakeAppByTcpClient<IThirdAppMessage>("localhost", 8099)
                // Ping電文のbyte[] <=> Model変換定義
                .RegisterConverter(new PangModelConverter());
            bootstrap
                .RegisterSequence((IIOContext<IThirdAppMessage> context, Pang model) =>
                {
                    bootstrap.Logger.Debug("Pang!");
                });

            Task.Run((Action)bootstrap.Run);

            Console.ReadLine();
        }
    }

    // IPingPongAppMessageアプリケーションプロトコル
    public interface IPingPongAppMessage { }

    public class Ping : IPingPongAppMessage
    {
        public byte ID { get; set; } = 0x01;
    }

    public class Pong : IPingPongAppMessage
    {
        public byte ID { get; set; } = 0x02;
    }

    public class Tunnel : IPingPongAppMessage
    {
        public byte ID { get; set; } = 0x03;
    }

    public class Gomi : IPingPongAppMessage
    {
        public byte ID { get; set; } = 0x99;
    }

    // IPingPongAppMessageアプリケーションプロトコルの電文コンバーター
    public class PingModelConverter : WrapperModelConverter<IPingPongAppMessage, Ping>
    {
        protected override IModelConverter<Ping> MakeConverter()
        {
            return new Ping().ToModelBuilder()
                    // Bigエンディアンで通信する
                    .ToBigEndian()
                    // 1byte目は定数（電文識別子）
                    .Constant((byte)0x01)
                    // ModelConverter型へ変換
                    .ToConverter();
        }
    }

    public class PongModelConverter : WrapperModelConverter<IPingPongAppMessage, Pong>
    {
        protected override IModelConverter<Pong> MakeConverter()
        {
            return new Pong().ToModelBuilder()
                    // Bigエンディアンで通信する
                    .ToBigEndian()
                    // 1byte目は定数（電文識別子）
                    .Constant((byte)0x02)
                    // ModelConverter型へ変換
                    .ToConverter();
        }
    }

    public class TunnelModelConverter : WrapperModelConverter<IPingPongAppMessage, Tunnel>
    {
        protected override IModelConverter<Tunnel> MakeConverter()
        {
            return new Tunnel().ToModelBuilder()
                    // Bigエンディアンで通信する
                    .ToBigEndian()
                    // 1byte目は定数（電文識別子）
                    .Constant((byte)0x03)
                    // ModelConverter型へ変換
                    .ToConverter();
        }
    }

    public class GomiModelConverter : WrapperModelConverter<IPingPongAppMessage, Gomi>
    {
        protected override IModelConverter<Gomi> MakeConverter()
        {
            return new Gomi().ToModelBuilder()
                    // Bigエンディアンで通信する
                    .ToBigEndian()
                    // 1byte目は定数（電文識別子）
                    .Constant((byte)0x99)
                    // ModelConverter型へ変換
                    .ToConverter();
        }
    }

    // IThirdAppMessageアプリケーションプロトコル
    public interface IThirdAppMessage { }

    public class Pang : IThirdAppMessage
    {
        public byte ID { get; set; } = 0x01;
        public int Value { get; set; }
        public IInnerModel InnerModel { get; set; }
        public InnerModel InnerModel2 { get; set; }
        public IEnumerable<InnerModel> Array { get; set; }
        public IEnumerable<byte> StructArray { get; set; }
        public InnerModel[] FixedArray { get; set; }
        public byte[] Bytes { get; set; } = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
    }
    public interface IInnerModel
    {
        int Value1 { get; set; }
        int Value2 { get; set; }
    }

    public class InnerModel : IInnerModel
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
    }
    // IThirdAppMessageアプリケーションプロトコルの電文コンバーター
    public class PangModelConverter : WrapperModelConverter<IThirdAppMessage>
    {
        protected override IModelConverter<IThirdAppMessage> MakeConverter()
        {
            // Pang電文のModelConverterを生成
            return new Pang().ToModelBuilder()
                                    // Bigエンディアンで通信する
                                    .ToBigEndian()
                                    // モデルの初期化が必要なメンバはここで初期化する
                                    .Init(m => m.InnerModel = new InnerModel())
                                    // 1byte目は定数（電文識別子）
                                    .Constant((byte)0x03)
                                    // Modelには表現されないけどPaddingブロックなんかがあるなら
                                    .Constant(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 })
                                    // メンバ変換
                                    .Property(m => m.Value)
                                    // メンバ変換(メンバのメンバ)
                                    .Property(m => m.InnerModel.Value1)
                                    .Property(m => m.InnerModel.Value2)
                                    // メンバクラスも入れ子で定義できたら便利だよね
                                    .Property(m => m.InnerModel2
                                        , b => b.Property(mi => mi.Value1)
                                                .Property(mi => mi.Value2))
                                    // 固定長のbyte配列もあるよね
                                    .FixedArrayProperty(8, m => m.Bytes)
                                    // 固定長の配列(メンバを持つ型の配列)
                                    .FixedArrayProperty(5, m => m.FixedArray
                                        // 型の定義を入れ子で書く
                                        , b => b.Property(mi => mi.Value1)
                                                .Property(mi => mi.Value2))
                                    // 可変長配列
                                    // 配列の数が電文に含まれてたりするよね。
                                    // 書き込むときはメンバの値を書けばいいけど復元する時は読むだけでいいよね。
                                    // っていうときはGetProperty。
                                    // そして読んだ値を覚えておきたいよねって時にAsTagで読み込んだ値に名前付けておく                                    
                                    .GetProperty(m => m.StructArray.Count()).AsTag("StructArrayCount")
                                    // さらにStructArrayCountを配列復元する時に使いたいよね
                                    .ArrayProperty("StructArrayCount", m => m.StructArray)
                                    // メンバを持つ型の可変長配列
                                    .GetProperty(m => m.Array.Count()).AsTag("InnerCount")
                                    .ArrayProperty("InnerCount", m => m.Array
                                        // Arrayメンバの要素の型InnerModelのModelBuilderを入れ子で
                                        , b => b.Property(mi => mi.Value1)
                                                .Property(mi => mi.Value2))
                                    // ModelConverter型へ変換
                                    .ToConverter()
                                    // ModelConverter<Pang> -> ModelConverter<IPingPongAppMessage>
                                    .ToBaseTypeConverter<Pang, IThirdAppMessage>();
        }
    }
}

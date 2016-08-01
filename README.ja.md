# Fluent.Hub

サーバーアプリケーションを構築する為のフレームワークです。

## How to Use
### server App

```csharp
public interface IPingPongAppMessage { }

public class Ping : IPingPongAppMessage
{
    public byte ID { get; set; } = 0x01;
}

public class Pong : IPingPongAppMessage
{
    public byte ID { get; set; } = 0x02;
}

public class Program
{
  static void Main(string[] args)
  {
    // アプリケーションコンテナ
    var appContainer = new ApplicationContainer();
    // IPingPongAppMessage型の電文をやり取りするサーバーアプリケーションを生成
    var app =
        // 待ち受けポートは8089
        appContainer.MakeAppByTcpServer<IPingPongAppMessage>(8089)
        // Ping電文のbyte[] <=> Model変換定義
        .RegisterConverter<IPingPongAppMessage, Ping>(modelBuilder =>
                                                        // Bigエンディアンで通信する
                                                        modelBuilder.ToBigEndian()
                                                        // 1byte目は定数（電文識別子）
                                                        .Constant((byte)0x01)
                                                        // ModelConverter型へ変換
                                                        .ToConverter())
        // Pong電文のbyte[] <=> Model変換定義
        .RegisterConverter<IPingPongAppMessage, Pong>(modelBuilder =>
                                                        // Bigエンディアンで通信する
                                                        modelBuilder.ToBigEndian()
                                                        // 1byte目は定数（電文識別子）
                                                        .Constant((byte)0x02)
                                                        // ModelConverter型へ変換
                                                        .ToConverter())
        .RegisterSequence((IIOContext<IPingPongAppMessage> context, Ping model) =>
        {
            // Pingを受信したらPongを送信するシーケンス
            context.Write(new Pong());
        });
    appContainer.Run();
  }
}
```

### client App

```csharp
public class Program
{
  static void Main(string[] args)
  {
    // アプリケーションコンテナ
    var appContainer = new ApplicationContainer();
    // IPingPongAppMessage型の電文をやり取りするサーバーアプリケーションを生成
    var app =
        // 待ち受けポートは8089
        appContainer.MakeAppByTcpClient<IPingPongAppMessage>("localhost", 8089)
        // Ping電文のbyte[] <=> Model変換定義
        .RegisterConverter<IPingPongAppMessage, Ping>(modelBuilder =>
                                                        // Bigエンディアンで通信する
                                                        modelBuilder.ToBigEndian()
                                                        // 1byte目は定数（電文識別子）
                                                        .Constant((byte)0x01)
                                                        // ModelConverter型へ変換
                                                        .ToConverter())
        // Pong電文のbyte[] <=> Model変換定義
        .RegisterConverter<IPingPongAppMessage, Pong>(modelBuilder =>
                                                        // Bigエンディアンで通信する
                                                        modelBuilder.ToBigEndian()
                                                        // 1byte目は定数（電文識別子）
                                                        .Constant((byte)0x02)
                                                        // ModelConverter型へ変換
                                                        .ToConverter())
        .RegisterSequence((IIOContext<IPingPongAppMessage> context, Ping model) =>
        {
            // Pingを受信したらPongを送信するシーケンス
            context.Write(new Pong());
        });
    Task.Run((Action)appContainer.Run);

    Thread.Sleep(1000 * 10);

    // サーバーにPingメッセージを送信
    appContainer.GetApp<IPingPongAppMessage>().InstantSequence((Action<IEnumerable<IIOContext<IPingPongAppMessage>>>)(contexts =>
    {
        var server = contexts.FirstOrDefault();
        if (server == null)
        {
            return;
        }
        server.Write(new Ping());
    }));
  }
}
```


## install
```
# TCP Server
Install-Package Fluent.Hub.TCP
```

## Licence

[MIT](https://raw.githubusercontent.com/shchy/FluentHub/master/LICENSE)

## Author

[shch](https://github.com/shchy)

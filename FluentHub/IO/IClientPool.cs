using FluentHub.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.IO
{
    public interface IContextPool<T>
    {
        void Add(IIOContext<T> modelContext);
        IEnumerable<IIOContext<T>> Get();
        event Action<IIOContext<T>> Updated;
    }

    public class ContextPool<T> : IContextPool<T>
    {
        private List<IIOContext<T>> pool;
        private ILogger logger;
        private EventWaitHandle updateCallEvent;

        public event Action<IIOContext<T>> Updated;

        public ContextPool(ILogger logger)
        {
            this.logger = logger;
            this.pool = new List<IIOContext<T>>();
            this.updateCallEvent = new ManualResetEvent(true);
        }

        public void Add(IIOContext<T> modelContext)
        {
            this.logger.Debug($"add context to pool {modelContext.GetType().Name}");
            modelContext.Received += ModelContext_Received;
            lock ((pool as ICollection).SyncRoot)
            {
                pool.Add(modelContext);
            }
        }

        // todo ModelContextが非同期でこのイベントを挙げてくるなら成り立つ？
        // todo 誰もメッセージを食べなかったら？
        // todo 受信済モデルが残っている限りUpdateを呼び出さないととまっちゃうね
        private void ModelContext_Received(object sender, EventArgs e)
        {
            lock (sender)
            {
                // 処理中だったら無視する
                if (this.updateCallEvent.WaitOne(0) == false)
                {
                    return;
                }
                // 処理中にする
                this.updateCallEvent.Reset();
            }

            var context = sender as IIOContext<T>;
            do
            {
                lock (sender)
                {
                    if (context.IsAny == false)
                    {
                        this.updateCallEvent.Set();
                        return;
                    }
                }
                OnUpdate(context);
            } while (true);

            
        }

        void OnUpdate(IIOContext<T> context)
        {
            if (Updated == null)
            {
                return;
            }
            Updated(context);
        }

        public IEnumerable<IIOContext<T>> Get()
        {
            lock ((pool as ICollection).SyncRoot)
            {
                return pool.ToArray();
            }
        }
    }



    public class ApplicationContainer : IDisposable
    {
        private Dictionary<Type, IContextApplication> applicationList;
        private List<Task> runningTasks;

        public ILogger Logger { get; private set; }

        public ApplicationContainer(ILogger logger)
        {
            this.Logger = logger;
            this.applicationList = new Dictionary<Type, IContextApplication>();
            this.runningTasks = new List<Task>();
        }

        public void Add<T>(IContextApplication<T> app)
        {
            var tType = typeof(T);
            System.Diagnostics.Debug.Assert(!this.applicationList.ContainsKey(tType), $"already exists {tType.Name}");
            this.applicationList.Add(tType, app);
        }

        public IContextApplication<T> GetApp<T>()
        {
            var tType = typeof(T);
            if (this.applicationList.ContainsKey(tType) == false)
            {
                return null;
            }

            return
                this.applicationList[tType] as IContextApplication<T>;
        }

        public virtual IContextPool<T> MakeContextPool<T>()
        {
            return new ContextPool<T>(this.Logger);
        }

        public void Run()
        {
            foreach (var app in applicationList.Values)
            {
                var runningTask =
                    Task.Run((Action)app.Run)
                    .ContinueWith(t => this.runningTasks.Remove(t));    // todo 要テストtがrunningTaskとイコールかどうか
                this.runningTasks.Add(runningTask);
            }
        }

        public void Dispose()
        {
            foreach (var app in applicationList.Values)
            {
                app.Dispose();
            }
            applicationList.Clear();

            while (IsRunning())
            {
                Thread.Sleep(10);
            }
        }

        bool IsRunning()
        {
            return
                this.runningTasks.Any(t => !t.Wait(0));
        }
    }

    public static class ApplicationContainerBuilder
    {
        /// <summary>
        /// TCPServerアプリケーションを生成して追加する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static IContextApplication<T> MakeAppByTcpServer<T>(
            this ApplicationContainer @this
            , int port
            , IEnumerable<IModelConverter<T>> converters)
        {
            var app = new TCPApplication<T>();
            app.Pool = @this.MakeContextPool<T>();
            app.Logger = @this.Logger;
            app.TcpFactory = new TcpServerFactory(port);
            app.ModelConverter = converters;
            @this.Add(app);
            return app;
        }

        public static IContextApplication<T> MakeAppByTcpClient<T>(
            this ApplicationContainer @this
            , string host
            , int port
            , IEnumerable<IModelConverter<T>> converters)
        {
            var app = new TCPApplication<T>();
            app.Pool = @this.MakeContextPool<T>();
            app.Logger = @this.Logger;
            app.TcpFactory = new TcpClientFactory(host, port);
            app.ModelConverter = converters;
            @this.Add(app);
            return app;
        }




        /// <summary>
        /// アプリケーションにシーケンスを登録する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IContextApplication<T> RegisterSequence<T>(
            this IContextApplication<T> @this
            , Action<IIOContext<T>> sequence)
        {
            @this.AddSequence(sequence);
            return @this;
        }

        /// <summary>
        /// アプリケーションにシーケンスを追加する
        /// 指定の仮引数型のモデルが受信済みであるかをあらかじめ確認してから発行される
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IContextApplication<T> RegisterSequence<T,U>(
            this IContextApplication<T> @this
            , Action<IIOContext<T>, U> sequence)
            where U : class, T
        {
            var typeT = typeof(U);

            @this.AddSequence(context =>
            {
                try
                {
                    var model = context.Read(x => typeT.IsInstanceOfType(x));
                    if (model == null)
                    {
                        return;
                    }
                    sequence(context, model as U);
                }
                catch (Exception ex)
                {
                    @this.Logger.Exception(ex);
                    throw;
                }
            });
            return @this;
        }


        /// <summary>
        /// 3者間シーケンスの登録
        /// T<->Server<->U的なシーケンス
        /// U群の内、どれかを特定するのはシーケンス内でやってね
        /// todo ISession的な概念を作る？IIOContextごとに持つやつ。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static ApplicationContainer RegisterSequence<T, U>(
            this ApplicationContainer @this
            , Action<IIOContext<T>, IEnumerable<IIOContext<U>>> sequence)
        {
            var app = @this.GetApp<T>();
            var appTarg = @this.GetApp<U>();
            System.Diagnostics.Debug.Assert(app != null, "RegisterSequence");
            System.Diagnostics.Debug.Assert(appTarg != null, "RegisterSequence");

            app.AddSequence(context =>
            {

                try
                {
                    var contexts = appTarg.Pool.Get().ToArray();
                    sequence(context, contexts);
                }
                catch (Exception ex)
                {
                    @this.Logger.Exception(ex);
                    throw;
                }
            });

            return @this;
        }

        /// <summary>
        /// 受信->応答型じゃなくてサーバーから開始するシーケンス
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Return"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static Return InstantSequence<T, Return>(
            this IContextApplication<T> @this
            , Func<IEnumerable<IIOContext<T>>, Return> sequence)
        {
            try
            {
                return sequence(@this.Pool.Get().ToArray());
            }
            catch (Exception ex)
            {
                @this.Logger.Exception(ex);
                throw;
            }
        }

        public static void InstantSequence<T>(
            this IContextApplication<T> @this
            , Action<IEnumerable<IIOContext<T>>> sequence)
        {

            try
            {
                sequence(@this.Pool.Get().ToArray());
            }
            catch (Exception ex)
            {
                @this.Logger.Exception(ex);
                throw;
            }
        }

        public static Return InstantSequence<T, U, Return>(
            this ApplicationContainer @this
            , Func<IEnumerable<IIOContext<T>>, IEnumerable<IIOContext<U>>, Return> sequence)
        {

            try
            {
                var app = @this.GetApp<T>();
                var appTarg = @this.GetApp<U>();
                System.Diagnostics.Debug.Assert(app != null, "RegisterSequence");
                System.Diagnostics.Debug.Assert(appTarg != null, "RegisterSequence");

                return
                    sequence(
                        app.Pool.Get().ToArray()
                        , appTarg.Pool.Get().ToArray());
            }
            catch (Exception ex)
            {
                @this.Logger.Exception(ex);
                throw;
            }
        }

        public static void InstantSequence<T, U>(
           this ApplicationContainer @this
           , Action<IEnumerable<IIOContext<T>>, IEnumerable<IIOContext<U>>> sequence)
        {

            try
            {
                var app = @this.GetApp<T>();
                var appTarg = @this.GetApp<U>();
                System.Diagnostics.Debug.Assert(app != null, "RegisterSequence");
                System.Diagnostics.Debug.Assert(appTarg != null, "RegisterSequence");
                sequence(
                    app.Pool.Get().ToArray()
                    , appTarg.Pool.Get().ToArray());
            }
            catch (Exception ex)
            {
                @this.Logger.Exception(ex);
                throw;
            }
        }


    }

    public interface IContextApplication : IDisposable
    {
        void Run();
    }

    public interface IContextApplication<T> : IContextApplication
    {
        void AddSequence(Action<IIOContext<T>> sequence);
        IContextPool<T> Pool { get; }
        ILogger Logger { get; }
    }

    public class TCPApplication<T> : IContextApplication<T>
    {
        private List<Action<IIOContext<T>>> sequences;

        
        public IContextPool<T> Pool { get; set; }

        // todo ここをもう1つ抽象化すればTCPじゃなくてもいける
        
        public ITcpClientFactory TcpFactory { get; set; }

        
        public IEnumerable<IModelConverter<T>> ModelConverter { get; set; }

        
        public ILogger Logger { get; set; }


        public TCPApplication()
        {
            this.sequences = new List<Action<IIOContext<T>>>();
        }

        public void Run()
        {
            this.Pool.Updated += UpdatedContext;
            this.TcpFactory.Maked = MakedClient;
            this.TcpFactory.Run();
            this.Pool.Updated -= UpdatedContext;
        }

        private void UpdatedContext(IIOContext<T> context)
        {
            // todo 非同期にする？
            foreach (var seq in sequences)
            {
                try
                {
                    seq(context);
                }
                catch (Exception ex)
                {
                    this.Logger.Exception(ex);
                    throw;
                }
            }
        }

        private void MakedClient(TcpClient client)
        {
            var context = client.BuildContextByTcp<T>(ModelConverter, Logger);
            this.Pool.Add(context);
        }

        public void Dispose()
        {
            this.TcpFactory.Dispose();
        }

        public void AddSequence(Action<IIOContext<T>> sequence)
        {
            this.sequences.Add(sequence);
        }
    }

    public interface ITcpClientFactory : IDisposable
    {
        void Run();
        Action<TcpClient> Maked { get; set; }
    }


    public abstract class TcpFactory : ITcpClientFactory
    {
        protected bool isDisporsed;

        public Action<TcpClient> Maked { get; set; }

        public void Run()
        {
            while (this.isDisporsed == false)
            {
                var client = GetTcpClient();
                if (client == null)
                {
                    continue;
                }
                OnMaked(client);
            }
        }

        private void OnMaked(TcpClient client)
        {
            if (Maked == null)
            {
                client.Close();
            }
            Maked(client);
        }

        protected abstract TcpClient GetTcpClient();

        protected bool IsEnd(Task task)
        {
            return
                task.IsCanceled
                || task.IsCompleted
                || task.IsFaulted;
        }

        public virtual void Dispose()
        {
            this.isDisporsed = true;
        }
    }

    public class TcpServerFactory : TcpFactory
    {
        private TcpListener listener;
        private int port;

        // todo 複数ポートいけるようにしたい
        public TcpServerFactory(int port)
        {
            this.port = port;

            this.listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }

        public override void Dispose()
        {
            base.Dispose();
            listener.Stop();
        }

        protected override TcpClient GetTcpClient()
        {
            var acceptTask = listener.AcceptTcpClientAsync();

            while (this.isDisporsed == false && IsEnd(acceptTask) == false)
            {
                Thread.Sleep(10);
            }
            
            if (isDisporsed || acceptTask.IsCompleted == false)
            {
                return null;
            }

            return acceptTask.Result;
        }
    }

    public class TcpClientFactory : TcpFactory
    {
        private int port;
        private string host;
        private TcpClient connectedClient;

        public TcpClientFactory(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        // todo 接続したのに何度もいっちゃうよね
        protected override TcpClient GetTcpClient()
        {
            // 接続済だったら接続しない
            if (this.connectedClient != null && this.connectedClient.Connected)
            {
                Thread.Sleep(1000);
                return null;
            }
            this.connectedClient = null;

            var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);

            while (this.isDisporsed == false && IsEnd(connectTask) == false)
            {
                Thread.Sleep(10);
            }

            if (isDisporsed || connectTask.IsFaulted || connectTask.Exception != null)
            {
                Thread.Sleep(1000);
                return null;
            }

            this.connectedClient = client;
            return client;
        }
    }
}

using FluentHub.Hub.Module;
using FluentHub.IO;
using FluentHub.IO.Extension;
using FluentHub.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public class ApplicationContainer : IApplicationContainer
    {
        private Dictionary<Type, IContextApplication> appList;
        private List<Task> runningTasks;

        public ILogger Logger { get; private set; }
        public IModuleInjection ModuleInjection { get; set; }

        public ApplicationContainer(ILogger logger = null, IModuleInjection moduleInjection = null)
        {
            this.Logger = logger ?? new DefaultLogger();
            this.appList = new Dictionary<Type, IContextApplication>();
            this.ModuleInjection = moduleInjection ?? new ModuleInjection();
            this.ModuleInjection.Missed += ModuleInjection_Missed;
            this.runningTasks = new List<Task>();

            // DIに登録
            this.ModuleInjection.Add(() => this as IApplicationContainer);
            this.ModuleInjection.Add(() => this.Logger as ILogger);
        }

        public void Add<T>(IContextApplication<T> app)
        {
            var tType = typeof(T);
            lock ((appList as ICollection).SyncRoot)
            {
                this.appList.Add(tType, app);
            }
            // DIコンテナに登録
            this.ModuleInjection.Add(() => app);
            this.ModuleInjection.Add<IEnumerable<IIOContext<T>>>(() => app.Pool.Get().ToArray());
        }

        public IEnumerable<IContextApplication> GetApps()
        {
            lock ((appList as ICollection).SyncRoot)
            {
                return appList.Values.ToArray();
            }
        }

        public IContextApplication<T> GetApp<T>()
        {
            var tType = typeof(T);

            lock ((appList as ICollection).SyncRoot)
            {
                if (this.appList.ContainsKey(tType) == false)
                {
                    return null;
                }
                return
                    this.appList[tType] as IContextApplication<T>;
            }
        }

        public virtual IContextPool<T> MakeContextPool<T>()
        {
            return new ContextPool<T>(this.Logger);
        }

        public void Run()
        {
            SetThreadPool();
            var apps = null as IContextApplication[];
            lock ((appList as ICollection).SyncRoot)
            {
                apps = appList.Values.ToArray();
            }

            foreach (var app in apps)
            {
                var runningTask = null as Task;
                runningTask =
                    Task.Run((Action)app.Run)
                    .ContinueWith(_ => DelTask(runningTask));
                AddTask(runningTask);
            }

            Task.WaitAll(GetRunningTasks().ToArray());
        }

        /// <summary>
        /// スレッドプールのパフォーマンスを上げるために最小値を設定しておく
        /// </summary>
        private static void SetThreadPool()
        {
            var maxX = 0;
            var minY = 0;
            var __ = 0;
            var recommended = 256 + 256 + 64;
            
            ThreadPool.GetMaxThreads(out maxX, out __);
            ThreadPool.GetMinThreads(out __, out minY);
            ThreadPool.SetMinThreads(Math.Min(recommended, maxX), minY);
        }

        void AddTask(Task task)
        {
            lock ((runningTasks as ICollection).SyncRoot)
            {
                this.runningTasks.Add(task);
            }
        }

        IEnumerable<Task> GetRunningTasks()
        {
            lock ((runningTasks as ICollection).SyncRoot)
            {
                return this.runningTasks.ToArray();
            }
        }


        void DelTask(Task task)
        {
            lock ((runningTasks as ICollection).SyncRoot)
            {
                this.runningTasks.Remove(task);
            }
        }

        public void Dispose()
        {
            lock ((appList as ICollection).SyncRoot)
            {
                foreach (var app in appList.Values)
                {
                    app.Dispose();
                }
                appList.Clear();
            }

            

            while (IsRunning())
            {
                Thread.Sleep(10);
            }

            this.ModuleInjection.Missed -= ModuleInjection_Missed;
        }

        bool IsRunning()
        {
            lock ((runningTasks as ICollection).SyncRoot)
            {
                return
                    this.runningTasks.Any(t => !t.Wait(0));
            }
        }


        // todo どこかへ移動したい
        // DIに失敗した時のイベントでそれがIEnumerable<ISessionContext<AppIF,SessionType>>だったら、ここでしか救済できないのでここでする。
        // どこかに委譲したい
        private object ModuleInjection_Missed(Type type)
        {
            if (typeof(IEnumerable<>) != type.GetGenericTypeDefinition())
            {
                return null;
            }
            // 
            var genericType = type.GetGenericArguments()[0];
            if (typeof(ISessionContext<,>) != genericType.GetGenericTypeDefinition())
            {
                return null;
            }

            // ISessionContext型を求めていたら
            // APPIFをチェック
            var appType = genericType.GetGenericArguments()[0];
            // 一致するアプリを取得
            var app = this.appList.Values.Where(x => x.GetType().GetGenericArguments()[0] == appType).FirstOrDefault();
            if (app == null)
            {
                return null;
            }

            // 求められているISessionの実装型を取得
            var sessionType = genericType.GetGenericArguments()[1];

            // ModelContext達を取得
            var contexts = app.GetContextsNotTyped();

            var query =
                from context in contexts
                let session = app.GetSessionNotTyped(context, sessionType)
                where session != null
                select new { context, session };
            var impl = query.ToArray();
           

            var sessionContexts = 
                this.GetType()
                .GetMethod(nameof(MakeSessionContexts), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .MakeGenericMethod(appType, sessionType)
                .Invoke(this, new object[] { impl.Select(x => x.context).ToArray(), impl.Select(x => x.session).ToArray() });
            return sessionContexts;
        }

        private IEnumerable<ISessionContext<AppIF, SessionType>> MakeSessionContexts<AppIF, SessionType>(IEnumerable<object> contexts, IEnumerable<SessionType> sessions)
            where SessionType : ISession
        {
            var query =
                from c in contexts.Zip(sessions, (c,s)=>new {c,s })
                let context = c.c
                let session = c.s
                let sessionContext = new SessionContext<AppIF, SessionType>((IIOContext<AppIF>)context, session)
                select sessionContext;
            return query.ToArray();
        }
    }
}

using FluentHub.Hub.Module;
using FluentHub.IO;
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
            this.ModuleInjection = moduleInjection ?? new ModuleInjection();
            this.appList = new Dictionary<Type, IContextApplication>();
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
        }

        bool IsRunning()
        {
            lock ((runningTasks as ICollection).SyncRoot)
            {
                return
                    this.runningTasks.Any(t => !t.Wait(0));
            }
        }
    }
}

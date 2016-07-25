using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public class ApplicationContainer : IApplicationContainer
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
}

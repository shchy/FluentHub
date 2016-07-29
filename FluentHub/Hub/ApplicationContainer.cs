﻿using FluentHub.Logger;
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

        public ApplicationContainer(ILogger logger = null)
        {
            this.Logger = logger ?? new DefaultLogger();
            // todo 同じタイプを登録できるようにする？Dictionaryやめる？
            this.appList = new Dictionary<Type, IContextApplication>();
            this.runningTasks = new List<Task>();
        }

        public void Add<T>(IContextApplication<T> app)
        {
            // todo problem when after Run
            var tType = typeof(T);
            lock ((appList as ICollection).SyncRoot)
            {
                this.appList.Add(tType, app);
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
        }

        void AddTask(Task task)
        {
            lock ((runningTasks as ICollection).SyncRoot)
            {
                this.runningTasks.Add(task);
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

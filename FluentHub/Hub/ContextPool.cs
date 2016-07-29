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


        public void Dispose()
        {
            lock ((pool as ICollection).SyncRoot)
            {
                foreach (var item in pool.ToArray())
                {
                    Remove(item);
                }
            }
        }

        public void Add(IIOContext<T> modelContext)
        {
            this.logger.Debug($"add context to pool typeof:{typeof(T).Name}");
            modelContext.Received += ModelContext_Received;
            lock ((pool as ICollection).SyncRoot)
            {
                CleaningPool(pool);
                pool.Add(modelContext);
            }
        }

        public void Remove(IIOContext<T> modelContext)
        {
            lock ((pool as ICollection).SyncRoot)
            {
                if (pool.Contains(modelContext) == false)
                {
                    return;
                }
                this.logger.Debug($"remove context to pool typeof:{typeof(T).Name}");
                modelContext.Received -= ModelContext_Received;
                modelContext.Dispose();
                pool.Remove(modelContext);
            }
        }

        // todo ModelContextが非同期でこのイベントを挙げてくるなら成り立つ？
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
                logger.TrySafe(() => OnUpdate(context));
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
                CleaningPool(pool);
                return pool.ToArray();
            }
        }

        private void CleaningPool(List<IIOContext<T>> pool)
        {
            var query =
                    from c in pool
                    where c.CanUse == false
                    select c;
            foreach (var item in query.ToArray())
            {
                Remove(item);
            }
        }

    }

}

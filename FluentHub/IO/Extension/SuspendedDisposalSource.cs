using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.IO.Extension
{
    public class SuspendedDisposalSource : ISuspendedDisposalSource
    {
        private List<ISuspendedDisposalToken> list;
        private Task runningTask;
        private bool isRunning;
        private int suspendMillisecond;
        private object syncObject = new object();

        public SuspendedDisposalSource(int suspendMillisecond)
        {
            this.list = new List<ISuspendedDisposalToken>();
            this.suspendMillisecond = suspendMillisecond;
        }

        public void Run()
        {
            if (this.runningTask != null || this.isRunning)
            {
                return;
            }
            lock (syncObject)
            {
                this.isRunning = true;
            }
            this.runningTask = Task.Run((Action)Running);
        }

        public void Stop()
        {
            lock (syncObject)
            {
                this.isRunning = false;
            }
            this.runningTask.Wait();
        }

        private void Running()
        {
            while (this.isRunning)
            {
                // 死んでるTokenを掃除する
                CleaningToken();
                // 処理が登録されていない間は動かない
                var targets = GetTargets(this.list).ToArray();

                var now = DateTime.Now;
                foreach (var token in targets)
                {
                    // 時が来たら実行
                    if (token.TargetTime < now)
                    {
                        token.Disposal();
                    }
                }
                System.Threading.Thread.Sleep(1);
            }
        }

        private void CleaningToken()
        {
            lock ((this.list as ICollection).SyncRoot)
            {
                var disposed = list.Where(x => x.IsDisposed).ToArray();
                foreach (var item in disposed)
                {
                    list.Remove(item);
                }
            }
        }

        private IEnumerable<ISuspendedDisposalToken> GetTargets(IEnumerable<ISuspendedDisposalToken> list)
        {
            while (this.isRunning)
            {
                var arrayed = null as IEnumerable<ISuspendedDisposalToken>;
                lock ((list as ICollection).SyncRoot)
                {
                    arrayed = list.ToArray();
                }
                var query =
                    from x in arrayed
                    where x.WaitHandle.WaitOne(0)
                    select x;
                var impl = query.ToArray();
                if (impl.Any() == false)
                {
                    Thread.Sleep(1);
                    continue;
                }
                return impl;
            }
            return Enumerable.Empty<ISuspendedDisposalToken>();
        }

        public ISuspendedDisposal MakeToken()
        {
            var token = new SuspendedDisposalToken(this.suspendMillisecond);
            lock ((this.list as ICollection).SyncRoot)
            {
                this.list.Add(token);
            }
            return token;
        }

        public void Dispose()
        {
            Stop();
            lock ((this.list as ICollection).SyncRoot)
            {
                foreach (var item in this.list.ToArray())
                {
                    item.Dispose();
                }
                this.list.Clear();
            }
        }
    }

}

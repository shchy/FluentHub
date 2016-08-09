using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.IO.Extension
{
    public class SuspendedDisposalToken : ISuspendedDisposal, ISuspendedDisposalToken
    {
        private Action method;
        private ManualResetEvent waitEvent;
        private int suspendMillisecond;
        private object syncObject = new object();

        public DateTime TargetTime { get; set; }
        public WaitHandle WaitHandle => waitEvent;
        public bool IsDisposed { get; set; }


        public SuspendedDisposalToken(int suspendMillisecond)
        {
            this.suspendMillisecond = suspendMillisecond;
            this.waitEvent = new ManualResetEvent(false);
        }

        public void Register(Action method)
        {
            lock (syncObject)
            {
                // すでに実行中の場合は無視
                if (this.IsDisposed || this.waitEvent.WaitOne(0) == true)
                {
                    return;
                }
                this.TargetTime = DateTime.Now.AddMilliseconds(this.suspendMillisecond);
                this.method = method;
                this.waitEvent.Set();
            }
        }

        public void Disposal()
        {
            lock (syncObject)
            {
                // 処理が空だったら未登録でやり直す
                if (method != null)
                {
                    method();
                    method = null;
                }
                this.waitEvent.Reset();
            }
        }

        public void Cancel()
        {
            lock (syncObject)
            {
                this.method = null;
                this.waitEvent.Reset();
            }
        }

        public void Dispose()
        {
            this.IsDisposed = true;
            lock (syncObject)
            {
                this.method = null;
                this.waitEvent.Set();
            }
        }
    }
}

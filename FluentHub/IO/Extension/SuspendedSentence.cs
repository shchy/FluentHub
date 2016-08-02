using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.IO.Extension
{
    public class SuspendedSentence : ISuspendedSentence
    {
        private Task runningTask;
        private Action method;
        private ManualResetEvent waitEvent;
        private DateTime targetTime;
        private bool isDisposed;
        private int suspendMillisecond;

        public SuspendedSentence(int suspendMillisecond)
        {
            this.waitEvent = new ManualResetEvent(false);
            this.suspendMillisecond = suspendMillisecond;
        }

        public void Run()
        {
            if (this.runningTask != null || this.isDisposed)
            {
                return;
            }
            this.runningTask = Task.Run((Action)Running);
        }

        private void Running()
        {
            while (this.isDisposed == false)
            {
                // 処理が登録されていない間は動かない
                this.waitEvent.WaitOne();

                // 時が来たら実行
                if (targetTime < DateTime.Now)
                {
                    // 処理が空だったら未登録でやり直す
                    if (method != null)
                    {
                        method();
                        method = null;
                    }
                    this.waitEvent.Reset();
                    continue;
                }

                System.Threading.Thread.Sleep(1);
            }
        }

        public void Sentence(Action method)
        {
            // すでに実行中の場合は無視
            if (this.waitEvent.WaitOne(0) == true)
            {
                return;
            }
            this.targetTime = DateTime.Now.AddMilliseconds(this.suspendMillisecond);
            this.method = method;
            this.waitEvent.Set();
        }

        public void Expiration()
        {
            this.method = null;
            this.waitEvent.Reset();
        }

        public void Dispose()
        {
            this.isDisposed = true;
            this.waitEvent.Set();
        }
    }
}

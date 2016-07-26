using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public interface INativeIOFactory<T> : IDisposable
    {
        T Make();
    }

    public class NativeIORunnableFactory<T> : IRunnableFactory<T>
    {
        protected bool isDisporsed;
        private INativeIOFactory<T> makeNativeIO;
        private Action<T> missedMakeNotify;

        public Action<T> Maked { get; set; }

        public NativeIORunnableFactory(
            INativeIOFactory<T> makeNativeIO
            , Action<T> missedMakeNotify)
        {
            this.makeNativeIO = makeNativeIO;
            this.missedMakeNotify = missedMakeNotify;
        }

        public void Run()
        {
            while (this.isDisporsed == false)
            {
                var client = this.makeNativeIO.Make();
                if (client == null)
                {
                    continue;
                }
                OnMaked(client);
            }
        }

        private void OnMaked(T client)
        {
            if (Maked == null)
            {
                this.missedMakeNotify(client);
            }
            Maked(client);
        }
        public virtual void Dispose()
        {
            this.isDisporsed = true;
            this.makeNativeIO.Dispose();
        }
    }

    public static class TaskExtension
    {
        public static bool IsEnd(this Task @this)
        {
            return
                @this.IsCanceled
                || @this.IsCompleted
                || @this.IsFaulted;
        }

    }

}

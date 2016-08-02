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
    public class NativeIOToContextMaker<T> : IIOContextMaker<byte[]>
    {
        private Func<T, IIOContext<byte[]>> convert;
        private INativeIOFactory<T> makeNativeIO;
        private Action<T> missedMakeNotify;
        private bool isDisposed;

        public Action<IIOContext<byte[]>> Maked { get; set; }

        public NativeIOToContextMaker(
            INativeIOFactory<T> makeNativeIO
            , Func<T, IIOContext<byte[]>> convert
            , Action<T> missedMakeNotify
            )
        {
            this.makeNativeIO = makeNativeIO;
            this.convert = convert;
            this.missedMakeNotify = missedMakeNotify;
        }
        
        public void Dispose()
        {
            this.isDisposed = true;
            makeNativeIO.Dispose();
        }

        public void Run()
        {
            while (this.isDisposed == false)
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
            var context = convert(client);
            Maked(context);
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

        public static bool SafeWait(this Task @this, CancellationToken token)
        {
            try
            {
                @this.Wait(token);
            }
            catch (OperationCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return true;

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
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

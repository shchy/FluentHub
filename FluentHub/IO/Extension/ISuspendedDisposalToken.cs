using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO.Extension
{
    public interface ISuspendedDisposalToken : IDisposable
    {
        System.Threading.WaitHandle WaitHandle { get; }
        DateTime TargetTime { get; }
        void Disposal();
        bool IsDisposed { get; }
    }
}

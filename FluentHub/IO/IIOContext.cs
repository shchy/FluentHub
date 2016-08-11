using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO
{
    public interface IIOContext<AppIF> : IDisposable
    {
        bool IsAny { get; }
        bool CanUse { get; }
        event EventHandler Received;

        void Write(AppIF model);
        AppIF Read();
        AppIF Read(Func<AppIF, bool> predicate);
    }
}

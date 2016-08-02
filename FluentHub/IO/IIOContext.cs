using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO
{
    public interface IIOContext<T> : IDisposable
    {
        bool IsAny { get; }
        bool CanUse { get; }
        event EventHandler Received;

        void Write(T model);
        T Read();
        T Read(Func<T, bool> predicate);
    }
}

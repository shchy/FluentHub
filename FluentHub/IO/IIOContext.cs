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
        void Write(T model);
        void WriteAll(IEnumerable<T> models);
        T Read();
        T Read(Func<T, bool> predicate);
        IEnumerable<T> ReadAll();
        event EventHandler Received;
        bool CanUse { get; }
    }

    
}

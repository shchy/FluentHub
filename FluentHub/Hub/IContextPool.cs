using FluentHub.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public interface IContextPool<T> : IDisposable
    {
        void Add(IIOContext<T> modelContext);
        void Remove(IIOContext<T> modelContext);
        IEnumerable<IIOContext<T>> Get();
        event Action<IIOContext<T>> Updated;
    }
}

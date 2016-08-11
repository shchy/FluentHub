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
    public interface IContextPool<AppIF> : IDisposable
    {
        void Add(IIOContext<AppIF> modelContext);
        void Remove(IIOContext<AppIF> modelContext);
        IEnumerable<IIOContext<AppIF>> Get();
        event Action<IIOContext<AppIF>> Updated;
        event Action<IIOContext<AppIF>> Added;
    }
}

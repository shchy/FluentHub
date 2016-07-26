using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public interface IContextApplication : IDisposable
    {
        void Run();
    }

    public interface IContextApplication<T> : IContextApplication
    {
        void AddSequence(Action<IIOContext<T>> sequence);
        void AddConverter(IModelConverter<T> converter);
        IContextPool<T> Pool { get; }
        ILogger Logger { get; }
    }
}

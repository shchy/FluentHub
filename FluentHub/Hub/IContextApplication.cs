using FluentHub.IO;
using FluentHub.IO.Extension;
using FluentHub.Logger;
using FluentHub.ModelConverter;
using FluentHub.Module;
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
        IContextPool<T> Pool { get; }
        IDictionary<IIOContext<T>, ISession> Sessions{get;}
        ILogger Logger { get; }
        IModuleInjection ModuleInjection { get; }
    }
}

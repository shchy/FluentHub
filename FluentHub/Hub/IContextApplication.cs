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

    public interface IContextApplication<AppIF> : IContextApplication
    {
        IContextPool<AppIF> Pool { get; }
        IDictionary<IIOContext<AppIF>, ISession> Sessions{get;}
        ILogger Logger { get; }
        IModuleDependencyContainer DependencyContainer { get; }
    }
}

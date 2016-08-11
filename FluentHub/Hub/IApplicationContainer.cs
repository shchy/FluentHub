using FluentHub.Logger;
using FluentHub.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public interface IApplicationContainer : IDisposable
    {
        ILogger Logger { get; }
        IModuleDependencyContainer DependencyContainer { get; }

        void Add<AppIF>(IContextApplication<AppIF> app);
        IContextApplication<AppIF> GetApp<AppIF>();
        IEnumerable<IContextApplication> GetApps();
        void Run();
    }
}

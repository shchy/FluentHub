using FluentHub.Hub;
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

namespace FluentHub
{
    public interface IAppBuilder
    {
        void Build(IApplicationContainer container);
    }

    public interface IAppBuilder<AppIF>
    {
        ILogger Logger { get; }

        IModuleInjection ModuleInjection { get; }

        Func<IIOContext<byte[]>, ISuspendedDisposal, IIOContext<AppIF>> StreamToModelContext { get; set; }

        Func<object, ISession> MakeSession { get; set; }

        List<IModelConverter<AppIF>> ModelConverters { get; }

        List<Action<IIOContext<AppIF>>> Sequences { get; }

        List<Action<IIOContext<AppIF>>> InitializeSequences { get; }

        Func<IContextPool<AppIF>> MakeContextPool { get; }

        IContextApplication<AppIF> App { get; set; }
    }
}

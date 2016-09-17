using FluentHub.Hub;
using FluentHub.Hub.ModelValidator;
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
    public interface IBuilder
    {
        void Build(IApplicationContainer container);
    }

    

    public interface IAppBuilder<AppIF>
    {
        ILogger Logger { get; }

        IModuleDependencyContainer DependencyContainer { get; }

        Func<IIOContext<byte[]>, ISuspendedDisposal, IIOContext<AppIF>> StreamToModelContext { get; set; }

        Func<object, ISession> MakeSession { get; set; }

        Func<IContextPool<AppIF>> MakeContextPool { get; set; }

        Func<IDictionary<IIOContext<AppIF>, ISession>> MakeSessionPool { get; set; }


        IList<IModelConverter<AppIF>> ModelConverters { get; }

        IList<IModelValidator<AppIF>> ModelValidators { get; }

        IList<Action<IIOContext<AppIF>>> Sequences { get; }

        IList<Action<IIOContext<AppIF>>> InitializeSequences { get; }

        IContextApplication<AppIF> App { get; }
    }

    public interface IAppBuilder<AppIF, NativeIO> : IAppBuilder<AppIF>
    {
        Func<NativeIO, IIOContext<byte[]>> NativeToStreamContext { get; set; }
    }


}

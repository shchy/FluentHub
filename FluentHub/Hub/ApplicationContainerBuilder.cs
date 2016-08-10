using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentHub.Logger;
using FluentHub.ModelConverter;
using System.Reflection;
using FluentHub.IO.Extension;

namespace FluentHub.Hub
{
    public static class ApplicationContainerBuilder
    {
        public static IContextApplication<T> MakeApp<T>(
            this IApplicationContainer @this
            , IIOContextMaker<byte[]> streamContextFactory)
        {
            var app =
                new Application<T>(
                    @this.MakeContextPool<T>()
                    , streamContextFactory
                    , new SuspendedDisposalSource(1000)    // todo defaultはこれでいいけどどこかで変更できるようにはしたいよね
                    , new SequenceRunnerFacade<T>(@this.Logger) // todo defaultはこれでいいけどどこかで変更できるようにはしたいよね
                    , @this.ModuleInjection
                    , @this.Logger
                    );
            @this.Add(app);
            return app;
        }

        public static IContextApplication<T> RegisterConverter<T>(
            this IContextApplication<T> @this
            , IModelConverter<T> converter)
        {
            @this.AddConverter(converter);
            return @this;
        }
        
        public static IContextApplication<T> RegisterConverter<T,U>(
            this IContextApplication<T> @this)
            where U : class,T, new()
            where T : class
        {
            var defaultConverter = new DefaultModelConverter<T,U>();
            @this.AddConverter(defaultConverter);
            return @this;
        }
    }

    
}

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
    public class AppBuilder<AppIF, NativeIO> : IBuilder, IAppBuilder<AppIF>
    {
        public ILogger Logger { get; set; }

        public Func<IIOContext<byte[]>, ISuspendedDisposal, IIOContext<AppIF>> StreamToModelContext { get; set; }
        
        public Func<object, ISession> MakeSession { get; set; }

        public Func<IContextPool<AppIF>> MakeContextPool { get; set; }

        public Func<IDictionary<IIOContext<AppIF>, ISession>> MakeSessionPool { get; set; }

        public IModuleDependencyContainer DependencyContainer { get; set; }
        
        public List<IModelConverter<AppIF>> ModelConverters { get; set; } = new List<IModelConverter<AppIF>>();

        public List<Action<IIOContext<AppIF>>> Sequences { get; set; } = new List<Action<IIOContext<AppIF>>>();

        public List<Action<IIOContext<AppIF>>> InitializeSequences { get; set; } = new List<Action<IIOContext<AppIF>>>();

        public IContextApplication<AppIF> App { get; set; }
        public INativeIOFactory<NativeIO> NativeIOFactory { get; set; }
        public Func<NativeIO, IIOContext<byte[]>> NativeToStreamContext { get; set; }

        public AppBuilder()
        {
            this.MakeContextPool = () => new ContextPool<AppIF>(this.Logger);
            this.Logger = new DefaultLogger();
            this.MakeSession = (nativeIO => new DefaultSession { NativeIO = nativeIO });
            this.MakeSessionPool = () => new Dictionary<IIOContext<AppIF>, ISession>();
            this.StreamToModelContext = ToModelContext;
        }

        private IIOContext<AppIF> ToModelContext(IIOContext<byte[]> streamContext, ISuspendedDisposal suspendedDisposal)
        {
            // StreamContextを例外ログとか出力できるように変換
            var loggerContext = new IOContextLoggerProxy<byte[]>(streamContext, Logger);

            return
                new ModelContext<AppIF>(loggerContext, ModelConverters, suspendedDisposal, Logger);
        }

        public void Build(IApplicationContainer container)
        {
            var contextPool = this.MakeContextPool();
            var sessionPool = this.MakeSessionPool();

            this.App =
                new Application<AppIF>(
                    contextPool
                    , Sequences
                    , InitializeSequences
                    , new ModelContextFactory<AppIF, NativeIO>(
                        NativeIOFactory
                        , NativeToStreamContext
                        , StreamToModelContext
                        , new SuspendedDisposalSource(1000)
                        , Logger)
                    , new SequenceRunnerFacade<AppIF>(Logger)
                    , DependencyContainer
                    , Logger
                    , MakeSession
                    , sessionPool); // todo Context作るとこでやればここに要らないね

            // DIコンテナに登録しておく
            DependencyContainer.Add<IContextApplication<AppIF>>(() => this.App);
            DependencyContainer.Add<ILogger>(() => Logger);
            DependencyContainer.Add<IApplicationContainer>(() => container);
            DependencyContainer.Add<IContextPool<AppIF>>(() => contextPool);
            DependencyContainer.Add<IEnumerable<IIOContext<AppIF>>>(() => contextPool.Get().ToArray());

            container.Add<AppIF>(this.App);
        }
    }
}

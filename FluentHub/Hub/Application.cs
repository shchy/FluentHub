using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using FluentHub.ModelConverter;
using FluentHub.IO.Extension;
using System.Threading.Tasks;
using System.Threading;
using FluentHub.Module;

namespace FluentHub.Hub
{
    public class Application<AppIF> : IContextApplication<AppIF>
    {
        private IEnumerable<Action<IIOContext<AppIF>>> sequences;
        private IEnumerable<Action<IIOContext<AppIF>>> initializeSequences;
        private ISequenceRunnerFacade<AppIF> sequenceRunnerFacade;
        private IModelContextFactory<AppIF> modelContextFactory;
        private Func<object, ISession> makeSession;

        public IContextPool<AppIF> Pool { get;  }
        public ILogger Logger { get; }

        public IModuleDependencyContainer DependencyContainer { get;  }
        IDictionary<IIOContext<AppIF>, ISession> sessionPool;
        public IDictionary<IIOContext<AppIF>, ISession> Sessions
        {
            get
            {
                lock ((sessionPool as ICollection).SyncRoot)
                {
                    return sessionPool;
                }
            }
        }

        public Application(
            IContextPool<AppIF> pool
            , IEnumerable<Action<IIOContext<AppIF>>> sequences
            , IEnumerable<Action<IIOContext<AppIF>>> initializeSequences
            , IModelContextFactory<AppIF> modelContextFactory
            , ISequenceRunnerFacade<AppIF> sequenceRunnerFacade
            , IModuleDependencyContainer dependencyContainer
            , ILogger logger
            , Func<object, ISession> makeSession
            , IDictionary<IIOContext<AppIF>, ISession> sessionPool)
        {
            this.sequences = sequences;
            this.initializeSequences = initializeSequences;
            this.Pool = pool;
            this.sessionPool = sessionPool;
            this.modelContextFactory = modelContextFactory;
            this.Logger = logger;
            this.sequenceRunnerFacade = sequenceRunnerFacade;
            this.DependencyContainer = dependencyContainer;
            this.makeSession = makeSession ?? (nativeIO => new DefaultSession { NativeIO = nativeIO });
        }

        public void Run()
        {
            this.Pool.Updated += UpdatedContext;
            this.Pool.Added += AddedContext;
            this.modelContextFactory.Maked += ModelContextFactory_Maked;
            this.modelContextFactory.Run();
            this.modelContextFactory.Maked -= ModelContextFactory_Maked;
            this.Pool.Added -= AddedContext;
            this.Pool.Updated -= UpdatedContext;
        }

        private void ModelContextFactory_Maked(IIOContext<AppIF> context, object nativeIO)
        {
            this.Pool.Add(context);
            lock ((sessionPool as ICollection).SyncRoot)
            {
                this.sessionPool[context] = makeSession(nativeIO);
            }
        }

        private void AddedContext(IIOContext<AppIF> context)
        {
            var xs = Enumerable.Empty<Action<IIOContext<AppIF>>>();
            lock ((initializeSequences as ICollection).SyncRoot)
            {
                xs = initializeSequences.ToArray();
            }

            foreach (var seq in xs)
            {
                Logger.TrySafe(() => seq(context));
            }
        }

        private void UpdatedContext(IIOContext<AppIF> context)
        {
            var ss = null as Action<IIOContext<AppIF>>[];

            lock ((sequences as ICollection).SyncRoot)
            {
                ss = sequences.ToArray();
            }

            foreach (var seq in ss)
            {
                this.sequenceRunnerFacade.Push(context, seq);
            }
        }

        
        public void Dispose()
        {
            this.modelContextFactory.Stop();
            this.Pool.Dispose();
            sequences = Enumerable.Empty<Action<IIOContext<AppIF>>>();
        }
    }
}

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
    public class Application<T> : IContextApplication<T>
    {
        private IEnumerable<Action<IIOContext<T>>> sequences;
        private IEnumerable<Action<IIOContext<T>>> initializeSequences;
        private ISequenceRunnerFacade<T> sequenceRunnerFacade;
        private IModelContextFactory<T> modelContextFactory;
        private Func<object, ISession> makeSession;

        public IContextPool<T> Pool { get;  }
        public ILogger Logger { get; }

        public IModuleInjection ModuleInjection { get;  }
        IDictionary<IIOContext<T>, ISession> sessionPool;
        public IDictionary<IIOContext<T>, ISession> Sessions
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
            IContextPool<T> pool
            , IEnumerable<Action<IIOContext<T>>> sequences
            , IEnumerable<Action<IIOContext<T>>> initializeSequences
            , IModelContextFactory<T> modelContextFactory
            , ISequenceRunnerFacade<T> sequenceRunnerFacade
            , IModuleInjection moduleInjection
            , ILogger logger
            , Func<object, ISession> makeSession
            , IDictionary<IIOContext<T>, ISession> sessionPool)
        {
            this.sequences = sequences;
            this.initializeSequences = initializeSequences;
            this.Pool = pool;
            this.sessionPool = sessionPool;
            this.modelContextFactory = modelContextFactory;
            this.Logger = logger;
            this.sequenceRunnerFacade = sequenceRunnerFacade;
            this.ModuleInjection = moduleInjection;
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

        private void ModelContextFactory_Maked(IIOContext<T> context, object nativeIO)
        {
            this.Pool.Add(context);
            lock ((sessionPool as ICollection).SyncRoot)
            {
                this.sessionPool[context] = makeSession(nativeIO);
            }
        }

        private void AddedContext(IIOContext<T> context)
        {
            var xs = Enumerable.Empty<Action<IIOContext<T>>>();
            lock ((initializeSequences as ICollection).SyncRoot)
            {
                xs = initializeSequences.ToArray();
            }

            foreach (var seq in xs)
            {
                Logger.TrySafe(() => seq(context));
            }
        }

        private void UpdatedContext(IIOContext<T> context)
        {
            var ss = null as Action<IIOContext<T>>[];

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
            sequences = Enumerable.Empty<Action<IIOContext<T>>>();
        }
    }
}

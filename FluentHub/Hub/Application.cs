using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using FluentHub.ModelConverter;
using FluentHub.IO.Extension;
using FluentHub.Hub.Module;
using System.Threading.Tasks;
using System.Threading;

namespace FluentHub.Hub
{
    public class Application<T> : IContextApplication<T>
    {
        private List<Action<IIOContext<T>>> sequences;
        private List<Action<IIOContext<T>>> initializeSequences;
        private List<IModelConverter<T>> modelConverters;
        private ISequenceRunnerFacade<T> sequenceRunnerFacade;
        private IModelContextFactory<T> modelContextFactory;

        public IContextPool<T> Pool { get;  }
        public ILogger Logger { get; }

        public IModuleInjection ModuleInjection { get;  }

        public Application(
            IContextPool<T> pool
            , IModelContextFactory<T> modelContextFactory
            , ISequenceRunnerFacade<T> sequenceRunnerFacade
            , IModuleInjection moduleInjection
            , ILogger logger)
        {
            this.sequences = new List<Action<IIOContext<T>>>();
            this.initializeSequences = new List<Action<IIOContext<T>>>();
            this.Pool = pool;
            this.modelContextFactory = modelContextFactory;
            this.modelConverters = new List<IModelConverter<T>>();
            this.Logger = logger;
            this.sequenceRunnerFacade = sequenceRunnerFacade;
            this.ModuleInjection = moduleInjection;
        }

        public void Run()
        {
            
            this.Pool.Updated += UpdatedContext;
            this.Pool.Added += AddedContext;
            this.modelContextFactory.Maked += ModelContextFactory_Maked;
            this.modelContextFactory.Run(this.modelConverters);
            this.modelContextFactory.Maked -= ModelContextFactory_Maked;
            this.Pool.Added -= AddedContext;
            this.Pool.Updated -= UpdatedContext;
            
        }

        private void ModelContextFactory_Maked(IIOContext<T> context)
        {
            this.Pool.Add(context);
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
            lock ((sequences as ICollection).SyncRoot)
            {
                sequences.Clear();
            }
            this.modelConverters = null;
        }

        public void AddSequence(Action<IIOContext<T>> sequence)
        {
            lock ((sequences as ICollection).SyncRoot)
            {
                this.sequences.Add(sequence);
            }
        }

        public void AddInitializeSequence(Action<IIOContext<T>> initializeSequence)
        {
            lock ((initializeSequences as ICollection).SyncRoot)
            {
                this.initializeSequences.Add(initializeSequence);
            }
        }

        public void AddConverter(IModelConverter<T> converter)
        {
            lock ((modelConverters as ICollection).SyncRoot)
            {
                modelConverters.Add(converter);
            }
        }
    }

}

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

namespace FluentHub.Hub
{
    public class Application<T> : IContextApplication<T>
    {
        private List<Action<IIOContext<T>>> sequences;
        private List<Action<IIOContext<T>>> initializeSequences;
        private IIOContextMaker<byte[]> streamContextFactory;
        private List<IModelConverter<T>> modelConverters;
        private ISuspendedDisposalSource suspendedSentenceSource;
        private ISequenceRunnerFacade<T> sequenceRunnerFacade;

        public IContextPool<T> Pool { get;  }
        public ILogger Logger { get; }

        public IModuleInjection ModuleInjection { get;  }

        public Application(
            IContextPool<T> pool
            , IIOContextMaker<byte[]> sreamContextFactory
            , ISuspendedDisposalSource suspendedSentenceSource
            , ISequenceRunnerFacade<T> sequenceRunnerFacade
            , IModuleInjection moduleInjection
            , ILogger logger)
        {
            this.sequences = new List<Action<IIOContext<T>>>();
            this.initializeSequences = new List<Action<IIOContext<T>>>();
            this.Pool = pool;
            this.streamContextFactory = sreamContextFactory;
            this.modelConverters = new List<IModelConverter<T>>();
            this.Logger = logger;
            this.suspendedSentenceSource = suspendedSentenceSource;
            this.sequenceRunnerFacade = sequenceRunnerFacade;
            this.ModuleInjection = moduleInjection;
        }

        public void Run()
        {
            
            this.Pool.Updated += UpdatedContext;
            this.Pool.Added += AddedContext;
            this.streamContextFactory.Maked = MakedClient;
            this.suspendedSentenceSource.Run();
            this.streamContextFactory.Run();
            this.suspendedSentenceSource.Stop();

            this.Pool.Added -= AddedContext;
            this.Pool.Updated -= UpdatedContext;
            
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

        private void MakedClient(IIOContext<byte[]> context)
        {
            this.Pool.Add(
                context.BuildContext(
                    this.modelConverters.ToArray()
                    , this.suspendedSentenceSource.MakeToken()
                    , this.Logger));
        }

        public void Dispose()
        {
            this.streamContextFactory.Dispose();
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

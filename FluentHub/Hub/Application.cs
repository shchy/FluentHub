using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using FluentHub.ModelConverter;

namespace FluentHub.Hub
{
    public class Application<T> : IContextApplication<T>
    {
        private List<Action<IIOContext<T>>> sequences;
        private IIOContextMaker<byte[]> streamContextFactory;
        private List<IModelConverter<T>> modelConverters;

        public IContextPool<T> Pool { get;  }
        public ILogger Logger { get; }

        public Application(
            IContextPool<T> pool
            , IIOContextMaker<byte[]> sreamContextFactory
            , ILogger logger)
        {
            this.sequences = new List<Action<IIOContext<T>>>();
            this.Pool = pool;
            this.streamContextFactory = sreamContextFactory;
            this.modelConverters = new List<IModelConverter<T>>();
            this.Logger = logger;
        }

        public void Run()
        {
            this.Pool.Updated += UpdatedContext;
            this.streamContextFactory.Maked = MakedClient;
            this.streamContextFactory.Run();
            this.Pool.Updated -= UpdatedContext;
        }

        private void UpdatedContext(IIOContext<T> context)
        {
            // todo 非同期にする？
            var ss = null as Action<IIOContext<T>>[];

            lock ((sequences as ICollection).SyncRoot)
            {
                ss = sequences.ToArray();
            }

            foreach (var seq in ss)
            {
                Logger.TrySafe(() => seq(context));
            }
        }

        private void MakedClient(IIOContext<byte[]> context)
        {
            this.Pool.Add(
                context.BuildContext(
                    this.modelConverters.ToArray()
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

        public void AddConverter(IModelConverter<T> converter)
        {
            lock ((modelConverters as ICollection).SyncRoot)
            {
                modelConverters.Add(converter);
            }
        }
    }

}

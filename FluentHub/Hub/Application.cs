using FluentHub.TCP;
using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public class Application<T> : IContextApplication<T>
    {
        private List<Action<IIOContext<T>>> sequences;
        private IRunnableFactory<IIOContext<byte>> streamContextFactory;
        private IEnumerable<IModelConverter<T>> modelConverters;

        public IContextPool<T> Pool { get;  }

        public ILogger Logger { get; }


        public Application(
            IContextPool<T> pool
            , IRunnableFactory<IIOContext<byte>> sreamContextFactory
            , IEnumerable<IModelConverter<T>> modelConverters
            , ILogger logger)
        {
            this.sequences = new List<Action<IIOContext<T>>>();
            this.Pool = pool;
            this.streamContextFactory = sreamContextFactory;
            this.modelConverters = modelConverters;
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
            foreach (var seq in sequences)
            {
                try
                {
                    seq(context);
                }
                catch (Exception ex)
                {
                    this.Logger.Exception(ex);
                    throw;
                }
            }
        }

        private void MakedClient(IIOContext<byte> context)
        {
            this.Pool.Add(
                context.BuildContext(
                    this.modelConverters
                    , this.Logger));
        }

        public void Dispose()
        {
            this.streamContextFactory.Dispose();
        }

        public void AddSequence(Action<IIOContext<T>> sequence)
        {
            this.sequences.Add(sequence);
        }
    }

}

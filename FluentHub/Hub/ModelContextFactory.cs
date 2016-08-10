using FluentHub.IO;
using FluentHub.IO.Extension;
using FluentHub.Logger;
using FluentHub.ModelConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public class ModelContextFactory<AppIF, NativeIO> : IModelContextFactory<AppIF>
    {
        private ILogger logger;
        private INativeIOFactory<NativeIO> nativeIOFactory;
        private ISuspendedDisposalSource suspendedSentenceSource;
        private Func<NativeIO, IIOContext<byte[]>> toStreamContext;
        private bool isRunning;

        public event Action<IIOContext<AppIF>> Maked;

        public ModelContextFactory(
            INativeIOFactory<NativeIO> nativeIOFactory
            , Func<NativeIO, IIOContext<byte[]>> toStreamContext
            , ISuspendedDisposalSource suspendedSentenceSource
            , ILogger logger)
        {
            this.nativeIOFactory = nativeIOFactory;
            this.toStreamContext = toStreamContext;
            this.suspendedSentenceSource = suspendedSentenceSource;
            this.logger = logger;
        }
        public void Run(IEnumerable<IModelConverter<AppIF>> modelConverters)
        {
            suspendedSentenceSource.Run();
            this.isRunning = true;
            while (this.isRunning)
            {
                var nativeIO = this.nativeIOFactory.Make();
                if (nativeIO == null)
                {
                    continue;
                }

                var streamContext = this.toStreamContext(nativeIO);
                var modelContext = streamContext.BuildContext(
                        modelConverters
                        , this.suspendedSentenceSource.MakeToken()
                        , this.logger);

                if (Maked == null)
                {
                    modelContext.Dispose();
                    continue;
                }
                Maked(modelContext);
            }

            suspendedSentenceSource.Stop();
        }

        public void Stop()
        {
            this.isRunning = false;
        }
    }

}

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

        public event Action<IIOContext<AppIF>, object> Maked;

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
            // パケ詰まりを何とかするタイミングを管理する何かを開始
            suspendedSentenceSource.Run();

            this.isRunning = true;
            while (this.isRunning)
            {
                try
                {
                    // Nativeな何かを取得
                    var nativeIO = this.nativeIOFactory.Make();
                    if (nativeIO == null)
                    {
                        continue;
                    }
                    // Nativeな何かをStreamContextに変換
                    var streamContext = this.toStreamContext(nativeIO);
                    // StreamContextを例外ログとか出力できるように変換
                    var loggerContext = new IOContextLoggerProxy<byte[]>(streamContext, logger);
                    // ↑をModelContextに変換
                    var modelContext = new ModelContext<AppIF>(loggerContext, modelConverters, suspendedSentenceSource.MakeToken(), logger);
                    // 生成イベントが未登録なら行き場がないので即破棄
                    if (Maked == null)
                    {
                        modelContext.Dispose();
                        continue;
                    }
                    // 生成イベントを発行
                    Maked(modelContext, nativeIO);
                }
                catch (Exception ex)
                {
                    logger.Exception(ex);
                }
            }

            suspendedSentenceSource.Stop();
        }

        public void Stop()
        {
            this.isRunning = false;
        }
    }
    

}

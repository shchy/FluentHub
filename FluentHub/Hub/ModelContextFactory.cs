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
        private Func<IIOContext<byte[]>, ISuspendedDisposal, IIOContext<AppIF>> toModelContext;

        public event Action<IIOContext<AppIF>, object> Maked;

        public ModelContextFactory(
            INativeIOFactory<NativeIO> nativeIOFactory
            , Func<NativeIO, IIOContext<byte[]>> toStreamContext
            , Func<IIOContext<byte[]>, ISuspendedDisposal, IIOContext<AppIF>> toModelContext
            , ISuspendedDisposalSource suspendedSentenceSource
            , ILogger logger)
        {
            this.nativeIOFactory = nativeIOFactory;
            this.toStreamContext = toStreamContext;
            this.toModelContext = toModelContext;
            this.suspendedSentenceSource = suspendedSentenceSource;
            this.logger = logger;
        }
        public void Run()
        {
            // パケ詰まりを何とかするタイミングを管理する何かを開始
            suspendedSentenceSource.Run();

            this.isRunning = true;
            while (this.isRunning)
            {
                try
                {
                    // すでに必要な接続が確立されている場合
                    if (this.nativeIOFactory.IsAlreadyEnough())
                    {
                        System.Threading.Thread.Sleep(1000);
                        continue;
                    }

                    // Nativeな何かを取得
                    var nativeIO = this.nativeIOFactory.Make();
                    if (nativeIO == null)
                    {
                        continue;
                    }
                    // Nativeな何かをStreamContextに変換
                    var streamContext = this.toStreamContext(nativeIO);

                    // ↑をModelContextに変換
                    var modelContext = this.toModelContext(streamContext, suspendedSentenceSource.MakeToken());

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

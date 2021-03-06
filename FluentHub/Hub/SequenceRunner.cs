﻿using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public class SequenceRunner<AppIF> : ISequenceRunner
    {
        private IIOContext<AppIF> context;
        private bool isReserved;
        private bool isRunning;
        private ILogger logger;
        private Action<IIOContext<AppIF>> sequence;
        private object syncObject = new object();

        public SequenceRunner(IIOContext<AppIF> context, Action<IIOContext<AppIF>> sequence, ILogger logger)
        {
            this.context = context;
            this.sequence = sequence;
            this.logger = logger;
        }

        public void Push()
        {
            lock (syncObject)
            {
                if (this.isRunning)
                {
                    this.isReserved = true;
                    return;
                }
                this.isRunning = true;
            }
            Task.Run((Action)Running);
        }

        private void Running()
        {
            while (this.isRunning)
            {
                // run sequence 
                this.logger.TrySafe(() => sequence(context));
                
                // シーケンス実行中に次のメッセージを受信してたらもう一回？
                lock (syncObject)
                {
                    if (this.isReserved)
                    {
                        this.isReserved = false;
                        continue;
                    }
                }

                // todo こっちがあるから上はいらなくなった？
                // memo IsAnyは必要。同じ電文が2つ連続で来たとき、1つ目の電文だけ拾って終わるパターンあるよね
                // hack でも処理されない電文（タイムアウト後の応答とか）が残ってるとCPUギュンギュン回るよね。
                if (context.IsAny)
                {
                    System.Threading.Thread.Sleep(1);
                    continue;
                }

                break;
            }

            lock (syncObject)
            {
                this.isRunning = false;
            }
        }
    }
}

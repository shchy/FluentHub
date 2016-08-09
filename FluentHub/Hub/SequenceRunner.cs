using FluentHub.IO;
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
            do
            {
                this.logger.TrySafe(() => sequence(context));
                // todo IsAnyもみとく？
                lock (syncObject)
                {
                    if (this.isReserved)
                    {
                        this.isReserved = false;
                        continue;
                    }
                }
            } while (false);

            lock (syncObject)
            {
                this.isRunning = false;
            }
        }
    }
}

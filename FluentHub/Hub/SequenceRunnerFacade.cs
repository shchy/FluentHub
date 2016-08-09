using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public interface ISequenceRunnerFacade<AppIF>
    {
        void Push(IIOContext<AppIF> context, Action<IIOContext<AppIF>> sequence);
    }


    public class SequenceRunnerFacade<AppIF> : ISequenceRunnerFacade<AppIF>
    {
        private IDictionary<IIOContext<AppIF>, IDictionary<Action<IIOContext<AppIF>>, ISequenceRunner>> cache;
        private ILogger logger;

        public SequenceRunnerFacade(ILogger logger)
        {
            this.cache = new Dictionary<IIOContext<AppIF>, IDictionary<Action<IIOContext<AppIF>>, ISequenceRunner>>();
            this.logger = logger;
        }

        public void Push(IIOContext<AppIF> context, Action<IIOContext<AppIF>> sequence)
        {
            var sequenceCache = GetCache(context);
            var runner = GetCache(sequenceCache, context, sequence);
            runner.Push();
        }

        private ISequenceRunner GetCache(IDictionary<Action<IIOContext<AppIF>>, ISequenceRunner> sequenceCache, IIOContext<AppIF> context, Action<IIOContext<AppIF>> sequence)
        {
            lock ((sequenceCache as ICollection).SyncRoot)
            {
                if (sequenceCache.ContainsKey(sequence) == false)
                {
                    sequenceCache[sequence] = new SequenceRunner<AppIF>(context, sequence, this.logger);
                }
                return sequenceCache[sequence];
            }
        }

        private IDictionary<Action<IIOContext<AppIF>>, ISequenceRunner> GetCache(IIOContext<AppIF> context)
        {
            lock ((this.cache as ICollection).SyncRoot)
            {
                if (this.cache.ContainsKey(context) == false)
                {
                    this.cache[context] = new Dictionary<Action<IIOContext<AppIF>>, ISequenceRunner>();
                }

                return cache[context];
            }
        }
    }
}

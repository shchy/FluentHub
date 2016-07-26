using FluentHub.IO;
using FluentHub.IO.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public class FakeContextFactory : IRunnableFactory<IIOContext<byte>>
    {
        private ManualResetEvent waithandle;
        private Func<FakeStream> make;

        public Action<IIOContext<byte>> Maked { get; set; }

        public FakeContextFactory(IIO io)
        {
            this.waithandle = new ManualResetEvent(false);
            this.make = () => new FakeStream(io);
        }

        public void Dispose()
        {
            this.waithandle.Set();
        }

        public void Run()
        {
            Maked(make().BuildContextByStream());
            this.waithandle.WaitOne();
        }
    }
}

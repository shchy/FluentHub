using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public class DelegateContextFactory<T> : IRunnableFactory<IIOContext<byte>>
    {
        private IRunnableFactory<T> factory;
        private Func<T, IIOContext<byte>> convert;

        public Action<IIOContext<byte>> Maked { get; set; }

        public DelegateContextFactory(
            IRunnableFactory<T> factory
            , Func<T, IIOContext<byte>> convert)
        {
            this.factory = factory;
            this.convert = convert;
            factory.Maked = MakeConvert;
        }

        private void MakeConvert(T baseContext)
        {
            var context = convert(baseContext);
            if (Maked == null)
            {
                context.Dispose();
                return;
            }
            Maked(context);
        }

        public void Dispose()
        {
            factory.Dispose();
        }

        public void Run()
        {
            factory.Run();
        }
    }
}

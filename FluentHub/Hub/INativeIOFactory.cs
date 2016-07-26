using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public interface INativeIOFactory<T> : IDisposable
    {
        T Make();
    }
}

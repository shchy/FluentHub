using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO
{
    public interface IRunnableFactory<T> : IDisposable
    {
        Action<T> Maked { get; set; }
        void Run();
    }
}

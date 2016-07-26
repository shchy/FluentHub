using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public interface IIOContextMaker<T> : IDisposable
    {
        Action<IIOContext<T>> Maked { get; set; }
        void Run();
    }
}

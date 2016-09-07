using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.UDP
{
    public interface IIO : IDisposable
    {
        byte[] Read();
        int Write(byte[] data);
    }
}

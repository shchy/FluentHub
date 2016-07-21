using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO
{
    public interface IModelConverter<T>
    {
        bool CanModelToBytes(object model);
        bool CanBytesToModel(IEnumerable<byte> bytes);
        byte[] ToByte(T model);
        Tuple<T, int> ToModel(IEnumerable<byte> bytes);
    }
}

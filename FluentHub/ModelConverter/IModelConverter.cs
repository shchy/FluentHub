using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter
{
    public interface IModelConverter<T>
    {
        bool CanModelToBytes(object model);
        bool CanBytesToModel(IEnumerable<byte> bytes);
        byte[] ToBytes(T model);
        Tuple<T, int> ToModel(IEnumerable<byte> bytes);
    }
}

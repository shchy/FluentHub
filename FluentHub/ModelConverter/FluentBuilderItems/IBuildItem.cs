using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public interface IBuildItem<T>
    {
        void Write(T model, BinaryWriter w);
        Tuple<bool,object> CanRead(BinaryReader r, IDictionary<string, object> _context);
        object Read(T model, BinaryReader r, IDictionary<string, object> _context);
        // todo なんとかここから削除したい
        string Tag { get; set; }
    }
}

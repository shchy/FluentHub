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

        int GetReadSize(IDictionary<string, object> _context);
        object Read(T model, BinaryReader r, IDictionary<string, object> _context);
        // todo そもそも_contextがダサいけどTag作るのとReadの戻りをobjectにするのとどっちがマシか
        string Tag { get; set; }
    }
}

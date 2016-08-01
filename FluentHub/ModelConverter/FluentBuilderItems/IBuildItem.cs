using System;
using System.Collections;
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
        bool CanRead(BinaryReader r, IDictionary<string, object> _context);
        void Read(T model, BinaryReader r, IDictionary<string, object> _context);
    }

    
}

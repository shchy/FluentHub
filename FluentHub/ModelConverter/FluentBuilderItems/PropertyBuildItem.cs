using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    // Read/Writeプロパティ
    public class PropertyBuildItem<T, V> : ITaggedBuildItem<T>
    {
        private Func<T, V> getter;
        private Action<T, V> setter;
        private IBinaryConverter converter;
        private int size;

        public event Action<string, object> SetContextValue;

        public string Tag { get; set; }

        public PropertyBuildItem(Func<T, V> getter, Action<T, V> setter, IBinaryConverter converter, int size)
        {
            this.converter = converter;
            this.getter = getter;
            this.setter = setter;
            this.size = size;
        }

        public void Write(T model, BinaryWriter w)
        {
            var v = getter(model);
            var data =
                converter.ToBytes(v);
            w.Write(data);
        }

        public void Read(T model, BinaryReader r, IDictionary<string, object> context)
        {
            var data = r.ReadBytes(size);
            var v = converter.ToModel<V>(data);
            setter(model, v);
            SetContextValue(Tag, v);
        }

        public bool CanRead(BinaryReader r, IDictionary<string, object> context)
        {
            var remain = r.BaseStream.Length - r.BaseStream.Position;
            if (remain < size)
            {
                return false;
            }

            var data = r.ReadBytes(size);
            var v = converter.ToModel<V>(data);
            SetContextValue(Tag, v);
            return true;
        }

    }
}

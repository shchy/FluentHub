using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    // Read/Writeプロパティ
    public class PropertyBuildItem<T, V> : IBuildItem<T>
    {
        private Func<T, V> getter;
        private Action<T, V> setter;

        // todo ちゃんとintは4とかになるかな？
        public int Size { get; } = System.Runtime.InteropServices.Marshal.SizeOf(typeof(V));

        public string Tag { get; set; }

        public PropertyBuildItem(Func<T, V> getter, Action<T, V> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public void Write(T model, BinaryWriter w)
        {
            var v = getter(model);
            var data =
                BinaryConverter.Instance.ToBytes(v);
            w.Write(data);
        }

        public void Read(T model, BinaryReader r, IDictionary<string, object> context)
        {
            var data = r.ReadBytes(Size);
            var v = BinaryConverter.Instance.ToModel<V>(data);
            setter(model, v);
            if (string.IsNullOrWhiteSpace(Tag) == false)
            {
                context[Tag] = v;
            }
        }
    }

}

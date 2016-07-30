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
        private IBinaryConverter converter;


        public string Tag { get; set; }

        public PropertyBuildItem(Func<T, V> getter, Action<T, V> setter, IBinaryConverter converter)
        {
            this.converter = converter;
            this.getter = getter;
            this.setter = setter;
        }

        public void Write(T model, BinaryWriter w)
        {
            var v = getter(model);
            var data =
                converter.ToBytes(v);
            w.Write(data);
        }

        public object Read(T model, BinaryReader r, IDictionary<string, object> context)
        {
            var data = r.ReadBytes(System.Runtime.InteropServices.Marshal.SizeOf(typeof(V)));
            var v = converter.ToModel<V>(data);
            setter(model, v);
            return v;
        }
    }

    


}

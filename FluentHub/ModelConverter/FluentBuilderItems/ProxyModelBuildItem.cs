using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public class ProxyModelBuildItem<T, V> : IBuildItem<T>
        where V : class, new()
    {
        private Func<T, V> getter;
        private Action<T, V> setter;
        private ModelBuilder<V> builder;


        public string Tag { get; set; }

        public ProxyModelBuildItem(Func<T, V> getter, Action<T, V> setter, ModelBuilder<V> builder)
        {
            this.builder = builder;
            this.getter = getter;
            this.setter = setter;
        }

        public void Write(T model, BinaryWriter w)
        {
            var v = getter(model);
            builder.ToBytes(w, v);
        }

        public object Read(T model, BinaryReader r, IDictionary<string, object> context)
        {
            var v = builder.ToModel(r);
            setter(model, v);
            return v;
        }
    }
}

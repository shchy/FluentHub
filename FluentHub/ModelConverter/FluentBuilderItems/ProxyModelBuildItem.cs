using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public class ProxyModelBuildItem<T, V> : IBuildItem<T>
    {
        private Func<T, V> getter;
        private Action<T, V> setter;
        private IModelBuilder<V> builder;

        public ProxyModelBuildItem(Func<T, V> getter, Action<T, V> setter, IModelBuilder<V> builder)
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

        public void Read(T model, BinaryReader r, IDictionary<string, object> context)
        {
            var v = builder.ToModel(r);
            setter(model, v);
        }

        public bool CanRead(BinaryReader r, IDictionary<string, object> context)
        {
            return
                builder.CanToModel(r, context);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    // 書き込むときはModelの値を参照するけど読み込みでは無視するプロパティ
    // ループカウンタなんかを想定
    public class GetPropertyBuildItem<T, V> : IBuildItem<T>
    {
        private Func<T, V> getter;
        private IBinaryConverter converter;

        // todo ちゃんとintは4とかになるかな？
        public int Size { get; } = System.Runtime.InteropServices.Marshal.SizeOf(typeof(V));

        public string Tag { get; set; }

        public GetPropertyBuildItem(Func<T, V> getter, IBinaryConverter converter)
        {
            this.converter = converter;
            this.getter = getter;
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
            var data = r.ReadBytes(Size);
            var v = converter.ToModel<V>(data);
            return v;
        }
    }
}

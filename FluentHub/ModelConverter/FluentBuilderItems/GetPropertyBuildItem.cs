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

        public object Read(T _, BinaryReader r, IDictionary<string, object> __)
        {
            var data = r.ReadBytes(GetReadSize(__));
            var v = converter.ToModel<V>(data);
            return v;
        }

        public int GetReadSize(IDictionary<string, object> _)
        {
            return
                System.Runtime.InteropServices.Marshal.SizeOf(typeof(V));
        }
    }
}

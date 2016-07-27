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

        // todo ちゃんとintは4とかになるかな？
        public int Size { get; } = System.Runtime.InteropServices.Marshal.SizeOf(typeof(V));

        public string Tag { get; set; }

        public GetPropertyBuildItem(Func<T, V> getter)
        {
            this.getter = getter;
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
            if (string.IsNullOrWhiteSpace(Tag) == false)
            {
                context[Tag] = v;
            }
        }
    }
}

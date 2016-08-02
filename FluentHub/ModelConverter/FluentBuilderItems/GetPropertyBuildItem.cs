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
    public class GetPropertyBuildItem<T, V> : ITaggedBuildItem<T>
    {
        private Func<T, V> getter;
        private IBinaryConverter converter;
        private int size;

        public string Tag { get; set; }
        public event Action<string, object> SetContextValue;

        public GetPropertyBuildItem(Func<T, V> getter, IBinaryConverter converter, int size)
        {
            this.converter = converter;
            this.getter = getter;
            this.size = size;
        }

        public void Write(T model, BinaryWriter w)
        {
            var v = getter(model);
            var data =
                converter.ToBytes(v);
            w.Write(data);
        }

        public void Read(T _, BinaryReader r, IDictionary<string, object> __)
        {
            var data = r.ReadBytes(size);
            var v = converter.ToModel<V>(data);
            SetContextValue(Tag, v);
        }
        

        public bool CanRead(BinaryReader r, IDictionary<string, object> __)
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

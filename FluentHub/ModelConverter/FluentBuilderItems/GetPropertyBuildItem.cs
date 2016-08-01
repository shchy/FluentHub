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

        public string Tag { get; set; }
        public event Action<string, object> SetContextValue;

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

        public void Read(T _, BinaryReader r, IDictionary<string, object> __)
        {
            var data = r.ReadBytes(Size);
            var v = converter.ToModel<V>(data);
            SetContextValue(Tag, v);
        }

        int Size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(V));


        public bool CanRead(BinaryReader r, IDictionary<string, object> __)
        {
            var remain = r.BaseStream.Length - r.BaseStream.Position;
            if (remain < Size)
            {
                return false;
            }
            var data = r.ReadBytes(Size);
            var v = converter.ToModel<V>(data);
            SetContextValue(Tag, v);
            return true;
        }
    }
}

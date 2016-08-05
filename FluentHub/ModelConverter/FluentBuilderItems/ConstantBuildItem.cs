using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    // 固定値
    public class ConstantBuildItem<T, V> : ITaggedBuildItem<T>
    {
        private V value;
        private IBinaryConverter converter;
        private int size;

        public event Action<string, object> SetContextValue;

        public string Tag { get; set; }

        public ConstantBuildItem(V value, IBinaryConverter converter, int size)
        {
            this.converter = converter;
            this.value = value;
            this.size = size;
        }

        public void Write(T _, BinaryWriter w)
        {
            // 固定値なのでmodel使わない
            var data =
                converter.ToBytes(value);
            w.Write(data);
        }

        public void Read(T _, BinaryReader r, IDictionary<string, object> __)
        {
            // 固定値なのでmodel使わない読み捨てる
            var data = r.ReadBytes(size);
            var v = converter.ToModel<V>(data);
            // 一応固定値と照合する
            if (converter.Equal(v, value) == false)
            {
                throw new Exception($"{v} != { value }");
            }
            SetContextValue(Tag, v);
        }

        public bool CanRead(BinaryReader r, IDictionary<string, object> __)
        {
            var remain = r.BaseStream.Length - r.BaseStream.Position;
            if (remain < size)
            {
                return false;
            }

            // 固定値なのでmodel使わない読み捨てる
            var data = r.ReadBytes(size);
            var v = converter.ToModel<V>(data);
            // 一応固定値と照合する
            var isEqual = converter.Equal(v, value);
            SetContextValue(Tag, v);
            return isEqual;
        }
    }
}

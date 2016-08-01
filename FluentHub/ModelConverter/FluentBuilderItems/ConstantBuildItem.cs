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

        public event Action<string, object> SetContextValue;

        public string Tag { get; set; }

        public ConstantBuildItem(V value, IBinaryConverter converter)
        {
            this.converter = converter;
            this.value = value;
        }

        public void Write(T _, BinaryWriter w)
        {
            // 固定値なのでmodel使わない
            // todo BinaryConverterには死ぬほど依存するけどしょうがない
            var data =
                converter.ToBytes(value);
            w.Write(data);
        }

        public void Read(T _, BinaryReader r, IDictionary<string, object> __)
        {
            // 固定値なのでmodel使わない読み捨てる
            var data = r.ReadBytes(Size);
            var v = converter.ToModel<V>(data);
            // 一応固定値と照合する
            if (converter.Equal(v, value) == false)
            {
                throw new Exception($"{v} != { value }");
            }
            SetContextValue(Tag, v);
        }

        // todo move in converter
        int Size
        {
            get
            {
                if (typeof(V).IsArray)
                {
                    var gType = typeof(V).GetElementType();
                    var count = (this.value as Array).Length;
                    return System.Runtime.InteropServices.Marshal.SizeOf(gType) * count;
                }
                if (typeof(IEnumerable).IsAssignableFrom(typeof(V)))
                {

                    var gType = typeof(V).GetGenericArguments()[0];
                    var count = (this.value as IEnumerable).OfType<object>().Count();
                    return System.Runtime.InteropServices.Marshal.SizeOf(gType) * count;

                }
                else
                {
                    return System.Runtime.InteropServices.Marshal.SizeOf(this.value);
                }
            }
        }

        public bool CanRead(BinaryReader r, IDictionary<string, object> __)
        {
            var remain = r.BaseStream.Length - r.BaseStream.Position;
            if (remain < Size)
            {
                return false;
            }

            // 固定値なのでmodel使わない読み捨てる
            var data = r.ReadBytes(Size);
            var v = converter.ToModel<V>(data);
            // 一応固定値と照合する
            var isEqual = converter.Equal(v, value);
            SetContextValue(Tag, v);
            return isEqual;
        }
        
    }

}

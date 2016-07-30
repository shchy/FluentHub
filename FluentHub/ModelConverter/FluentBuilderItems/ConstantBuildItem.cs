using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    // 固定値
    public class ConstantBuildItem<T, V> : IBuildItem<T>
        where V : struct
    {
        private V value;
        private IBinaryConverter converter;

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

        public object Read(T _, BinaryReader r, IDictionary<string, object> context)
        {
            // 固定値なのでmodel使わない読み捨てる
            var data = r.ReadBytes(System.Runtime.InteropServices.Marshal.SizeOf(typeof(V)));
            var v = converter.ToModel<V>(data);
            // 一応固定値と照合する
            if (v.Equals(value) == false)
            {
                throw new Exception($"{v} != { value }");
            }
            return v;
        }
    }

}

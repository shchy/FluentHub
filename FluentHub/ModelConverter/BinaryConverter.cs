using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter
{
    using ToBytes = Func<object, byte[]>;
    using ToValue = Func<byte[], object>;
    using Converter = Tuple<Func<object, byte[]>, Func<byte[], object>>;

    public class BinaryConverter
    {
        public static BinaryConverter Instance { get; private set; } = new BinaryConverter();

        private Dictionary<Type, Tuple<ToBytes, ToValue>> converters;

        private BinaryConverter()
        {
            this.converters = new Dictionary<Type, Converter>();
            // todo もっと
            RegisterConverter(typeof(int), m => BitConverter.GetBytes((int)m), data => BitConverter.ToInt32(data, 0));
        }

        public void RegisterConverter(Type t, ToBytes toBytes, ToValue toValue)
        {
            this.converters.Add(t, Tuple.Create(toBytes, toValue));
        }

        public byte[] ToBytes<T>(T v)
        {
            var key = typeof(T);
            if (converters.ContainsKey(key) == false)
            {
                throw new Exception($"{key.Name} is not registed");
            }
            return
                converters[key].Item1(v);
        }

        public T ToModel<T>(byte[] data)
        {
            var key = typeof(T);
            if (converters.ContainsKey(key) == false)
            {
                throw new Exception($"{key.Name} is not registed");
            }
            return
                (T)converters[key].Item2(data);
        }
    }
}

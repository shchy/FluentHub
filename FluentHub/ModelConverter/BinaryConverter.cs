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

            RegisterConverter(typeof(bool), m => BitConverter.GetBytes((bool)m), data => BitConverter.ToBoolean(data, 0));
            RegisterConverter(typeof(char), m => BitConverter.GetBytes((char)m), data => BitConverter.ToChar(data, 0));
            RegisterConverter(typeof(short), m => BitConverter.GetBytes((short)m), data => BitConverter.ToInt16(data, 0));
            RegisterConverter(typeof(int), m => BitConverter.GetBytes((int)m), data => BitConverter.ToInt32(data, 0));
            RegisterConverter(typeof(long), m => BitConverter.GetBytes((long)m), data => BitConverter.ToInt64(data, 0));
            RegisterConverter(typeof(ushort), m => BitConverter.GetBytes((ushort)m), data => BitConverter.ToUInt16(data, 0));
            RegisterConverter(typeof(uint), m => BitConverter.GetBytes((uint)m), data => BitConverter.ToUInt32(data, 0));
            RegisterConverter(typeof(ulong), m => BitConverter.GetBytes((ulong)m), data => BitConverter.ToUInt64(data, 0));
            RegisterConverter(typeof(float), m => BitConverter.GetBytes((float)m), data => BitConverter.ToSingle(data, 0));
            RegisterConverter(typeof(double), m => BitConverter.GetBytes((double)m), data => BitConverter.ToDouble(data, 0));
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter
{
    using ToBytes = Func<object, byte[]>;
    using ToValue = Func<byte[], object>;
    using GetSize = Func<int>;
    using Converter = Tuple<Func<object, byte[]>, Func<byte[], object>, Func<int>>;
    using Eq = Func<object, object, bool>;
    using System.Collections;

    public interface IBinaryConverter
    {
        void RegisterEqual<T>(Eq eq);
        void RegisterConverter<T>(ToBytes toBytes, ToValue toValue, GetSize getSize);

        byte[] ToBytes<T>(T v);
        T ToModel<T>(byte[] data);
        bool Equal<T>(T x, T y);
        int GetTypeSize<T>();
    }

    public class BinaryConverter : IBinaryConverter
    {
        private Dictionary<Type, Converter> converters;
        private Dictionary<Type, Eq> equals;

        public BinaryConverter()
        {
            this.converters = new Dictionary<Type, Converter>();
            this.equals = new Dictionary<Type, Eq>();
        }

        public bool Equal<T>(T x, T y)
        {
            var key = this.equals.Keys.FirstOrDefault(k => typeof(T).IsSubclassOf(k));
            if (key != null)
            {
                return this.equals[key](x, y);
            }
            else
            {
                return x.Equals(y);
            }
        }

        public void RegisterEqual<T>(Eq eq)
        {
            this.equals[typeof(T)] = eq;
        }

        public void RegisterConverter<T>( ToBytes toBytes, ToValue toValue, GetSize getSize)
        {
            this.converters[typeof(T)] = Tuple.Create(toBytes, toValue, getSize);
        }

        public int GetTypeSize<T>()
        {
            var key = typeof(T);
            if (this.converters.ContainsKey(key) == false)
            {
                return GetDefaultTypeSize<T>();
            }
            return this.converters[key].Item3();
        }

        int GetDefaultTypeSize<T>()
        {
            if (typeof(T).IsEnum)
            {
                return System.Runtime.InteropServices.Marshal.SizeOf(Enum.GetUnderlyingType(typeof(T)));
            }
            else
            {
                return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            }
        }

        public byte[] ToBytes<T>(T v)
        {
            var key = typeof(T);
            // byte[]だったら変換の必要がないのでスルー
            if (key == typeof(byte[]))
            {
                return v as byte[];
            }

            if (typeof(T).IsEnum)
            {
                key = Enum.GetUnderlyingType(typeof(T));
            }
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
            // byte[]だったら変換の必要がないのでスルー
            if (key == typeof(byte[]))
            {
                var box = data as object;
                return (T)box;
            }

            if (typeof(T).IsEnum)
            {
                key = Enum.GetUnderlyingType(typeof(T));
            }
            if (converters.ContainsKey(key) == false)
            {
                throw new Exception($"{key.Name} is not registed");
            }
            return
                (T)converters[key].Item2(data);
        }
    }
}

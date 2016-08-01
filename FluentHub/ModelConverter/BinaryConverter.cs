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
    using Eq = Func<object, object, bool>;

    public interface IBinaryConverter
    {
        void RegisterConverter(Type t, ToBytes toBytes, ToValue toValue);
        byte[] ToBytes<T>(T v);
        T ToModel<T>(byte[] data);
        bool Equal<T>(T x, T y);
        void RegisterEqual(Type t, Eq eq);
    }

    public class BinaryConverter : IBinaryConverter
    {
        private Dictionary<Type, Tuple<ToBytes, ToValue>> converters;
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

        public void RegisterEqual(Type t, Eq eq)
        {
            this.equals[t] = eq;
        }


        public void RegisterConverter(Type t, ToBytes toBytes, ToValue toValue)
        {
            this.converters[t] = Tuple.Create(toBytes, toValue);
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

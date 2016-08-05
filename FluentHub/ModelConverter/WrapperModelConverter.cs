using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter
{
    public abstract class WrapperModelConverter<T> : IModelConverter<T>
    {
        private IModelConverter<T> real;

        public WrapperModelConverter()
        {
            this.real = MakeConverter();
        }

        protected abstract IModelConverter<T> MakeConverter();

        public bool CanBytesToModel(IEnumerable<byte> bytes)
        {
            return this.real.CanBytesToModel(bytes);
        }

        public bool CanModelToBytes(object model)
        {
            return this.real.CanModelToBytes(model);
        }

        public byte[] ToBytes(T model)
        {
            return this.real.ToBytes(model);
        }

        public Tuple<T, int> ToModel(IEnumerable<byte> bytes)
        {
            return this.real.ToModel(bytes);
        }
    }

    public abstract class WrapperModelConverter<T, U> : IModelConverter<T>
        where U : T
    {
        private IModelConverter<T> real;

        public WrapperModelConverter()
        {
            this.real = MakeConverter()
                .ToBaseTypeConverter<U, T>();
        }

        protected abstract IModelConverter<U> MakeConverter();

        public bool CanBytesToModel(IEnumerable<byte> bytes)
        {
            return this.real.CanBytesToModel(bytes);
        }

        public bool CanModelToBytes(object model)
        {
            return this.real.CanModelToBytes(model);
        }

        public byte[] ToBytes(T model)
        {
            return this.real.ToBytes(model);
        }

        public Tuple<T, int> ToModel(IEnumerable<byte> bytes)
        {
            return this.real.ToModel(bytes);
        }
    }
}

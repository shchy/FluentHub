using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter
{
    public class DefaultModelConverter<T,U> : IModelConverter<T>
        where U : class, T, new()
        where T : class
    {
        private int modelSize;

        public DefaultModelConverter()
        {
            using (var ms = new MemoryStream())
            {
                var f = new BinaryFormatter();
                f.Serialize(ms, new U());
                var bytes = ms.ToArray();
                this.modelSize = bytes.Length;
            }
        }

        public bool CanBytesToModel(IEnumerable<byte> bytes)
        {
            if (bytes.Count() < modelSize)
            {
                return false;
            }
            try
            {
                return ToModel(bytes).Item2 == modelSize;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CanModelToBytes(object model)
        {
            return model is U;
        }

        public byte[] ToBytes(T model)
        {
            var m = model as U;
            using (var ms = new MemoryStream())
            {
                var f = new BinaryFormatter();
                f.Serialize(ms, m);
                return ms.ToArray();
            }
        }

        public Tuple<T, int> ToModel(IEnumerable<byte> bytes)
        {
            var taked = bytes.Take(this.modelSize).ToArray();
            using (var ms = new MemoryStream(taked))
            {
                var f = new BinaryFormatter();
                var obj = f.Deserialize(ms);
                return
                    Tuple.Create((U)obj as T, this.modelSize);
            }
        }
    }
}

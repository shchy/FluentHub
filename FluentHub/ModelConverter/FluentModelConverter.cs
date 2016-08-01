using FluentHub.ModelConverter.FluentBuilderItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter
{
    public class FluentModelConverter<T> : IModelConverter<T>
        where T : class, new()
    {
        private ModelBuilder<T> builder;

        public FluentModelConverter(ModelBuilder<T> builder)
        {
            this.builder = builder;
        }

        public bool CanBytesToModel(IEnumerable<byte> bytes)
        {
            try
            {
                using (var ms = new MemoryStream(bytes.ToArray()))
                using (var r = new BinaryReader(ms))
                {
                    var result = builder.CanToModel(r);
                    return result.Item1;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CanModelToBytes(object model)
        {
            return model is T;
        }

        public byte[] ToBytes(T model)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                builder.ToBytes(w, model );
                w.Flush();
                return ms.ToArray();
            }
        }

        public Tuple<T, int> ToModel(IEnumerable<byte> bytes)
        {
            using (var ms = new MemoryStream(bytes.ToArray()))
            using (var r = new BinaryReader(ms))
            {
                var model =  builder.ToModel(r);
                var readed = ms.Position;
                return
                    Tuple.Create(model, (int)readed);
            }
        }
    }
}

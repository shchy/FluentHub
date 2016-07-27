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

        // todo 識別しくらいまで見たいときは？ID専用のリストを作る？IsIDみたいな？
        public bool CanBytesToModel(IEnumerable<byte> bytes)
        {
            // todo 速度に問題でそうなのでなんとかする。
            // todo 全長や可変長にしても最低限のサイズとか
            // todo 識別子まで読んで確認とか

            // todo 固定値識別しがないとなんでも読めちゃう
            try
            {
                using (var ms = new MemoryStream(bytes.ToArray()))
                using (var r = new BinaryReader(ms))
                {
                    var model = builder.ToModel(r);
                }
                return true;
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

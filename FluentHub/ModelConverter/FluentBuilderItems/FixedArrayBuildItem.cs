using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public class FixedArrayBuildItem<T, VModel> : IBuildItem<T>
        where VModel : class, new()
    {
        private ModelBuilder<VModel> childBuilder;
        private Func<T, IEnumerable<VModel>> getter;
        private Action<T, IEnumerable<VModel>> setter;
        private int loopCount;

        public int Size { get; }
        public string Tag { get; set; }

        public FixedArrayBuildItem(ModelBuilder<VModel> childBuilder
            , Func<T, IEnumerable<VModel>> getter
            , Action<T, IEnumerable<VModel>> setter
            , int loopCount)
        {
            this.childBuilder = childBuilder;
            this.getter = getter;
            this.setter = setter;
            this.loopCount = loopCount;
        }

        public void Write(T model, BinaryWriter w)
        {
            // 変換する配列をもらう
            var query = getter(model);
            var array = query.ToArray();
            for (int i = 0; i < loopCount; i++)
            {
                var item = null as VModel;
                if (array.Length > i)
                {
                    item = array[i];
                }
                else
                {
                    // todo 要素数が足りなかったら？
                    item = new VModel();
                } 
                this.childBuilder.ToBytes(w, item);
            }
        }

        public object Read(T model, BinaryReader r, IDictionary<string, object> context)
        {
            var list = new List<VModel>();
            for (var i = 0; i < loopCount; i++)
            {
                var vModel = this.childBuilder.ToModel(r);
                list.Add(vModel);
            }
            setter(model, list);
            return list;
        }
    }
}

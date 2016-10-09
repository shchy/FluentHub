using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public class ArrayBuildItem<T, VModel> : IBuildItem<T>
    {
        private IModelBuilder<VModel> childBuilder;
        private Func<T, IEnumerable<VModel>> getter;
        private Action<T, IEnumerable<VModel>> setter;
        private Func<IDictionary<string, object>, int> getLoopCount;

        public ArrayBuildItem(IModelBuilder<VModel> childBuilder
            , Func<T, IEnumerable<VModel>> getter
            , Action<T, IEnumerable<VModel>> setter
            , Func<IDictionary<string, object>, int> getLoopCount)
        {
            this.childBuilder = childBuilder;
            this.getter = getter;
            this.setter = setter;
            this.getLoopCount = getLoopCount;
        }

        public void Write(T model, BinaryWriter w)
        {
            // 変換する配列をもらう
            var array = getter(model);
            foreach (var item in array)
            {
                this.childBuilder.ToBytes(w, item);
            }
        }
        
        public void Read(T model, BinaryReader r, IDictionary<string, object> context)
        {
            var loopCount = getLoopCount(context);
                //GetLoopCount(this.loopCountName, context);
            var list = new List<VModel>();
            for (var i = 0; i < loopCount; i++)
            {
                var vModel = this.childBuilder.ToModel(r);
                list.Add(vModel);
            }
            setter(model, list);
        }

        public bool CanRead(BinaryReader r, IDictionary<string, object> context)
        {
            var loopCount = getLoopCount(context);
            //GetLoopCount(this.loopCountName, context);
            for (var i = 0; i < loopCount; i++)
            {
                var result = this.childBuilder.CanToModel(r, context);
                if (result == false)
                    return false;
            }
            return true;
        }
    }
}

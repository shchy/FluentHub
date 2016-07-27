using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public class ArrayBuildItem<T, Array, VModel> : IBuildItem<T>
        where VModel : class, new()
        where Array : IEnumerable<VModel>
    {
        private ModelBuilder<VModel> childBuilder;
        private Func<T, Array> getter;
        private Action<T, IEnumerable<VModel>> setter;
        private string loopCountName;

        public int Size { get; }
        public string Tag { get; set; }

        public ArrayBuildItem(ModelBuilder<VModel> childBuilder, Func<T, Array> getter, Action<T, IEnumerable<VModel>> setter, string loopCountName)
        {
            this.childBuilder = childBuilder;
            this.getter = getter;
            this.setter = setter;
            this.loopCountName = loopCountName;
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

        // todo 長さを外からもらうことにしてみるModelBuilderと結合しちゃうね。Setで一律変数もらう？
        public void Read(T model, BinaryReader r, IDictionary<string, object> context)
        {
            var loopCount = GetLoopCount(this.loopCountName, context);
            var list = new List<VModel>();
            for (var i = 0ul; i < loopCount; i++)
            {
                var vModel = this.childBuilder.ToModel(r);
                list.Add(vModel);
            }
            setter(model, list);

            if (string.IsNullOrWhiteSpace(Tag) == false)
            {
                context[Tag] = list;
            }
        }

        ulong GetLoopCount(string keyName, IDictionary<string, object> _context)
        {
            if (_context.ContainsKey(keyName) == false)
            {
                throw new Exception($"{keyName} is not found");
            }
            var contextValue = _context[keyName];
            var loopcount = 0uL;
            if (ulong.TryParse(contextValue.ToString(), out loopcount) == false)
            {
                throw new Exception($"{contextValue} is not unsigned number");
            }
            return loopcount;
        }
    }

}

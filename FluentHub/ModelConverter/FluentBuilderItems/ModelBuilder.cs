using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public class ModelBuilder<TModel>
        where TModel : class, new()
    {
        private List<IBuildItem<TModel>> buildItems;
        private Action<TModel> init;
        public IBinaryConverter Converter { get; set; }

        public ModelBuilder()
        {
            // default converter
            this.Converter = new BinaryConverter() as IBinaryConverter;
            this.buildItems = new List<IBuildItem<TModel>>();
            this.init = _ => { };
        }

        public void RegisterInit(Action<TModel> init)
        {
            this.init = init;
        }

        public void AddBuildItem(IBuildItem<TModel> item)
        {
            this.buildItems.Add(item);
        }

        // todo buildItemをチェーンにしておけばいいのか。
        public void SetTagForLastOne(string tagName)
        {
            var lastOne = buildItems.Last();
            lastOne.Tag = tagName;
        }

        public TModel ToModel(BinaryReader r)
        {
            var context = new Dictionary<string, object>();
            var model = new TModel();
            // ここでメンバのインナークラスなどを初期化してもらう
            this.init(model);

            // 登録したビルド情報に従って電文からモデルを構築する
            foreach (var item in buildItems)
            {
                // 構築
                var result = item.Read(model, r, context);
                if (string.IsNullOrWhiteSpace(item.Tag) == false)
                {
                    context[item.Tag] = result;
                }
            }
            return model;
        }

        public void ToBytes(BinaryWriter w, TModel model)
        {
            // 登録したビルド情報に従って分解する
            foreach (var item in buildItems)
            {
                // パーツ分をwに書き込み
                item.Write(model, w);
            }
        }
    }

}

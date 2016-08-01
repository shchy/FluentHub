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
            var lastOne = buildItems.Last() as ITaggedBuildItem<TModel>;
            if (lastOne == null)
            {
                return;
            }
            lastOne.Tag = tagName;
        }
        
        public bool CanToModel(BinaryReader r, IDictionary<string, object> context = null)
        {
            if (context == null)
            {
                context = new Dictionary<string, object>();
            }

            var tempContextValueSetter = (Action<string, object>)((tag, val) =>
             {
                 // AsTagされてたらコンテキストに生成した値を入れておく
                 if (string.IsNullOrWhiteSpace(tag) == false)
                 {
                     context[tag] = val;
                 }
             });

            // 登録したビルド情報に従って電文からモデルを構築する
            foreach (var item in buildItems)
            {
                var taggedItem = item as ITaggedBuildItem<TModel>;
                if (taggedItem != null)
                {
                    taggedItem.SetContextValue += tempContextValueSetter;
                }
                var result = item.CanRead(r, context);
                if (taggedItem != null)
                {
                    taggedItem.SetContextValue -= tempContextValueSetter;
                }
                if (result == false)
                {
                    return false;
                }
            }
            return true;
        }
        

        public TModel ToModel(BinaryReader r, IDictionary<string,object> context = null)
        {
            if (context == null)
            {
                context = new Dictionary<string, object>();
            }
            var model = new TModel();
            // ここでメンバのインナークラスなどを初期化してもらう
            this.init(model);

            var tempContextValueSetter = (Action<string, object>)((tag, val) =>
            {
                // AsTagされてたらコンテキストに生成した値を入れておく
                if (string.IsNullOrWhiteSpace(tag) == false)
                {
                    context[tag] = val;
                }
            });

            // 登録したビルド情報に従って電文からモデルを構築する
            foreach (var item in buildItems)
            {
                var taggedItem = item as ITaggedBuildItem<TModel>;
                if (taggedItem != null)
                {
                    taggedItem.SetContextValue += tempContextValueSetter;
                }
                item.Read(model, r, context);
                if (taggedItem != null)
                {
                    taggedItem.SetContextValue -= tempContextValueSetter;
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public interface IModelBuilder<TModel> : IBuildItemChain<TModel, IBuildItem<TModel>>
        where TModel : class, new()
    {
        void RegisterInit(Action<TModel> init);
        bool CanToModel(BinaryReader r, IDictionary<string, object> context = null);
        TModel ToModel(BinaryReader r, IDictionary<string, object> context = null);
        void ToBytes(BinaryWriter w, TModel model);
    }

    public class ModelBuilder<TModel> : IModelBuilder<TModel>
        where TModel : class, new()
    {
        private Action<TModel> init;

        public IBinaryConverter Converter { get; set; }

        #region IBuildItemChain
        public IModelBuilder<TModel> Builder => this;

        public IBuildItem<TModel> Value { get; } = null;

        public IChain<IBuildItem<TModel>> Next { get; set; }
        #endregion

        public ModelBuilder()
        {
            // default converter
            this.Converter = new BinaryConverter() as IBinaryConverter;
            this.init = _ => { };
        }

        public void RegisterInit(Action<TModel> init)
        {
            this.init = init;
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
            foreach (var item in this.GetEnumerable())
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
            foreach (var item in this.GetEnumerable())
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
            foreach (var item in this.GetEnumerable())
            {
                // パーツ分をwに書き込み
                item.Write(model, w);
            }
        }
    }

}

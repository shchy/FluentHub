using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public class BuildItemChain<T, BuildItem> : IBuildItemChain<T, BuildItem>
        where BuildItem : IBuildItem<T>
    {
        public IModelBuilder<T> Builder { get; set; }

        public IBinaryConverter Converter { get; set; }

        public IChain<IBuildItem<T>> Next { get; set; }

        public IBuildItem<T> Value { get; set; }
    }
}

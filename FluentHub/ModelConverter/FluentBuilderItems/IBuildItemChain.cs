using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public interface IBuildItemChain<T, out BuildItem> : IChain<IBuildItem<T>>
        where BuildItem : IBuildItem<T>
    {
        IBinaryConverter Converter { get; }
        IModelBuilder<T> Builder { get; }
    }
}

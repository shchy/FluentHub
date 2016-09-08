using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public static class BuildItemChainExtension
    {
        public static IBuildItemChain<T, BuildItemTo> SetNext<T, BuildItemTo>(this IBuildItemChain<T, IBuildItem<T>> @this, BuildItemTo v)
            where BuildItemTo : IBuildItem<T>
        {
            var chain = new BuildItemChain<T, BuildItemTo>
            {
                Builder = @this.Builder,
                Converter = @this.Converter,
                Value = v,
            };
            @this.Next = chain as IChain<IBuildItem<T>>;
            return chain;
        }

        public static IEnumerable<T> GetEnumerable<T>(this IChain<T> @this)
        {
            var next = @this;
            while (next != null)
            {
                if (next.Value != null)
                {
                    yield return next.Value;
                }
                next = next.Next;
            }
        }
    }
}

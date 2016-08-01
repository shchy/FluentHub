using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public interface IChain<T>
    {
        T Value { get; }
        IChain<T> Next { get; set; }
    }
}

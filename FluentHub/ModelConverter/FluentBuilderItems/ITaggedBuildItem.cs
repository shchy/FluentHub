using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public interface ITaggedBuildItem<T> : IBuildItem<T>
    {
        string Tag { get; set; }
        event Action<string, object> SetContextValue;
    }
}

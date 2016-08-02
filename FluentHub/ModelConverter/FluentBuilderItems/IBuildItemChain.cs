﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    public interface IBuildItemChain<T, BuildItem> : IChain<IBuildItem<T>>
        where BuildItem : IBuildItem<T>
        where T : class, new()
    {
        IBinaryConverter Converter { get; }
        IModelBuilder<T> Builder { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub.Module
{
    public interface IModuleInjection
    {
        object Resolve(Type type);
        void Add(Type type, Func<object> resolver);
    }
}

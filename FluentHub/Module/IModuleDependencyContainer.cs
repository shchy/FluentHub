using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Module
{
    public interface IModuleDependencyContainer
    {
        object Resolve(Type type);
        void Add<T>(Func<T> resolver);
        void Add<T, U>(Func<U> resolver)
            where U : T;
        event Func<Type, object> Missed;
    }
}

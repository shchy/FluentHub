using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub.Module
{
    public class ModuleInjection : IModuleInjection
    {
        private IModuleInjection parent;
        private Dictionary<Type, Func<object>> resolvers;

        public ModuleInjection(): this(null)
        {
        }

        public ModuleInjection(IModuleInjection parent)
        {
            this.parent = parent;
            this.resolvers = new Dictionary<Type, Func<object>>();
        }

        public void Add<T>(Func<T> resolver)
        {
            this.resolvers[typeof(T)] = ()=>resolver();
        }

        public void Add<T, U>(Func<U> resolver) where U : T
        {
            this.resolvers[typeof(T)] = () => resolver();
        }

        public virtual object Resolve(Type type)
        {
            if (this.resolvers.ContainsKey(type))
            {
                return this.resolvers[type]();
            }
            else if (this.parent != null)
            {
                return this.parent.Resolve(type);
            }
            return null;
        }
    }
}

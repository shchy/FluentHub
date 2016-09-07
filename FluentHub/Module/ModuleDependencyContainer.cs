using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Module
{
    public class ModuleDependencyContainer : IModuleDependencyContainer
    {
        private IModuleDependencyContainer parent;
        private Dictionary<Type, Func<object>> resolvers;

        public ModuleDependencyContainer(): this(null)
        {
        }

        public ModuleDependencyContainer(IModuleDependencyContainer parent)
        {
            this.parent = parent;
            this.resolvers = new Dictionary<Type, Func<object>>();
        }

        public event Func<Type, object> Missed;

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
            else if (Missed != null)
            {
                return Missed(type);
            }
            return null;
        }
    }
}

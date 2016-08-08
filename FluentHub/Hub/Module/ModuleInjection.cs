using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub.Module
{
    public class ModuleInjection : IModuleInjection
    {
        private Dictionary<Type, Func<object>> resolvers;

        public ModuleInjection()
        {
            this.resolvers = new Dictionary<Type, Func<object>>();
        }

        public void Add(Type type, Func<object> resolver)
        {
            this.resolvers[type] = resolver;
        }

        public object Resolve(Type type)
        {
            if (this.resolvers.ContainsKey(type))
            {
                return this.resolvers[type]();
            }
            return null;
        }
    }
}

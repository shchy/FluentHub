using System;
using FluentHub.Hub.Module;
using Microsoft.Practices.Unity;

namespace FluentHub.Unity
{
    public class UnityModuleInjection : IModuleInjection
    {
        public IUnityContainer Container { get; private set; }

        public UnityModuleInjection(IUnityContainer container)
        {
            this.Container = container;
        }

        public event Func<Type, object> Missed;

        public void Add<T>(Func<T> resolver)
        {
            this.Container.RegisterType<T>(new InjectionFactory(_=> resolver()));
        }

        public void Add<T, U>(Func<U> resolver) where U : T
        {
            this.Container.RegisterType<T>(new InjectionFactory(_ => resolver()));
        }

        public object Resolve(Type type)
        {
            if (Container.IsRegistered(type))
            {
                return this.Container.Resolve(type);
            }
            else if (Missed != null)
            {
                return Missed(type);
            }
            return null;
        }
    }
}
using System;
using FluentHub.Hub.Module;
using Microsoft.Practices.Unity;

namespace FluentHub.Unity
{
    internal class UnityModuleInjection : IModuleInjection
    {
        private IUnityContainer container;

        public UnityModuleInjection(IUnityContainer container)
        {
            this.container = container;
        }

        public void Add<T>(Func<T> resolver)
        {
            this.container.RegisterType<T>(new InjectionFactory(_=> resolver()));
        }

        public object Resolve(Type type)
        {
            return this.container.Resolve(type);
        }
    }
}
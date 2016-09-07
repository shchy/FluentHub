﻿using System;
using Microsoft.Practices.Unity;
using FluentHub.Module;

namespace FluentHub.Unity
{
    public class UnityModuleDependencyContainer : IModuleDependencyContainer
    {
        public IUnityContainer Container { get; private set; }

        public UnityModuleDependencyContainer(IUnityContainer container)
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
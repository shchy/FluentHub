﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using FluentHub.Unity;
using FluentHub.Logger;

namespace FluentHub
{
    public static class UnityExtension
    {
        public static ContainerBootstrap RegisterModule<Module>(
            this ContainerBootstrap @this)
        {
            var unity = @this.DependencyContainer as UnityModuleDependencyContainer;
            var container = unity.Container;
            var module = container.Resolve<Module>();
            return @this.RegisterModule<Module>(()=> module);
        }
    }
}

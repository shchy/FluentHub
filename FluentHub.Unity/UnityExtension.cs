﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using FluentHub.Unity;
using FluentHub.Logger;

namespace FluentHub.Hub
{
    public static class UnityExtension
    {
        public static IApplicationContainer RegisterModule<Module>(
            this IApplicationContainer @this)
        {
            var unity = @this.ModuleInjection as UnityModuleInjection;
            var container = unity.Container;
            return @this.RegisterModule<Module>(()=> container.Resolve<Module>());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using FluentHub.Unity;
using FluentHub.Logger;
using FluentHub.Hub;

namespace FluentHub
{
    public static class UnityExtension
    {
        public static ContainerBootstrap RegisterModule<Module>(
            this ContainerBootstrap @this)
        {
            @this.OtherBuilders.Add(new FakeBuilder(() =>
            {
                var unity = @this.DependencyContainer as UnityModuleDependencyContainer;
                var container = unity.Container;
                var module = container.Resolve<Module>();
                @this.RegisterModule<Module>(() => module);
            }));
            return @this;
        }
    }

    class FakeBuilder : IBuilder
    {
        private Action build;

        public FakeBuilder(Action build)
        {
            this.build = build;
        }


        public void Build(IApplicationContainer _)
        {
            this.build();
        }
    }
}

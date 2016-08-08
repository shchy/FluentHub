using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using FluentHub.Unity;

namespace FluentHub.Hub
{
    public static class UnityExtension
    {
        /// <summary>
        /// Module型のクラスが持っているPublicメソッドをAppのシーケンスメソッドとして登録する
        /// </summary>
        /// <typeparam name="Module"></typeparam>
        /// <param name="this"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public static IApplicationContainer RegisterModule<Module>(
            this IApplicationContainer @this
            , IUnityContainer container)
        {
            // Moduleのメソッド引数を解決するDIコンテナ
            var resolver = new UnityModuleInjection(container);
            return @this.RegisterModule(resolver, () => container.Resolve<Module>());
        }
    }
}

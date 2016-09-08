using FluentHub.Hub;
using FluentHub.IO;
using FluentHub.IO.Extension;
using FluentHub.Logger;
using FluentHub.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub
{
    public class ContainerBootstrap
    {
        public ILogger Logger { get; set; } = new DefaultLogger();

        public IApplicationContainer Container { get; set; }

        public IModuleDependencyContainer DependencyContainer { get; set; }

        public List<IAppBuilder> Builders { get; set; } = new List<IAppBuilder>();

        public ContainerBootstrap()
        {
            this.DependencyContainer = new ModuleDependencyContainer();
        }

        public void Build()
        {
            this.Container = new ApplicationContainer(this.Logger, this.DependencyContainer);

            foreach (var builder in Builders)
            {
                builder.Build(Container);
            }
        }

        public void Run()
        {
            Build();
            ContainerRun();
        }

        public Task RunAsync()
        {
            Build();

            return Task.Run((Action)ContainerRun);
        }

        private void ContainerRun()
        {
            this.DependencyContainer.Missed += ModuleInjection_Missed;

            Container.Run();

            this.DependencyContainer.Missed -= ModuleInjection_Missed;
        }

        // todo どこかへ移動したい
        // DIに失敗した時のイベントでそれがIEnumerable<ISessionContext<AppIF,SessionType>>だったら、ここでしか救済できないのでここでする。
        // どこかに委譲したい
        private object ModuleInjection_Missed(Type type)
        {
            if (typeof(IEnumerable<>) != type.GetGenericTypeDefinition())
            {
                return null;
            }
            // 
            var genericType = type.GetGenericArguments()[0];
            if (typeof(ISessionContext<,>) != genericType.GetGenericTypeDefinition())
            {
                return null;
            }

            // ISessionContext型を求めていたら
            // APPIFをチェック
            var appType = genericType.GetGenericArguments()[0];
            // 一致するアプリを取得
            var app = this.Container.GetApps().Where(x => x.GetType().GetGenericArguments()[0] == appType).FirstOrDefault();
            if (app == null)
            {
                return null;
            }

            // 求められているISessionの実装型を取得
            var sessionType = genericType.GetGenericArguments()[1];

            // ModelContext達を取得
            var contexts = app.GetContextsNotTyped();

            var query =
                from context in contexts
                let session = app.GetSessionNotTyped(context, sessionType)
                where session != null
                select new { context, session };
            var impl = query.ToArray();


            var sessionContexts =
                this.GetType()
                .GetMethod(nameof(MakeSessionContexts), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .MakeGenericMethod(appType, sessionType)
                .Invoke(this, new object[] { impl.Select(x => x.context).ToArray(), impl.Select(x => x.session).ToArray() });
            return sessionContexts;
        }

        private IEnumerable<ISessionContext<AppIF, SessionType>> MakeSessionContexts<AppIF, SessionType>(IEnumerable<object> contexts, IEnumerable<ISession> sessions)
            where SessionType : ISession
        {
            var query =
                from c in contexts.Zip(sessions, (c, s) => new { c, s })
                let context = c.c
                let session = c.s
                let sessionContext = new SessionContext<AppIF, SessionType>((IIOContext<AppIF>)context, (SessionType)session)
                select sessionContext;
            return query.ToArray();
        }
    }

}

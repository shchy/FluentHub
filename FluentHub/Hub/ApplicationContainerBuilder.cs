using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentHub.Logger;
using FluentHub.ModelConverter;
using System.Reflection;
using FluentHub.IO.Extension;

namespace FluentHub.Hub
{
    public static class ApplicationContainerBuilder
    {
        public static IContextApplication<T> MakeApp<T>(
            this IApplicationContainer @this
            , IIOContextMaker<byte[]> streamContextFactory)
        {
            var app =
                new Application<T>(
                    @this.MakeContextPool<T>()
                    , streamContextFactory
                    , new SuspendedDisposalSource(1000)    // todo defaultはこれでいいけどどこかで変更できるようにはしたいよね
                    , new SequenceRunnerFacade<T>(@this.Logger) // todo defaultはこれでいいけどどこかで変更できるようにはしたいよね
                    , @this.Logger
                    );
            @this.Add(app);
            return app;
        }

        public static IContextApplication<T> RegisterConverter<T>(
            this IContextApplication<T> @this
            , IModelConverter<T> converter)
        {
            @this.AddConverter(converter);
            return @this;
        }
        
        public static IContextApplication<T> RegisterConverter<T,U>(
            this IContextApplication<T> @this)
            where U : class,T, new()
            where T : class
        {
            var defaultConverter = new DefaultModelConverter<T,U>();
            @this.AddConverter(defaultConverter);
            return @this;
        }

        
    }

    public static class Gokan
    {

        public static IApplicationContainer RegisterSequence<T1>(
            this IApplicationContainer @this
            , Action<T1> lambda)
        {
            return @this.RegisterSequence(lambda.Method, ()=> lambda.Target);
        }

        public static IApplicationContainer RegisterSequence<T1, T2>(
            this IApplicationContainer @this
            , Action<T1,T2> lambda)
        {
            return @this.RegisterSequence(lambda.Method, () => lambda.Target);
        }


        public static IApplicationContainer RegisterSequence<T1, T2, T3>(
            this IApplicationContainer @this
            , Action<T1, T2,T3> lambda)
        {
            return @this.RegisterSequence(lambda.Method, () => lambda.Target);
        }
        public static IApplicationContainer RegisterSequence<T1, T2, T3, T4>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4> lambda)
        {
            return @this.RegisterSequence(lambda.Method, () => lambda.Target);
        }


        public static IApplicationContainer RegisterSequence<T1, T2, T3, T4, T5>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5> lambda)
        {
            return @this.RegisterSequence(lambda.Method, () => lambda.Target);
        }


        public static IApplicationContainer RegisterSequence<T1, T2, T3, T4, T5, T6>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6> lambda)
        {
            return @this.RegisterSequence(lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterSequence<T1, T2, T3, T4, T5, T6, T7>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6, T7> lambda)
        {
            return @this.RegisterSequence(lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterSequence<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6, T7, T8> lambda)
        {
            return @this.RegisterSequence(lambda.Method, () => lambda.Target);
        }


        [Obsolete("This class has been deprecated. ")]
        public static IContextApplication<T> RegisterInitializeSequence<T>(
            this IContextApplication<T> @this
            , Action<IIOContext<T>> sequence)
        {
            @this.AddInitializeSequence(sequence);
            return @this;
        }

        [Obsolete("This class has been deprecated. ")]
        public static IApplicationContainer RegisterInitializeSequence<T, U>(
            this IApplicationContainer @this
            , Action<IIOContext<T>, IEnumerable<IIOContext<U>>> sequence)
        {
            var app = @this.GetApp<T>();
            var appTarg = @this.GetApp<U>();
            System.Diagnostics.Debug.Assert(app != null, "RegisterSequence");
            System.Diagnostics.Debug.Assert(appTarg != null, "RegisterSequence");

            app.AddInitializeSequence(context =>
            {
                var contexts = appTarg.Pool.Get().ToArray();
                sequence(context, contexts);
            });

            return @this;
        }

        /// <summary>
        /// 受信->応答型じゃなくてサーバーから開始するシーケンス
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Return"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        [Obsolete("This class has been deprecated. ")]
        public static Return InstantSequence<T, Return>(
            this IContextApplication<T> @this
            , Func<IEnumerable<IIOContext<T>>, Return> sequence)
        {
            return
                @this.Logger.TrySafe(
                    () => sequence(@this.Pool.Get().ToArray())).Item2;
        }

        /// <summary>
        /// 戻り値なし
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        [Obsolete("This class has been deprecated. ")]
        public static void InstantSequence<T>(
            this IContextApplication<T> @this
            , Action<IEnumerable<IIOContext<T>>> sequence)
        {
            @this.Logger.TrySafe(
                () => sequence(@this.Pool.Get().ToArray()));
        }

        /// <summary>
        /// ３者戻り値あり
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="Return"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        [Obsolete("This class has been deprecated. ")]
        public static Return InstantSequence<T, U, Return>(
            this IApplicationContainer @this
            , Func<IEnumerable<IIOContext<T>>, IEnumerable<IIOContext<U>>, Return> sequence)
        {
            return
                @this.Logger.TrySafe(() =>
                {
                    var app = @this.GetApp<T>();
                    var appTarg = @this.GetApp<U>();
                    System.Diagnostics.Debug.Assert(app != null, "RegisterSequence");
                    System.Diagnostics.Debug.Assert(appTarg != null, "RegisterSequence");

                    return
                        sequence(
                            app.Pool.Get().ToArray()
                            , appTarg.Pool.Get().ToArray());
                }).Item2;
        }

        /// <summary>
        /// ３者戻り値なし
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        [Obsolete("This class has been deprecated. ")]
        public static void InstantSequence<T, U>(
           this IApplicationContainer @this
           , Action<IEnumerable<IIOContext<T>>, IEnumerable<IIOContext<U>>> sequence)
        {
            @this.Logger.TrySafe(() =>
            {
                var app = @this.GetApp<T>();
                var appTarg = @this.GetApp<U>();
                System.Diagnostics.Debug.Assert(app != null, "RegisterSequence");
                System.Diagnostics.Debug.Assert(appTarg != null, "RegisterSequence");
                sequence(
                    app.Pool.Get().ToArray()
                    , appTarg.Pool.Get().ToArray());
            });
        }
    }
}

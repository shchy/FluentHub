using FluentHub.Hub.TCP;
using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public static class ApplicationContainerBuilder
    {
        public static IContextApplication<T> MakeApp<T>(
            this IApplicationContainer @this
            , IEnumerable<IModelConverter<T>> converters
            , IRunnableFactory<IIOContext<byte>> streamContextFactory)
        {
            var app =
                new Application<T>(
                    @this.MakeContextPool<T>()
                    , streamContextFactory
                    , converters
                    , @this.Logger
                    );
            @this.Add(app);
            return app;
        }

        /// <summary>
        /// アプリケーションにシーケンスを登録する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IContextApplication<T> RegisterSequence<T>(
            this IContextApplication<T> @this
            , Action<IIOContext<T>> sequence)
        {
            @this.AddSequence(sequence);
            return @this;
        }

        /// <summary>
        /// アプリケーションにシーケンスを追加する
        /// 指定の仮引数型のモデルが受信済みであるかをあらかじめ確認してから発行される
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IContextApplication<T> RegisterSequence<T, U>(
            this IContextApplication<T> @this
            , Action<IIOContext<T>, U> sequence)
            where U : class, T
        {
            var typeT = typeof(U);

            @this.AddSequence(context =>
            {
                try
                {
                    var model = context.Read(x => typeT.IsInstanceOfType(x));
                    if (model == null)
                    {
                        return;
                    }
                    sequence(context, model as U);
                }
                catch (Exception ex)
                {
                    @this.Logger.Exception(ex);
                    throw;
                }
            });
            return @this;
        }


        /// <summary>
        /// 3者間シーケンスの登録
        /// T<->Server<->U的なシーケンス
        /// U群の内、どれかを特定するのはシーケンス内でやってね
        /// todo ISession的な概念を作る？IIOContextごとに持つやつ。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IApplicationContainer RegisterSequence<T, U>(
            this IApplicationContainer @this
            , Action<IIOContext<T>, IEnumerable<IIOContext<U>>> sequence)
        {
            var app = @this.GetApp<T>();
            var appTarg = @this.GetApp<U>();
            System.Diagnostics.Debug.Assert(app != null, "RegisterSequence");
            System.Diagnostics.Debug.Assert(appTarg != null, "RegisterSequence");

            app.AddSequence(context =>
            {

                try
                {
                    var contexts = appTarg.Pool.Get().ToArray();
                    sequence(context, contexts);
                }
                catch (Exception ex)
                {
                    @this.Logger.Exception(ex);
                    throw;
                }
            });

            return @this;
        }

        /// <summary>
        /// モデル指定バージョン
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IApplicationContainer RegisterSequence<T, U>(
            this IApplicationContainer @this
            , Action<T, IEnumerable<IIOContext<U>>> sequence)
        {
            var app = @this.GetApp<T>();
            var appTarg = @this.GetApp<U>();
            System.Diagnostics.Debug.Assert(app != null, "RegisterSequence");
            System.Diagnostics.Debug.Assert(appTarg != null, "RegisterSequence");

            var typeT = typeof(T);

            app.AddSequence(context =>
            {
                try
                {
                    var model = context.Read(x => typeT.IsInstanceOfType(x));
                    if (model == null)
                    {
                        return;
                    }
                    var contexts = appTarg.Pool.Get().ToArray();
                    sequence(model, contexts);
                }
                catch (Exception ex)
                {
                    @this.Logger.Exception(ex);
                    throw;
                }
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
        public static Return InstantSequence<T, Return>(
            this IContextApplication<T> @this
            , Func<IEnumerable<IIOContext<T>>, Return> sequence)
        {
            try
            {
                return sequence(@this.Pool.Get().ToArray());
            }
            catch (Exception ex)
            {
                @this.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// 戻り値なし
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        public static void InstantSequence<T>(
            this IContextApplication<T> @this
            , Action<IEnumerable<IIOContext<T>>> sequence)
        {

            try
            {
                sequence(@this.Pool.Get().ToArray());
            }
            catch (Exception ex)
            {
                @this.Logger.Exception(ex);
                throw;
            }
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
        public static Return InstantSequence<T, U, Return>(
            this IApplicationContainer @this
            , Func<IEnumerable<IIOContext<T>>, IEnumerable<IIOContext<U>>, Return> sequence)
        {

            try
            {
                var app = @this.GetApp<T>();
                var appTarg = @this.GetApp<U>();
                System.Diagnostics.Debug.Assert(app != null, "RegisterSequence");
                System.Diagnostics.Debug.Assert(appTarg != null, "RegisterSequence");

                return
                    sequence(
                        app.Pool.Get().ToArray()
                        , appTarg.Pool.Get().ToArray());
            }
            catch (Exception ex)
            {
                @this.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// ３者戻り値なし
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="this"></param>
        /// <param name="sequence"></param>
        public static void InstantSequence<T, U>(
           this IApplicationContainer @this
           , Action<IEnumerable<IIOContext<T>>, IEnumerable<IIOContext<U>>> sequence)
        {

            try
            {
                var app = @this.GetApp<T>();
                var appTarg = @this.GetApp<U>();
                System.Diagnostics.Debug.Assert(app != null, "RegisterSequence");
                System.Diagnostics.Debug.Assert(appTarg != null, "RegisterSequence");
                sequence(
                    app.Pool.Get().ToArray()
                    , appTarg.Pool.Get().ToArray());
            }
            catch (Exception ex)
            {
                @this.Logger.Exception(ex);
                throw;
            }
        }
    }

}

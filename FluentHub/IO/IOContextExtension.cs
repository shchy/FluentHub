using FluentHub.IO.Extension;
using FluentHub.Logger;
using FluentHub.ModelConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO
{
    public static class IOContextExtension
    {
        public static T Read<T>(
            this IIOContext<T> @this
            , Func<T, bool> predicate
            , int timeoutMillisecond)
            where T : class
        {
            var target = DateTime.Now.AddMilliseconds(timeoutMillisecond);
            do
            {
                var model = @this.Read(predicate);
                if (model != null)
                {
                    return model;
                }

            } while (DateTime.Now < target);
            return null;
        }

        public static U ReadAs<T,U>(
            this IIOContext<T> @this
            , int timeoutMillisecond)
            where T : class
            where U : class, T
        {
            var target = DateTime.Now.AddMilliseconds(timeoutMillisecond);
            do
            {
                var model = @this.ReadAs<T,U>();
                if (model != null)
                {
                    return model;
                }

            } while (DateTime.Now < target);
            return null;
        }

        public static U ReadAs<T,U>(
            this IIOContext<T> @this)
            where T : class
            where U : class, T
        {
            var model = @this.Read(m => m is U) as U;
            return model;
        }
    }

    public static class ModelContext
    {
        public static IIOContext<T> BuildContext<T>(
            this IIOContext<byte[]> @this
            , IEnumerable<IModelConverter<T>> converters
            , ILogger logger)
        {
            // todo jammed packet clear のため。もっといい入れ方ないかな
            // todo 猶予時間もどっかで設定したい
            var suspendedSentence = new SuspendedSentence(1000 * 10); 
            // todo Runもここじゃない感強い
            // todo Contextごとにスレッドできちゃう。アプリケーション単位でRunすればいい？
            suspendedSentence.Run();

            return
                new ModelContext<T>(
                    new IOContextLoggerProxy<byte[]>(
                        @this
                        , logger)
                    , converters
                    , suspendedSentence
                    , logger);
        }
    }
}

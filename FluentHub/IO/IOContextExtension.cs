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
    }

    public static class ModelContext
    {
        public static IIOContext<T> BuildContext<T>(
            this IIOContext<byte> @this
            , IEnumerable<IModelConverter<T>> converters
            , ILogger logger)
        {
            return
                new ModelContext<T>(
                    new IOContextLoggerProxy<byte>(
                        @this
                        , logger)
                    , converters
                    , logger);
        }
    }
}

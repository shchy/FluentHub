using FluentHub.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub
{
    public static class ValidationBuilder
    {
        public static IAppBuilder<AppIF> AddValidators<AppIF>(
            this IAppBuilder<AppIF> @this
            , params IModelValidator<AppIF>[] validators)
        {
            var tmp = @this.StreamToModelContext;

            @this.StreamToModelContext = (streamContext, _) =>
            {
                var modelContext = tmp(streamContext, _);
                var validationContext =
                        new ValidationModelContext<AppIF>(
                            modelContext
                            , @this.Logger
                            , validators);
                return validationContext;
            };

            return @this;
        }
    }
}

using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Validation
{
    public interface IModelValidator
    {
        bool Validate(object model);
    }

    public interface IModelValidator<in TModel> : IModelValidator
    {
        bool Validate(TModel model);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub.ModelValidator
{
    public interface IModelValidator
    {
        bool CanValidate(object model);
        bool Validate(object model);
    }

    public interface IModelValidator<in TModel> : IModelValidator
    {
        bool Validate(TModel model);
    }
}

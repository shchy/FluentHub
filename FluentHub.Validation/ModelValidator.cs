using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Validation
{
    public class ModelValidator<Model> : AbstractValidator<Model>, IModelValidator<Model>
    {
        public bool Validate(object model)
        {
            return (this as IModelValidator<Model>).Validate((Model)model);
        }

        bool IModelValidator<Model>.Validate(Model model)
        {
            var result = base.Validate(model);
            return result.IsValid;
        }
    }
}

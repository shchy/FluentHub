using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Validation
{
    public class ModelValidator<Model, ModelIF> : AbstractValidator<Model>, IModelValidator<ModelIF>
        where Model : ModelIF
    {
        public bool CanValidate(object model)
        {
            return typeof(Model).IsInstanceOfType(model);
        }

        public bool Validate(object model)
        {
            return (this as IModelValidator<ModelIF>).Validate((ModelIF)model);
        }

        bool IModelValidator<ModelIF>.Validate(ModelIF model)
        {
            var result = base.Validate((Model)model);
            return result.IsValid;
        }
    }
}

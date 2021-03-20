using Dingo.Data.GeneralModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dingo.Data.Validators
{
    public class SingleSearchTermValidator : AbstractValidator<SingleSearchTermModel>, IValidator<SingleSearchTermModel>
    {
        public SingleSearchTermValidator()
        {
            RuleFor(x => x.Term).NotEmpty().WithMessage("Field required");
        }
    }
}

using Dingo.Data.UserInfo;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dingo.Data.Validators
{
    public class DisplayNameValidator : AbstractValidator<DisplayNameModel>, IValidator<DisplayNameModel>
    {
        public DisplayNameValidator()
        {
            RuleFor(x => x.DisplayName).NotEmpty().WithMessage("Field required").Length(6, 100).WithMessage("Must be between 6 and 100 characters").Must(x => x?.Contains('#') ?? false).WithMessage("Include tag number, ex. Username#1234");
        }
    }
}

using FluentValidation;
using VCRC.Fluids;

namespace VCRC.Validators
{
    public class RefrigerantValidator : AbstractValidator<Refrigerant>
    {
        public RefrigerantValidator()
        {
            RuleFor(refrigerant => refrigerant.Name).Must(name => name.ToString().StartsWith('R'))
                .WithMessage("The selected fluid is not a refrigerant (its name should start with 'R')!");
        }
    }
}
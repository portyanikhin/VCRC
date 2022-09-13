using FluentValidation;

namespace VCRC;

internal class RefrigerantTypeValidator : AbstractValidator<Refrigerant>
{
    internal RefrigerantTypeValidator()
    {
        RuleFor(refrigerant => refrigerant)
            .Must(refrigerant => refrigerant.IsSingleComponent || refrigerant.IsAzeotropicBlend)
            .WithMessage("Refrigerant should be a single component or an azeotropic blend!");
    }
}
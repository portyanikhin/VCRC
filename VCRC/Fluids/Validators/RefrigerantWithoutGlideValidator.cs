using FluentValidation;

namespace VCRC.Fluids.Validators;

public class RefrigerantWithoutGlideValidator : AbstractValidator<Refrigerant>
{
    public RefrigerantWithoutGlideValidator()
    {
        RuleFor(refrigerant => refrigerant.HasGlide)
            .Equal(false)
            .WithMessage("Refrigerant should not have a temperature glide!");
    }
}
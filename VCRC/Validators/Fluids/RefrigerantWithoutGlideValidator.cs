using FluentValidation;
using VCRC.Fluids;

namespace VCRC.Validators.Fluids;

public class RefrigerantWithoutGlideValidator : AbstractValidator<Refrigerant>
{
    public RefrigerantWithoutGlideValidator()
    {
        RuleFor(refrigerant => refrigerant.HasGlide)
            .Equal(false)
            .WithMessage("Refrigerant should not have a temperature glide!");
    }
}
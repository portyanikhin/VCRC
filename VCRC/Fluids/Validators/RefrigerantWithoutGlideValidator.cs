using FluentValidation;

namespace VCRC;

internal class RefrigerantWithoutGlideValidator : AbstractValidator<Refrigerant>
{
    internal RefrigerantWithoutGlideValidator()
    {
        RuleFor(refrigerant => refrigerant.HasGlide)
            .Equal(false)
            .WithMessage("Refrigerant should not have a temperature glide!");
    }
}
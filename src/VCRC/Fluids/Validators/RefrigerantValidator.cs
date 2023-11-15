namespace VCRC;

#pragma warning disable CA1866
internal class RefrigerantValidator : AbstractValidator<IRefrigerant>
{
    public RefrigerantValidator() =>
        RuleFor(refrigerant => refrigerant.Name)
            .Must(name => name.ToString().StartsWith("R"))
            .WithMessage(
                "The selected fluid is not a refrigerant "
                    + "(its name should start with 'R')!"
            );
}
#pragma warning restore CA1866

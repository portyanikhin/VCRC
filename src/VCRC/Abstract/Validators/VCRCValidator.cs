namespace VCRC;

internal class VCRCValidator : AbstractValidator<IVCRC>
{
    public VCRCValidator()
    {
        RuleFor(vcrc => vcrc.Evaporator.RefrigerantName)
            .Equal(vcrc => vcrc.HeatReleaser.RefrigerantName)
            .WithMessage("Only one refrigerant should be selected!");
        RuleFor(vcrc => vcrc.HeatReleaser.Temperature)
            .GreaterThan(vcrc => vcrc.Evaporator.Temperature)
            .When(vcrc => vcrc.HeatReleaser is Condenser)
            .WithMessage(
                "Condensing temperature should be "
                    + "greater than evaporating temperature!"
            );
    }
}

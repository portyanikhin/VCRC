namespace VCRC;

internal class VCRCWithEjectorEconomizerAndTPIValidator
    : AbstractValidator<IVCRCWithEjectorEconomizerAndTPI>
{
    public VCRCWithEjectorEconomizerAndTPIValidator()
    {
        RuleFor(
                vcrc =>
                    vcrc.Point6.Temperature
                    + vcrc.Economizer.TemperatureDifference
            )
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage(
                "Too high temperature difference at the economizer 'cold' side!"
            );
    }
}

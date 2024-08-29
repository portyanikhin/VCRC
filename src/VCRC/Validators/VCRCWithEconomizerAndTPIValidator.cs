namespace VCRC;

internal sealed class VCRCWithEconomizerAndTPIValidator
    : AbstractValidator<IVCRCWithEconomizerAndTPI>
{
    public VCRCWithEconomizerAndTPIValidator()
    {
        RuleFor(vcrc => vcrc.Point6.Temperature + vcrc.Economizer.TemperatureDifference)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Too high temperature difference at the economizer 'cold' side!");
    }
}

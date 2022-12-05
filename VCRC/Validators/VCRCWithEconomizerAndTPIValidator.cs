namespace VCRC;

internal class VCRCWithEconomizerAndTPIValidator : AbstractValidator<VCRCWithEconomizerAndTPI>
{
    internal VCRCWithEconomizerAndTPIValidator()
    {
        RuleFor(vcrc => vcrc.Point6.Temperature + vcrc.Economizer.TemperatureDifference)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Too high temperature difference at economizer 'cold' side!");
    }
}
namespace VCRC;

internal class VCRCWithEconomizerAndPCValidator
    : AbstractValidator<VCRCWithEconomizerAndPC>
{
    internal VCRCWithEconomizerAndPCValidator()
    {
        RuleFor(vcrc => vcrc.Point3.Temperature)
            .LessThan(vcrc => vcrc.Point6.Temperature)
            .WithMessage(
                "Wrong temperature difference at economizer 'hot' side!"
            );
        RuleFor(
                vcrc =>
                    vcrc.Point7.Temperature
                    + vcrc.Economizer.TemperatureDifference
            )
            .LessThan(vcrc => vcrc.Point6.Temperature)
            .WithMessage(
                "Too high temperature difference at economizer 'cold' side!"
            );
    }
}

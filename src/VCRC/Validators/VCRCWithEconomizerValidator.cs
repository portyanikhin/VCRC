namespace VCRC;

internal class VCRCWithEconomizerValidator
    : AbstractValidator<IVCRCWithEconomizer>
{
    public VCRCWithEconomizerValidator()
    {
        RuleFor(vcrc => vcrc.Point7.Temperature)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage(
                "Wrong temperature difference at the economizer 'hot' side!"
            );
        RuleFor(vcrc =>
                vcrc.Point6.Temperature + vcrc.Economizer.TemperatureDifference
            )
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage(
                "Too high temperature difference at the economizer 'cold' side!"
            );
    }
}

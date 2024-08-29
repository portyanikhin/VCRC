namespace VCRC;

internal sealed class VCRCWithEconomizerAndPCValidator : AbstractValidator<IVCRCWithEconomizerAndPC>
{
    public VCRCWithEconomizerAndPCValidator()
    {
        RuleFor(vcrc => vcrc.Point3.Temperature)
            .LessThan(vcrc => vcrc.Point6.Temperature)
            .WithMessage("Wrong temperature difference at the economizer 'hot' side!");
        RuleFor(vcrc => vcrc.Point7.Temperature + vcrc.Economizer.TemperatureDifference)
            .LessThan(vcrc => vcrc.Point6.Temperature)
            .WithMessage("Too high temperature difference at the economizer 'cold' side!");
    }
}

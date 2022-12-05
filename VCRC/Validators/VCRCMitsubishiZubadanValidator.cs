namespace VCRC;

internal class VCRCMitsubishiZubadanValidator : AbstractValidator<VCRCMitsubishiZubadan>
{
    internal VCRCMitsubishiZubadanValidator()
    {
        RuleFor(vcrc => vcrc.Point7.Quality)
            .Must(quality => quality?.DecimalFractions is > 0 and < 1)
            .WithMessage("There should be a two-phase refrigerant at the recuperator 'hot' inlet!");
        RuleFor(vcrc => vcrc.Point7.Temperature)
            .GreaterThan(vcrc => vcrc.Point2.Temperature)
            .WithMessage("Wrong temperature difference at recuperator 'hot' side!");
        RuleFor(vcrc => vcrc.Point8.Temperature)
            .GreaterThan(vcrc => vcrc.Point1.Temperature)
            .WithMessage("Wrong temperature difference at recuperator 'cold' side!");
        RuleFor(vcrc => vcrc.Point11.Temperature)
            .LessThan(vcrc => vcrc.Point8.Temperature)
            .WithMessage("Too high temperature difference at economizer 'cold' side!");
    }
}
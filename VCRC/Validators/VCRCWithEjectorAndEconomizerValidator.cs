using FluentValidation;

namespace VCRC;

internal class VCRCWithEjectorAndEconomizerValidator : AbstractValidator<VCRCWithEjectorAndEconomizer>
{
    internal VCRCWithEjectorAndEconomizerValidator()
    {
        RuleFor(vcrc => vcrc.Point7.Temperature)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Wrong temperature difference at economizer 'hot' side!");
        RuleFor(vcrc => vcrc.Point6.Temperature + vcrc.Economizer.TemperatureDifference)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Too high temperature difference at economizer 'cold' side!");
    }
}
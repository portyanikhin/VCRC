using FluentValidation;

namespace VCRC;

internal class VCRCWithEjectorEconomizerAndTPIValidator : AbstractValidator<VCRCWithEjectorEconomizerAndTPI>
{
    internal VCRCWithEjectorEconomizerAndTPIValidator()
    {
        RuleFor(vcrc => vcrc.Point6.Temperature + vcrc.Economizer.TemperatureDifference)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Too high temperature difference at economizer 'cold' side!");
    }
}
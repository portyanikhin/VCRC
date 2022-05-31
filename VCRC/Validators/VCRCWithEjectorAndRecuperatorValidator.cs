using FluentValidation;

namespace VCRC;

internal class VCRCWithEjectorAndRecuperatorValidator :
    AbstractValidator<VCRCWithEjectorAndRecuperator>
{
    internal VCRCWithEjectorAndRecuperatorValidator()
    {
        RuleFor(vcrc => vcrc.Point4.Temperature - vcrc.Recuperator.TemperatureDifference)
            .GreaterThan(vcrc => vcrc.Point1.Temperature)
            .WithMessage("Too high temperature difference at recuperator 'hot' side!");
    }
}
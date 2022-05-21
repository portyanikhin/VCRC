using FluentValidation;

namespace VCRC;

internal class VCRCWithRecuperatorValidator : AbstractValidator<VCRCWithRecuperator>
{
    internal VCRCWithRecuperatorValidator()
    {
        RuleFor(vcrc => vcrc.Point2.Temperature)
            .GreaterThan(vcrc => vcrc.Point1.Temperature)
            .WithMessage("Too high temperature difference at recuperator 'hot' side!");
    }
}
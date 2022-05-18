using FluentValidation;

namespace VCRC.Abstract.Validators;

internal class AbstractVCRCWithRecuperatorValidator : AbstractValidator<AbstractVCRCWithRecuperator>
{
    internal AbstractVCRCWithRecuperatorValidator()
    {
        RuleFor(vcrc => vcrc.Point2.Temperature)
            .GreaterThan(vcrc => vcrc.Point1.Temperature)
            .WithMessage("Too high temperature difference at recuperator 'hot' side!");
    }
}
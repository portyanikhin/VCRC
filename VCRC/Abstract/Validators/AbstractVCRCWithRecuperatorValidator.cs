using FluentValidation;

namespace VCRC.Abstract.Validators;

public class AbstractVCRCWithRecuperatorValidator : AbstractValidator<AbstractVCRCWithRecuperator>
{
    public AbstractVCRCWithRecuperatorValidator()
    {
        RuleFor(vcrc => vcrc.Point4.Temperature)
            .GreaterThan(vcrc => vcrc.Point2.Temperature)
            .WithMessage("Wrong temperature difference at recuperator 'hot' side!");
        RuleFor(vcrc => vcrc.Point5.Temperature)
            .GreaterThan(vcrc => vcrc.Point1.Temperature)
            .WithMessage("Wrong temperature difference at recuperator 'cold' side!");
    }
}
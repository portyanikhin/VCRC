using FluentValidation;

namespace VCRC.Validators;

public class VCRCWithRecuperatorValidator : AbstractValidator<VCRCWithRecuperator>
{
    public VCRCWithRecuperatorValidator()
    {
        RuleFor(vcrc => vcrc.Point6.Temperature).GreaterThan(vcrc => vcrc.Point2.Temperature)
            .WithMessage("Wrong temperature difference at recuperator 'hot' side!");
        RuleFor(vcrc => vcrc.Point7.Temperature).GreaterThan(vcrc => vcrc.Point1.Temperature)
            .WithMessage("Wrong temperature difference at recuperator 'cold' side!");
    }
}
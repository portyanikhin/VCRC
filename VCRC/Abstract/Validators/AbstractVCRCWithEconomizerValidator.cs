using FluentValidation;

namespace VCRC.Abstract.Validators;

internal class AbstractVCRCWithEconomizerValidator : AbstractValidator<AbstractVCRCWithEconomizer>
{
    internal AbstractVCRCWithEconomizerValidator()
    {
        RuleFor(vcrc => vcrc.Point7.Temperature)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Wrong temperature difference at economizer 'hot' side!");
        RuleFor(vcrc => vcrc.Point8.Temperature)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Too high temperature difference at economizer 'cold' side!");
    }
}
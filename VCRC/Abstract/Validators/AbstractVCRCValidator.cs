using FluentValidation;
using VCRC.Components;

namespace VCRC.Abstract.Validators;

public class AbstractVCRCValidator : AbstractValidator<AbstractVCRC>
{
    public AbstractVCRCValidator()
    {
        RuleFor(vcrc => vcrc.Evaporator.RefrigerantName)
            .Equal(vcrc => vcrc.HeatEmitter.RefrigerantName)
            .WithMessage("Only one refrigerant should be selected!");
        RuleFor(vcrc => vcrc.HeatEmitter.Temperature)
            .GreaterThan(vcrc => vcrc.Evaporator.Temperature)
            .When(vcrc => vcrc.HeatEmitter is Condenser)
            .WithMessage("Condensing temperature should be greater than evaporating temperature!");
    }
}
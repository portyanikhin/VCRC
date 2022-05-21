using FluentValidation;

namespace VCRC;

internal class AbstractVCRCValidator : AbstractValidator<AbstractVCRC>
{
    internal AbstractVCRCValidator()
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
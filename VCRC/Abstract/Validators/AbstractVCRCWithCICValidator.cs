using FluentValidation;
using VCRC.Components;

namespace VCRC.Abstract.Validators;

internal class AbstractVCRCWithCICValidator : AbstractValidator<AbstractVCRCWithCIC>
{
    internal AbstractVCRCWithCICValidator()
    {
        RuleFor(vcrc => vcrc.IntermediateVessel.Pressure)
            .GreaterThan(vcrc => vcrc.Evaporator.Pressure)
            .WithMessage("Intermediate pressure should be greater than evaporating pressure!");
        RuleFor(vcrc => vcrc.IntermediateVessel.Pressure)
            .LessThan(vcrc => vcrc.HeatEmitter.Pressure)
            .When(vcrc => vcrc.HeatEmitter is Condenser)
            .WithMessage("Intermediate pressure should be less than condensing pressure!");
        RuleFor(vcrc => vcrc.IntermediateVessel.Pressure)
            .LessThan(vcrc => vcrc.HeatEmitter.Pressure)
            .When(vcrc => vcrc.HeatEmitter is GasCooler)
            .WithMessage("Intermediate pressure should be less than gas cooler pressure!");
        RuleFor(vcrc => vcrc.Point6.Quality)
            .Must(quality => quality?.DecimalFractions is > 0 and < 1)
            .WithMessage("There should be a two-phase refrigerant at the intermediate vessel inlet!");
    }
}
using FluentValidation;

namespace VCRC.Subcritical.Validators;

public class VCRCWithCompleteIntercoolingValidator : AbstractValidator<VCRCWithCompleteIntercooling>
{
    public VCRCWithCompleteIntercoolingValidator()
    {
        RuleFor(vcrc => vcrc.IntermediateVessel.Pressure)
            .GreaterThan(vcrc => vcrc.Evaporator.Pressure)
            .WithMessage("Intermediate pressure should be greater than evaporating pressure!");
        RuleFor(vcrc => vcrc.IntermediateVessel.Pressure)
            .LessThan(vcrc => vcrc.Condenser.Pressure)
            .WithMessage("Intermediate pressure should be less than condensing pressure!");
        RuleFor(vcrc => vcrc.Point6.Quality)
            .Must(quality => quality?.DecimalFractions is > 0 and < 1)
            .WithMessage("There should be a two-phase refrigerant at the intermediate vessel inlet!");
    }
}
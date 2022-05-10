using FluentValidation;
using VCRC.Components;

namespace VCRC.Abstract.Validators;

public class AbstractVCRCWithEconomizerTPIValidator : AbstractValidator<AbstractVCRCWithEconomizerTPI>
{
    public AbstractVCRCWithEconomizerTPIValidator()
    {
        RuleFor(vcrc => vcrc.Economizer.Pressure)
            .GreaterThan(vcrc => vcrc.Evaporator.Pressure)
            .WithMessage("Intermediate pressure should be greater than evaporating pressure!");
        RuleFor(vcrc => vcrc.Economizer.Pressure)
            .LessThan(vcrc => vcrc.HeatEmitter.Pressure)
            .When(vcrc => vcrc.HeatEmitter is Condenser)
            .WithMessage("Intermediate pressure should be less than condensing pressure!");
        RuleFor(vcrc => vcrc.Economizer.Pressure)
            .LessThan(vcrc => vcrc.HeatEmitter.Pressure)
            .When(vcrc => vcrc.HeatEmitter is GasCooler)
            .WithMessage("Intermediate pressure should be less than gas cooler pressure!");
        RuleFor(vcrc => vcrc.Point7.Quality)
            .Must(quality => quality?.DecimalFractions is > 0 and < 1)
            .WithMessage("There should be a two-phase refrigerant at the compressor injection circuit!");
        RuleFor(vcrc => vcrc.Point7.Temperature)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Wrong temperature difference at economizer 'hot' side!");
        RuleFor(vcrc => vcrc.Point8.Temperature)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Too high temperature difference at economizer 'cold' side!");
    }
}
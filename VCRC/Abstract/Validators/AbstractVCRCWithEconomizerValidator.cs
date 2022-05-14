using FluentValidation;
using VCRC.Components;

namespace VCRC.Abstract.Validators;

internal class AbstractVCRCWithEconomizerValidator : AbstractValidator<AbstractVCRCWithEconomizer>
{
    internal AbstractVCRCWithEconomizerValidator()
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
        RuleFor(vcrc => vcrc.Point7.Temperature)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Wrong temperature difference at economizer 'hot' side!");
        RuleFor(vcrc => vcrc.Point8.Temperature)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Too high temperature difference at economizer 'cold' side!");
    }
}
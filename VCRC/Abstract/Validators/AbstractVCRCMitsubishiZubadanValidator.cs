using FluentValidation;
using UnitsNet;
using VCRC.Components;

namespace VCRC.Abstract.Validators;

public class AbstractVCRCMitsubishiZubadanValidator : AbstractValidator<AbstractVCRCMitsubishiZubadan>
{
    public AbstractVCRCMitsubishiZubadanValidator()
    {
        RuleFor(vcrc => vcrc.Recuperator.Superheat)
            .GreaterThan(TemperatureDelta.Zero)
            .WithMessage("Superheat in the recuperator should be greater than zero!");
        RuleFor(vcrc => vcrc.Point7.Quality)
            .Must(quality => quality?.DecimalFractions is > 0 and < 1)
            .WithMessage("There should be a two-phase refrigerant at the recuperator 'hot' inlet!");
        RuleFor(vcrc => vcrc.Point7.Temperature)
            .GreaterThan(vcrc => vcrc.Point2.Temperature)
            .WithMessage("Wrong temperature difference at recuperator 'hot' side!");
        RuleFor(vcrc => vcrc.Point8.Temperature)
            .GreaterThan(vcrc => vcrc.Point1.Temperature)
            .WithMessage("Wrong temperature difference at recuperator 'cold' side!");
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
        RuleFor(vcrc => vcrc.Point10.Quality)
            .Must(quality => quality?.DecimalFractions is > 0 and < 1)
            .WithMessage("There should be a two-phase refrigerant at the compressor injection circuit!");
        RuleFor(vcrc => vcrc.Point10.Temperature)
            .LessThan(vcrc => vcrc.Point8.Temperature)
            .WithMessage("Wrong temperature difference at economizer 'hot' side!");
        RuleFor(vcrc => vcrc.Point11.Temperature)
            .LessThan(vcrc => vcrc.Point8.Temperature)
            .WithMessage("Too high temperature difference at economizer 'cold' side!");
    }
}
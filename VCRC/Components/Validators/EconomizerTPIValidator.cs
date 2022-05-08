using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Components.Validators;

public class EconomizerTPIValidator : AbstractValidator<EconomizerTPI>
{
    public EconomizerTPIValidator()
    {
        RuleFor(economizer => economizer.TemperatureDifference)
            .InclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
            .WithMessage("Temperature difference at the economizer 'cold' side should be in [0;50] K!");
    }
}
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Components.Validators;

internal class RecuperatorValidator : AbstractValidator<Recuperator>
{
    internal RecuperatorValidator()
    {
        RuleFor(recuperator => recuperator.TemperatureDifference)
            .ExclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
            .WithMessage("Temperature difference at recuperator 'hot' side should be in (0;50) K!");
    }
}
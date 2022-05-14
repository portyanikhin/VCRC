using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Components.Validators;

internal class RecuperatorValidator : AbstractValidator<Recuperator>
{
    internal RecuperatorValidator()
    {
        RuleFor(recuperator => recuperator.Superheat)
            .InclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
            .WithMessage("Superheat in the recuperator should be in [0;50] K!");
    }
}
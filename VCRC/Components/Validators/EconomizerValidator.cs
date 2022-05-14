using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Components.Validators;

internal class EconomizerValidator : AbstractValidator<Economizer>
{
    internal EconomizerValidator()
    {
        RuleFor(economizer => economizer.Superheat)
            .InclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
            .WithMessage("Superheat in the economizer should be in [0;50] K!");
    }
}
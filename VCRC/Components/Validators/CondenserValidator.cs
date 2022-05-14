using System;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;
using VCRC.Fluids;

namespace VCRC.Components.Validators;

internal class CondenserValidator : AbstractValidator<Condenser>
{
    internal CondenserValidator(Refrigerant refrigerant)
    {
        RuleFor(condenser => condenser.Temperature)
            .ExclusiveBetween(refrigerant.TripleTemperature, refrigerant.CriticalTemperature)
            .WithMessage(
                "Condensing temperature should be in " +
                $"({Math.Round(refrigerant.TripleTemperature.DegreesCelsius, 2)};" +
                $"{Math.Round(refrigerant.CriticalTemperature.DegreesCelsius, 2)}) °C!");
        RuleFor(condenser => condenser.Subcooling)
            .InclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
            .WithMessage("Subcooling in the condenser should be in [0;50] K!");
    }
}
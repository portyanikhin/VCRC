using System;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;
using VCRC.Components;
using VCRC.Fluids;

namespace VCRC.Validators
{
    public class EvaporatorValidator : AbstractValidator<Evaporator>
    {
        public EvaporatorValidator(Refrigerant refrigerant)
        {
            RuleFor(evaporator => evaporator.Temperature)
                .ExclusiveBetween(refrigerant.TripleTemperature, refrigerant.CriticalTemperature)
                .WithMessage(
                    "Evaporating temperature should be in " +
                    $"({Math.Round(refrigerant.TripleTemperature.DegreesCelsius, 2)};" +
                    $"{Math.Round(refrigerant.CriticalTemperature.DegreesCelsius, 2)}) °C!");
            RuleFor(evaporator => evaporator.Superheat).InclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
                .WithMessage("Superheat in the evaporator should be in [0;50] K!");
        }
    }
}
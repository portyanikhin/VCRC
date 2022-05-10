using System;
using FluentValidation;
using VCRC.Fluids;

namespace VCRC.Components.Validators;

public class GasCoolerValidator : AbstractValidator<GasCooler>
{
    public GasCoolerValidator(Refrigerant refrigerant)
    {
        RuleFor(gasCooler => gasCooler.OutletTemperature)
            .GreaterThan(refrigerant.CriticalTemperature)
            .WithMessage(
                "Gas cooler outlet temperature should be greater than " +
                $"{Math.Round(refrigerant.CriticalTemperature.DegreesCelsius, 2)} °C!");
        RuleFor(gasCooler => gasCooler.Pressure)
            .GreaterThan(refrigerant.CriticalPressure)
            .WithMessage(
                "Gas cooler absolute pressure should be greater than " +
                $"{Math.Round(refrigerant.CriticalPressure.Megapascals, 2)} MPa!");
    }
}
using System;
using FluentValidation;
using VCRC.Fluids;

namespace VCRC.Components.Validators;

public class GasCoolerValidator : AbstractValidator<GasCooler>
{
    public GasCoolerValidator(Refrigerant refrigerant)
    {
        RuleFor(gasCooler => gasCooler.OutletTemperature)
            .ExclusiveBetween(refrigerant.CriticalTemperature, refrigerant.MaxTemperature)
            .WithMessage(
                "Gas cooler outlet temperature should be in " +
                $"({Math.Round(refrigerant.CriticalTemperature.DegreesCelsius, 2)};" +
                $"{Math.Round(refrigerant.MaxTemperature.DegreesCelsius, 2)}) °C!");
    }
}
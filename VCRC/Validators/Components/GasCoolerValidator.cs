using System;
using FluentValidation;
using VCRC.Components;
using VCRC.Fluids;

namespace VCRC.Validators.Components;

public class GasCoolerValidator : AbstractValidator<GasCooler>
{
    public GasCoolerValidator(Refrigerant refrigerant)
    {
        RuleFor(gasCooler => gasCooler.OutletTemperature)
            .ExclusiveBetween(refrigerant.CriticalTemperature, refrigerant.MaxTemperature)
            .WithMessage(
                "Gas temperature at the gas cooler outlet should be in " +
                $"({Math.Round(refrigerant.CriticalTemperature.DegreesCelsius, 2)};" +
                $"{Math.Round(refrigerant.MaxTemperature.DegreesCelsius, 2)}) °C!");
    }
}
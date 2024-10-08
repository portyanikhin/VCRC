﻿namespace VCRC;

internal sealed class GasCoolerValidator : AbstractValidator<GasCooler>
{
    public GasCoolerValidator(IRefrigerant refrigerant)
    {
        RuleFor(gasCooler => gasCooler.Temperature)
            .GreaterThan(refrigerant.CriticalTemperature)
            .WithMessage(
                "Gas cooler outlet temperature should be greater than "
                    + $"{Math.Round(refrigerant.CriticalTemperature.DegreesCelsius, 2)} °C!"
            );
        RuleFor(gasCooler => gasCooler.Pressure)
            .GreaterThan(refrigerant.CriticalPressure)
            .WithMessage(
                "Gas cooler absolute pressure should be greater than "
                    + $"{Math.Round(refrigerant.CriticalPressure.Megapascals, 2)} MPa!"
            );
    }
}

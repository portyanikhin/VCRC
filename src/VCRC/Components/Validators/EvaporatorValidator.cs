using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC;

internal sealed class EvaporatorValidator : AbstractValidator<IEvaporator>
{
    public EvaporatorValidator(IRefrigerant refrigerant)
    {
        RuleFor(evaporator => evaporator.Temperature)
            .ExclusiveBetween(refrigerant.TripleTemperature, refrigerant.CriticalTemperature)
            .WithMessage(
                "Evaporating temperature should be in "
                    + $"({Math.Round(refrigerant.TripleTemperature.DegreesCelsius, 2)};"
                    + $"{Math.Round(refrigerant.CriticalTemperature.DegreesCelsius, 2)}) °C!"
            );
        RuleFor(evaporator => evaporator.Superheat)
            .InclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
            .WithMessage("Superheat in the evaporator should be in [0;50] K!");
    }
}

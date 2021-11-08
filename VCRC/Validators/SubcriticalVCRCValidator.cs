using System;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Validators
{
    public class SubcriticalVCRCValidator : AbstractValidator<SubcriticalVCRC>
    {
        public SubcriticalVCRCValidator(Refrigerant refrigerant)
        {
            RuleFor(vcrc => vcrc.CondensingTemperature).GreaterThan(vcrc => vcrc.EvaporatingTemperature)
                .WithMessage("Condensing temperature should be greater than evaporating temperature!");
            RuleFor(vcrc => vcrc.CondensingTemperature)
                .ExclusiveBetween(refrigerant.TripleTemperature, refrigerant.CriticalTemperature)
                .WithMessage(
                    "Condensing temperature should be in " +
                    $"({Math.Round(refrigerant.TripleTemperature.DegreesCelsius, 2)};" +
                    $"{Math.Round(refrigerant.CriticalTemperature.DegreesCelsius, 2)}) °C!");
            RuleFor(vcrc => vcrc.Subcooling).InclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
                .WithMessage("Subcooling in the condenser should be in [0;50] K!");
        }
    }
}
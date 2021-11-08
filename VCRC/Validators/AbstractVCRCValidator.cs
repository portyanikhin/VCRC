using System;
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC.Validators
{
    public class AbstractVCRCValidator : AbstractValidator<AbstractVCRC>
    {
        public AbstractVCRCValidator(Refrigerant refrigerant)
        {
            RuleFor(vcrc => vcrc.EvaporatingTemperature)
                .ExclusiveBetween(refrigerant.TripleTemperature, refrigerant.CriticalTemperature)
                .WithMessage(
                    "Evaporating temperature should be in " +
                    $"({Math.Round(refrigerant.TripleTemperature.DegreesCelsius, 2)};" +
                    $"{Math.Round(refrigerant.CriticalTemperature.DegreesCelsius, 2)}) °C!");
            RuleFor(vcrc => vcrc.Superheat).InclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
                .WithMessage("Superheat in the evaporator should be in [0;50] K!");
            RuleFor(vcrc => vcrc.IsentropicEfficiency).ExclusiveBetween(Ratio.Zero, 100.Percent())
                .WithMessage("Isentropic efficiency of the compressor should be in (0;100) %!");
        }
    }
}
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
            RuleFor(vcrc => vcrc.EvaporatingTemperature).GreaterThan(refrigerant.TripleTemperature)
                .LessThan(refrigerant.CriticalTemperature)
                .WithMessage(
                    "Evaporating temperature should be in " +
                    $"({refrigerant.TripleTemperature.DegreesCelsius};" +
                    $"{refrigerant.CriticalTemperature.DegreesCelsius}) °C!");
            RuleFor(vcrc => vcrc.Superheat).GreaterThanOrEqualTo(TemperatureDelta.Zero).LessThanOrEqualTo(50.Kelvins())
                .WithMessage("Superheat in the evaporator should be in [0;50] K!");
            RuleFor(vcrc => vcrc.IsentropicEfficiency).GreaterThan(Ratio.Zero).LessThan(100.Percent())
                .WithMessage("Isentropic efficiency of the compressor should be in (0;100) %!");
        }
    }
}
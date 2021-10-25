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
            RuleFor(vcrc => vcrc.CondensingTemperature).GreaterThan(refrigerant.TripleTemperature)
                .LessThan(refrigerant.CriticalTemperature)
                .WithMessage(
                    "Condensing temperature should be in " +
                    $"({refrigerant.TripleTemperature.DegreesCelsius};" +
                    $"{refrigerant.CriticalTemperature.DegreesCelsius}) °C!");
            RuleFor(vcrc => vcrc.Subcooling).GreaterThanOrEqualTo(TemperatureDelta.Zero).LessThanOrEqualTo(50.Kelvins())
                .WithMessage("Subcooling in the condenser should be in [0;50] K!");
        }
    }
}
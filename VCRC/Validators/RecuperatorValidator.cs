using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;
using VCRC.Components;

namespace VCRC.Validators
{
    public class RecuperatorValidator : AbstractValidator<Recuperator>
    {
        public RecuperatorValidator()
        {
            RuleFor(recuperator => recuperator.Superheat).InclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
                .WithMessage("Superheat in the recuperator should be in [0;50] K!");
        }
    }
}
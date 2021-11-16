using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;
using VCRC.Components;

namespace VCRC.Validators
{
    public class EconomizerValidator : AbstractValidator<Economizer>
    {
        public EconomizerValidator()
        {
            RuleFor(economizer => economizer.Superheat).InclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
                .WithMessage("Superheat in the economizer should be in [0;50] K!");
        }
    }
}
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC;

internal class EconomizerWithTPIValidator
    : AbstractValidator<IAuxiliaryHeatExchanger>
{
    public EconomizerWithTPIValidator() =>
        RuleFor(economizer => economizer.TemperatureDifference)
            .ExclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
            .WithMessage(
                "Temperature difference at the economizer 'cold' side "
                    + "should be in (0;50) K!"
            );
}

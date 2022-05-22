using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;

namespace VCRC;

internal record MixingInfo(Refrigerant Outlet, Ratio FirstSpecificMassFlow, Refrigerant First,
    Ratio SecondSpecificMassFlow, Refrigerant Second)
{
    internal SpecificEnergy EnergyLoss(Temperature hotSource) =>
        (hotSource.Kelvins *
         ((FirstSpecificMassFlow +
           SecondSpecificMassFlow).DecimalFractions * Outlet.Entropy -
          (FirstSpecificMassFlow.DecimalFractions * First.Entropy +
           SecondSpecificMassFlow.DecimalFractions * Second.Entropy))
         .JoulesPerKilogramKelvin)
        .JoulesPerKilogram();
}
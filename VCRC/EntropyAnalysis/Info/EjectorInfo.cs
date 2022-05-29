using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;

namespace VCRC;

internal record EjectorInfo(Refrigerant Outlet,
    Ratio NozzleSpecificMassFlow, Refrigerant NozzleInlet,
    Ratio SuctionSpecificMassFlow, Refrigerant SuctionInlet)
{
    internal SpecificEnergy EnergyLoss(Temperature hotSource) =>
        (hotSource.Kelvins *
         ((NozzleSpecificMassFlow +
           SuctionSpecificMassFlow).DecimalFractions * Outlet.Entropy -
          (NozzleSpecificMassFlow.DecimalFractions * NozzleInlet.Entropy +
           SuctionSpecificMassFlow.DecimalFractions * SuctionInlet.Entropy))
         .JoulesPerKilogramKelvin)
        .JoulesPerKilogram();
}
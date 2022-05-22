using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;

namespace VCRC;

internal record EVInfo(Ratio SpecificMassFlow, Refrigerant Inlet, Refrigerant Outlet)
{
    internal SpecificEnergy EnergyLoss(Temperature hotSource) =>
        (hotSource.Kelvins *
         (SpecificMassFlow.DecimalFractions * (Outlet.Entropy - Inlet.Entropy))
         .JoulesPerKilogramKelvin)
        .JoulesPerKilogram();
}
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;

namespace VCRC;

internal record HeatReleaserInfo(Ratio SpecificMassFlow, Refrigerant IsentropicInlet, Refrigerant Outlet)
{
    internal SpecificEnergy EnergyLoss(Temperature hotSource) =>
        SpecificMassFlow.DecimalFractions *
        (IsentropicInlet.Enthalpy - Outlet.Enthalpy -
         (hotSource.Kelvins * (IsentropicInlet.Entropy - Outlet.Entropy)
             .JoulesPerKilogramKelvin)
         .JoulesPerKilogram());
}
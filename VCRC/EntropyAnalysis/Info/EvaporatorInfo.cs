using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;

namespace VCRC;

internal record EvaporatorInfo(Ratio SpecificMassFlow, Refrigerant Inlet, Refrigerant Outlet)
{
    internal SpecificEnergy EnergyLoss(Temperature coldSource, Temperature hotSource) =>
        (SpecificMassFlow.DecimalFractions * hotSource.Kelvins *
         ((Outlet.Entropy - Inlet.Entropy).JoulesPerKilogramKelvin -
          (Outlet.Enthalpy - Inlet.Enthalpy).JoulesPerKilogram / coldSource.Kelvins))
        .JoulesPerKilogram();
}
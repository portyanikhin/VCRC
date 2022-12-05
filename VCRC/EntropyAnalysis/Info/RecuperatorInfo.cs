namespace VCRC;

internal record RecuperatorInfo(Ratio ColdSideSpecificMassFlow, Refrigerant ColdInlet, Refrigerant ColdOutlet,
    Ratio HotSideSpecificMassFlow, Refrigerant HotInlet, Refrigerant HotOutlet)
{
    internal SpecificEnergy EnergyLoss(Temperature hotSource) =>
        (hotSource.Kelvins *
         (ColdSideSpecificMassFlow.DecimalFractions * (ColdOutlet.Entropy - ColdInlet.Entropy) -
          HotSideSpecificMassFlow.DecimalFractions * (HotInlet.Entropy - HotOutlet.Entropy))
         .JoulesPerKilogramKelvin)
        .JoulesPerKilogram();
}
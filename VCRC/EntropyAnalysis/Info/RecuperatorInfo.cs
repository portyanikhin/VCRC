namespace VCRC;

internal record RecuperatorInfo(
    Ratio ColdSideSpecificMassFlow,
    Refrigerant ColdInlet,
    Refrigerant ColdOutlet,
    Ratio HotSideSpecificMassFlow,
    Refrigerant HotInlet,
    Refrigerant HotOutlet
)
{
    public Ratio ColdSideSpecificMassFlow { get; } = ColdSideSpecificMassFlow;
    public Refrigerant ColdInlet { get; } = ColdInlet;
    public Refrigerant ColdOutlet { get; } = ColdOutlet;
    public Ratio HotSideSpecificMassFlow { get; } = HotSideSpecificMassFlow;
    public Refrigerant HotInlet { get; } = HotInlet;
    public Refrigerant HotOutlet { get; } = HotOutlet;

    internal SpecificEnergy EnergyLoss(Temperature hotSource) =>
        (
            hotSource.Kelvins
            * (
                ColdSideSpecificMassFlow.DecimalFractions
                    * (ColdOutlet.Entropy - ColdInlet.Entropy)
                - HotSideSpecificMassFlow.DecimalFractions
                    * (HotInlet.Entropy - HotOutlet.Entropy)
            ).JoulesPerKilogramKelvin
        ).JoulesPerKilogram();
}

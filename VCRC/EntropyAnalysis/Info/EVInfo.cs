namespace VCRC;

internal record EVInfo(
    Ratio SpecificMassFlow,
    Refrigerant Inlet,
    Refrigerant Outlet
)
{
    public Ratio SpecificMassFlow { get; } = SpecificMassFlow;
    public Refrigerant Inlet { get; } = Inlet;
    public Refrigerant Outlet { get; } = Outlet;

    internal SpecificEnergy EnergyLoss(Temperature hotSource) =>
        (
            hotSource.Kelvins
            * (
                SpecificMassFlow.DecimalFractions
                * (Outlet.Entropy - Inlet.Entropy)
            ).JoulesPerKilogramKelvin
        ).JoulesPerKilogram();
}

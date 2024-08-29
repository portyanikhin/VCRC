namespace VCRC;

internal sealed class EvaporatorNode(
    Ratio specificMassFlow,
    IRefrigerant inlet,
    IRefrigerant outlet
) : IEvaporatorNode
{
    public IRefrigerant Outlet { get; } = outlet;

    public SpecificEnergy CalculateEnergyLoss(Temperature coldSource, Temperature hotSource) =>
        (
            specificMassFlow.DecimalFractions
            * hotSource.Kelvins
            * (
                (Outlet.Entropy - inlet.Entropy).JoulesPerKilogramKelvin
                - (Outlet.Enthalpy - inlet.Enthalpy).JoulesPerKilogram / coldSource.Kelvins
            )
        ).JoulesPerKilogram();
}

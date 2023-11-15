namespace VCRC;

internal class EVNode(
    Ratio specificMassFlow,
    IRefrigerant inlet,
    IRefrigerant outlet
) : IEntropyAnalysisNode
{
    public SpecificEnergy CalculateEnergyLoss(Temperature hotSource) =>
        (
            hotSource.Kelvins
            * (
                specificMassFlow.DecimalFractions
                * (outlet.Entropy - inlet.Entropy)
            ).JoulesPerKilogramKelvin
        ).JoulesPerKilogram();
}

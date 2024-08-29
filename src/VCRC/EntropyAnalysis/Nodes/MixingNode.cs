namespace VCRC;

internal sealed class MixingNode(
    IRefrigerant outlet,
    Ratio firstSpecificMassFlow,
    IRefrigerant first,
    Ratio secondSpecificMassFlow,
    IRefrigerant second
) : IEntropyAnalysisNode
{
    public SpecificEnergy CalculateEnergyLoss(Temperature hotSource) =>
        (
            hotSource.Kelvins
            * (
                (firstSpecificMassFlow + secondSpecificMassFlow).DecimalFractions * outlet.Entropy
                - (
                    firstSpecificMassFlow.DecimalFractions * first.Entropy
                    + secondSpecificMassFlow.DecimalFractions * second.Entropy
                )
            ).JoulesPerKilogramKelvin
        ).JoulesPerKilogram();
}

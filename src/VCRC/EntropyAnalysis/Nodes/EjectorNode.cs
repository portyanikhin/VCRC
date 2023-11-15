namespace VCRC;

internal class EjectorNode(
    IRefrigerant outlet,
    Ratio nozzleSpecificMassFlow,
    IRefrigerant nozzleInlet,
    Ratio suctionSpecificMassFlow,
    IRefrigerant suctionInlet
) : IEntropyAnalysisNode
{
    public SpecificEnergy CalculateEnergyLoss(Temperature hotSource) =>
        (
            hotSource.Kelvins
            * (
                (
                    nozzleSpecificMassFlow + suctionSpecificMassFlow
                ).DecimalFractions * outlet.Entropy
                - (
                    nozzleSpecificMassFlow.DecimalFractions
                        * nozzleInlet.Entropy
                    + suctionSpecificMassFlow.DecimalFractions
                        * suctionInlet.Entropy
                )
            ).JoulesPerKilogramKelvin
        ).JoulesPerKilogram();
}

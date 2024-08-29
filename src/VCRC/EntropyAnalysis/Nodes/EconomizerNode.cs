namespace VCRC;

internal sealed class EconomizerNode(
    Ratio coldSideSpecificMassFlow,
    IRefrigerant coldInlet,
    IRefrigerant coldOutlet,
    Ratio hotSideSpecificMassFlow,
    IRefrigerant hotInlet,
    IRefrigerant hotOutlet
) : IEntropyAnalysisNode
{
    public SpecificEnergy CalculateEnergyLoss(Temperature hotSource) =>
        (
            hotSource.Kelvins
            * (
                coldSideSpecificMassFlow.DecimalFractions * (coldOutlet.Entropy - coldInlet.Entropy)
                - hotSideSpecificMassFlow.DecimalFractions * (hotInlet.Entropy - hotOutlet.Entropy)
            ).JoulesPerKilogramKelvin
        ).JoulesPerKilogram();
}

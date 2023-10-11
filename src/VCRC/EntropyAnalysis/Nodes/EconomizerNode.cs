namespace VCRC;

internal class EconomizerNode : IEntropyAnalysisNode
{
    private readonly IRefrigerant _coldInlet;
    private readonly IRefrigerant _coldOutlet;
    private readonly Ratio _coldSideSpecificMassFlow;
    private readonly IRefrigerant _hotInlet;
    private readonly IRefrigerant _hotOutlet;
    private readonly Ratio _hotSideSpecificMassFlow;

    public EconomizerNode(
        Ratio coldSideSpecificMassFlow,
        IRefrigerant coldInlet,
        IRefrigerant coldOutlet,
        Ratio hotSideSpecificMassFlow,
        IRefrigerant hotInlet,
        IRefrigerant hotOutlet
    )
    {
        _coldSideSpecificMassFlow = coldSideSpecificMassFlow;
        _coldInlet = coldInlet;
        _coldOutlet = coldOutlet;
        _hotSideSpecificMassFlow = hotSideSpecificMassFlow;
        _hotInlet = hotInlet;
        _hotOutlet = hotOutlet;
    }

    public SpecificEnergy CalculateEnergyLoss(Temperature hotSource) =>
        (
            hotSource.Kelvins
            * (
                _coldSideSpecificMassFlow.DecimalFractions
                    * (_coldOutlet.Entropy - _coldInlet.Entropy)
                - _hotSideSpecificMassFlow.DecimalFractions
                    * (_hotInlet.Entropy - _hotOutlet.Entropy)
            ).JoulesPerKilogramKelvin
        ).JoulesPerKilogram();
}

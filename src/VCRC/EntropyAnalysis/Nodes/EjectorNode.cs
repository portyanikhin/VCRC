namespace VCRC;

internal class EjectorNode : IEntropyAnalysisNode
{
    private readonly IRefrigerant _nozzleInlet;
    private readonly Ratio _nozzleSpecificMassFlow;
    private readonly IRefrigerant _outlet;
    private readonly IRefrigerant _suctionInlet;
    private readonly Ratio _suctionSpecificMassFlow;

    public EjectorNode(
        IRefrigerant outlet,
        Ratio nozzleSpecificMassFlow,
        IRefrigerant nozzleInlet,
        Ratio suctionSpecificMassFlow,
        IRefrigerant suctionInlet
    )
    {
        _outlet = outlet;
        _nozzleSpecificMassFlow = nozzleSpecificMassFlow;
        _nozzleInlet = nozzleInlet;
        _suctionSpecificMassFlow = suctionSpecificMassFlow;
        _suctionInlet = suctionInlet;
    }

    public SpecificEnergy CalculateEnergyLoss(Temperature hotSource) =>
        (
            hotSource.Kelvins
            * (
                (
                    _nozzleSpecificMassFlow + _suctionSpecificMassFlow
                ).DecimalFractions * _outlet.Entropy
                - (
                    _nozzleSpecificMassFlow.DecimalFractions
                        * _nozzleInlet.Entropy
                    + _suctionSpecificMassFlow.DecimalFractions
                        * _suctionInlet.Entropy
                )
            ).JoulesPerKilogramKelvin
        ).JoulesPerKilogram();
}

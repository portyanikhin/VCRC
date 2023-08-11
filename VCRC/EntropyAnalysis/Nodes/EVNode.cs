namespace VCRC;

internal class EVNode : IEntropyAnalysisNode
{
    private readonly IRefrigerant _inlet;
    private readonly IRefrigerant _outlet;
    private readonly Ratio _specificMassFlow;

    public EVNode(
        Ratio specificMassFlow,
        IRefrigerant inlet,
        IRefrigerant outlet
    )
    {
        _specificMassFlow = specificMassFlow;
        _inlet = inlet;
        _outlet = outlet;
    }

    public SpecificEnergy CalculateEnergyLoss(Temperature hotSource) =>
        (
            hotSource.Kelvins
            * (
                _specificMassFlow.DecimalFractions
                * (_outlet.Entropy - _inlet.Entropy)
            ).JoulesPerKilogramKelvin
        ).JoulesPerKilogram();
}

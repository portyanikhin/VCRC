namespace VCRC;

internal class MixingNode : IEntropyAnalysisNode
{
    private readonly IRefrigerant _first;
    private readonly Ratio _firstSpecificMassFlow;
    private readonly IRefrigerant _outlet;
    private readonly IRefrigerant _second;
    private readonly Ratio _secondSpecificMassFlow;

    public MixingNode(
        IRefrigerant outlet,
        Ratio firstSpecificMassFlow,
        IRefrigerant first,
        Ratio secondSpecificMassFlow,
        IRefrigerant second
    )
    {
        _outlet = outlet;
        _firstSpecificMassFlow = firstSpecificMassFlow;
        _first = first;
        _secondSpecificMassFlow = secondSpecificMassFlow;
        _second = second;
    }

    public SpecificEnergy CalculateEnergyLoss(Temperature hotSource) =>
        (
            hotSource.Kelvins
            * (
                (
                    _firstSpecificMassFlow + _secondSpecificMassFlow
                ).DecimalFractions * _outlet.Entropy
                - (
                    _firstSpecificMassFlow.DecimalFractions * _first.Entropy
                    + _secondSpecificMassFlow.DecimalFractions * _second.Entropy
                )
            ).JoulesPerKilogramKelvin
        ).JoulesPerKilogram();
}

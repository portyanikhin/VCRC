namespace VCRC;

internal class EvaporatorNode : IEvaporatorNode
{
    private readonly IRefrigerant _inlet;
    private readonly Ratio _specificMassFlow;

    public EvaporatorNode(
        Ratio specificMassFlow,
        IRefrigerant inlet,
        IRefrigerant outlet
    )
    {
        _specificMassFlow = specificMassFlow;
        _inlet = inlet;
        Outlet = outlet;
    }

    public IRefrigerant Outlet { get; }

    public SpecificEnergy CalculateEnergyLoss(
        Temperature coldSource,
        Temperature hotSource
    ) =>
        (
            _specificMassFlow.DecimalFractions
            * hotSource.Kelvins
            * (
                (Outlet.Entropy - _inlet.Entropy).JoulesPerKilogramKelvin
                - (Outlet.Enthalpy - _inlet.Enthalpy).JoulesPerKilogram
                    / coldSource.Kelvins
            )
        ).JoulesPerKilogram();
}

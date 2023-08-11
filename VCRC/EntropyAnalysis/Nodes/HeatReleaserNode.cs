namespace VCRC;

internal class HeatReleaserNode : IHeatReleaserNode
{
    private readonly IRefrigerant _isentropicInlet;
    private readonly Ratio _specificMassFlow;

    public HeatReleaserNode(
        Ratio specificMassFlow,
        IRefrigerant isentropicInlet,
        IRefrigerant outlet
    )
    {
        _specificMassFlow = specificMassFlow;
        _isentropicInlet = isentropicInlet;
        Outlet = outlet;
    }

    public IRefrigerant Outlet { get; }

    public SpecificEnergy CalculateEnergyLoss(Temperature hotSource) =>
        _specificMassFlow.DecimalFractions
        * (
            _isentropicInlet.Enthalpy
            - Outlet.Enthalpy
            - (
                hotSource.Kelvins
                * (
                    _isentropicInlet.Entropy - Outlet.Entropy
                ).JoulesPerKilogramKelvin
            ).JoulesPerKilogram()
        );
}

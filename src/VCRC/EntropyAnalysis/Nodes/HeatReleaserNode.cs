namespace VCRC;

internal sealed class HeatReleaserNode(
    Ratio specificMassFlow,
    IRefrigerant isentropicInlet,
    IRefrigerant outlet
) : IHeatReleaserNode
{
    public IRefrigerant Outlet { get; } = outlet;

    public SpecificEnergy CalculateEnergyLoss(Temperature hotSource) =>
        specificMassFlow.DecimalFractions
        * (
            isentropicInlet.Enthalpy
            - Outlet.Enthalpy
            - (
                hotSource.Kelvins
                * (isentropicInlet.Entropy - Outlet.Entropy).JoulesPerKilogramKelvin
            ).JoulesPerKilogram()
        );
}

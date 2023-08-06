namespace VCRC;

internal record HeatReleaserInfo(
    Ratio SpecificMassFlow,
    Refrigerant IsentropicInlet,
    Refrigerant Outlet
)
{
    public Ratio SpecificMassFlow { get; } = SpecificMassFlow;
    public Refrigerant IsentropicInlet { get; } = IsentropicInlet;
    public Refrigerant Outlet { get; } = Outlet;

    internal SpecificEnergy EnergyLoss(Temperature hotSource) =>
        SpecificMassFlow.DecimalFractions
        * (
            IsentropicInlet.Enthalpy
            - Outlet.Enthalpy
            - (
                hotSource.Kelvins
                * (
                    IsentropicInlet.Entropy - Outlet.Entropy
                ).JoulesPerKilogramKelvin
            ).JoulesPerKilogram()
        );
}

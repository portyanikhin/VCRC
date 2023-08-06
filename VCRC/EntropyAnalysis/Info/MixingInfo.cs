namespace VCRC;

internal record MixingInfo(
    Refrigerant Outlet,
    Ratio FirstSpecificMassFlow,
    Refrigerant First,
    Ratio SecondSpecificMassFlow,
    Refrigerant Second
)
{
    public Refrigerant Outlet { get; } = Outlet;
    public Ratio FirstSpecificMassFlow { get; } = FirstSpecificMassFlow;
    public Refrigerant First { get; } = First;
    public Ratio SecondSpecificMassFlow { get; } = SecondSpecificMassFlow;
    public Refrigerant Second { get; } = Second;

    internal SpecificEnergy EnergyLoss(Temperature hotSource) =>
        (
            hotSource.Kelvins
            * (
                (
                    FirstSpecificMassFlow + SecondSpecificMassFlow
                ).DecimalFractions * Outlet.Entropy
                - (
                    FirstSpecificMassFlow.DecimalFractions * First.Entropy
                    + SecondSpecificMassFlow.DecimalFractions * Second.Entropy
                )
            ).JoulesPerKilogramKelvin
        ).JoulesPerKilogram();
}

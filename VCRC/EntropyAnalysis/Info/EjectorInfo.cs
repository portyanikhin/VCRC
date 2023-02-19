namespace VCRC;

internal record EjectorInfo(Refrigerant Outlet,
    Ratio NozzleSpecificMassFlow, Refrigerant NozzleInlet,
    Ratio SuctionSpecificMassFlow, Refrigerant SuctionInlet)
{
    public Refrigerant Outlet { get; } = Outlet;
    public Ratio NozzleSpecificMassFlow { get; } = NozzleSpecificMassFlow;
    public Refrigerant NozzleInlet { get; } = NozzleInlet;
    public Ratio SuctionSpecificMassFlow { get; } = SuctionSpecificMassFlow;
    public Refrigerant SuctionInlet { get; } = SuctionInlet;

    internal SpecificEnergy EnergyLoss(Temperature hotSource) =>
        (hotSource.Kelvins *
         ((NozzleSpecificMassFlow +
           SuctionSpecificMassFlow).DecimalFractions * Outlet.Entropy -
          (NozzleSpecificMassFlow.DecimalFractions * NozzleInlet.Entropy +
           SuctionSpecificMassFlow.DecimalFractions * SuctionInlet.Entropy))
         .JoulesPerKilogramKelvin)
        .JoulesPerKilogram();
}
namespace VCRC;

/// <summary>
///     Ejector flows.
/// </summary>
public class EjectorFlows : IEjectorFlows
{
    /// <summary>
    ///     Ejector flows.
    /// </summary>
    /// <param name="ejector">Ejector.</param>
    /// <param name="nozzleInlet">Nozzle inlet.</param>
    /// <param name="suctionInlet">Suction section inlet.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Ejector nozzle inlet pressure
    ///     should be greater than suction inlet pressure!
    /// </exception>
    public EjectorFlows(
        IEjector ejector,
        IRefrigerant nozzleInlet,
        IRefrigerant suctionInlet
    )
    {
        Refrigerant = new Refrigerant(nozzleInlet.Name);
        Ejector = ejector;
        NozzleInlet = nozzleInlet;
        SuctionInlet = suctionInlet;
        new EjectorFlowsValidator().ValidateAndThrow(this);
        NozzleOutlet = NozzleInlet.ExpansionTo(
            MixingPressure,
            Ejector.NozzleEfficiency
        );
        SuctionOutlet = SuctionInlet.ExpansionTo(
            MixingPressure,
            Ejector.SuctionEfficiency
        );
        CalculateFlowRatio();
    }

    private IRefrigerant Refrigerant { get; }

    private IEjector Ejector { get; }

    private Pressure MixingPressure => 0.9 * SuctionInlet.Pressure;

    private Speed MixingInletSpeed =>
        FlowRatio.DecimalFractions
            * CalculateOutletSpeed(NozzleInlet, NozzleOutlet)
        + (1 - FlowRatio.DecimalFractions)
            * CalculateOutletSpeed(SuctionInlet, SuctionOutlet);

    private SpecificEnergy MixingInletKineticEnergy =>
        (
            Math.Pow(MixingInletSpeed.MetersPerSecond, 2) / 2.0
        ).JoulesPerKilogram();

    public IRefrigerant NozzleInlet { get; }

    public IRefrigerant SuctionInlet { get; }

    public IRefrigerant MixingInlet { get; private set; } = null!;

    public IRefrigerant NozzleOutlet { get; }

    public IRefrigerant SuctionOutlet { get; }

    public IRefrigerant DiffuserOutlet { get; private set; } = null!;

    public Ratio FlowRatio { get; private set; }

    private void CalculateFlowRatio()
    {
        NewtonRaphson.FindRootNearGuess(
            ToSolve,
            Differentiate.FirstDerivativeFunc(ToSolve),
            50,
            1e-9,
            100 - 1e-9,
            1e-6
        );
        return;

        double ToSolve(double flowRatio)
        {
            FlowRatio = flowRatio.Percent();
            MixingInlet = Refrigerant.WithState(
                Input.Pressure(MixingPressure),
                Input.Enthalpy(
                    FlowRatio.DecimalFractions * NozzleInlet.Enthalpy
                        + (1 - FlowRatio.DecimalFractions)
                            * SuctionInlet.Enthalpy
                        - MixingInletKineticEnergy
                )
            );
            DiffuserOutlet = Refrigerant.WithState(
                Input.Pressure(
                    Refrigerant
                        .WithState(
                            Input.Entropy(MixingInlet.Entropy),
                            Input.Enthalpy(
                                MixingInlet.Enthalpy
                                    + Ejector
                                        .DiffuserEfficiency
                                        .DecimalFractions
                                        * MixingInletKineticEnergy
                            )
                        )
                        .Pressure
                ),
                Input.Enthalpy(MixingInlet.Enthalpy + MixingInletKineticEnergy)
            );
            return (DiffuserOutlet.Quality!.Value - FlowRatio).Percent;
        }
    }

    private static Speed CalculateOutletSpeed(
        IFluidState inlet,
        IFluidState outlet
    ) =>
        Math.Sqrt(2 * (inlet.Enthalpy - outlet.Enthalpy).JoulesPerKilogram)
            .MetersPerSecond();
}

namespace VCRC;

/// <inheritdoc cref="IEjector"/>
public record Ejector : IEjector
{
    /// <inheritdoc cref="Ejector"/>
    /// <param name="efficiency">
    ///     Isentropic efficiency of the nozzle, suction section and diffuser.
    /// </param>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the nozzle should be in (0;100) %!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the suction section should be in (0;100) %!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the diffuser should be in (0;100) %!
    /// </exception>
    public Ejector(Ratio efficiency)
        : this(efficiency, efficiency, efficiency) { }

    /// <inheritdoc cref="Ejector"/>
    /// <param name="nozzleEfficiency">Isentropic efficiency of the nozzle.</param>
    /// <param name="suctionEfficiency">Isentropic efficiency of the suction section.</param>
    /// <param name="diffuserEfficiency">Isentropic efficiency of the diffuser.</param>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the nozzle should be in (0;100) %!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the suction section should be in (0;100) %!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the diffuser should be in (0;100) %!
    /// </exception>
    public Ejector(Ratio nozzleEfficiency, Ratio suctionEfficiency, Ratio diffuserEfficiency)
    {
        NozzleEfficiency = nozzleEfficiency.ToUnit(RatioUnit.Percent);
        SuctionEfficiency = suctionEfficiency.ToUnit(RatioUnit.Percent);
        DiffuserEfficiency = diffuserEfficiency.ToUnit(RatioUnit.Percent);
        new EjectorValidator().ValidateAndThrow(this);
    }

    public Ratio NozzleEfficiency { get; }
    public Ratio SuctionEfficiency { get; }
    public Ratio DiffuserEfficiency { get; }

    public IEjectorFlows CalculateFlows(IRefrigerant nozzleInlet, IRefrigerant suctionInlet) =>
        new EjectorFlows(this, nozzleInlet, suctionInlet);
}

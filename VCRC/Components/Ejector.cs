namespace VCRC;

/// <summary>
///     Ejector as a VCRC component.
/// </summary>
public record Ejector
{
    /// <summary>
    ///     Ejector as a VCRC component.
    /// </summary>
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

    /// <summary>
    ///     Ejector as a VCRC component.
    /// </summary>
    /// <param name="nozzleEfficiency">
    ///     Isentropic efficiency of the nozzle.
    /// </param>
    /// <param name="suctionEfficiency">
    ///     Isentropic efficiency of the suction section.
    /// </param>
    /// <param name="diffuserEfficiency">
    ///     Isentropic efficiency of the diffuser.
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
    public Ejector(
        Ratio nozzleEfficiency,
        Ratio suctionEfficiency,
        Ratio diffuserEfficiency
    )
    {
        (NozzleEfficiency, SuctionEfficiency, DiffuserEfficiency) = (
            nozzleEfficiency.ToUnit(RatioUnit.Percent),
            suctionEfficiency.ToUnit(RatioUnit.Percent),
            diffuserEfficiency.ToUnit(RatioUnit.Percent)
        );
        new EjectorValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Isentropic efficiency of the nozzle.
    /// </summary>
    public Ratio NozzleEfficiency { get; }

    /// <summary>
    ///     Isentropic efficiency of the suction section.
    /// </summary>
    public Ratio SuctionEfficiency { get; }

    /// <summary>
    ///     Isentropic efficiency of the diffuser.
    /// </summary>
    public Ratio DiffuserEfficiency { get; }

    /// <summary>
    ///     Calculate ejector flows.
    /// </summary>
    /// <param name="nozzleInlet">Nozzle inlet.</param>
    /// <param name="suctionInlet">Suction section inlet.</param>
    /// <returns>Ejector flows.</returns>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Ejector nozzle inlet pressure
    ///     should be greater than suction inlet pressure!
    /// </exception>
    public EjectorFlows CalculateFlows(
        Refrigerant nozzleInlet,
        Refrigerant suctionInlet
    ) => new(this, nozzleInlet, suctionInlet);
}

namespace VCRC;

/// <summary>
///     Ejector.
/// </summary>
public interface IEjector
{
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
    public IEjectorFlows CalculateFlows(
        IRefrigerant nozzleInlet,
        IRefrigerant suctionInlet
    );
}

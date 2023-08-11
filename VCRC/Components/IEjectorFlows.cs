namespace VCRC;

/// <summary>
///     Ejector flows.
/// </summary>
public interface IEjectorFlows
{
    /// <summary>
    ///     Nozzle inlet.
    /// </summary>
    public IRefrigerant NozzleInlet { get; }

    /// <summary>
    ///     Suction section inlet.
    /// </summary>
    public IRefrigerant SuctionInlet { get; }

    /// <summary>
    ///     Mixing section inlet.
    /// </summary>
    public IRefrigerant MixingInlet { get; }

    /// <summary>
    ///     Nozzle outlet.
    /// </summary>
    public IRefrigerant NozzleOutlet { get; }

    /// <summary>
    ///     Suction section outlet.
    /// </summary>
    public IRefrigerant SuctionOutlet { get; }

    /// <summary>
    ///     Diffuser outlet.
    /// </summary>
    public IRefrigerant DiffuserOutlet { get; }

    /// <summary>
    ///     Flow ratio.
    /// </summary>
    public Ratio FlowRatio { get; }
}

namespace VCRC;

/// <summary>
///     Ejector flows.
/// </summary>
public interface IEjectorFlows
{
    /// <summary>
    ///     Nozzle inlet.
    /// </summary>
    IRefrigerant NozzleInlet { get; }

    /// <summary>
    ///     Suction section inlet.
    /// </summary>
    IRefrigerant SuctionInlet { get; }

    /// <summary>
    ///     Mixing section inlet.
    /// </summary>
    IRefrigerant MixingInlet { get; }

    /// <summary>
    ///     Nozzle outlet.
    /// </summary>
    IRefrigerant NozzleOutlet { get; }

    /// <summary>
    ///     Suction section outlet.
    /// </summary>
    IRefrigerant SuctionOutlet { get; }

    /// <summary>
    ///     Diffuser outlet.
    /// </summary>
    IRefrigerant DiffuserOutlet { get; }

    /// <summary>
    ///     Flow ratio.
    /// </summary>
    Ratio FlowRatio { get; }
}

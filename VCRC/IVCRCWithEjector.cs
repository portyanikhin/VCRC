namespace VCRC;

/// <summary>
///     Single-stage VCRC with an ejector as an expansion device.
/// </summary>
public interface IVCRCWithEjector : IVCRC, IHaveEjector
{
    /// <summary>
    ///     Point 1 - separator vapor outlet / compression stage suction.
    /// </summary>
    public IRefrigerant Point1 { get; }

    /// <summary>
    ///     Point 2s – isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point2s { get; }

    /// <summary>
    ///     Point 2 – compression stage discharge /
    ///     condenser or gas cooler inlet.
    /// </summary>
    public IRefrigerant Point2 { get; }

    /// <summary>
    ///     Point 3 – condenser or gas cooler outlet / ejector nozzle inlet.
    /// </summary>
    public IRefrigerant Point3 { get; }

    /// <summary>
    ///     Point 4 – ejector nozzle outlet.
    /// </summary>
    public IRefrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – ejector mixing section inlet.
    /// </summary>
    public IRefrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – ejector diffuser outlet / separator inlet.
    /// </summary>
    public IRefrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – separator liquid outlet / EV inlet.
    /// </summary>
    public IRefrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – EV outlet / evaporator inlet.
    /// </summary>
    public IRefrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – evaporator outlet / ejector suction section inlet.
    /// </summary>
    public IRefrigerant Point9 { get; }

    /// <summary>
    ///     Point 10 – ejector suction section outlet.
    /// </summary>
    public IRefrigerant Point10 { get; }
}

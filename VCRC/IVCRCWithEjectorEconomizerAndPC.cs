namespace VCRC;

/// <summary>
///     Two-stage VCRC with an ejector as an expansion device,
///     economizer and parallel compression.
/// </summary>
public interface IVCRCWithEjectorEconomizerAndPC
    : ITwoStageVCRC,
        IHaveEjector,
        IHaveEconomizer
{
    /// <summary>
    ///     Point 1 – separator vapor outlet / first compression stage suction.
    /// </summary>
    public IRefrigerant Point1 { get; }

    /// <summary>
    ///     Point 2s – first isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point2s { get; }

    /// <summary>
    ///     Point 2 – first compression stage discharge.
    /// </summary>
    public IRefrigerant Point2 { get; }

    /// <summary>
    ///     Point 3 – economizer "cold" outlet /
    ///     second compression stage suction.
    /// </summary>
    public IRefrigerant Point3 { get; }

    /// <summary>
    ///     Point 4s – second isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point4s { get; }

    /// <summary>
    ///     Point 4 – second compression stage discharge.
    /// </summary>
    public IRefrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – condenser or gas cooler inlet.
    /// </summary>
    public IRefrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – condenser or gas cooler outlet /
    ///     first EV inlet / economizer "hot" inlet.
    /// </summary>
    public IRefrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – first EV outlet / economizer "cold" inlet.
    /// </summary>
    public IRefrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – economizer "hot" outlet / ejector nozzle inlet.
    /// </summary>
    public IRefrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – ejector nozzle outlet.
    /// </summary>
    public IRefrigerant Point9 { get; }

    /// <summary>
    ///     Point 10 – ejector mixing section inlet.
    /// </summary>
    public IRefrigerant Point10 { get; }

    /// <summary>
    ///     Point 11 – ejector diffuser outlet / separator inlet.
    /// </summary>
    public IRefrigerant Point11 { get; }

    /// <summary>
    ///     Point 12 – separator liquid outlet / second EV inlet.
    /// </summary>
    public IRefrigerant Point12 { get; }

    /// <summary>
    ///     Point 13 – second EV outlet / evaporator inlet.
    /// </summary>
    public IRefrigerant Point13 { get; }

    /// <summary>
    ///     Point 14 – evaporator outlet / ejector suction section inlet.
    /// </summary>
    public IRefrigerant Point14 { get; }

    /// <summary>
    ///     Point 15 – ejector suction section outlet.
    /// </summary>
    public IRefrigerant Point15 { get; }
}

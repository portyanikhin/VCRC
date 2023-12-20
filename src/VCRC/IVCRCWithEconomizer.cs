namespace VCRC;

/// <summary>
///     Two-stage VCRC with economizer.
/// </summary>
public interface IVCRCWithEconomizer : ITwoStageVCRC, IHaveEconomizer
{
    /// <summary>
    ///     Point 1 – evaporator outlet / first compression stage suction.
    /// </summary>
    IRefrigerant Point1 { get; }

    /// <summary>
    ///     Point 2s – first isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    IRefrigerant Point2s { get; }

    /// <summary>
    ///     Point 2 – first compression stage discharge.
    /// </summary>
    IRefrigerant Point2 { get; }

    /// <summary>
    ///     Point 3 – second compression stage suction.
    /// </summary>
    IRefrigerant Point3 { get; }

    /// <summary>
    ///     Point 4s – second isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    IRefrigerant Point4s { get; }

    /// <summary>
    ///     Point 4 – second compression stage discharge /
    ///     condenser or gas cooler inlet.
    /// </summary>
    IRefrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – condenser or gas cooler outlet /
    ///     first EV inlet / economizer "hot" inlet.
    /// </summary>
    IRefrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – first EV outlet / economizer "cold" inlet.
    /// </summary>
    IRefrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – economizer "cold" outlet /
    ///     injection of cooled vapor into the compressor.
    /// </summary>
    IRefrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – economizer "hot" outlet / second EV inlet.
    /// </summary>
    IRefrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – second EV outlet / evaporator inlet.
    /// </summary>
    IRefrigerant Point9 { get; }
}

namespace VCRC;

/// <summary>
///     Mitsubishi Zubadan VCRC (subcritical only).
/// </summary>
/// <remarks>
///     Two-stage subcritical VCRC with economizer,
///     recuperator and two-phase injection into the compressor.
/// </remarks>
public interface IVCRCMitsubishiZubadan
    : ITwoStageVCRC,
        IHaveRecuperator,
        IHaveEconomizerWithTPI
{
    /// <summary>
    ///     Condenser.
    /// </summary>
    public new ICondenser Condenser { get; }

    /// <summary>
    ///     Absolute recuperator high pressure.
    /// </summary>
    public Pressure RecuperatorHighPressure { get; }

    /// <summary>
    ///     Point 1 – evaporator outlet / recuperator "cold" inlet.
    /// </summary>
    public IRefrigerant Point1 { get; }

    /// <summary>
    ///     Point 2 – recuperator "cold" outlet /
    ///     first compression stage suction.
    /// </summary>
    public IRefrigerant Point2 { get; }

    /// <summary>
    ///     Point 3s – first isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point3s { get; }

    /// <summary>
    ///     Point 3 – first compression stage discharge.
    /// </summary>
    public IRefrigerant Point3 { get; }

    /// <summary>
    ///     Point 4 – second compression stage suction.
    /// </summary>
    public IRefrigerant Point4 { get; }

    /// <summary>
    ///     Point 5s – second isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point5s { get; }

    /// <summary>
    ///     Point 5 – second compression stage discharge / condenser inlet.
    /// </summary>
    public IRefrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – condenser outlet / first EV inlet.
    /// </summary>
    public IRefrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – first EV outlet / recuperator "hot" inlet.
    /// </summary>
    public IRefrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – recuperator "hot" outlet /
    ///     second EV inlet / economizer "hot" inlet.
    /// </summary>
    public IRefrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – second EV outlet / economizer "cold" inlet.
    /// </summary>
    public IRefrigerant Point9 { get; }

    /// <summary>
    ///     Point 10 – economizer "cold" outlet /
    ///     injection of two-phase refrigerant into the compressor.
    /// </summary>
    public IRefrigerant Point10 { get; }

    /// <summary>
    ///     Point 11 – economizer "hot" outlet / third EV inlet.
    /// </summary>
    public IRefrigerant Point11 { get; }

    /// <summary>
    ///     Point 12 – third EV outlet / evaporator inlet.
    /// </summary>
    public IRefrigerant Point12 { get; }
}

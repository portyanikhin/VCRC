﻿namespace VCRC;

/// <summary>
///     Two-stage VCRC with incomplete intercooling.
/// </summary>
public interface IVCRCWithIIC : ITwoStageVCRC
{
    /// <summary>
    ///     Point 1 – evaporator outlet / first compression stage suction.
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
    ///     Point 3 – second compression stage suction.
    /// </summary>
    public IRefrigerant Point3 { get; }

    /// <summary>
    ///     Point 4s – second isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public IRefrigerant Point4s { get; }

    /// <summary>
    ///     Point 4 – second compression stage discharge /
    ///     condenser or gas cooler inlet.
    /// </summary>
    public IRefrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – condenser or gas cooler outlet / first EV inlet.
    /// </summary>
    public IRefrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – first EV outlet / separator inlet.
    /// </summary>
    public IRefrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – separator vapor outlet /
    ///     injection of cooled vapor into the compressor.
    /// </summary>
    public IRefrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – separator liquid outlet / second EV inlet.
    /// </summary>
    public IRefrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – second EV outlet / evaporator inlet.
    /// </summary>
    public IRefrigerant Point9 { get; }
}
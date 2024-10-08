﻿// ReSharper disable InconsistentNaming

namespace VCRC;

/// <summary>
///     Simple single-stage VCRC.
/// </summary>
public interface ISimpleVCRC : IVCRC
{
    /// <summary>
    ///     Point 1 – evaporator outlet / compression stage suction.
    /// </summary>
    IRefrigerant Point1 { get; }

    /// <summary>
    ///     Point 2s – isentropic compression stage discharge.
    /// </summary>
    IRefrigerant Point2s { get; }

    /// <summary>
    ///     Point 2 – compression stage discharge / condenser or gas cooler inlet.
    /// </summary>
    IRefrigerant Point2 { get; }

    /// <summary>
    ///     Point 3 – condenser or gas cooler outlet / EV inlet.
    /// </summary>
    IRefrigerant Point3 { get; }

    /// <summary>
    ///     Point 4 – EV outlet / evaporator inlet.
    /// </summary>
    IRefrigerant Point4 { get; }
}

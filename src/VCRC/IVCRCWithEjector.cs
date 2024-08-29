// ReSharper disable InconsistentNaming

namespace VCRC;

/// <summary>
///     Single-stage VCRC with an ejector as an expansion device.
/// </summary>
public interface IVCRCWithEjector : IVCRC, IHaveEjector
{
    /// <summary>
    ///     Point 1 - separator vapor outlet / compression stage suction.
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
    ///     Point 3 – condenser or gas cooler outlet / ejector nozzle inlet.
    /// </summary>
    IRefrigerant Point3 { get; }

    /// <summary>
    ///     Point 4 – ejector nozzle outlet.
    /// </summary>
    IRefrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – ejector mixing section inlet.
    /// </summary>
    IRefrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – ejector diffuser outlet / separator inlet.
    /// </summary>
    IRefrigerant Point6 { get; }

    /// <summary>
    ///     Point 7 – separator liquid outlet / EV inlet.
    /// </summary>
    IRefrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – EV outlet / evaporator inlet.
    /// </summary>
    IRefrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – evaporator outlet / ejector suction section inlet.
    /// </summary>
    IRefrigerant Point9 { get; }

    /// <summary>
    ///     Point 10 – ejector suction section outlet.
    /// </summary>
    IRefrigerant Point10 { get; }
}

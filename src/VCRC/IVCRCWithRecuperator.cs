// ReSharper disable InconsistentNaming

namespace VCRC;

/// <summary>
///     Single-stage VCRC with recuperator.
/// </summary>
public interface IVCRCWithRecuperator : IVCRC, IHaveRecuperator
{
    /// <summary>
    ///     Point 1 – evaporator outlet / recuperator "cold" inlet.
    /// </summary>
    IRefrigerant Point1 { get; }

    /// <summary>
    ///     Point 2 – recuperator "cold" outlet / compression stage suction.
    /// </summary>
    IRefrigerant Point2 { get; }

    /// <summary>
    ///     Point 3s – isentropic compression stage discharge.
    /// </summary>
    IRefrigerant Point3s { get; }

    /// <summary>
    ///     Point 3 – compression stage discharge / condenser or gas cooler inlet.
    /// </summary>
    IRefrigerant Point3 { get; }

    /// <summary>
    ///     Point 4 – condenser or gas cooler outlet / recuperator "hot" inlet.
    /// </summary>
    IRefrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – recuperator "hot" outlet / EV inlet.
    /// </summary>
    IRefrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – EV outlet / evaporator inlet.
    /// </summary>
    IRefrigerant Point6 { get; }
}

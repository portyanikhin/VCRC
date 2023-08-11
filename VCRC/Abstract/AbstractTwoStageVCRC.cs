namespace VCRC;

/// <summary>
///     Two-stage vapor-compression refrigeration cycle.
/// </summary>
public abstract class AbstractTwoStageVCRC : AbstractVCRC, ITwoStageVCRC
{
    /// <summary>
    ///     Two-stage vapor-compression refrigeration cycle.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature
    ///     should be greater than evaporating temperature!
    /// </exception>
    protected AbstractTwoStageVCRC(
        IEvaporator evaporator,
        ICompressor compressor,
        IHeatReleaser heatReleaser
    )
        : base(evaporator, compressor, heatReleaser) { }

    public virtual Pressure IntermediatePressure =>
        CalculateIntermediatePressure(
            Evaporator.Pressure,
            HeatReleaser.Pressure
        );

    public virtual Ratio IntermediateSpecificMassFlow =>
        HeatReleaserSpecificMassFlow - EvaporatorSpecificMassFlow;

    protected Pressure CalculateIntermediatePressure(
        Pressure low,
        Pressure high
    )
    {
        var result = GeometricMean(low, high);
        return result < Refrigerant.CriticalPressure
            ? result
            : GeometricMean(low, Refrigerant.CriticalPressure);
    }

    private static Pressure GeometricMean(Pressure low, Pressure high) =>
        Math.Sqrt(low.Pascals * high.Pascals)
            .Pascals()
            .ToUnit(PressureUnit.Kilopascal);
}

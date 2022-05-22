using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SharpProp;
using UnitsNet;

namespace VCRC;

/// <summary>
///     Two-stage VCRC with parallel compression.
/// </summary>
public class VCRCWithParallelCompression : AbstractTwoStageVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Two-stage VCRC with parallel compression.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatReleaser">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Refrigerant should not have a temperature glide!
    /// </exception>
    public VCRCWithParallelCompression(Evaporator evaporator, Compressor compressor,
        IHeatReleaser heatReleaser) : base(evaporator, compressor, heatReleaser)
    {
        new RefrigerantWithoutGlideValidator().ValidateAndThrow(Refrigerant);
        Point2s = Refrigerant.WithState(Input.Pressure(HeatReleaser.Pressure),
            Input.Entropy(Point1.Entropy));
        Point2 = Refrigerant.WithState(Input.Pressure(HeatReleaser.Pressure),
            Input.Enthalpy(Point1.Enthalpy + FirstStageSpecificWork));
        Point3 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Quality(TwoPhase.Dew.VaporQuality()));
        Point7 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Enthalpy(Point6.Enthalpy));
        Point4s = Refrigerant.WithState(Input.Pressure(HeatReleaser.Pressure),
            Input.Entropy(Point3.Entropy));
        Point4 = Refrigerant.WithState(Input.Pressure(HeatReleaser.Pressure),
            Input.Enthalpy(Point3.Enthalpy + SecondStageSpecificWork /
                SecondStageSpecificMassFlow.DecimalFractions));
        Point5s = Refrigerant.WithState(Input.Pressure(HeatReleaser.Pressure),
            Input.Enthalpy(
                (FirstStageSpecificMassFlow.DecimalFractions * Point2s.Enthalpy +
                 SecondStageSpecificMassFlow.DecimalFractions * Point4s.Enthalpy) /
                (FirstStageSpecificMassFlow + SecondStageSpecificMassFlow).DecimalFractions));
        Point5 = Refrigerant.WithState(Input.Pressure(HeatReleaser.Pressure),
            Input.Enthalpy(
                (FirstStageSpecificMassFlow.DecimalFractions * Point2.Enthalpy +
                 SecondStageSpecificMassFlow.DecimalFractions * Point4.Enthalpy) /
                (FirstStageSpecificMassFlow + SecondStageSpecificMassFlow).DecimalFractions));
        Point8 = Refrigerant.WithState(Input.Pressure(IntermediatePressure),
            Input.Quality(TwoPhase.Bubble.VaporQuality()));
        Point9 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
            Input.Enthalpy(Point8.Enthalpy));
    }

    /// <summary>
    ///     Point 1 – evaporator outlet / first compression stage suction.
    /// </summary>
    public new Refrigerant Point1 => base.Point1;

    /// <summary>
    ///     Point 2s – first isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point2s { get; }

    /// <summary>
    ///     Point 2 – first compression stage discharge.
    /// </summary>
    public Refrigerant Point2 { get; }

    /// <summary>
    ///     Point 3 – intermediate vessel vapor outlet / second compression stage suction.
    /// </summary>
    public Refrigerant Point3 { get; }

    /// <summary>
    ///     Point 4s – second isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point4s { get; }

    /// <summary>
    ///     Point 4 – second compression stage discharge.
    /// </summary>
    public Refrigerant Point4 { get; }

    /// <summary>
    ///     Point 5s – condenser or gas cooler inlet (for isentropic compression).
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private Refrigerant Point5s { get; }

    /// <summary>
    ///     Point 5 – condenser or gas cooler inlet.
    /// </summary>
    public Refrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – condenser or gas cooler outlet / first EV inlet.
    /// </summary>
    public Refrigerant Point6 => HeatReleaserOutlet;

    /// <summary>
    ///     Point 7 – first EV outlet / intermediate vessel inlet.
    /// </summary>
    public Refrigerant Point7 { get; }

    /// <summary>
    ///     Point 8 – intermediate vessel liquid outlet / second EV inlet.
    /// </summary>
    public Refrigerant Point8 { get; }

    /// <summary>
    ///     Point 9 – second EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point9 { get; }

    public sealed override Ratio SecondStageSpecificMassFlow =>
        FirstStageSpecificMassFlow *
        Point7.Quality!.Value.DecimalFractions /
        (1 - Point7.Quality!.Value.DecimalFractions);

    protected sealed override SpecificEnergy FirstStageIsentropicSpecificWork =>
        Point2s.Enthalpy - Point1.Enthalpy;

    protected sealed override SpecificEnergy SecondStageIsentropicSpecificWork =>
        SecondStageSpecificMassFlow.DecimalFractions * (Point4s.Enthalpy - Point3.Enthalpy);

    public sealed override SpecificEnergy SpecificCoolingCapacity =>
        Point1.Enthalpy - Point9.Enthalpy;

    public sealed override SpecificEnergy SpecificHeatingCapacity =>
        (FirstStageSpecificMassFlow + SecondStageSpecificMassFlow).DecimalFractions *
        (Point5.Enthalpy - Point6.Enthalpy);

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor) =>
        new EntropyAnalyzer(
                this, indoor, outdoor,
                new EvaporatorInfo(FirstStageSpecificMassFlow, Point9, Point1),
                new HeatReleaserInfo(HeatReleaser,
                    FirstStageSpecificMassFlow + SecondStageSpecificMassFlow, Point5s, Point6),
                new EVInfo(FirstStageSpecificMassFlow + SecondStageSpecificMassFlow, Point6, Point7),
                new EVInfo(FirstStageSpecificMassFlow, Point8, Point9), null, null, null,
                new MixingInfo(Point5, FirstStageSpecificMassFlow, Point2,
                    SecondStageSpecificMassFlow, Point4))
            .Result;
}
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.Units;
using VCRC.Abstract.Validators;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Abstract;

/// <summary>
///     Single-stage VCRC with recuperator base class.
/// </summary>
public abstract class AbstractVCRCWithRecuperator : AbstractVCRC, IEntropyAnalysable
{
    /// <summary>
    ///     Single-stage VCRC with recuperator base class.
    /// </summary>
    /// <param name="evaporator">Evaporator.</param>
    /// <param name="recuperator">Recuperator.</param>
    /// <param name="compressor">Compressor.</param>
    /// <param name="heatEmitter">Condenser or gas cooler.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Condensing temperature should be greater than evaporating temperature!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at recuperator 'hot' side!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Wrong temperature difference at recuperator 'cold' side!
    /// </exception>
    protected AbstractVCRCWithRecuperator(Evaporator evaporator, Recuperator recuperator, Compressor compressor,
        IHeatEmitter heatEmitter) : base(evaporator, compressor, heatEmitter)
    {
        Recuperator = recuperator;
        Point2 = Recuperator.Superheat == TemperatureDelta.Zero
            ? Point1.Clone()
            : Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
                Input.Temperature(Point1.Temperature + Recuperator.Superheat));
        Point3s = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Entropy(Point2.Entropy));
        IsentropicSpecificWork = Point3s.Enthalpy - Point2.Enthalpy;
        SpecificWork = IsentropicSpecificWork / Compressor.IsentropicEfficiency.DecimalFractions;
        Point3 = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Enthalpy(Point2.Enthalpy + SpecificWork));
        Point4 = HeatEmitter is Condenser condenser
            ? condenser.Subcooling == TemperatureDelta.Zero
                ? Refrigerant.WithState(Input.Pressure(condenser.Pressure),
                    Input.Quality(TwoPhase.Bubble.VaporQuality()))
                : Refrigerant.WithState(Input.Pressure(condenser.Pressure),
                    Input.Temperature(condenser.Temperature - condenser.Subcooling))
            : Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
                Input.Temperature(HeatEmitter.Temperature));
        Point5 = Refrigerant.WithState(Input.Pressure(HeatEmitter.Pressure),
            Input.Enthalpy(Point4.Enthalpy - (Point2.Enthalpy - Point1.Enthalpy)));
        Point6 = Refrigerant.WithState(Input.Pressure(Evaporator.Pressure),
            Input.Enthalpy(Point5.Enthalpy));
        SpecificCoolingCapacity = Point1.Enthalpy - Point6.Enthalpy;
        SpecificHeatingCapacity = Point3.Enthalpy - Point4.Enthalpy;
        new AbstractVCRCWithRecuperatorValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Recuperator as a VCRC component.
    /// </summary>
    public Recuperator Recuperator { get; }

    /// <summary>
    ///     Point 2 – recuperator "cold" outlet / compression stage suction.
    /// </summary>
    public Refrigerant Point2 { get; }

    /// <summary>
    ///     Point 3s – isentropic compression stage discharge.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public Refrigerant Point3s { get; }

    /// <summary>
    ///     Point 3 – compression stage discharge / condenser or gas cooler inlet.
    /// </summary>
    internal Refrigerant Point3 { get; }

    /// <summary>
    ///     Point 4 – condenser or gas cooler outlet / recuperator "hot" inlet.
    /// </summary>
    internal Refrigerant Point4 { get; }

    /// <summary>
    ///     Point 5 – recuperator "hot" outlet / EV inlet.
    /// </summary>
    public Refrigerant Point5 { get; }

    /// <summary>
    ///     Point 6 – EV outlet / evaporator inlet.
    /// </summary>
    public Refrigerant Point6 { get; }

    public EntropyAnalysisResult EntropyAnalysis(Temperature indoor, Temperature outdoor)
    {
        var (coldSource, hotSource) =
            IEntropyAnalysable.SourceTemperatures(indoor, outdoor, Point1.Temperature, Point4.Temperature);
        var minSpecificWork = SpecificCoolingCapacity * (hotSource - coldSource).Kelvins / coldSource.Kelvins;
        var thermodynamicPerfection = Ratio
            .FromDecimalFractions(minSpecificWork / SpecificWork).ToUnit(RatioUnit.Percent);
        var heatEmitterEnergyLoss =
            Point3s.Enthalpy - Point4.Enthalpy -
            (hotSource.Kelvins * (Point3s.Entropy - Point4.Entropy).JoulesPerKilogramKelvin).JoulesPerKilogram();
        var expansionValvesEnergyLoss =
            (hotSource.Kelvins * (Point6.Entropy - Point5.Entropy).JoulesPerKilogramKelvin).JoulesPerKilogram();
        var evaporatorEnergyLoss =
            (hotSource.Kelvins *
             ((Point1.Entropy - Point6.Entropy).JoulesPerKilogramKelvin -
              (Point1.Enthalpy - Point6.Enthalpy).JoulesPerKilogram / coldSource.Kelvins))
            .JoulesPerKilogram();
        var recuperatorEnergyLoss =
            (hotSource.Kelvins * (Point2.Entropy - Point1.Entropy - (Point4.Entropy - Point5.Entropy))
                .JoulesPerKilogramKelvin).JoulesPerKilogram();
        var calculatedIsentropicSpecificWork =
            minSpecificWork + heatEmitterEnergyLoss + expansionValvesEnergyLoss + evaporatorEnergyLoss +
            recuperatorEnergyLoss;
        var compressorEnergyLoss =
            calculatedIsentropicSpecificWork * (1.0 / Compressor.IsentropicEfficiency.DecimalFractions - 1);
        var calculatedSpecificWork = calculatedIsentropicSpecificWork + compressorEnergyLoss;
        var minSpecificWorkRatio = Ratio
            .FromDecimalFractions(minSpecificWork / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var compressorEnergyLossRatio = Ratio
            .FromDecimalFractions(compressorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var heatEmitterEnergyLossRatio = Ratio
            .FromDecimalFractions(heatEmitterEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var expansionValvesEnergyLossRatio = Ratio
            .FromDecimalFractions(expansionValvesEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var evaporatorEnergyLossRatio = Ratio
            .FromDecimalFractions(evaporatorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var recuperatorEnergyLossRatio = Ratio
            .FromDecimalFractions(recuperatorEnergyLoss / calculatedSpecificWork).ToUnit(RatioUnit.Percent);
        var analysisRelativeError = Ratio
            .FromDecimalFractions((calculatedIsentropicSpecificWork - IsentropicSpecificWork).Abs() /
                                  IsentropicSpecificWork).ToUnit(RatioUnit.Percent);
        return new EntropyAnalysisResult(
            thermodynamicPerfection,
            minSpecificWorkRatio,
            compressorEnergyLossRatio,
            HeatEmitter is Condenser ? heatEmitterEnergyLossRatio : Ratio.Zero,
            HeatEmitter is GasCooler ? heatEmitterEnergyLossRatio : Ratio.Zero,
            expansionValvesEnergyLossRatio,
            evaporatorEnergyLossRatio,
            recuperatorEnergyLossRatio,
            Ratio.Zero,
            Ratio.Zero,
            analysisRelativeError);
    }
}
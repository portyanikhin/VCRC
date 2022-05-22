using FluentValidation;
using UnitsNet;
using UnitsNet.Units;

namespace VCRC;

internal class EntropyAnalyzer
{
    internal EntropyAnalyzer(
        AbstractVCRC cycle,
        Temperature indoor,
        Temperature outdoor,
        EvaporatorInfo evaporatorInfo,
        HeatReleaserInfo heatReleaserInfo,
        EVInfo firstEVInfo,
        EVInfo? secondEVInfo = null,
        EVInfo? thirdEVInfo = null,
        RecuperatorInfo? recuperatorInfo = null,
        EconomizerInfo? economizerInfo = null,
        MixingInfo? mixingInfo = null)
    {
        (Cycle, EvaporatorInfo, HeatReleaserInfo, FirstEVInfo, SecondEVInfo, ThirdEVInfo,
                RecuperatorInfo, EconomizerInfo, MixingInfo) =
            (cycle, evaporatorInfo, heatReleaserInfo, firstEVInfo, secondEVInfo, thirdEVInfo,
                recuperatorInfo, economizerInfo, mixingInfo);
        (ColdSource, HotSource) =
            (UnitMath.Min(indoor, outdoor), UnitMath.Max(indoor, outdoor));
        new EntropyAnalyzerValidator().ValidateAndThrow(this);
    }

    private AbstractVCRC Cycle { get; }

    internal Temperature ColdSource { get; }

    internal Temperature HotSource { get; }

    internal EvaporatorInfo EvaporatorInfo { get; }

    internal HeatReleaserInfo HeatReleaserInfo { get; }

    private EVInfo FirstEVInfo { get; }

    private EVInfo? SecondEVInfo { get; }

    private EVInfo? ThirdEVInfo { get; }

    private RecuperatorInfo? RecuperatorInfo { get; }

    private EconomizerInfo? EconomizerInfo { get; }

    private MixingInfo? MixingInfo { get; }

    private Ratio ThermodynamicPerfection =>
        Ratio.FromDecimalFractions(MinSpecificWork / Cycle.SpecificWork)
            .ToUnit(RatioUnit.Percent);

    private SpecificEnergy MinSpecificWork =>
        Cycle.SpecificCoolingCapacity *
        (HotSource - ColdSource).Kelvins / ColdSource.Kelvins;

    private SpecificEnergy HeatReleaserEnergyLoss =>
        HeatReleaserInfo.EnergyLoss(HotSource);

    private SpecificEnergy ExpansionValvesEnergyLoss =>
        FirstEVInfo.EnergyLoss(HotSource) +
        (SecondEVInfo?.EnergyLoss(HotSource) ?? SpecificEnergy.Zero) +
        (ThirdEVInfo?.EnergyLoss(HotSource) ?? SpecificEnergy.Zero);

    private SpecificEnergy EvaporatorEnergyLoss =>
        EvaporatorInfo.EnergyLoss(ColdSource, HotSource);

    private SpecificEnergy RecuperatorEnergyLoss =>
        RecuperatorInfo?.EnergyLoss(HotSource) ?? SpecificEnergy.Zero;

    private SpecificEnergy EconomizerEnergyLoss =>
        EconomizerInfo?.EnergyLoss(HotSource) ?? SpecificEnergy.Zero;

    private SpecificEnergy MixingEnergyLoss =>
        MixingInfo?.EnergyLoss(HotSource) ?? SpecificEnergy.Zero;

    private SpecificEnergy CalculatedIsentropicSpecificWork =>
        MinSpecificWork + HeatReleaserEnergyLoss +
        ExpansionValvesEnergyLoss + EvaporatorEnergyLoss +
        RecuperatorEnergyLoss + EconomizerEnergyLoss + MixingEnergyLoss;

    private SpecificEnergy CompressorEnergyLoss =>
        CalculatedIsentropicSpecificWork *
        (1.0 / Cycle.Compressor.IsentropicEfficiency.DecimalFractions - 1);

    private SpecificEnergy CalculatedSpecificWork =>
        CalculatedIsentropicSpecificWork + CompressorEnergyLoss;

    private Ratio AnalysisRelativeError =>
        Ratio.FromDecimalFractions(
                (CalculatedIsentropicSpecificWork - Cycle.IsentropicSpecificWork).Abs() /
                Cycle.IsentropicSpecificWork)
            .ToUnit(RatioUnit.Percent);

    internal EntropyAnalysisResult Result =>
        new(
            ThermodynamicPerfection,
            EnergyLossRatio(MinSpecificWork),
            EnergyLossRatio(CompressorEnergyLoss),
            HeatReleaserInfo.HeatReleaser is Condenser
                ? EnergyLossRatio(HeatReleaserEnergyLoss)
                : Ratio.Zero,
            HeatReleaserInfo.HeatReleaser is GasCooler
                ? EnergyLossRatio(HeatReleaserEnergyLoss)
                : Ratio.Zero,
            EnergyLossRatio(ExpansionValvesEnergyLoss),
            EnergyLossRatio(EvaporatorEnergyLoss),
            EnergyLossRatio(RecuperatorEnergyLoss),
            EnergyLossRatio(EconomizerEnergyLoss),
            EnergyLossRatio(MixingEnergyLoss),
            AnalysisRelativeError);

    private Ratio EnergyLossRatio(SpecificEnergy energyLoss) =>
        Ratio.FromDecimalFractions(energyLoss / CalculatedSpecificWork)
            .ToUnit(RatioUnit.Percent);
}
using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC;

internal class EntropyAnalyzer : IEntropyAnalyzer
{
    private readonly IVCRC _cycle;
    private readonly IEntropyAnalysisNode? _economizerInfo;
    private readonly IEntropyAnalysisNode? _ejectorInfo;
    private readonly IEvaporatorNode _evaporatorNode;
    private readonly IEntropyAnalysisNode _firstEVNode;
    private readonly IHeatReleaserNode _heatReleaserNode;
    private readonly IEntropyAnalysisNode? _mixingInfo;
    private readonly IEntropyAnalysisNode? _recuperatorInfo;
    private readonly IEntropyAnalysisNode? _secondEVInfo;
    private readonly IEntropyAnalysisNode? _thirdEVInfo;
    private Temperature _coldSource;
    private Temperature _hotSource;

    public EntropyAnalyzer(
        IVCRC cycle,
        IEvaporatorNode evaporatorNode,
        IHeatReleaserNode heatReleaserNode,
        IEntropyAnalysisNode firstEVNode,
        IEntropyAnalysisNode? secondEVInfo = null,
        IEntropyAnalysisNode? thirdEVInfo = null,
        IEntropyAnalysisNode? ejectorInfo = null,
        IEntropyAnalysisNode? recuperatorInfo = null,
        IEntropyAnalysisNode? economizerInfo = null,
        IEntropyAnalysisNode? mixingInfo = null
    )
    {
        _cycle = cycle;
        _evaporatorNode = evaporatorNode;
        _heatReleaserNode = heatReleaserNode;
        _firstEVNode = firstEVNode;
        _secondEVInfo = secondEVInfo;
        _thirdEVInfo = thirdEVInfo;
        _ejectorInfo = ejectorInfo;
        _recuperatorInfo = recuperatorInfo;
        _economizerInfo = economizerInfo;
        _mixingInfo = mixingInfo;
    }

    private Ratio ThermodynamicPerfection =>
        Ratio
            .FromDecimalFractions(MinSpecificWork / _cycle.SpecificWork)
            .ToUnit(RatioUnit.Percent);

    private SpecificEnergy MinSpecificWork =>
        _cycle.SpecificCoolingCapacity
        * (_hotSource - _coldSource).Kelvins
        / _coldSource.Kelvins;

    private SpecificEnergy HeatReleaserEnergyLoss =>
        _heatReleaserNode.CalculateEnergyLoss(_hotSource);

    private SpecificEnergy ExpansionValvesEnergyLoss =>
        _firstEVNode.CalculateEnergyLoss(_hotSource)
        + (
            _secondEVInfo?.CalculateEnergyLoss(_hotSource)
            ?? SpecificEnergy.Zero
        )
        + (
            _thirdEVInfo?.CalculateEnergyLoss(_hotSource) ?? SpecificEnergy.Zero
        );

    private SpecificEnergy EjectorEnergyLoss =>
        _ejectorInfo?.CalculateEnergyLoss(_hotSource) ?? SpecificEnergy.Zero;

    private SpecificEnergy EvaporatorEnergyLoss =>
        _evaporatorNode.CalculateEnergyLoss(_coldSource, _hotSource);

    private SpecificEnergy RecuperatorEnergyLoss =>
        _recuperatorInfo?.CalculateEnergyLoss(_hotSource)
        ?? SpecificEnergy.Zero;

    private SpecificEnergy EconomizerEnergyLoss =>
        _economizerInfo?.CalculateEnergyLoss(_hotSource) ?? SpecificEnergy.Zero;

    private SpecificEnergy MixingEnergyLoss =>
        _mixingInfo?.CalculateEnergyLoss(_hotSource) ?? SpecificEnergy.Zero;

    private SpecificEnergy CalculatedIsentropicSpecificWork =>
        MinSpecificWork
        + HeatReleaserEnergyLoss
        + ExpansionValvesEnergyLoss
        + EjectorEnergyLoss
        + EvaporatorEnergyLoss
        + RecuperatorEnergyLoss
        + EconomizerEnergyLoss
        + MixingEnergyLoss;

    private SpecificEnergy CompressorEnergyLoss =>
        CalculatedIsentropicSpecificWork
        * (1.0 / _cycle.Compressor.Efficiency.DecimalFractions - 1);

    private SpecificEnergy CalculatedSpecificWork =>
        CalculatedIsentropicSpecificWork + CompressorEnergyLoss;

    private Ratio AnalysisRelativeError =>
        Ratio
            .FromDecimalFractions(
                (
                    CalculatedIsentropicSpecificWork
                    - _cycle.IsentropicSpecificWork
                ).Abs() / _cycle.IsentropicSpecificWork
            )
            .ToUnit(RatioUnit.Percent);

    public IEntropyAnalysisResult PerformAnalysis(
        Temperature indoor,
        Temperature outdoor
    )
    {
        _coldSource = UnitMath.Min(indoor, outdoor);
        _hotSource = UnitMath.Max(indoor, outdoor);
        if (_coldSource.Equals(_hotSource, 1e-3.DegreesCelsius()))
        {
            throw new ArgumentException(
                "Indoor and outdoor temperatures should not be equal!"
            );
        }

        if (_coldSource <= _evaporatorNode.Outlet.Temperature)
        {
            throw new ArgumentException(
                "Wrong temperature difference in the evaporator! "
                    + "Increase 'cold' source temperature."
            );
        }

        if (_hotSource >= _heatReleaserNode.Outlet.Temperature)
        {
            throw new ArgumentException(
                "Wrong temperature difference in the condenser or gas cooler! "
                    + "Decrease 'hot' source temperature."
            );
        }

        return new EntropyAnalysisResult(
            ThermodynamicPerfection,
            EnergyLossRatio(MinSpecificWork),
            EnergyLossRatio(CompressorEnergyLoss),
            _cycle.HeatReleaser is Condenser
                ? EnergyLossRatio(HeatReleaserEnergyLoss)
                : Ratio.Zero,
            _cycle.HeatReleaser is GasCooler
                ? EnergyLossRatio(HeatReleaserEnergyLoss)
                : Ratio.Zero,
            EnergyLossRatio(ExpansionValvesEnergyLoss),
            EnergyLossRatio(EjectorEnergyLoss),
            EnergyLossRatio(EvaporatorEnergyLoss),
            EnergyLossRatio(RecuperatorEnergyLoss),
            EnergyLossRatio(EconomizerEnergyLoss),
            EnergyLossRatio(MixingEnergyLoss),
            AnalysisRelativeError
        );
    }

    private Ratio EnergyLossRatio(SpecificEnergy energyLoss) =>
        Ratio
            .FromDecimalFractions(energyLoss / CalculatedSpecificWork)
            .ToUnit(RatioUnit.Percent);
}

using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC;

internal sealed class EntropyAnalyzer(
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
) : IEntropyAnalyzer
{
    private Temperature _coldSource;
    private Temperature _hotSource;

    private Ratio ThermodynamicPerfection =>
        Ratio.FromDecimalFractions(MinSpecificWork / cycle.SpecificWork).ToUnit(RatioUnit.Percent);

    private SpecificEnergy MinSpecificWork =>
        cycle.SpecificCoolingCapacity * (_hotSource - _coldSource).Kelvins / _coldSource.Kelvins;

    private SpecificEnergy HeatReleaserEnergyLoss =>
        heatReleaserNode.CalculateEnergyLoss(_hotSource);

    private SpecificEnergy ExpansionValvesEnergyLoss =>
        firstEVNode.CalculateEnergyLoss(_hotSource)
        + (secondEVInfo?.CalculateEnergyLoss(_hotSource) ?? SpecificEnergy.Zero)
        + (thirdEVInfo?.CalculateEnergyLoss(_hotSource) ?? SpecificEnergy.Zero);

    private SpecificEnergy EjectorEnergyLoss =>
        ejectorInfo?.CalculateEnergyLoss(_hotSource) ?? SpecificEnergy.Zero;

    private SpecificEnergy EvaporatorEnergyLoss =>
        evaporatorNode.CalculateEnergyLoss(_coldSource, _hotSource);

    private SpecificEnergy RecuperatorEnergyLoss =>
        recuperatorInfo?.CalculateEnergyLoss(_hotSource) ?? SpecificEnergy.Zero;

    private SpecificEnergy EconomizerEnergyLoss =>
        economizerInfo?.CalculateEnergyLoss(_hotSource) ?? SpecificEnergy.Zero;

    private SpecificEnergy MixingEnergyLoss =>
        mixingInfo?.CalculateEnergyLoss(_hotSource) ?? SpecificEnergy.Zero;

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
        CalculatedIsentropicSpecificWork * (1.0 / cycle.Compressor.Efficiency.DecimalFractions - 1);

    private SpecificEnergy CalculatedSpecificWork =>
        CalculatedIsentropicSpecificWork + CompressorEnergyLoss;

    private Ratio AnalysisRelativeError =>
        Ratio
            .FromDecimalFractions(
                (CalculatedIsentropicSpecificWork - cycle.IsentropicSpecificWork).Abs()
                    / cycle.IsentropicSpecificWork
            )
            .ToUnit(RatioUnit.Percent);

    public IEntropyAnalysisResult PerformAnalysis(Temperature indoor, Temperature outdoor)
    {
        _coldSource = UnitMath.Min(indoor, outdoor);
        _hotSource = UnitMath.Max(indoor, outdoor);
        if (_coldSource.Equals(_hotSource, 1e-3.DegreesCelsius()))
        {
            throw new ArgumentException("Indoor and outdoor temperatures should not be equal!");
        }

        if (_coldSource <= evaporatorNode.Outlet.Temperature)
        {
            throw new ArgumentException(
                "Wrong temperature difference in the evaporator! "
                    + "Increase 'cold' source temperature."
            );
        }

        if (_hotSource >= heatReleaserNode.Outlet.Temperature)
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
            cycle.HeatReleaser is Condenser ? EnergyLossRatio(HeatReleaserEnergyLoss) : Ratio.Zero,
            cycle.HeatReleaser is GasCooler ? EnergyLossRatio(HeatReleaserEnergyLoss) : Ratio.Zero,
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
        Ratio.FromDecimalFractions(energyLoss / CalculatedSpecificWork).ToUnit(RatioUnit.Percent);
}

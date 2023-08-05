using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class VCRCMitsubishiZubadanTests :
    IClassFixture<ComparisonFixture>,
    IClassFixture<SubcriticalVCRCFixture<VCRCMitsubishiZubadan>>
{
    private readonly ComparisonFixture _comparison;
    private readonly SubcriticalVCRCFixture<VCRCMitsubishiZubadan> _vcrc;

    public VCRCMitsubishiZubadanTests(
        ComparisonFixture comparison, SubcriticalVCRCFixture<VCRCMitsubishiZubadan> vcrc)
    {
        _comparison = comparison;
        _comparison.Tolerance = 1e-3;
        _vcrc = vcrc;
    }

    [Fact]
    public void VCRCMitsubishiZubadan_ExtraHighSubcooling_SolutionNotFound()
    {
        Action action = () => _ = new VCRCMitsubishiZubadan(
            _vcrc.Evaporator, _vcrc.Compressor,
            new Condenser(_vcrc.Refrigerant.Name, _vcrc.Condenser.Temperature,
                TemperatureDelta.FromKelvins(50)),
            _vcrc.Economizer);
        action.Should().Throw<ArgumentException>().WithMessage("Solution not found!");
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Evaporator.Should().Be(_vcrc.Evaporator);

    [Fact]
    public void Recuperator_Always_CreatesAutomaticallyByTemperatureDifference() =>
        _vcrc.Instance.Recuperator.Should().Be(
            new Recuperator(_vcrc.Instance.Point7.Temperature - _vcrc.Instance.Point2.Temperature));

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Compressor.Should().Be(_vcrc.Compressor);

    [Fact]
    public void Condenser_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Condenser.Should().Be(_vcrc.Condenser);

    [Fact]
    public void GasCooler_Always_ReturnsNull() =>
        _vcrc.Instance.GasCooler.Should().BeNull();

    [Fact]
    public void Economizer_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Economizer.Should().Be(_vcrc.Economizer);

    [Fact]
    public void IsTranscritical_Always_ReturnsFalse() =>
        _vcrc.Instance.IsTranscritical.Should().BeFalse();

    [Fact]
    public void RecuperatorHighPressure_Always_CalculatesAutomaticallyByPressuresInEvaporatorAndCondenser()
    {
        _vcrc.Instance.RecuperatorHighPressure.Equals(
                Math.Sqrt(_vcrc.Instance.IntermediatePressure.Pascals *
                          _vcrc.Condenser.Pressure.Pascals).Pascals(),
                _comparison.Tolerance.Pascals())
            .Should().BeTrue();
        var lowTemperatureCycle = new VCRCMitsubishiZubadan(
            new Evaporator(_vcrc.Refrigerant.Name, (-20).DegreesCelsius(), _vcrc.Evaporator.Superheat),
            _vcrc.Compressor, _vcrc.Condenser, _vcrc.Economizer);
        lowTemperatureCycle.RecuperatorHighPressure.Should().BeGreaterThan(
            Math.Sqrt(lowTemperatureCycle.IntermediatePressure.Pascals *
                      _vcrc.Condenser.Pressure.Pascals).Pascals());
    }

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        _vcrc.Instance.Point1.Should().Be(_vcrc.Evaporator.Outlet);
        _vcrc.Instance.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2_Always_ReturnsRecuperatorColdOutlet()
    {
        _vcrc.Instance.Point2.Should().Be(
            _vcrc.Instance.Point1.HeatingTo(
                _vcrc.Instance.Point1.Enthalpy +
                _vcrc.Instance.HeatReleaserSpecificMassFlow / _vcrc.Instance.EvaporatorSpecificMassFlow *
                (_vcrc.Instance.Point7.Enthalpy - _vcrc.Instance.Point8.Enthalpy)));
        _vcrc.Instance.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point3s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        _vcrc.Instance.Point3s.Should().Be(
            _vcrc.Instance.Point2.IsentropicCompressionTo(_vcrc.Instance.IntermediatePressure));
        _vcrc.Instance.Point3s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point3_Always_ReturnsFirstCompressionStageDischarge()
    {
        _vcrc.Instance.Point3.Should().Be(
            _vcrc.Instance.Point2.CompressionTo(
                _vcrc.Instance.IntermediatePressure, _vcrc.Compressor.Efficiency));
        _vcrc.Instance.Point3.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point4_Always_ReturnsDewPointAtIntermediatePressure()
    {
        _vcrc.Instance.Point4.Should().Be(
            _vcrc.Refrigerant.DewPointAt(_vcrc.Instance.IntermediatePressure));
        _vcrc.Instance.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point5s_Always_ReturnsSecondIsentropicCompressionStageDischarge()
    {
        _vcrc.Instance.Point5s.Should().Be(
            _vcrc.Instance.Point4.IsentropicCompressionTo(_vcrc.Condenser.Pressure));
        _vcrc.Instance.Point5s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point5_Always_ReturnsSecondCompressionStageDischarge()
    {
        _vcrc.Instance.Point5.Should().Be(
            _vcrc.Instance.Point4.CompressionTo(
                _vcrc.Condenser.Pressure, _vcrc.Compressor.Efficiency));
        _vcrc.Instance.Point5.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point6_Always_ReturnsCondenserOutlet()
    {
        _vcrc.Instance.Point6.Should().Be(_vcrc.Condenser.Outlet);
        _vcrc.Instance.Point6.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point7_Always_ReturnsRecuperatorHotInlet()
    {
        _vcrc.Instance.Point7.Should().Be(
            _vcrc.Instance.Point6.IsenthalpicExpansionTo(_vcrc.Instance.RecuperatorHighPressure));
        _vcrc.Instance.RecuperatorHighPressure.Should().BeGreaterThan(_vcrc.Instance.IntermediatePressure);
        _vcrc.Instance.RecuperatorHighPressure.Should().BeLessThan(_vcrc.Condenser.Pressure);
        _vcrc.Instance.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsRecuperatorHotOutlet()
    {
        _vcrc.Instance.Point8.Should().Be(
            _vcrc.Refrigerant.BubblePointAt(_vcrc.Instance.RecuperatorHighPressure));
        _vcrc.Instance.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point9_Always_ReturnsEconomizerColdInlet()
    {
        _vcrc.Instance.Point9.Should().Be(
            _vcrc.Instance.Point8.IsenthalpicExpansionTo(_vcrc.Instance.IntermediatePressure));
        _vcrc.Instance.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point10_Always_ReturnsEconomizerColdOutlet()
    {
        _vcrc.Instance.Point10.Pressure.Equals(
                _vcrc.Instance.IntermediatePressure, _comparison.Tolerance.Pascals())
            .Should().BeTrue();
        _vcrc.Instance.Point10.Enthalpy.Equals(
                _vcrc.Instance.Point4.Enthalpy - _vcrc.Instance.EvaporatorSpecificMassFlow /
                (_vcrc.Instance.HeatReleaserSpecificMassFlow - _vcrc.Instance.EvaporatorSpecificMassFlow) *
                (_vcrc.Instance.Point3.Enthalpy - _vcrc.Instance.Point4.Enthalpy),
                _comparison.Tolerance.JoulesPerKilogram())
            .Should().BeTrue();
        _vcrc.Instance.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point11_Always_ReturnsEconomizerHotOutlet()
    {
        _vcrc.Instance.Point11.Should().Be(
            _vcrc.Instance.Point8.CoolingTo(
                _vcrc.Instance.Point9.Temperature + _vcrc.Economizer.TemperatureDifference));
        _vcrc.Instance.Point11.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point12_Always_ReturnsEvaporatorInlet()
    {
        _vcrc.Instance.Point12.Should().Be(
            _vcrc.Instance.Point11.IsenthalpicExpansionTo(_vcrc.Evaporator.Pressure));
        _vcrc.Instance.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        _vcrc.Instance.EvaporatorSpecificMassFlow.Equals(
                100.Percent(), _comparison.Tolerance.Percent())
            .Should().BeTrue();
        _vcrc.Instance.HeatReleaserSpecificMassFlow.Equals(
                _vcrc.Instance.EvaporatorSpecificMassFlow *
                (1 + (_vcrc.Instance.Point3.Enthalpy - _vcrc.Instance.Point4.Enthalpy) /
                    (_vcrc.Instance.Point4.Enthalpy - _vcrc.Instance.Point10.Enthalpy)),
                _comparison.Tolerance.Percent())
            .Should().BeTrue();
        _vcrc.Instance.IntermediateSpecificMassFlow.Equals(
                _vcrc.Instance.HeatReleaserSpecificMassFlow - _vcrc.Instance.EvaporatorSpecificMassFlow,
                _comparison.Tolerance.Percent())
            .Should().BeTrue();
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        _vcrc.Instance.IsentropicSpecificWork.Equals(
                _vcrc.Instance.Point3s.Enthalpy - _vcrc.Instance.Point2.Enthalpy +
                _vcrc.Instance.HeatReleaserSpecificMassFlow.DecimalFractions *
                (_vcrc.Instance.Point5s.Enthalpy - _vcrc.Instance.Point4.Enthalpy),
                _comparison.Tolerance.JoulesPerKilogram())
            .Should().BeTrue();

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        _vcrc.Instance.SpecificWork.Equals(
                _vcrc.Instance.IsentropicSpecificWork / _vcrc.Compressor.Efficiency.DecimalFractions,
                _comparison.Tolerance.JoulesPerKilogram())
            .Should().BeTrue();

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        _vcrc.Instance.SpecificCoolingCapacity.Equals(
                _vcrc.Instance.Point1.Enthalpy - _vcrc.Instance.Point12.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram())
            .Should().BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        _vcrc.Instance.SpecificHeatingCapacity.Equals(
                _vcrc.Instance.HeatReleaserSpecificMassFlow.DecimalFractions *
                (_vcrc.Instance.Point5.Enthalpy - _vcrc.Instance.Point6.Enthalpy),
                _comparison.Tolerance.JoulesPerKilogram())
            .Should().BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        _vcrc.Instance.EER.Should().Be(
            _vcrc.Instance.SpecificCoolingCapacity / _vcrc.Instance.SpecificWork);
        _vcrc.Instance.EER.Should().BeApproximately(4.380342435725647, _comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        _vcrc.Instance.COP.Should().Be(
            _vcrc.Instance.SpecificHeatingCapacity / _vcrc.Instance.SpecificWork);
        _vcrc.Instance.COP.Should().BeApproximately(5.380333104312685, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout25() =>
        _vcrc.AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(25.57644561474704, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout25() =>
        _vcrc.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(25.27093000190046, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _vcrc.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout17() =>
        _vcrc.AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(17.402320942172274, _comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_Always_Returns0() =>
        _vcrc.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout7() =>
        _vcrc.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(7.442291905240232, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        _vcrc.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        _vcrc.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(21.16669887701517, _comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        _vcrc.AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should().BeApproximately(1.3431046427670745, _comparison.Tolerance);

    [Fact]
    public void EconomizerEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        _vcrc.AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(0.44158251658286674, _comparison.Tolerance);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        _vcrc.AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(6.9330711143219235, _comparison.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        _vcrc.AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(1.208960702370679, _comparison.Tolerance);
        _vcrc.AnalysisResult.Sum().Percent.Should().BeApproximately(100, _comparison.Tolerance);
    }
}

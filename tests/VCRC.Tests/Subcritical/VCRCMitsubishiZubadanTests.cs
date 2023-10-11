using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class VCRCMitsubishiZubadanTests
    : IClassFixture<ComparisonFixture>,
        IClassFixture<SubcriticalVCRCFixture<IVCRCMitsubishiZubadan>>
{
    private readonly ComparisonFixture _comparison;
    private readonly SubcriticalVCRCFixture<IVCRCMitsubishiZubadan> _fixture;

    public VCRCMitsubishiZubadanTests(
        ComparisonFixture comparison,
        SubcriticalVCRCFixture<IVCRCMitsubishiZubadan> fixture
    )
    {
        _comparison = comparison;
        _fixture = fixture;
    }

    [Fact]
    public void VCRCMitsubishiZubadan_ExtraHighSubcooling_SolutionNotFound()
    {
        Action action = () =>
            _ = new VCRCMitsubishiZubadan(
                _fixture.Evaporator,
                _fixture.Compressor,
                new Condenser(
                    _fixture.Refrigerant.Name,
                    _fixture.Condenser.Temperature,
                    TemperatureDelta.FromKelvins(50)
                ),
                _fixture.Economizer
            );
        action
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Solution not found!");
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        _fixture.Instance.Evaporator.Should().Be(_fixture.Evaporator);

    [Fact]
    public void Recuperator_Always_CreatesAutomaticallyByTemperatureDifference()
    {
        IHaveRecuperator sut = _fixture.Instance;
        sut.Recuperator
            .Should()
            .Be(
                new Recuperator(
                    _fixture.Instance.Point7.Temperature
                        - _fixture.Instance.Point2.Temperature
                )
            );
    }

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        _fixture.Instance.Compressor.Should().Be(_fixture.Compressor);

    [Fact]
    public void Condenser_Always_ReturnsEnteredValue() =>
        _fixture.Instance.Condenser.Should().Be(_fixture.Condenser);

    [Fact]
    public void GasCooler_Always_ReturnsNull() =>
        _fixture.Instance.GasCooler.Should().BeNull();

    [Fact]
    public void Economizer_Always_ReturnsEnteredValue()
    {
        IHaveEconomizerWithTPI sut = _fixture.Instance;
        sut.Economizer.Should().Be(_fixture.Economizer);
    }

    [Fact]
    public void IsTranscritical_Always_ReturnsFalse() =>
        _fixture.Instance.IsTranscritical.Should().BeFalse();

    [Fact]
    public void RecuperatorHighPressure_Always_CalculatesAutomaticallyByPressuresInEvaporatorAndCondenser()
    {
        _fixture.Instance.RecuperatorHighPressure
            .Equals(
                Math.Sqrt(
                        _fixture.Instance.IntermediatePressure.Pascals
                            * _fixture.Condenser.Pressure.Pascals
                    )
                    .Pascals(),
                _comparison.Tolerance.Pascals()
            )
            .Should()
            .BeTrue();
        var lowTemperatureCycle = new VCRCMitsubishiZubadan(
            new Evaporator(
                _fixture.Refrigerant.Name,
                (-20).DegreesCelsius(),
                _fixture.Evaporator.Superheat
            ),
            _fixture.Compressor,
            _fixture.Condenser,
            _fixture.Economizer
        );
        lowTemperatureCycle.RecuperatorHighPressure
            .Should()
            .BeGreaterThan(
                Math.Sqrt(
                        lowTemperatureCycle.IntermediatePressure.Pascals
                            * _fixture.Condenser.Pressure.Pascals
                    )
                    .Pascals()
            );
    }

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        _fixture.Instance.Point1.Should().Be(_fixture.Evaporator.Outlet);
        _fixture.Instance.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2_Always_ReturnsRecuperatorColdOutlet()
    {
        _fixture.Instance.Point2
            .Should()
            .Be(
                _fixture.Instance.Point1.HeatingTo(
                    _fixture.Instance.Point1.Enthalpy
                        + _fixture.Instance.HeatReleaserSpecificMassFlow
                            / _fixture.Instance.EvaporatorSpecificMassFlow
                            * (
                                _fixture.Instance.Point7.Enthalpy
                                - _fixture.Instance.Point8.Enthalpy
                            )
                )
            );
        _fixture.Instance.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point3s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        _fixture.Instance.Point3s
            .Should()
            .Be(
                _fixture.Instance.Point2.IsentropicCompressionTo(
                    _fixture.Instance.IntermediatePressure
                )
            );
        _fixture.Instance.Point3s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point3_Always_ReturnsFirstCompressionStageDischarge()
    {
        _fixture.Instance.Point3
            .Should()
            .Be(
                _fixture.Instance.Point2.CompressionTo(
                    _fixture.Instance.IntermediatePressure,
                    _fixture.Compressor.Efficiency
                )
            );
        _fixture.Instance.Point3.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point4_Always_ReturnsDewPointAtIntermediatePressure()
    {
        _fixture.Instance.Point4
            .Should()
            .Be(
                _fixture.Refrigerant.DewPointAt(
                    _fixture.Instance.IntermediatePressure
                )
            );
        _fixture.Instance.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point5s_Always_ReturnsSecondIsentropicCompressionStageDischarge()
    {
        _fixture.Instance.Point5s
            .Should()
            .Be(
                _fixture.Instance.Point4.IsentropicCompressionTo(
                    _fixture.Condenser.Pressure
                )
            );
        _fixture.Instance.Point5s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point5_Always_ReturnsSecondCompressionStageDischarge()
    {
        _fixture.Instance.Point5
            .Should()
            .Be(
                _fixture.Instance.Point4.CompressionTo(
                    _fixture.Condenser.Pressure,
                    _fixture.Compressor.Efficiency
                )
            );
        _fixture.Instance.Point5.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point6_Always_ReturnsCondenserOutlet()
    {
        _fixture.Instance.Point6.Should().Be(_fixture.Condenser.Outlet);
        _fixture.Instance.Point6.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point7_Always_ReturnsRecuperatorHotInlet()
    {
        _fixture.Instance.Point7
            .Should()
            .Be(
                _fixture.Instance.Point6.IsenthalpicExpansionTo(
                    _fixture.Instance.RecuperatorHighPressure
                )
            );
        _fixture.Instance.RecuperatorHighPressure
            .Should()
            .BeGreaterThan(_fixture.Instance.IntermediatePressure);
        _fixture.Instance.RecuperatorHighPressure
            .Should()
            .BeLessThan(_fixture.Condenser.Pressure);
        _fixture.Instance.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsRecuperatorHotOutlet()
    {
        _fixture.Instance.Point8
            .Should()
            .Be(
                _fixture.Refrigerant.BubblePointAt(
                    _fixture.Instance.RecuperatorHighPressure
                )
            );
        _fixture.Instance.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point9_Always_ReturnsEconomizerColdInlet()
    {
        _fixture.Instance.Point9
            .Should()
            .Be(
                _fixture.Instance.Point8.IsenthalpicExpansionTo(
                    _fixture.Instance.IntermediatePressure
                )
            );
        _fixture.Instance.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point10_Always_ReturnsEconomizerColdOutlet()
    {
        _fixture.Instance.Point10.Pressure
            .Equals(
                _fixture.Instance.IntermediatePressure,
                _comparison.Tolerance.Pascals()
            )
            .Should()
            .BeTrue();
        _fixture.Instance.Point10.Enthalpy
            .Equals(
                _fixture.Instance.Point4.Enthalpy
                    - _fixture.Instance.EvaporatorSpecificMassFlow
                        / (
                            _fixture.Instance.HeatReleaserSpecificMassFlow
                            - _fixture.Instance.EvaporatorSpecificMassFlow
                        )
                        * (
                            _fixture.Instance.Point3.Enthalpy
                            - _fixture.Instance.Point4.Enthalpy
                        ),
                100.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();
        _fixture.Instance.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point11_Always_ReturnsEconomizerHotOutlet()
    {
        _fixture.Instance.Point11
            .Should()
            .Be(
                _fixture.Instance.Point8.CoolingTo(
                    _fixture.Instance.Point9.Temperature
                        + _fixture.Economizer.TemperatureDifference
                )
            );
        _fixture.Instance.Point11.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point12_Always_ReturnsEvaporatorInlet()
    {
        _fixture.Instance.Point12
            .Should()
            .Be(
                _fixture.Instance.Point11.IsenthalpicExpansionTo(
                    _fixture.Evaporator.Pressure
                )
            );
        _fixture.Instance.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        _fixture.Instance.EvaporatorSpecificMassFlow
            .Equals(100.Percent(), _comparison.Tolerance.Percent())
            .Should()
            .BeTrue();
        _fixture.Instance.HeatReleaserSpecificMassFlow
            .Equals(
                _fixture.Instance.EvaporatorSpecificMassFlow
                    * (
                        1
                        + (
                            _fixture.Instance.Point3.Enthalpy
                            - _fixture.Instance.Point4.Enthalpy
                        )
                            / (
                                _fixture.Instance.Point4.Enthalpy
                                - _fixture.Instance.Point10.Enthalpy
                            )
                    ),
                _comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
        _fixture.Instance.IntermediateSpecificMassFlow
            .Equals(
                _fixture.Instance.HeatReleaserSpecificMassFlow
                    - _fixture.Instance.EvaporatorSpecificMassFlow,
                _comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        _fixture.Instance.IsentropicSpecificWork
            .Equals(
                _fixture.Instance.Point3s.Enthalpy
                    - _fixture.Instance.Point2.Enthalpy
                    + _fixture
                        .Instance
                        .HeatReleaserSpecificMassFlow
                        .DecimalFractions
                        * (
                            _fixture.Instance.Point5s.Enthalpy
                            - _fixture.Instance.Point4.Enthalpy
                        ),
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        _fixture.Instance.SpecificWork
            .Equals(
                _fixture.Instance.IsentropicSpecificWork
                    / _fixture.Compressor.Efficiency.DecimalFractions,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        _fixture.Instance.SpecificCoolingCapacity
            .Equals(
                _fixture.Instance.Point1.Enthalpy
                    - _fixture.Instance.Point12.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        _fixture.Instance.SpecificHeatingCapacity
            .Equals(
                _fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                    * (
                        _fixture.Instance.Point5.Enthalpy
                        - _fixture.Instance.Point6.Enthalpy
                    ),
                _comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        _fixture.Instance.EER
            .Should()
            .Be(
                _fixture.Instance.SpecificCoolingCapacity
                    / _fixture.Instance.SpecificWork
            );
        _fixture.Instance.EER
            .Should()
            .BeApproximately(4.380342435725647, _comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        _fixture.Instance.COP
            .Should()
            .Be(
                _fixture.Instance.SpecificHeatingCapacity
                    / _fixture.Instance.SpecificWork
            );
        _fixture.Instance.COP
            .Should()
            .BeApproximately(5.380333104312685, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout25() =>
        _fixture.AnalysisResult.ThermodynamicPerfection.Percent
            .Should()
            .BeApproximately(25.57644561474704, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout25() =>
        _fixture.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should()
            .BeApproximately(25.27093000190046, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _fixture.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout17() =>
        _fixture.AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should()
            .BeApproximately(17.402320942172274, _comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout7() =>
        _fixture.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should()
            .BeApproximately(7.442291905240232, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        _fixture.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(21.16669887701517, _comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        _fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(1.3431046427670745, _comparison.Tolerance);

    [Fact]
    public void EconomizerEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        _fixture.AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should()
            .BeApproximately(0.44158251658286674, _comparison.Tolerance);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        _fixture.AnalysisResult.MixingEnergyLossRatio.Percent
            .Should()
            .BeApproximately(6.9330711143219235, _comparison.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        _fixture.AnalysisResult.AnalysisRelativeError.Percent
            .Should()
            .BeApproximately(1.208960702370679, _comparison.Tolerance);
        _fixture.AnalysisResult
            .Sum()
            .Percent.Should()
            .BeApproximately(100, _comparison.Tolerance);
    }
}

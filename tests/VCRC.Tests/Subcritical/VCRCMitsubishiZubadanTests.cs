using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class VCRCMitsubishiZubadanTests(
    ComparisonFixture comparison,
    SubcriticalVCRCFixture<IVCRCMitsubishiZubadan> fixture
)
    : IClassFixture<ComparisonFixture>,
        IClassFixture<SubcriticalVCRCFixture<IVCRCMitsubishiZubadan>>
{
    [Fact]
    public void VCRCMitsubishiZubadan_ExtraHighSubcooling_SolutionNotFound()
    {
        Action action = () =>
            _ = new VCRCMitsubishiZubadan(
                fixture.Evaporator,
                fixture.Compressor,
                new Condenser(
                    fixture.Refrigerant.Name,
                    fixture.Condenser.Temperature,
                    TemperatureDelta.FromKelvins(50)
                ),
                fixture.Economizer
            );
        action
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Solution not found!");
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        fixture.Instance.Evaporator.Should().Be(fixture.Evaporator);

    [Fact]
    public void Recuperator_Always_CreatesAutomaticallyByTemperatureDifference()
    {
        IHaveRecuperator sut = fixture.Instance;
        sut.Recuperator
            .Should()
            .Be(
                new Recuperator(
                    fixture.Instance.Point7.Temperature
                        - fixture.Instance.Point2.Temperature
                )
            );
    }

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        fixture.Instance.Compressor.Should().Be(fixture.Compressor);

    [Fact]
    public void Condenser_Always_ReturnsEnteredValue() =>
        fixture.Instance.Condenser.Should().Be(fixture.Condenser);

    [Fact]
    public void GasCooler_Always_ReturnsNull() =>
        fixture.Instance.GasCooler.Should().BeNull();

    [Fact]
    public void Economizer_Always_ReturnsEnteredValue()
    {
        IHaveEconomizerWithTPI sut = fixture.Instance;
        sut.Economizer.Should().Be(fixture.Economizer);
    }

    [Fact]
    public void IsTranscritical_Always_ReturnsFalse() =>
        fixture.Instance.IsTranscritical.Should().BeFalse();

    [Fact]
    public void RecuperatorHighPressure_Always_CalculatesAutomaticallyByPressuresInEvaporatorAndCondenser()
    {
        fixture
            .Instance
            .RecuperatorHighPressure
            .Equals(
                Math.Sqrt(
                        fixture.Instance.IntermediatePressure.Pascals
                            * fixture.Condenser.Pressure.Pascals
                    )
                    .Pascals(),
                comparison.Tolerance.Pascals()
            )
            .Should()
            .BeTrue();
        var lowTemperatureCycle = new VCRCMitsubishiZubadan(
            new Evaporator(
                fixture.Refrigerant.Name,
                (-20).DegreesCelsius(),
                fixture.Evaporator.Superheat
            ),
            fixture.Compressor,
            fixture.Condenser,
            fixture.Economizer
        );
        lowTemperatureCycle
            .RecuperatorHighPressure
            .Should()
            .BeGreaterThan(
                Math.Sqrt(
                        lowTemperatureCycle.IntermediatePressure.Pascals
                            * fixture.Condenser.Pressure.Pascals
                    )
                    .Pascals()
            );
    }

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        fixture.Instance.Point1.Should().Be(fixture.Evaporator.Outlet);
        fixture.Instance.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2_Always_ReturnsRecuperatorColdOutlet()
    {
        fixture
            .Instance
            .Point2
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point1
                    .HeatingTo(
                        fixture.Instance.Point1.Enthalpy
                            + fixture.Instance.HeatReleaserSpecificMassFlow
                                / fixture.Instance.EvaporatorSpecificMassFlow
                                * (
                                    fixture.Instance.Point7.Enthalpy
                                    - fixture.Instance.Point8.Enthalpy
                                )
                    )
            );
        fixture.Instance.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point3s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        fixture
            .Instance
            .Point3s
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point2
                    .IsentropicCompressionTo(
                        fixture.Instance.IntermediatePressure
                    )
            );
        fixture.Instance.Point3s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point3_Always_ReturnsFirstCompressionStageDischarge()
    {
        fixture
            .Instance
            .Point3
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point2
                    .CompressionTo(
                        fixture.Instance.IntermediatePressure,
                        fixture.Compressor.Efficiency
                    )
            );
        fixture.Instance.Point3.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point4_Always_ReturnsDewPointAtIntermediatePressure()
    {
        fixture
            .Instance
            .Point4
            .Should()
            .Be(
                fixture
                    .Refrigerant
                    .DewPointAt(fixture.Instance.IntermediatePressure)
            );
        fixture.Instance.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point5s_Always_ReturnsSecondIsentropicCompressionStageDischarge()
    {
        fixture
            .Instance
            .Point5s
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point4
                    .IsentropicCompressionTo(fixture.Condenser.Pressure)
            );
        fixture.Instance.Point5s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point5_Always_ReturnsSecondCompressionStageDischarge()
    {
        fixture
            .Instance
            .Point5
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point4
                    .CompressionTo(
                        fixture.Condenser.Pressure,
                        fixture.Compressor.Efficiency
                    )
            );
        fixture.Instance.Point5.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point6_Always_ReturnsCondenserOutlet()
    {
        fixture.Instance.Point6.Should().Be(fixture.Condenser.Outlet);
        fixture.Instance.Point6.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point7_Always_ReturnsRecuperatorHotInlet()
    {
        fixture
            .Instance
            .Point7
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point6
                    .IsenthalpicExpansionTo(
                        fixture.Instance.RecuperatorHighPressure
                    )
            );
        fixture
            .Instance
            .RecuperatorHighPressure
            .Should()
            .BeGreaterThan(fixture.Instance.IntermediatePressure);
        fixture
            .Instance
            .RecuperatorHighPressure
            .Should()
            .BeLessThan(fixture.Condenser.Pressure);
        fixture.Instance.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsRecuperatorHotOutlet()
    {
        fixture
            .Instance
            .Point8
            .Should()
            .Be(
                fixture
                    .Refrigerant
                    .BubblePointAt(fixture.Instance.RecuperatorHighPressure)
            );
        fixture.Instance.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point9_Always_ReturnsEconomizerColdInlet()
    {
        fixture
            .Instance
            .Point9
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point8
                    .IsenthalpicExpansionTo(
                        fixture.Instance.IntermediatePressure
                    )
            );
        fixture.Instance.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point10_Always_ReturnsEconomizerColdOutlet()
    {
        fixture
            .Instance
            .Point10
            .Pressure
            .Equals(
                fixture.Instance.IntermediatePressure,
                comparison.Tolerance.Pascals()
            )
            .Should()
            .BeTrue();
        fixture
            .Instance
            .Point10
            .Enthalpy
            .Equals(
                fixture.Instance.Point4.Enthalpy
                    - fixture.Instance.EvaporatorSpecificMassFlow
                        / (
                            fixture.Instance.HeatReleaserSpecificMassFlow
                            - fixture.Instance.EvaporatorSpecificMassFlow
                        )
                        * (
                            fixture.Instance.Point3.Enthalpy
                            - fixture.Instance.Point4.Enthalpy
                        ),
                100.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();
        fixture.Instance.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point11_Always_ReturnsEconomizerHotOutlet()
    {
        fixture
            .Instance
            .Point11
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point8
                    .CoolingTo(
                        fixture.Instance.Point9.Temperature
                            + fixture.Economizer.TemperatureDifference
                    )
            );
        fixture.Instance.Point11.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point12_Always_ReturnsEvaporatorInlet()
    {
        fixture
            .Instance
            .Point12
            .Should()
            .Be(
                fixture
                    .Instance
                    .Point11
                    .IsenthalpicExpansionTo(fixture.Evaporator.Pressure)
            );
        fixture.Instance.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        fixture
            .Instance
            .EvaporatorSpecificMassFlow
            .Equals(100.Percent(), comparison.Tolerance.Percent())
            .Should()
            .BeTrue();
        fixture
            .Instance
            .HeatReleaserSpecificMassFlow
            .Equals(
                fixture.Instance.EvaporatorSpecificMassFlow
                    * (
                        1
                        + (
                            fixture.Instance.Point3.Enthalpy
                            - fixture.Instance.Point4.Enthalpy
                        )
                            / (
                                fixture.Instance.Point4.Enthalpy
                                - fixture.Instance.Point10.Enthalpy
                            )
                    ),
                comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
        fixture
            .Instance
            .IntermediateSpecificMassFlow
            .Equals(
                fixture.Instance.HeatReleaserSpecificMassFlow
                    - fixture.Instance.EvaporatorSpecificMassFlow,
                comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        fixture
            .Instance
            .IsentropicSpecificWork
            .Equals(
                fixture.Instance.Point3s.Enthalpy
                    - fixture.Instance.Point2.Enthalpy
                    + fixture
                        .Instance
                        .HeatReleaserSpecificMassFlow
                        .DecimalFractions
                        * (
                            fixture.Instance.Point5s.Enthalpy
                            - fixture.Instance.Point4.Enthalpy
                        ),
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        fixture
            .Instance
            .SpecificWork
            .Equals(
                fixture.Instance.IsentropicSpecificWork
                    / fixture.Compressor.Efficiency.DecimalFractions,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        fixture
            .Instance
            .SpecificCoolingCapacity
            .Equals(
                fixture.Instance.Point1.Enthalpy
                    - fixture.Instance.Point12.Enthalpy,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        fixture
            .Instance
            .SpecificHeatingCapacity
            .Equals(
                fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                    * (
                        fixture.Instance.Point5.Enthalpy
                        - fixture.Instance.Point6.Enthalpy
                    ),
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        fixture
            .Instance
            .EER
            .Should()
            .Be(
                fixture.Instance.SpecificCoolingCapacity
                    / fixture.Instance.SpecificWork
            );
        fixture
            .Instance
            .EER
            .Should()
            .BeApproximately(4.380342435725647, comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        fixture
            .Instance
            .COP
            .Should()
            .Be(
                fixture.Instance.SpecificHeatingCapacity
                    / fixture.Instance.SpecificWork
            );
        fixture
            .Instance
            .COP
            .Should()
            .BeApproximately(5.380333104312685, comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout25() =>
        fixture
            .AnalysisResult
            .ThermodynamicPerfection
            .Percent
            .Should()
            .BeApproximately(25.57644561474704, comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout25() =>
        fixture
            .AnalysisResult
            .MinSpecificWorkRatio
            .Percent
            .Should()
            .BeApproximately(25.27093000190046, comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        fixture
            .AnalysisResult
            .CompressorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(20, comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout17() =>
        fixture
            .AnalysisResult
            .CondenserEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(17.402320942172274, comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout7() =>
        fixture
            .AnalysisResult
            .ExpansionValvesEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(7.442291905240232, comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        fixture
            .AnalysisResult
            .EvaporatorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(21.16669887701517, comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        fixture
            .AnalysisResult
            .RecuperatorEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(1.3431046427670745, comparison.Tolerance);

    [Fact]
    public void EconomizerEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        fixture
            .AnalysisResult
            .EconomizerEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(0.44158251658286674, comparison.Tolerance);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        fixture
            .AnalysisResult
            .MixingEnergyLossRatio
            .Percent
            .Should()
            .BeApproximately(6.9330711143219235, comparison.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        fixture
            .AnalysisResult
            .AnalysisRelativeError
            .Percent
            .Should()
            .BeApproximately(1.208960702370679, comparison.Tolerance);
        fixture
            .AnalysisResult
            .Sum()
            .Percent
            .Should()
            .BeApproximately(100, comparison.Tolerance);
    }
}

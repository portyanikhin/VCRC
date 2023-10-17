﻿namespace VCRC.Tests;

public class VCRCWithCICTranscriticalTests
    : IClassFixture<ComparisonFixture>,
        IClassFixture<TranscriticalVCRCFixture<IVCRCWithCIC>>
{
    private readonly ComparisonFixture _comparison;
    private readonly TranscriticalVCRCFixture<IVCRCWithCIC> _fixture;

    public VCRCWithCICTranscriticalTests(
        ComparisonFixture comparison,
        TranscriticalVCRCFixture<IVCRCWithCIC> fixture
    )
    {
        _comparison = comparison;
        _fixture = fixture;
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        _fixture.Instance.Evaporator.Should().Be(_fixture.Evaporator);

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        _fixture.Instance.Compressor.Should().Be(_fixture.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsNull() =>
        _fixture.Instance.Condenser.Should().BeNull();

    [Fact]
    public void GasCooler_ForThisCase_ReturnsEnteredValue() =>
        _fixture.Instance.GasCooler.Should().Be(_fixture.GasCooler);

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsTrue() =>
        _fixture.Instance.IsTranscritical.Should().BeTrue();

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        _fixture.Instance.Point1.Should().Be(_fixture.Evaporator.Outlet);
        _fixture.Instance.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point2s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        _fixture.Instance.Point2s
            .Should()
            .Be(
                _fixture.Instance.Point1.IsentropicCompressionTo(
                    _fixture.Instance.IntermediatePressure
                )
            );
        _fixture.Instance.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point2_Always_ReturnsFirstCompressionStageDischarge()
    {
        _fixture.Instance.Point2
            .Should()
            .Be(
                _fixture.Instance.Point1.CompressionTo(
                    _fixture.Instance.IntermediatePressure,
                    _fixture.Compressor.Efficiency
                )
            );
        _fixture.Instance.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_Always_ReturnsSecondCompressionStageSuction()
    {
        _fixture.Instance.Point3
            .Should()
            .Be(
                _fixture.Refrigerant.DewPointAt(
                    _fixture.Instance.IntermediatePressure
                )
            );
        _fixture.Instance.Point3.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point4s_Always_ReturnsSecondIsentropicCompressionStageDischarge()
    {
        _fixture.Instance.Point4s
            .Should()
            .Be(
                _fixture.Instance.Point3.IsentropicCompressionTo(
                    _fixture.GasCooler.Pressure
                )
            );
        _fixture.Instance.Point4s.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point4_Always_ReturnsSecondCompressionStageDischarge()
    {
        _fixture.Instance.Point4
            .Should()
            .Be(
                _fixture.Instance.Point3.CompressionTo(
                    _fixture.GasCooler.Pressure,
                    _fixture.Compressor.Efficiency
                )
            );
        _fixture.Instance.Point4.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point5_ForThisCase_ReturnsGasCoolerOutlet()
    {
        _fixture.Instance.Point5.Should().Be(_fixture.GasCooler.Outlet);
        _fixture.Instance.Point5.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point6_Always_ReturnsSeparatorInlet()
    {
        _fixture.Instance.Point6
            .Should()
            .Be(
                _fixture.Instance.Point5.IsenthalpicExpansionTo(
                    _fixture.Instance.IntermediatePressure
                )
            );
        _fixture.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point7_Always_ReturnsSeparatorLiquidOutlet()
    {
        _fixture.Instance.Point7
            .Should()
            .Be(
                _fixture.Refrigerant.BubblePointAt(
                    _fixture.Instance.IntermediatePressure
                )
            );
        _fixture.Instance.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsEvaporatorInlet()
    {
        _fixture.Instance.Point8
            .Should()
            .Be(
                _fixture.Instance.Point7.IsenthalpicExpansionTo(
                    _fixture.Evaporator.Pressure
                )
            );
        _fixture.Instance.Point8.Phase.Should().Be(Phases.TwoPhase);
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
                            _fixture.Instance.Point2.Enthalpy
                            - _fixture.Instance.Point3.Enthalpy
                        )
                            / (
                                _fixture.Instance.Point3.Enthalpy
                                - _fixture.Instance.Point7.Enthalpy
                            )
                    )
                    / (
                        1
                        - _fixture
                            .Instance
                            .Point6
                            .Quality!
                            .Value
                            .DecimalFractions
                    ),
                _comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
        _fixture.Instance.IntermediateSpecificMassFlow
            .Equals(
                _fixture.Instance.HeatReleaserSpecificMassFlow,
                _comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        _fixture.Instance.IsentropicSpecificWork
            .Equals(
                _fixture.Instance.Point2s.Enthalpy
                    - _fixture.Instance.Point1.Enthalpy
                    + _fixture
                        .Instance
                        .HeatReleaserSpecificMassFlow
                        .DecimalFractions
                        * (
                            _fixture.Instance.Point4s.Enthalpy
                            - _fixture.Instance.Point3.Enthalpy
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
                    - _fixture.Instance.Point8.Enthalpy,
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
                        _fixture.Instance.Point4.Enthalpy
                        - _fixture.Instance.Point5.Enthalpy
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
            .BeApproximately(2.7550099889190762, _comparison.Tolerance);
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
            .BeApproximately(3.755009988919076, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout16() =>
        _fixture.AnalysisResult.ThermodynamicPerfection.Percent
            .Should()
            .BeApproximately(16.086268181907712, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout16() =>
        _fixture.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should()
            .BeApproximately(16.017893541004142, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _fixture.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_Returns0() =>
        _fixture.AnalysisResult.CondenserEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_ReturnsAbout13() =>
        _fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent
            .Should()
            .BeApproximately(13.773659697012475, _comparison.Tolerance);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout33() =>
        _fixture.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should()
            .BeApproximately(33.119810838209304, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout13() =>
        _fixture.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should()
            .BeApproximately(13.20925639670459, _comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should()
            .Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_Always_Returns0() =>
        _fixture.AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should()
            .Be(0);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout3() =>
        _fixture.AnalysisResult.MixingEnergyLossRatio.Percent
            .Should()
            .BeApproximately(3.8793795270694904, _comparison.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        _fixture.AnalysisResult.AnalysisRelativeError.Percent
            .Should()
            .BeApproximately(0.42686412372849164, _comparison.Tolerance);
        _fixture.AnalysisResult
            .Sum()
            .Percent.Should()
            .BeApproximately(100, _comparison.Tolerance);
    }
}
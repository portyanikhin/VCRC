﻿namespace VCRC.Tests;

public sealed class VCRCWithEjectorSubcriticalTests(
    ComparisonFixture comparison,
    SubcriticalVCRCFixture<IVCRCWithEjector> fixture
) : IClassFixture<ComparisonFixture>, IClassFixture<SubcriticalVCRCFixture<IVCRCWithEjector>>
{
    [Fact]
    public void VCRCWithEjector_WrongRefrigerant_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithEjector(
                new Evaporator(
                    FluidsList.R407C,
                    fixture.Evaporator.Temperature,
                    fixture.Evaporator.Superheat
                ),
                fixture.Compressor,
                new Condenser(
                    FluidsList.R407C,
                    fixture.Condenser.Temperature,
                    fixture.Condenser.Subcooling
                ),
                fixture.Ejector
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Refrigerant should be a single component or an azeotropic blend!*");
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        fixture.Instance.Evaporator.Should().Be(fixture.Evaporator);

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        fixture.Instance.Compressor.Should().Be(fixture.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsEnteredValue() =>
        fixture.Instance.Condenser.Should().Be(fixture.Condenser);

    [Fact]
    public void GasCooler_ForThisCase_ReturnsNull() => fixture.Instance.GasCooler.Should().BeNull();

    [Fact]
    public void Ejector_Always_ReturnsEnteredValue()
    {
        IHaveEjector sut = fixture.Instance;
        sut.Ejector.Should().Be(fixture.Ejector);
    }

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsFalse() =>
        fixture.Instance.IsTranscritical.Should().BeFalse();

    [Fact]
    public void Point1_Always_ReturnsSeparatorVaporOutlet()
    {
        fixture
            .Instance.Point1.Should()
            .Be(fixture.Refrigerant.DewPointAt(fixture.EjectorFlows.DiffuserOutlet.Pressure));
        fixture.Instance.Point1.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point2s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        fixture
            .Instance.Point2s.Should()
            .Be(fixture.Instance.Point1.IsentropicCompressionTo(fixture.Condenser.Pressure));
        fixture.Instance.Point2s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2_Always_ReturnsCompressionStageDischarge()
    {
        fixture
            .Instance.Point2.Should()
            .Be(
                fixture.Instance.Point1.CompressionTo(
                    fixture.Condenser.Pressure,
                    fixture.Compressor.Efficiency
                )
            );
        fixture.Instance.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_ForThisCase_ReturnsCondenserOutlet()
    {
        fixture.Instance.Point3.Should().Be(fixture.Condenser.Outlet);
        fixture.Instance.Point3.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point4_Always_ReturnsEjectorNozzleOutlet()
    {
        fixture.Instance.Point4.Should().Be(fixture.EjectorFlows.NozzleOutlet);
        fixture.Instance.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point5_Always_ReturnsEjectorMixingInlet()
    {
        fixture.Instance.Point5.Should().Be(fixture.EjectorFlows.MixingInlet);
        fixture.Instance.Point5.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point6_Always_ReturnsEjectorDiffuserOutlet()
    {
        fixture.Instance.Point6.Should().Be(fixture.EjectorFlows.DiffuserOutlet);
        fixture.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point7_Always_ReturnsSeparatorLiquidOutlet()
    {
        fixture
            .Instance.Point7.Should()
            .Be(fixture.Refrigerant.BubblePointAt(fixture.EjectorFlows.DiffuserOutlet.Pressure));
        fixture.Instance.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsEvaporatorInlet()
    {
        fixture
            .Instance.Point8.Should()
            .Be(fixture.Instance.Point7.IsenthalpicExpansionTo(fixture.Evaporator.Pressure));
        fixture.Instance.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point9_Always_ReturnsEvaporatorOutlet()
    {
        fixture.Instance.Point9.Should().Be(fixture.Evaporator.Outlet);
        fixture.Instance.Point9.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point10_Always_ReturnsEjectorSuctionOutlet()
    {
        fixture.Instance.Point10.Should().Be(fixture.EjectorFlows.SuctionOutlet);
        fixture.Instance.Point10.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        fixture
            .Instance.EvaporatorSpecificMassFlow.Equals(
                100.Percent(),
                comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
        fixture
            .Instance.HeatReleaserSpecificMassFlow.Equals(
                fixture.Instance.EvaporatorSpecificMassFlow
                    * (
                        fixture.Instance.Point6.Quality!.Value.DecimalFractions
                        / (1 - fixture.Instance.Point6.Quality!.Value.DecimalFractions)
                    ),
                comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        fixture
            .Instance.IsentropicSpecificWork.Equals(
                fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                    * (fixture.Instance.Point2s.Enthalpy - fixture.Instance.Point1.Enthalpy),
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        fixture
            .Instance.SpecificWork.Equals(
                fixture.Instance.IsentropicSpecificWork
                    / fixture.Compressor.Efficiency.DecimalFractions,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        fixture
            .Instance.SpecificCoolingCapacity.Equals(
                fixture.Instance.Point9.Enthalpy - fixture.Instance.Point8.Enthalpy,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        fixture
            .Instance.SpecificHeatingCapacity.Equals(
                fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                    * (fixture.Instance.Point2.Enthalpy - fixture.Instance.Point3.Enthalpy),
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        fixture
            .Instance.EER.Should()
            .Be(fixture.Instance.SpecificCoolingCapacity / fixture.Instance.SpecificWork);
        fixture.Instance.EER.Should().BeApproximately(4.836643336835533, comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        fixture
            .Instance.COP.Should()
            .Be(fixture.Instance.SpecificHeatingCapacity / fixture.Instance.SpecificWork);
        fixture.Instance.COP.Should().BeApproximately(5.836563370628909, comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout28() =>
        fixture
            .AnalysisResult.ThermodynamicPerfection.Percent.Should()
            .BeApproximately(28.240747630501133, comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout28() =>
        fixture
            .AnalysisResult.MinSpecificWorkRatio.Percent.Should()
            .BeApproximately(28.24357079452348, comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        fixture
            .AnalysisResult.CompressorEnergyLossRatio.Percent.Should()
            .BeApproximately(20, comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        fixture
            .AnalysisResult.CondenserEnergyLossRatio.Percent.Should()
            .BeApproximately(21.00249583519198, comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        fixture
            .AnalysisResult.ExpansionValvesEnergyLossRatio.Percent.Should()
            .BeApproximately(0.19078569109720636, comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        fixture
            .AnalysisResult.EjectorEnergyLossRatio.Percent.Should()
            .BeApproximately(6.8730525745277244, comparison.Tolerance);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout23() =>
        fixture
            .AnalysisResult.EvaporatorEnergyLossRatio.Percent.Should()
            .BeApproximately(23.69009510465962, comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.EconomizerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void TestMixingEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.MixingEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        fixture
            .AnalysisResult.AnalysisRelativeError.Percent.Should()
            .BeApproximately(0.009995775827658926, comparison.Tolerance);
        fixture.AnalysisResult.Sum().Percent.Should().BeApproximately(100, comparison.Tolerance);
    }
}

namespace VCRC.Tests;

public sealed class VCRCWithEconomizerSubcriticalTests(
    ComparisonFixture comparison,
    SubcriticalVCRCFixture<IVCRCWithEconomizer> fixture
) : IClassFixture<ComparisonFixture>, IClassFixture<SubcriticalVCRCFixture<IVCRCWithEconomizer>>
{
    [Fact]
    public void VCRCWithEconomizer_WrongEconomizerSuperheat_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithEconomizer(
                fixture.Evaporator,
                fixture.Compressor,
                fixture.Condenser,
                new Economizer(
                    fixture.Economizer.TemperatureDifference,
                    TemperatureDelta.FromKelvins(50)
                )
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at the economizer 'hot' side!*");
    }

    [Fact]
    public void VCRCWithEconomizer_WrongEconomizerTemperatureDifference_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithEconomizer(
                fixture.Evaporator,
                fixture.Compressor,
                fixture.Condenser,
                new Economizer(TemperatureDelta.FromKelvins(49), fixture.Economizer.Superheat)
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Too high temperature difference at the economizer 'cold' side!*");
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
    public void Economizer_Always_ReturnsEnteredValue()
    {
        IHaveEconomizer sut = fixture.Instance;
        sut.Economizer.Should().Be(fixture.Economizer);
    }

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsFalse() =>
        fixture.Instance.IsTranscritical.Should().BeFalse();

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        fixture.Instance.Point1.Should().Be(fixture.Evaporator.Outlet);
        fixture.Instance.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        fixture
            .Instance.Point2s.Should()
            .Be(
                fixture.Instance.Point1.IsentropicCompressionTo(
                    fixture.Instance.IntermediatePressure
                )
            );
        fixture.Instance.Point2s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2_Always_ReturnsFirstCompressionStageDischarge()
    {
        fixture
            .Instance.Point2.Should()
            .Be(
                fixture.Instance.Point1.CompressionTo(
                    fixture.Instance.IntermediatePressure,
                    fixture.Compressor.Efficiency
                )
            );
        fixture.Instance.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point3_Always_ReturnsSecondCompressionStageSuction()
    {
        fixture
            .Instance.Point3.Should()
            .Be(
                fixture.Refrigerant.Mixing(
                    fixture.Instance.EvaporatorSpecificMassFlow,
                    fixture.Instance.Point2,
                    fixture.Instance.HeatReleaserSpecificMassFlow
                        - fixture.Instance.EvaporatorSpecificMassFlow,
                    fixture.Instance.Point7
                )
            );
        fixture.Instance.Point3.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point4s_Always_ReturnsSecondIsentropicCompressionStageDischarge()
    {
        fixture
            .Instance.Point4s.Should()
            .Be(fixture.Instance.Point3.IsentropicCompressionTo(fixture.Condenser.Pressure));
        fixture.Instance.Point4s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point4_Always_ReturnsSecondCompressionStageDischarge()
    {
        fixture
            .Instance.Point4.Should()
            .Be(
                fixture.Instance.Point3.CompressionTo(
                    fixture.Condenser.Pressure,
                    fixture.Compressor.Efficiency
                )
            );
        fixture.Instance.Point4.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point5_ForThisCase_ReturnsCondenserOutlet()
    {
        fixture.Instance.Point5.Should().Be(fixture.Condenser.Outlet);
        fixture.Instance.Point5.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point6_Always_ReturnsEconomizerColdInlet()
    {
        fixture
            .Instance.Point6.Should()
            .Be(
                fixture.Instance.Point5.IsenthalpicExpansionTo(
                    fixture.Instance.IntermediatePressure
                )
            );
        fixture.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point7_Always_ReturnsEconomizerColdOutlet()
    {
        fixture
            .Instance.Point7.Should()
            .Be(
                fixture.Refrigerant.Superheated(
                    fixture.Instance.IntermediatePressure,
                    fixture.Economizer.Superheat
                )
            );
        fixture.Instance.Point7.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point8_Always_ReturnsEconomizerHotOutlet()
    {
        fixture
            .Instance.Point8.Should()
            .Be(
                fixture.Instance.Point5.CoolingTo(
                    fixture.Instance.Point6.Temperature + fixture.Economizer.TemperatureDifference
                )
            );
        fixture.Instance.Point8.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point9_Always_ReturnsEvaporatorInlet()
    {
        fixture
            .Instance.Point9.Should()
            .Be(fixture.Instance.Point8.IsenthalpicExpansionTo(fixture.Evaporator.Pressure));
        fixture.Instance.Point9.Phase.Should().Be(Phases.TwoPhase);
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
                        1
                        + (fixture.Instance.Point5.Enthalpy - fixture.Instance.Point8.Enthalpy)
                            / (fixture.Instance.Point7.Enthalpy - fixture.Instance.Point6.Enthalpy)
                    ),
                comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();
        fixture
            .Instance.IntermediateSpecificMassFlow.Equals(
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
            .Instance.IsentropicSpecificWork.Equals(
                fixture.Instance.Point2s.Enthalpy
                    - fixture.Instance.Point1.Enthalpy
                    + fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                        * (fixture.Instance.Point4s.Enthalpy - fixture.Instance.Point3.Enthalpy),
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
                fixture.Instance.Point1.Enthalpy - fixture.Instance.Point9.Enthalpy,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        fixture
            .Instance.SpecificHeatingCapacity.Equals(
                fixture.Instance.HeatReleaserSpecificMassFlow.DecimalFractions
                    * (fixture.Instance.Point4.Enthalpy - fixture.Instance.Point5.Enthalpy),
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
        fixture.Instance.EER.Should().BeApproximately(4.511109316237719, comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        fixture
            .Instance.COP.Should()
            .Be(fixture.Instance.SpecificHeatingCapacity / fixture.Instance.SpecificWork);
        fixture.Instance.COP.Should().BeApproximately(5.511109316237718, comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout26() =>
        fixture
            .AnalysisResult.ThermodynamicPerfection.Percent.Should()
            .BeApproximately(26.339982268947697, comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout26() =>
        fixture
            .AnalysisResult.MinSpecificWorkRatio.Percent.Should()
            .BeApproximately(26.213423177542623, comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        fixture
            .AnalysisResult.CompressorEnergyLossRatio.Percent.Should()
            .BeApproximately(20, comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout23() =>
        fixture
            .AnalysisResult.CondenserEnergyLossRatio.Percent.Should()
            .BeApproximately(23.01216380485239, comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        fixture
            .AnalysisResult.ExpansionValvesEnergyLossRatio.Percent.Should()
            .BeApproximately(6.888699877347386, comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        fixture
            .AnalysisResult.EvaporatorEnergyLossRatio.Percent.Should()
            .BeApproximately(21.956407482133088, comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        fixture
            .AnalysisResult.EconomizerEnergyLossRatio.Percent.Should()
            .BeApproximately(1.7587100360640768, comparison.Tolerance);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        fixture
            .AnalysisResult.MixingEnergyLossRatio.Percent.Should()
            .BeApproximately(0.1705956220604388, comparison.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        fixture
            .AnalysisResult.AnalysisRelativeError.Percent.Should()
            .BeApproximately(0.4828026105094935, comparison.Tolerance);
        fixture.AnalysisResult.Sum().Percent.Should().BeApproximately(100, comparison.Tolerance);
    }
}

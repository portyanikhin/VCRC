namespace VCRC.Tests;

public sealed class VCRCWithRecuperatorSubcriticalTests(
    ComparisonFixture comparison,
    SubcriticalVCRCFixture<IVCRCWithRecuperator> fixture
) : IClassFixture<ComparisonFixture>, IClassFixture<SubcriticalVCRCFixture<IVCRCWithRecuperator>>
{
    [Fact]
    public void VCRCWithRecuperator_WrongTemperatureDifferenceAtRecuperatorHotSide_ThrowsValidationException()
    {
        Action action = () =>
            _ = new VCRCWithRecuperator(
                fixture.Evaporator,
                new Recuperator(TemperatureDelta.FromKelvins(49)),
                fixture.Compressor,
                fixture.Condenser
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Too high temperature difference at the recuperator 'hot' side!*");
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        fixture.Instance.Evaporator.Should().Be(fixture.Evaporator);

    [Fact]
    public void Recuperator_Always_ReturnsEnteredValue()
    {
        IHaveRecuperator sut = fixture.Instance;
        sut.Recuperator.Should().Be(fixture.Recuperator);
    }

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        fixture.Instance.Compressor.Should().Be(fixture.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsEnteredValue() =>
        fixture.Instance.Condenser.Should().Be(fixture.Condenser);

    [Fact]
    public void GasCooler_ForThisCase_ReturnsNull() => fixture.Instance.GasCooler.Should().BeNull();

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
    public void Point2_Always_ReturnsRecuperatorColdOutlet()
    {
        fixture
            .Instance.Point2.Should()
            .Be(
                fixture.Instance.Point1.HeatingTo(
                    fixture.Instance.Point4.Temperature - fixture.Recuperator.TemperatureDifference
                )
            );
        fixture.Instance.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point3s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        fixture
            .Instance.Point3s.Should()
            .Be(fixture.Instance.Point2.IsentropicCompressionTo(fixture.Condenser.Pressure));
        fixture.Instance.Point3s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_Always_ReturnsCompressionStageDischarge()
    {
        fixture
            .Instance.Point3.Should()
            .Be(
                fixture.Instance.Point2.CompressionTo(
                    fixture.Condenser.Pressure,
                    fixture.Compressor.Efficiency
                )
            );
        fixture.Instance.Point3.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point4_ForThisCase_ReturnsCondenserOutlet()
    {
        fixture.Instance.Point4.Should().Be(fixture.Condenser.Outlet);
        fixture.Instance.Point4.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point5_Always_ReturnsRecuperatorHotOutlet()
    {
        fixture
            .Instance.Point5.Should()
            .Be(
                fixture.Instance.Point4.CoolingTo(
                    fixture.Instance.Point4.Enthalpy
                        - (fixture.Instance.Point2.Enthalpy - fixture.Instance.Point1.Enthalpy)
                )
            );
        fixture.Instance.Point5.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point6_Always_ReturnsEvaporatorInlet()
    {
        fixture
            .Instance.Point6.Should()
            .Be(fixture.Instance.Point5.IsenthalpicExpansionTo(fixture.Evaporator.Pressure));
        fixture.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        fixture
            .Instance.IsentropicSpecificWork.Equals(
                fixture.Instance.Point3s.Enthalpy - fixture.Instance.Point2.Enthalpy,
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
                fixture.Instance.Point1.Enthalpy - fixture.Instance.Point6.Enthalpy,
                comparison.Tolerance.JoulesPerKilogram()
            )
            .Should()
            .BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        fixture
            .Instance.SpecificHeatingCapacity.Equals(
                fixture.Instance.Point3.Enthalpy - fixture.Instance.Point4.Enthalpy,
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
        fixture.Instance.EER.Should().BeApproximately(4.201006672315493, comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        fixture
            .Instance.COP.Should()
            .Be(fixture.Instance.SpecificHeatingCapacity / fixture.Instance.SpecificWork);
        fixture.Instance.COP.Should().BeApproximately(5.201006672315493, comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout24() =>
        fixture
            .AnalysisResult.ThermodynamicPerfection.Percent.Should()
            .BeApproximately(24.529319398716595, comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout24() =>
        fixture
            .AnalysisResult.MinSpecificWorkRatio.Percent.Should()
            .BeApproximately(24.529319398716606, comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        fixture
            .AnalysisResult.CompressorEnergyLossRatio.Percent.Should()
            .BeApproximately(20, comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout27() =>
        fixture
            .AnalysisResult.CondenserEnergyLossRatio.Percent.Should()
            .BeApproximately(27.273640321252557, comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        fixture
            .AnalysisResult.ExpansionValvesEnergyLossRatio.Percent.Should()
            .BeApproximately(6.141707411883935, comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout20() =>
        fixture
            .AnalysisResult.EvaporatorEnergyLossRatio.Percent.Should()
            .BeApproximately(20.545203060576515, comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        fixture
            .AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should()
            .BeApproximately(1.510129807570391, comparison.Tolerance);

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
            .BeApproximately(4.099022440185992e-14, comparison.Tolerance);
        fixture.AnalysisResult.Sum().Percent.Should().BeApproximately(100, comparison.Tolerance);
    }
}

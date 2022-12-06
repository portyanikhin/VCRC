namespace VCRC.Tests;

public class VCRCWithRecuperatorSubcriticalTests :
    IClassFixture<SubcriticalVCRCFixture<VCRCWithRecuperator>>
{
    public VCRCWithRecuperatorSubcriticalTests(
        SubcriticalVCRCFixture<VCRCWithRecuperator> fixture) => Fixture = fixture;

    private SubcriticalVCRCFixture<VCRCWithRecuperator> Fixture { get; }

    [Fact]
    public void VCRCWithRecuperator_WrongTemperatureDifferenceAtRecuperatorHotSide_ThrowsValidationException()
    {
        Action action = () => _ = new VCRCWithRecuperator(
            Fixture.Evaporator, new Recuperator(TemperatureDelta.FromKelvins(49)),
            Fixture.Compressor, Fixture.Condenser);
        action.Should().Throw<ValidationException>().WithMessage(
            "*Too high temperature difference at recuperator 'hot' side!*");
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Evaporator.Should().Be(Fixture.Evaporator);

    [Fact]
    public void Recuperator_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Recuperator.Should().Be(Fixture.Recuperator);

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Compressor.Should().Be(Fixture.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsEnteredValue() =>
        Fixture.Cycle.Condenser.Should().Be(Fixture.Condenser);

    [Fact]
    public void GasCooler_ForThisCase_ReturnsNull() =>
        Fixture.Cycle.GasCooler.Should().BeNull();

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsFalse() =>
        Fixture.Cycle.IsTranscritical.Should().BeFalse();

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        Fixture.Cycle.Point1.Should().Be(Fixture.Evaporator.Outlet);
        Fixture.Cycle.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2_Always_ReturnsRecuperatorColdOutlet()
    {
        Fixture.Cycle.Point2.Should().Be(Fixture.Cycle.Point1.HeatingTo(
            Fixture.Cycle.Point4.Temperature - Fixture.Recuperator.TemperatureDifference));
        Fixture.Cycle.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point3s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        Fixture.Cycle.Point3s.Should().Be(
            Fixture.Cycle.Point2.IsentropicCompressionTo(Fixture.Condenser.Pressure));
        Fixture.Cycle.Point3s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_Always_ReturnsCompressionStageDischarge()
    {
        Fixture.Cycle.Point3.Should().Be(
            Fixture.Cycle.Point2.CompressionTo(
                Fixture.Condenser.Pressure, Fixture.Compressor.Efficiency));
        Fixture.Cycle.Point3.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point4_ForThisCase_ReturnsCondenserOutlet()
    {
        Fixture.Cycle.Point4.Should().Be(Fixture.Condenser.Outlet);
        Fixture.Cycle.Point4.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point5_Always_ReturnsRecuperatorHotOutlet()
    {
        Fixture.Cycle.Point5.Should().Be(Fixture.Cycle.Point4.CoolingTo(
            Fixture.Cycle.Point4.Enthalpy -
            (Fixture.Cycle.Point2.Enthalpy - Fixture.Cycle.Point1.Enthalpy)));
        Fixture.Cycle.Point5.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point6_Always_ReturnsEvaporatorInlet()
    {
        Fixture.Cycle.Point6.Should().Be(
            Fixture.Cycle.Point5.IsenthalpicExpansionTo(Fixture.Evaporator.Pressure));
        Fixture.Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        Fixture.Cycle.IsentropicSpecificWork.Should().Be(
            Fixture.Cycle.Point3s.Enthalpy - Fixture.Cycle.Point2.Enthalpy);

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        Fixture.Cycle.SpecificWork.Should().Be(
            Fixture.Cycle.IsentropicSpecificWork / Fixture.Compressor.Efficiency.DecimalFractions);

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        Fixture.Cycle.SpecificCoolingCapacity.Should().Be(
            Fixture.Cycle.Point1.Enthalpy - Fixture.Cycle.Point6.Enthalpy);

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        Fixture.Cycle.SpecificHeatingCapacity.Should().Be(
            Fixture.Cycle.Point3.Enthalpy - Fixture.Cycle.Point4.Enthalpy);

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        Fixture.Cycle.EER.Should().Be(
            Fixture.Cycle.SpecificCoolingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.EER.Should().BeApproximately(4.201006672315493, Fixture.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        Fixture.Cycle.COP.Should().Be(
            Fixture.Cycle.SpecificHeatingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.COP.Should().BeApproximately(5.201006672315493, Fixture.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout24() =>
        Fixture.AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(24.529319398716595, Fixture.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout24() =>
        Fixture.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(24.529319398716606, Fixture.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        Fixture.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Fixture.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout27() =>
        Fixture.AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(27.273640321252557, Fixture.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        Fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        Fixture.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(6.141707411883935, Fixture.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout20() =>
        Fixture.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(20.545203060576515, Fixture.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        Fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should().BeApproximately(1.510129807570391, Fixture.Tolerance);

    [Fact]
    public void EconomizerEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.EconomizerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void TestMixingEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.MixingEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        Fixture.AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(4.099022440185992e-14, Fixture.Tolerance);
        Fixture.AnalysisResult.Sum().Percent.Should().BeApproximately(100, Fixture.Tolerance);
    }
}
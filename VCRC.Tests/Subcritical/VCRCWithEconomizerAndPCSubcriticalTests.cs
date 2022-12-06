namespace VCRC.Tests;

public class VCRCWithEconomizerAndPCSubcriticalTests :
    IClassFixture<SubcriticalVCRCFixture<VCRCWithEconomizerAndPC>>
{
    public VCRCWithEconomizerAndPCSubcriticalTests(
        SubcriticalVCRCFixture<VCRCWithEconomizerAndPC> fixture) => Fixture = fixture;

    private SubcriticalVCRCFixture<VCRCWithEconomizerAndPC> Fixture { get; }

    [Fact]
    public void VCRCWithEconomizerAndPC_WrongEconomizerSuperheat_ThrowsValidationException()
    {
        Action action = () => _ = new VCRCWithEconomizerAndPC(
            Fixture.Evaporator, Fixture.Compressor, Fixture.Condenser,
            new Economizer(Fixture.Economizer.TemperatureDifference,
                TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at economizer 'hot' side!*");
    }

    [Fact]
    public void VCRCWithEconomizerAndPC_WrongEconomizerTemperatureDifference_ThrowsValidationException()
    {
        Action action = () => _ = new VCRCWithEconomizerAndPC(
            Fixture.Evaporator, Fixture.Compressor, Fixture.Condenser,
            new Economizer(TemperatureDelta.FromKelvins(49),
                Fixture.Economizer.Superheat));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Too high temperature difference at economizer 'cold' side!*");
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Evaporator.Should().Be(Fixture.Evaporator);

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
    public void Economizer_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Economizer.Should().Be(Fixture.Economizer);

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
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point2s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        Fixture.Cycle.Point2s.Should().Be(
            Fixture.Cycle.Point1.IsentropicCompressionTo(Fixture.Condenser.Pressure));
        Fixture.Cycle.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point2_Always_ReturnsFirstCompressionStageDischarge()
    {
        Fixture.Cycle.Point2.Should().Be(
            Fixture.Cycle.Point1.CompressionTo(
                Fixture.Condenser.Pressure, Fixture.Compressor.Efficiency));
        Fixture.Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_Always_ReturnsSecondCompressionStageSuction()
    {
        Fixture.Cycle.Point3.Should().Be(
            Fixture.Refrigerant.Superheated(
                Fixture.Cycle.IntermediatePressure, Fixture.Economizer.Superheat));
        Fixture.Cycle.Point3.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point4s_Always_ReturnsSecondIsentropicCompressionStageDischarge()
    {
        Fixture.Cycle.Point4s.Should().Be(
            Fixture.Cycle.Point3.IsentropicCompressionTo(Fixture.Condenser.Pressure));
        Fixture.Cycle.Point4s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point4_Always_ReturnsSecondCompressionStageDischarge()
    {
        Fixture.Cycle.Point4.Should().Be(
            Fixture.Cycle.Point3.CompressionTo(
                Fixture.Condenser.Pressure, Fixture.Compressor.Efficiency));
        Fixture.Cycle.Point4.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point5_ForThisCase_ReturnsCondenserInlet()
    {
        Fixture.Cycle.Point5.Should().Be(
            Fixture.Refrigerant.Mixing(
                Fixture.Cycle.EvaporatorSpecificMassFlow, Fixture.Cycle.Point2,
                Fixture.Cycle.HeatReleaserSpecificMassFlow - Fixture.Cycle.EvaporatorSpecificMassFlow,
                Fixture.Cycle.Point4));
        Fixture.Cycle.Point5.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point6_ForThisCase_ReturnsCondenserOutlet()
    {
        Fixture.Cycle.Point6.Should().Be(Fixture.Condenser.Outlet);
        Fixture.Cycle.Point6.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point7_Always_ReturnsEconomizerColdInlet()
    {
        Fixture.Cycle.Point7.Should().Be(
            Fixture.Cycle.Point6.IsenthalpicExpansionTo(Fixture.Cycle.IntermediatePressure));
        Fixture.Cycle.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsEconomizerHotOutlet()
    {
        Fixture.Cycle.Point8.Should().Be(
            Fixture.Cycle.Point6.CoolingTo(
                Fixture.Cycle.Point7.Temperature + Fixture.Economizer.TemperatureDifference));
        Fixture.Cycle.Point8.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point9_Always_ReturnsEvaporatorInlet()
    {
        Fixture.Cycle.Point9.Should().Be(
            Fixture.Cycle.Point8.IsenthalpicExpansionTo(Fixture.Evaporator.Pressure));
        Fixture.Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        Fixture.Cycle.EvaporatorSpecificMassFlow.Should().Be(100.Percent());
        Fixture.Cycle.HeatReleaserSpecificMassFlow.Percent.Should().BeApproximately(
            (Fixture.Cycle.EvaporatorSpecificMassFlow *
             (1 + (Fixture.Cycle.Point6.Enthalpy - Fixture.Cycle.Point8.Enthalpy) /
                 (Fixture.Cycle.Point3.Enthalpy - Fixture.Cycle.Point7.Enthalpy))).Percent,
            Fixture.Tolerance);
        Fixture.Cycle.IntermediateSpecificMassFlow.Should().Be(
            Fixture.Cycle.HeatReleaserSpecificMassFlow - Fixture.Cycle.EvaporatorSpecificMassFlow);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        Fixture.Cycle.IsentropicSpecificWork.Should().Be(
            Fixture.Cycle.Point2s.Enthalpy - Fixture.Cycle.Point1.Enthalpy +
            (Fixture.Cycle.HeatReleaserSpecificMassFlow -
             Fixture.Cycle.EvaporatorSpecificMassFlow).DecimalFractions *
            (Fixture.Cycle.Point4s.Enthalpy - Fixture.Cycle.Point3.Enthalpy));

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        Fixture.Cycle.SpecificWork.Should().Be(
            Fixture.Cycle.IsentropicSpecificWork / Fixture.Compressor.Efficiency.DecimalFractions);

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        Fixture.Cycle.SpecificCoolingCapacity.Should().Be(
            Fixture.Cycle.Point1.Enthalpy - Fixture.Cycle.Point9.Enthalpy);

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        Fixture.Cycle.SpecificHeatingCapacity.Should().Be(
            Fixture.Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Fixture.Cycle.Point5.Enthalpy - Fixture.Cycle.Point6.Enthalpy));

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        Fixture.Cycle.EER.Should().Be(
            Fixture.Cycle.SpecificCoolingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.EER.Should().BeApproximately(4.571161584395723, Fixture.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        Fixture.Cycle.COP.Should().Be(
            Fixture.Cycle.SpecificHeatingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.COP.Should().BeApproximately(5.571161584395725, Fixture.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout26() =>
        Fixture.AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(26.69062233718953, Fixture.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout26() =>
        Fixture.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(26.672018619966924, Fixture.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        Fixture.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Fixture.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout22() =>
        Fixture.AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(22.02518203021821, Fixture.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        Fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout7() =>
        Fixture.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(7.009215475275353, Fixture.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout22() =>
        Fixture.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(22.340527798473286, Fixture.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        Fixture.AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(1.7894781048364086, Fixture.Tolerance);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        Fixture.AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(0.16357797122982776, Fixture.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        Fixture.AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.06974994089376046, Fixture.Tolerance);
        Fixture.AnalysisResult.Sum().Percent.Should().BeApproximately(100, Fixture.Tolerance);
    }
}
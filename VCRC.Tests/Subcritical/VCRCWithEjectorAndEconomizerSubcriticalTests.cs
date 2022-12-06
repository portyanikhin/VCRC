namespace VCRC.Tests;

public class VCRCWithEjectorAndEconomizerSubcriticalTests :
    IClassFixture<SubcriticalVCRCFixture<VCRCWithEjectorAndEconomizer>>
{
    public VCRCWithEjectorAndEconomizerSubcriticalTests(
        SubcriticalVCRCFixture<VCRCWithEjectorAndEconomizer> fixture) => Fixture = fixture;

    private SubcriticalVCRCFixture<VCRCWithEjectorAndEconomizer> Fixture { get; }

    [Fact]
    public void VCRCWithEjectorAndEconomizer_WrongRefrigerant_ThrowsValidationException()
    {
        Action action = () => _ = new VCRCWithEjectorAndEconomizer(
            new Evaporator(FluidsList.R407C,
                Fixture.Evaporator.Temperature, Fixture.Evaporator.Superheat),
            Fixture.Compressor,
            new Condenser(FluidsList.R407C,
                Fixture.Condenser.Temperature, Fixture.Condenser.Subcooling),
            Fixture.Ejector, Fixture.Economizer);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Refrigerant should be a single component or an azeotropic blend!*");
    }

    [Fact]
    public void VCRCWithEjectorAndEconomizer_WrongEconomizerSuperheat_ThrowsValidationException()
    {
        Action action = () => _ = new VCRCWithEjectorAndEconomizer(
            Fixture.Evaporator, Fixture.Compressor, Fixture.Condenser, Fixture.Ejector,
            new Economizer(Fixture.Economizer.TemperatureDifference,
                TemperatureDelta.FromKelvins(50)));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Wrong temperature difference at economizer 'hot' side!*");
    }

    [Fact]
    public void VCRCWithEjectorAndEconomizer_WrongEconomizerTemperatureDifference_ThrowsValidationException()
    {
        Action action = () => _ = new VCRCWithEjectorAndEconomizer(
            Fixture.Evaporator, Fixture.Compressor, Fixture.Condenser, Fixture.Ejector,
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
    public void Ejector_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Ejector.Should().Be(Fixture.Ejector);

    [Fact]
    public void Economizer_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Economizer.Should().Be(Fixture.Economizer);

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsFalse() =>
        Fixture.Cycle.IsTranscritical.Should().BeFalse();

    [Fact]
    public void Point1_Always_ReturnsSeparatorVaporOutlet()
    {
        Fixture.Cycle.Point1.Should().Be(
            Fixture.Refrigerant.DewPointAt(Fixture.EjectorFlows.DiffuserOutlet.Pressure));
        Fixture.Cycle.Point1.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point2s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        Fixture.Cycle.Point2s.Should().Be(
            Fixture.Cycle.Point1.IsentropicCompressionTo(Fixture.Cycle.IntermediatePressure));
        Fixture.Cycle.Point2s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point2_Always_ReturnsFirstCompressionStageDischarge()
    {
        Fixture.Cycle.Point2.Should().Be(
            Fixture.Cycle.Point1.CompressionTo(
                Fixture.Cycle.IntermediatePressure, Fixture.Compressor.Efficiency));
        Fixture.Cycle.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point3_Always_ReturnsSecondCompressionStageSuction()
    {
        Fixture.Cycle.Point3.Should().Be(
            Fixture.Refrigerant.Mixing(
                Fixture.Cycle.HeatReleaserSpecificMassFlow -
                Fixture.Cycle.IntermediateSpecificMassFlow, Fixture.Cycle.Point2,
                Fixture.Cycle.IntermediateSpecificMassFlow, Fixture.Cycle.Point7));
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
        Fixture.Cycle.Point4.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point5_ForThisCase_ReturnsCondenserOutlet()
    {
        Fixture.Cycle.Point5.Should().Be(Fixture.Condenser.Outlet);
        Fixture.Cycle.Point5.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point6_Always_ReturnsEconomizerColdInlet()
    {
        Fixture.Cycle.Point6.Should().Be(
            Fixture.Cycle.Point5.IsenthalpicExpansionTo(Fixture.Cycle.IntermediatePressure));
        Fixture.Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point7_Always_ReturnsEconomizerColdOutlet()
    {
        Fixture.Cycle.Point7.Should().Be(
            Fixture.Refrigerant.Superheated(
                Fixture.Cycle.IntermediatePressure, Fixture.Economizer.Superheat));
        Fixture.Cycle.Point7.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point8_Always_ReturnsEconomizerHotOutlet()
    {
        Fixture.Cycle.Point8.Should().Be(
            Fixture.Cycle.Point5.CoolingTo(
                Fixture.Cycle.Point6.Temperature + Fixture.Economizer.TemperatureDifference));
        Fixture.Cycle.Point8.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point9_Always_ReturnsEjectorNozzleOutlet()
    {
        Fixture.Cycle.Point9.Should().Be(Fixture.EjectorFlows.NozzleOutlet);
        Fixture.Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point10_Always_ReturnsEjectorMixingInlet()
    {
        Fixture.Cycle.Point10.Should().Be(Fixture.EjectorFlows.MixingInlet);
        Fixture.Cycle.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point11_Always_ReturnsEjectorDiffuserOutlet()
    {
        Fixture.Cycle.Point11.Should().Be(Fixture.EjectorFlows.DiffuserOutlet);
        Fixture.Cycle.Point11.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point12_Always_ReturnsSeparatorLiquidOutlet()
    {
        Fixture.Cycle.Point12.Should().Be(
            Fixture.Refrigerant.BubblePointAt(Fixture.EjectorFlows.DiffuserOutlet.Pressure));
        Fixture.Cycle.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point13_Always_ReturnsEvaporatorInlet()
    {
        Fixture.Cycle.Point13.Should().Be(
            Fixture.Cycle.Point12.IsenthalpicExpansionTo(Fixture.Evaporator.Pressure));
        Fixture.Cycle.Point13.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point14_Always_ReturnsEvaporatorOutlet()
    {
        Fixture.Cycle.Point14.Should().Be(Fixture.Evaporator.Outlet);
        Fixture.Cycle.Point14.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point15_Always_ReturnsEjectorSuctionOutlet()
    {
        Fixture.Cycle.Point15.Should().Be(Fixture.EjectorFlows.SuctionOutlet);
        Fixture.Cycle.Point15.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void IntermediatePressure_ForThisCase_ReturnsMiddlePressureBetweenDiffuserOutletAndCondenserPressures() =>
        Fixture.Cycle.IntermediatePressure.Pascals.Should().BeApproximately(
            Math.Sqrt(Fixture.EjectorFlows.DiffuserOutlet.Pressure.Pascals *
                      Fixture.Condenser.Pressure.Pascals), 50);

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        Fixture.Cycle.EvaporatorSpecificMassFlow.Should().Be(100.Percent());
        Fixture.Cycle.HeatReleaserSpecificMassFlow.Percent.Should().BeApproximately(
            (Fixture.Cycle.EvaporatorSpecificMassFlow *
             (Fixture.Cycle.Point11.Quality!.Value.DecimalFractions /
              (1 - Fixture.Cycle.Point11.Quality!.Value.DecimalFractions)) *
             (1 + (Fixture.Cycle.Point5.Enthalpy - Fixture.Cycle.Point8.Enthalpy) /
                 (Fixture.Cycle.Point7.Enthalpy - Fixture.Cycle.Point6.Enthalpy))).Percent,
            Fixture.Tolerance);
        Fixture.Cycle.IntermediateSpecificMassFlow.Percent.Should().BeApproximately(
            (Fixture.Cycle.HeatReleaserSpecificMassFlow - Fixture.Cycle.EvaporatorSpecificMassFlow *
                (Fixture.Cycle.Point11.Quality!.Value.DecimalFractions /
                 (1 - Fixture.Cycle.Point11.Quality!.Value.DecimalFractions))).Percent,
            Fixture.Tolerance);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        Fixture.Cycle.IsentropicSpecificWork.Should().Be(
            (Fixture.Cycle.HeatReleaserSpecificMassFlow -
             Fixture.Cycle.IntermediateSpecificMassFlow).DecimalFractions *
            (Fixture.Cycle.Point2s.Enthalpy - Fixture.Cycle.Point1.Enthalpy) +
            Fixture.Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Fixture.Cycle.Point4s.Enthalpy - Fixture.Cycle.Point3.Enthalpy));

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        Fixture.Cycle.SpecificWork.Should().Be(
            Fixture.Cycle.IsentropicSpecificWork / Fixture.Compressor.Efficiency.DecimalFractions);

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        Fixture.Cycle.SpecificCoolingCapacity.Should().Be(
            Fixture.Cycle.Point14.Enthalpy - Fixture.Cycle.Point13.Enthalpy);

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        Fixture.Cycle.SpecificHeatingCapacity.Should().Be(
            Fixture.Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Fixture.Cycle.Point4.Enthalpy - Fixture.Cycle.Point5.Enthalpy));

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        Fixture.Cycle.EER.Should().Be(
            Fixture.Cycle.SpecificCoolingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.EER.Should().BeApproximately(4.783695679338165, Fixture.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        Fixture.Cycle.COP.Should().Be(
            Fixture.Cycle.SpecificHeatingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.COP.Should().BeApproximately(5.783610594561565, Fixture.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout27() =>
        Fixture.AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(27.931590777519773, Fixture.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout27() =>
        Fixture.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(27.8851872538937, Fixture.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        Fixture.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Fixture.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        Fixture.AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(21.946588574688754, Fixture.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        Fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        Fixture.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(0.5128777263565665, Fixture.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_ForThisCase_ReturnsAbout4() =>
        Fixture.AnalysisResult.EjectorEnergyLossRatio.Percent
            .Should().BeApproximately(4.4324100846906065, Fixture.Tolerance);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout23() =>
        Fixture.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(23.391508143710862, Fixture.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        Fixture.AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(1.7725334295847335, Fixture.Tolerance);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        Fixture.AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(0.05889478707478723, Fixture.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        Fixture.AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.166409223662599, Fixture.Tolerance);
        Fixture.AnalysisResult.Sum().Percent.Should().BeApproximately(100, Fixture.Tolerance);
    }
}
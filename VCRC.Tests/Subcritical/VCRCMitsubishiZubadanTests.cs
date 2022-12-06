using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class VCRCMitsubishiZubadanTests :
    IClassFixture<SubcriticalVCRCFixture<VCRCMitsubishiZubadan>>
{
    public VCRCMitsubishiZubadanTests(
        SubcriticalVCRCFixture<VCRCMitsubishiZubadan> fixture) => Fixture = fixture;

    private SubcriticalVCRCFixture<VCRCMitsubishiZubadan> Fixture { get; }

    [Fact]
    public void VCRCMitsubishiZubadan_ExtraHighSubcooling_SolutionNotFound()
    {
        Action action = () => _ = new VCRCMitsubishiZubadan(
            Fixture.Evaporator, Fixture.Compressor,
            new Condenser(Fixture.Refrigerant.Name, Fixture.Condenser.Temperature,
                TemperatureDelta.FromKelvins(50)),
            Fixture.Economizer);
        action.Should().Throw<ArgumentException>().WithMessage("Solution not found!");
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Evaporator.Should().Be(Fixture.Evaporator);

    [Fact]
    public void Recuperator_Always_CreatesAutomaticallyByTemperatureDifference() =>
        Fixture.Cycle.Recuperator.Should().Be(
            new Recuperator(Fixture.Cycle.Point7.Temperature - Fixture.Cycle.Point2.Temperature));

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Compressor.Should().Be(Fixture.Compressor);

    [Fact]
    public void Condenser_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Condenser.Should().Be(Fixture.Condenser);

    [Fact]
    public void GasCooler_Always_ReturnsNull() =>
        Fixture.Cycle.GasCooler.Should().BeNull();

    [Fact]
    public void Economizer_Always_ReturnsEnteredValue() =>
        Fixture.Cycle.Economizer.Should().Be(Fixture.Economizer);

    [Fact]
    public void IsTranscritical_Always_ReturnsFalse() =>
        Fixture.Cycle.IsTranscritical.Should().BeFalse();

    [Fact]
    public void RecuperatorHighPressure_Always_CalculatesAutomaticallyByPressuresInEvaporatorAndCondenser()
    {
        Fixture.Cycle.RecuperatorHighPressure.Should().Be(
            Math.Sqrt(Fixture.Cycle.IntermediatePressure.Pascals *
                      Fixture.Condenser.Pressure.Pascals).Pascals());
        var lowTemperatureCycle = new VCRCMitsubishiZubadan(
            new Evaporator(Fixture.Refrigerant.Name, (-20).DegreesCelsius(), Fixture.Evaporator.Superheat),
            Fixture.Compressor, Fixture.Condenser, Fixture.Economizer);
        lowTemperatureCycle.RecuperatorHighPressure.Should().BeGreaterThan(
            Math.Sqrt(lowTemperatureCycle.IntermediatePressure.Pascals *
                      Fixture.Condenser.Pressure.Pascals).Pascals());
    }

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
            Fixture.Cycle.Point1.Enthalpy +
            Fixture.Cycle.HeatReleaserSpecificMassFlow / Fixture.Cycle.EvaporatorSpecificMassFlow *
            (Fixture.Cycle.Point7.Enthalpy - Fixture.Cycle.Point8.Enthalpy)));
        Fixture.Cycle.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point3s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        Fixture.Cycle.Point3s.Should().Be(
            Fixture.Cycle.Point2.IsentropicCompressionTo(Fixture.Cycle.IntermediatePressure));
        Fixture.Cycle.Point3s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point3_Always_ReturnsFirstCompressionStageDischarge()
    {
        Fixture.Cycle.Point3.Should().Be(
            Fixture.Cycle.Point2.CompressionTo(
                Fixture.Cycle.IntermediatePressure, Fixture.Compressor.Efficiency));
        Fixture.Cycle.Point3.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point4_Always_ReturnsDewPointAtIntermediatePressure()
    {
        Fixture.Cycle.Point4.Should().Be(
            Fixture.Refrigerant.DewPointAt(Fixture.Cycle.IntermediatePressure));
        Fixture.Cycle.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point5s_Always_ReturnsSecondIsentropicCompressionStageDischarge()
    {
        Fixture.Cycle.Point5s.Should().Be(
            Fixture.Cycle.Point4.IsentropicCompressionTo(Fixture.Condenser.Pressure));
        Fixture.Cycle.Point5s.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point5_Always_ReturnsSecondCompressionStageDischarge()
    {
        Fixture.Cycle.Point5.Should().Be(
            Fixture.Cycle.Point4.CompressionTo(
                Fixture.Condenser.Pressure, Fixture.Compressor.Efficiency));
        Fixture.Cycle.Point5.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point6_Always_ReturnsCondenserOutlet()
    {
        Fixture.Cycle.Point6.Should().Be(Fixture.Condenser.Outlet);
        Fixture.Cycle.Point6.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point7_Always_ReturnsRecuperatorHotInlet()
    {
        Fixture.Cycle.Point7.Should().Be(
            Fixture.Cycle.Point6.IsenthalpicExpansionTo(Fixture.Cycle.RecuperatorHighPressure));
        Fixture.Cycle.RecuperatorHighPressure.Should().BeGreaterThan(Fixture.Cycle.IntermediatePressure);
        Fixture.Cycle.RecuperatorHighPressure.Should().BeLessThan(Fixture.Condenser.Pressure);
        Fixture.Cycle.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsRecuperatorHotOutlet()
    {
        Fixture.Cycle.Point8.Should().Be(
            Fixture.Refrigerant.BubblePointAt(Fixture.Cycle.RecuperatorHighPressure));
        Fixture.Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point9_Always_ReturnsEconomizerColdInlet()
    {
        Fixture.Cycle.Point9.Should().Be(
            Fixture.Cycle.Point8.IsenthalpicExpansionTo(Fixture.Cycle.IntermediatePressure));
        Fixture.Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point10_Always_ReturnsEconomizerColdOutlet()
    {
        Fixture.Cycle.Point10.Pressure.Should().Be(Fixture.Cycle.IntermediatePressure);
        Fixture.Cycle.Point10.Enthalpy.JoulesPerKilogram.Should().BeApproximately(
            (Fixture.Cycle.Point4.Enthalpy - Fixture.Cycle.EvaporatorSpecificMassFlow /
                (Fixture.Cycle.HeatReleaserSpecificMassFlow - Fixture.Cycle.EvaporatorSpecificMassFlow) *
                (Fixture.Cycle.Point3.Enthalpy - Fixture.Cycle.Point4.Enthalpy)).JoulesPerKilogram, 10);
        Fixture.Cycle.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point11_Always_ReturnsEconomizerHotOutlet()
    {
        Fixture.Cycle.Point11.Should().Be(
            Fixture.Cycle.Point8.CoolingTo(
                Fixture.Cycle.Point9.Temperature + Fixture.Economizer.TemperatureDifference));
        Fixture.Cycle.Point11.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point12_Always_ReturnsEvaporatorInlet()
    {
        Fixture.Cycle.Point12.Should().Be(
            Fixture.Cycle.Point11.IsenthalpicExpansionTo(Fixture.Evaporator.Pressure));
        Fixture.Cycle.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        Fixture.Cycle.EvaporatorSpecificMassFlow.Should().Be(100.Percent());
        Fixture.Cycle.HeatReleaserSpecificMassFlow.Percent.Should().BeApproximately(
            (Fixture.Cycle.EvaporatorSpecificMassFlow *
             (1 + (Fixture.Cycle.Point3.Enthalpy - Fixture.Cycle.Point4.Enthalpy) /
                 (Fixture.Cycle.Point4.Enthalpy - Fixture.Cycle.Point10.Enthalpy))).Percent,
            1e-3);
        Fixture.Cycle.IntermediateSpecificMassFlow.Should().Be(
            Fixture.Cycle.HeatReleaserSpecificMassFlow - Fixture.Cycle.EvaporatorSpecificMassFlow);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        Fixture.Cycle.IsentropicSpecificWork.Should().Be(
            Fixture.Cycle.Point3s.Enthalpy - Fixture.Cycle.Point2.Enthalpy +
            Fixture.Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Fixture.Cycle.Point5s.Enthalpy - Fixture.Cycle.Point4.Enthalpy));

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        Fixture.Cycle.SpecificWork.Should().Be(
            Fixture.Cycle.IsentropicSpecificWork / Fixture.Compressor.Efficiency.DecimalFractions);

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        Fixture.Cycle.SpecificCoolingCapacity.Should().Be(
            Fixture.Cycle.Point1.Enthalpy - Fixture.Cycle.Point12.Enthalpy);

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
        Fixture.Cycle.EER.Should().BeApproximately(4.380342435725647, Fixture.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        Fixture.Cycle.COP.Should().Be(
            Fixture.Cycle.SpecificHeatingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.COP.Should().BeApproximately(5.380333104312685, Fixture.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout25() =>
        Fixture.AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(25.57644561474704, Fixture.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout25() =>
        Fixture.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(25.27093000190046, Fixture.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        Fixture.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Fixture.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout17() =>
        Fixture.AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(17.402320942172274, Fixture.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout7() =>
        Fixture.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(7.442291905240232, Fixture.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        Fixture.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(21.16669887701517, Fixture.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_ForThisCase_ReturnsAbout1() =>
        Fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should().BeApproximately(1.3431046427670745, Fixture.Tolerance);

    [Fact]
    public void EconomizerEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        Fixture.AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(0.44158251658286674, Fixture.Tolerance);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        Fixture.AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(6.9330711143219235, Fixture.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        Fixture.AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(1.208960702370679, Fixture.Tolerance);
        Fixture.AnalysisResult.Sum().Percent.Should().BeApproximately(100, Fixture.Tolerance);
    }
}
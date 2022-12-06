using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class SimpleVCRCSubcriticalTests :
    IClassFixture<SubcriticalVCRCFixture<SimpleVCRC>>
{
    public SimpleVCRCSubcriticalTests(
        SubcriticalVCRCFixture<SimpleVCRC> fixture) => Fixture = fixture;

    private SubcriticalVCRCFixture<SimpleVCRC> Fixture { get; }

    [Theory]
    [InlineData(5, 4)]
    [InlineData(41, 40)]
    public void SimpleVCRC_WrongTemperatures_ThrowsValidationException(
        double evaporatingTemperature, double condensingTemperature)
    {
        Action action = () => _ = new SimpleVCRC(
            new Evaporator(Fixture.Refrigerant.Name,
                evaporatingTemperature.DegreesCelsius(), Fixture.Evaporator.Superheat),
            Fixture.Compressor,
            new Condenser(Fixture.Refrigerant.Name,
                condensingTemperature.DegreesCelsius(), Fixture.Condenser.Subcooling));
        action.Should().Throw<ValidationException>().WithMessage(
            "*Condensing temperature should be greater than evaporating temperature!*");
    }

    [Fact]
    public void SimpleVCRC_WrongRefrigerants_ThrowsValidationException()
    {
        Action action = () => _ = new SimpleVCRC(
            new Evaporator(FluidsList.R22, Fixture.Evaporator.Temperature, Fixture.Evaporator.Superheat),
            Fixture.Compressor, Fixture.Condenser);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Only one refrigerant should be selected!*");
    }

    [Theory]
    [InlineData(18, 18, "Indoor and outdoor temperatures should not be equal!")]
    [InlineData(13, 35, "Wrong temperature difference in the evaporator! Increase 'cold' source temperature.")]
    [InlineData(18, 42,
        "Wrong temperature difference in the condenser or gas cooler! Decrease 'hot' source temperature.")]
    public void EntropyAnalysis_WrongTemperatures_ThrowsValidationException(
        double indoor, double outdoor, string message)
    {
        IEntropyAnalysable vcrc = Fixture.Cycle;
        Action action = () => vcrc.EntropyAnalysis(indoor.DegreesCelsius(), outdoor.DegreesCelsius());
        action.Should().Throw<ValidationException>().WithMessage($"*{message}*");
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
    public void Point2s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        Fixture.Cycle.Point2s.Should().Be(
            Fixture.Cycle.Point1.IsentropicCompressionTo(Fixture.Condenser.Pressure));
        Fixture.Cycle.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point2_Always_ReturnsCompressionStageDischarge()
    {
        Fixture.Cycle.Point2.Should().Be(
            Fixture.Cycle.Point1.CompressionTo(
                Fixture.Condenser.Pressure, Fixture.Compressor.Efficiency));
        Fixture.Cycle.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_ForThisCase_ReturnsCondenserOutlet()
    {
        Fixture.Cycle.Point3.Should().Be(Fixture.Condenser.Outlet);
        Fixture.Cycle.Point3.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point4_Always_ReturnsEvaporatorInlet()
    {
        Fixture.Cycle.Point4.Should().Be(
            Fixture.Cycle.Point3.IsenthalpicExpansionTo(Fixture.Evaporator.Pressure));
        Fixture.Cycle.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        Fixture.Cycle.IsentropicSpecificWork.Should().Be(
            Fixture.Cycle.Point2s.Enthalpy - Fixture.Cycle.Point1.Enthalpy);

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        Fixture.Cycle.SpecificWork.Should().Be(
            Fixture.Cycle.IsentropicSpecificWork / Fixture.Compressor.Efficiency.DecimalFractions);

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        Fixture.Cycle.SpecificCoolingCapacity.Should().Be(
            Fixture.Cycle.Point1.Enthalpy - Fixture.Cycle.Point4.Enthalpy);

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        Fixture.Cycle.SpecificHeatingCapacity.Should().Be(
            Fixture.Cycle.Point2.Enthalpy - Fixture.Cycle.Point3.Enthalpy);

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        Fixture.Cycle.EER.Should().Be(
            Fixture.Cycle.SpecificCoolingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.EER.Should().BeApproximately(4.326011919496399, Fixture.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        Fixture.Cycle.COP.Should().Be(
            Fixture.Cycle.SpecificHeatingCapacity / Fixture.Cycle.SpecificWork);
        Fixture.Cycle.COP.Should().BeApproximately(5.326011919496398, Fixture.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout25() =>
        Fixture.AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(25.259214367658867, Fixture.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout25() =>
        Fixture.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(25.259214367658878, Fixture.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        Fixture.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Fixture.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        Fixture.AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(21.46877321647669, Fixture.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        Fixture.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout12() =>
        Fixture.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(12.141693490520616, Fixture.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        Fixture.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(21.13031892534382, Fixture.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        Fixture.AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

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
            .Should().BeApproximately(6.236322468275591e-14, Fixture.Tolerance);
        Fixture.AnalysisResult.Sum().Percent.Should().BeApproximately(100, Fixture.Tolerance);
    }
}
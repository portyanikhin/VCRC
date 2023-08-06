using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class SimpleVCRCSubcriticalTests :
    IClassFixture<ComparisonFixture>,
    IClassFixture<SubcriticalVCRCFixture<SimpleVCRC>>
{
    private readonly ComparisonFixture _comparison;
    private readonly SubcriticalVCRCFixture<SimpleVCRC> _vcrc;

    public SimpleVCRCSubcriticalTests(
        ComparisonFixture comparison, SubcriticalVCRCFixture<SimpleVCRC> vcrc)
    {
        _comparison = comparison;
        _vcrc = vcrc;
    }

    [Theory]
    [InlineData(5, 4)]
    [InlineData(41, 40)]
    public void SimpleVCRC_WrongTemperatures_ThrowsValidationException(
        double evaporatingTemperature, double condensingTemperature)
    {
        Action action = () => _ = new SimpleVCRC(
            new Evaporator(_vcrc.Refrigerant.Name,
                evaporatingTemperature.DegreesCelsius(), _vcrc.Evaporator.Superheat),
            _vcrc.Compressor,
            new Condenser(_vcrc.Refrigerant.Name,
                condensingTemperature.DegreesCelsius(), _vcrc.Condenser.Subcooling));
        action.Should().Throw<ValidationException>().WithMessage(
            "*Condensing temperature should be greater than evaporating temperature!*");
    }

    [Fact]
    public void SimpleVCRC_WrongRefrigerants_ThrowsValidationException()
    {
        Action action = () => _ = new SimpleVCRC(
            new Evaporator(FluidsList.R22, _vcrc.Evaporator.Temperature, _vcrc.Evaporator.Superheat),
            _vcrc.Compressor, _vcrc.Condenser);
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
        IEntropyAnalysable vcrc = _vcrc.Instance;
        Action action = () => vcrc.EntropyAnalysis(indoor.DegreesCelsius(), outdoor.DegreesCelsius());
        action.Should().Throw<ValidationException>().WithMessage($"*{message}*");
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Evaporator.Should().Be(_vcrc.Evaporator);

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Compressor.Should().Be(_vcrc.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsEnteredValue() =>
        _vcrc.Instance.Condenser.Should().Be(_vcrc.Condenser);

    [Fact]
    public void GasCooler_ForThisCase_ReturnsNull() =>
        _vcrc.Instance.GasCooler.Should().BeNull();

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsFalse() =>
        _vcrc.Instance.IsTranscritical.Should().BeFalse();

    [Fact]
    public void Point1_Always_ReturnsEvaporatorOutlet()
    {
        _vcrc.Instance.Point1.Should().Be(_vcrc.Evaporator.Outlet);
        _vcrc.Instance.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point2s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        _vcrc.Instance.Point2s.Should().Be(
            _vcrc.Instance.Point1.IsentropicCompressionTo(_vcrc.Condenser.Pressure));
        _vcrc.Instance.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point2_Always_ReturnsCompressionStageDischarge()
    {
        _vcrc.Instance.Point2.Should().Be(
            _vcrc.Instance.Point1.CompressionTo(
                _vcrc.Condenser.Pressure, _vcrc.Compressor.Efficiency));
        _vcrc.Instance.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_ForThisCase_ReturnsCondenserOutlet()
    {
        _vcrc.Instance.Point3.Should().Be(_vcrc.Condenser.Outlet);
        _vcrc.Instance.Point3.Phase.Should().Be(Phases.Liquid);
    }

    [Fact]
    public void Point4_Always_ReturnsEvaporatorInlet()
    {
        _vcrc.Instance.Point4.Should().Be(
            _vcrc.Instance.Point3.IsenthalpicExpansionTo(_vcrc.Evaporator.Pressure));
        _vcrc.Instance.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        _vcrc.Instance.IsentropicSpecificWork.Equals(
                _vcrc.Instance.Point2s.Enthalpy - _vcrc.Instance.Point1.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram())
            .Should().BeTrue();

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        _vcrc.Instance.SpecificWork.Equals(
                _vcrc.Instance.IsentropicSpecificWork / _vcrc.Compressor.Efficiency.DecimalFractions,
                _comparison.Tolerance.JoulesPerKilogram())
            .Should().BeTrue();

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        _vcrc.Instance.SpecificCoolingCapacity.Equals(
                _vcrc.Instance.Point1.Enthalpy - _vcrc.Instance.Point4.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram())
            .Should().BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        _vcrc.Instance.SpecificHeatingCapacity.Equals(
                _vcrc.Instance.Point2.Enthalpy - _vcrc.Instance.Point3.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram())
            .Should().BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        _vcrc.Instance.EER.Should().Be(
            _vcrc.Instance.SpecificCoolingCapacity / _vcrc.Instance.SpecificWork);
        _vcrc.Instance.EER.Should().BeApproximately(4.326011919496399, _comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        _vcrc.Instance.COP.Should().Be(
            _vcrc.Instance.SpecificHeatingCapacity / _vcrc.Instance.SpecificWork);
        _vcrc.Instance.COP.Should().BeApproximately(5.326011919496398, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout25() =>
        _vcrc.AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(25.259214367658867, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout25() =>
        _vcrc.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(25.259214367658878, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _vcrc.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        _vcrc.AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(21.46877321647669, _comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        _vcrc.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout12() =>
        _vcrc.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(12.141693490520616, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_Always_Returns0() =>
        _vcrc.AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        _vcrc.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(21.13031892534382, _comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        _vcrc.AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_Always_Returns0() =>
        _vcrc.AnalysisResult.EconomizerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void TestMixingEnergyLossRatio_Always_Returns0() =>
        _vcrc.AnalysisResult.MixingEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        _vcrc.AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(6.236322468275591e-14, _comparison.Tolerance);
        _vcrc.AnalysisResult.Sum().Percent.Should().BeApproximately(100, _comparison.Tolerance);
    }
}

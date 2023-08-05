﻿namespace VCRC.Tests;

public class VCRCWithEjectorSubcriticalTests :
    IClassFixture<ComparisonFixture>,
    IClassFixture<SubcriticalVCRCFixture<VCRCWithEjector>>
{
    private readonly ComparisonFixture _comparison;
    private readonly SubcriticalVCRCFixture<VCRCWithEjector> _vcrc;

    public VCRCWithEjectorSubcriticalTests(
        ComparisonFixture comparison, SubcriticalVCRCFixture<VCRCWithEjector> vcrc)
    {
        _comparison = comparison;
        _vcrc = vcrc;
    }

    [Fact]
    public void VCRCWithEjector_WrongRefrigerant_ThrowsValidationException()
    {
        Action action = () => _ = new VCRCWithEjector(
            new Evaporator(FluidsList.R407C,
                _vcrc.Evaporator.Temperature, _vcrc.Evaporator.Superheat),
            _vcrc.Compressor,
            new Condenser(FluidsList.R407C,
                _vcrc.Condenser.Temperature, _vcrc.Condenser.Subcooling),
            _vcrc.Ejector);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Refrigerant should be a single component or an azeotropic blend!*");
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
    public void Ejector_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Ejector.Should().Be(_vcrc.Ejector);

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsFalse() =>
        _vcrc.Instance.IsTranscritical.Should().BeFalse();

    [Fact]
    public void Point1_Always_ReturnsSeparatorVaporOutlet()
    {
        _vcrc.Instance.Point1.Should().Be(
            _vcrc.Refrigerant.DewPointAt(_vcrc.EjectorFlows.DiffuserOutlet.Pressure));
        _vcrc.Instance.Point1.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point2s_Always_ReturnsIsentropicCompressionStageDischarge()
    {
        _vcrc.Instance.Point2s.Should().Be(
            _vcrc.Instance.Point1.IsentropicCompressionTo(_vcrc.Condenser.Pressure));
        _vcrc.Instance.Point2s.Phase.Should().Be(Phases.Gas);
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
    public void Point4_Always_ReturnsEjectorNozzleOutlet()
    {
        _vcrc.Instance.Point4.Should().Be(_vcrc.EjectorFlows.NozzleOutlet);
        _vcrc.Instance.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point5_Always_ReturnsEjectorMixingInlet()
    {
        _vcrc.Instance.Point5.Should().Be(_vcrc.EjectorFlows.MixingInlet);
        _vcrc.Instance.Point5.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point6_Always_ReturnsEjectorDiffuserOutlet()
    {
        _vcrc.Instance.Point6.Should().Be(_vcrc.EjectorFlows.DiffuserOutlet);
        _vcrc.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point7_Always_ReturnsSeparatorLiquidOutlet()
    {
        _vcrc.Instance.Point7.Should().Be(
            _vcrc.Refrigerant.BubblePointAt(_vcrc.EjectorFlows.DiffuserOutlet.Pressure));
        _vcrc.Instance.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsEvaporatorInlet()
    {
        _vcrc.Instance.Point8.Should().Be(
            _vcrc.Instance.Point7.IsenthalpicExpansionTo(_vcrc.Evaporator.Pressure));
        _vcrc.Instance.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point9_Always_ReturnsEvaporatorOutlet()
    {
        _vcrc.Instance.Point9.Should().Be(_vcrc.Evaporator.Outlet);
        _vcrc.Instance.Point9.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point10_Always_ReturnsEjectorSuctionOutlet()
    {
        _vcrc.Instance.Point10.Should().Be(_vcrc.EjectorFlows.SuctionOutlet);
        _vcrc.Instance.Point10.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        _vcrc.Instance.EvaporatorSpecificMassFlow.Equals(
                100.Percent(), _comparison.Tolerance.Percent())
            .Should().BeTrue();
        _vcrc.Instance.HeatReleaserSpecificMassFlow.Equals(
                _vcrc.Instance.EvaporatorSpecificMassFlow *
                (_vcrc.Instance.Point6.Quality!.Value.DecimalFractions /
                 (1 - _vcrc.Instance.Point6.Quality!.Value.DecimalFractions)),
                _comparison.Tolerance.Percent())
            .Should().BeTrue();
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        _vcrc.Instance.IsentropicSpecificWork.Equals(
                _vcrc.Instance.HeatReleaserSpecificMassFlow.DecimalFractions *
                (_vcrc.Instance.Point2s.Enthalpy - _vcrc.Instance.Point1.Enthalpy),
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
                _vcrc.Instance.Point9.Enthalpy - _vcrc.Instance.Point8.Enthalpy,
                _comparison.Tolerance.JoulesPerKilogram())
            .Should().BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        _vcrc.Instance.SpecificHeatingCapacity.Equals(
                _vcrc.Instance.HeatReleaserSpecificMassFlow.DecimalFractions *
                (_vcrc.Instance.Point2.Enthalpy - _vcrc.Instance.Point3.Enthalpy),
                _comparison.Tolerance.JoulesPerKilogram())
            .Should().BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        _vcrc.Instance.EER.Should().Be(
            _vcrc.Instance.SpecificCoolingCapacity / _vcrc.Instance.SpecificWork);
        _vcrc.Instance.EER.Should().BeApproximately(4.836643336835533, _comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        _vcrc.Instance.COP.Should().Be(
            _vcrc.Instance.SpecificHeatingCapacity / _vcrc.Instance.SpecificWork);
        _vcrc.Instance.COP.Should().BeApproximately(5.836563370628909, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout28() =>
        _vcrc.AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(28.240747630501133, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout28() =>
        _vcrc.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(28.24357079452348, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _vcrc.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_ReturnsAbout21() =>
        _vcrc.AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(21.00249583519198, _comparison.Tolerance);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_Returns0() =>
        _vcrc.AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout0() =>
        _vcrc.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(0.19078569109720636, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_ForThisCase_ReturnsAbout6() =>
        _vcrc.AnalysisResult.EjectorEnergyLossRatio.Percent
            .Should().BeApproximately(6.8730525745277244, _comparison.Tolerance);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout23() =>
        _vcrc.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(23.69009510465962, _comparison.Tolerance);

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
            .Should().BeApproximately(0.009995775827658926, _comparison.Tolerance);
        _vcrc.AnalysisResult.Sum().Percent.Should().BeApproximately(100, _comparison.Tolerance);
    }
}

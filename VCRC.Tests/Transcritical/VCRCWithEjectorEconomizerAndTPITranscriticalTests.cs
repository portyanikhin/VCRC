namespace VCRC.Tests;

public class VCRCWithEjectorEconomizerAndTPITranscriticalTests :
    IClassFixture<ComparisonFixture>,
    IClassFixture<TranscriticalVCRCFixture<VCRCWithEjectorEconomizerAndTPI>>
{
    private readonly ComparisonFixture _comparison;
    private readonly TranscriticalVCRCFixture<VCRCWithEjectorEconomizerAndTPI> _vcrc;

    public VCRCWithEjectorEconomizerAndTPITranscriticalTests(
        ComparisonFixture comparison, TranscriticalVCRCFixture<VCRCWithEjectorEconomizerAndTPI> vcrc)
    {
        _comparison = comparison;
        _vcrc = vcrc;
    }

    [Fact]
    public void Evaporator_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Evaporator.Should().Be(_vcrc.Evaporator);

    [Fact]
    public void Compressor_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Compressor.Should().Be(_vcrc.Compressor);

    [Fact]
    public void Condenser_ForThisCase_ReturnsNull() =>
        _vcrc.Instance.Condenser.Should().BeNull();

    [Fact]
    public void GasCooler_ForThisCase_ReturnsEnteredValue() =>
        _vcrc.Instance.GasCooler.Should().Be(_vcrc.GasCooler);

    [Fact]
    public void Ejector_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Ejector.Should().Be(_vcrc.Ejector);

    [Fact]
    public void Economizer_Always_ReturnsEnteredValue() =>
        _vcrc.Instance.Economizer.Should().Be(_vcrc.Economizer);

    [Fact]
    public void IsTranscritical_ForThisCase_ReturnsTrue() =>
        _vcrc.Instance.IsTranscritical.Should().BeTrue();

    [Fact]
    public void Point1_Always_ReturnsSeparatorVaporOutlet()
    {
        _vcrc.Instance.Point1.Pressure.Pascals.Should().BeApproximately(
            _vcrc.EjectorFlows.DiffuserOutlet.Pressure.Pascals, 50);
        _vcrc.Instance.Point1.Should().Be(
            _vcrc.Refrigerant.DewPointAt(_vcrc.Instance.Point1.Pressure));
        _vcrc.Instance.Point1.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point2s_Always_ReturnsFirstIsentropicCompressionStageDischarge()
    {
        _vcrc.Instance.Point2s.Should().Be(
            _vcrc.Instance.Point1.IsentropicCompressionTo(_vcrc.Instance.IntermediatePressure));
        _vcrc.Instance.Point2s.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point2_Always_ReturnsFirstCompressionStageDischarge()
    {
        _vcrc.Instance.Point2.Should().Be(
            _vcrc.Instance.Point1.CompressionTo(
                _vcrc.Instance.IntermediatePressure, _vcrc.Compressor.Efficiency));
        _vcrc.Instance.Point2.Phase.Should().Be(Phases.SupercriticalGas);
    }

    [Fact]
    public void Point3_Always_ReturnsSecondCompressionStageSuction()
    {
        _vcrc.Instance.Point3.Should().Be(
            _vcrc.Refrigerant.DewPointAt(_vcrc.Instance.IntermediatePressure));
        _vcrc.Instance.Point3.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Point4s_Always_ReturnsSecondIsentropicCompressionStageDischarge()
    {
        _vcrc.Instance.Point4s.Should().Be(
            _vcrc.Instance.Point3.IsentropicCompressionTo(_vcrc.GasCooler.Pressure));
        _vcrc.Instance.Point4s.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point4_Always_ReturnsSecondCompressionStageDischarge()
    {
        _vcrc.Instance.Point4.Should().Be(
            _vcrc.Instance.Point3.CompressionTo(
                _vcrc.GasCooler.Pressure, _vcrc.Compressor.Efficiency));
        _vcrc.Instance.Point4.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point5_ForThisCase_ReturnsGasCoolerOutlet()
    {
        _vcrc.Instance.Point5.Should().Be(_vcrc.GasCooler.Outlet);
        _vcrc.Instance.Point5.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point6_Always_ReturnsEconomizerColdInlet()
    {
        _vcrc.Instance.Point6.Should().Be(
            _vcrc.Instance.Point5.IsenthalpicExpansionTo(_vcrc.Instance.IntermediatePressure));
        _vcrc.Instance.Point6.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point7_Always_ReturnsEconomizerColdOutlet()
    {
        _vcrc.Instance.Point7.Pressure.Equals(
                _vcrc.Instance.IntermediatePressure, _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();
        _vcrc.Instance.Point7.Should().Be(
            _vcrc.Instance.Point6.HeatingTo(
                ((_vcrc.Instance.Point6.Enthalpy.JoulesPerKilogram *
                  (_vcrc.Instance.Point2.Enthalpy.JoulesPerKilogram -
                   _vcrc.Instance.Point3.Enthalpy.JoulesPerKilogram) +
                  _vcrc.Instance.Point3.Enthalpy.JoulesPerKilogram *
                  (_vcrc.Instance.Point5.Enthalpy.JoulesPerKilogram -
                   _vcrc.Instance.Point8.Enthalpy.JoulesPerKilogram)) /
                 (_vcrc.Instance.Point2.Enthalpy.JoulesPerKilogram -
                  _vcrc.Instance.Point3.Enthalpy.JoulesPerKilogram +
                  _vcrc.Instance.Point5.Enthalpy.JoulesPerKilogram -
                  _vcrc.Instance.Point8.Enthalpy.JoulesPerKilogram))
                .JoulesPerKilogram()));
        _vcrc.Instance.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point8_Always_ReturnsEconomizerHotOutlet()
    {
        _vcrc.Instance.Point8.Should().Be(
            _vcrc.Instance.Point5.CoolingTo(
                _vcrc.Instance.Point6.Temperature + _vcrc.Economizer.TemperatureDifference));
        _vcrc.Instance.Point8.Phase.Should().Be(Phases.Supercritical);
    }

    [Fact]
    public void Point9_Always_ReturnsEjectorNozzleOutlet()
    {
        _vcrc.Instance.Point9.Should().Be(_vcrc.EjectorFlows.NozzleOutlet);
        _vcrc.Instance.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point10_Always_ReturnsEjectorMixingInlet()
    {
        _vcrc.Instance.Point10.Should().Be(_vcrc.EjectorFlows.MixingInlet);
        _vcrc.Instance.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point11_Always_ReturnsEjectorDiffuserOutlet()
    {
        _vcrc.Instance.Point11.Should().Be(_vcrc.EjectorFlows.DiffuserOutlet);
        _vcrc.Instance.Point11.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point12_Always_ReturnsSeparatorLiquidOutlet()
    {
        _vcrc.Instance.Point12.Should().Be(
            _vcrc.Refrigerant.BubblePointAt(_vcrc.EjectorFlows.DiffuserOutlet.Pressure));
        _vcrc.Instance.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point13_Always_ReturnsEvaporatorInlet()
    {
        _vcrc.Instance.Point13.Should().Be(
            _vcrc.Instance.Point12.IsenthalpicExpansionTo(_vcrc.Evaporator.Pressure));
        _vcrc.Instance.Point13.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void Point14_Always_ReturnsEvaporatorOutlet()
    {
        _vcrc.Instance.Point14.Should().Be(_vcrc.Evaporator.Outlet);
        _vcrc.Instance.Point14.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void Point15_Always_ReturnsEjectorSuctionOutlet()
    {
        _vcrc.Instance.Point15.Should().Be(_vcrc.EjectorFlows.SuctionOutlet);
        _vcrc.Instance.Point15.Phase.Should().Be(Phases.Gas);
    }

    [Fact]
    public void IntermediatePressure_ForThisCase_ReturnsMiddlePressureBetweenDiffuserOutletAndGasCoolerPressures() =>
        _vcrc.Instance.IntermediatePressure.Pascals.Should().BeApproximately(
            Math.Sqrt(_vcrc.EjectorFlows.DiffuserOutlet.Pressure.Pascals *
                      _vcrc.GasCooler.Pressure.Pascals), 50);

    [Fact]
    public void SpecificMassFlows_Always_CalculatesAutomaticallyByHeatBalance()
    {
        _vcrc.Instance.EvaporatorSpecificMassFlow.Equals(
                100.Percent(), _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();
        _vcrc.Instance.HeatReleaserSpecificMassFlow.Equals(
                _vcrc.Instance.EvaporatorSpecificMassFlow *
                (_vcrc.Instance.Point11.Quality!.Value.DecimalFractions /
                 (1 - _vcrc.Instance.Point11.Quality!.Value.DecimalFractions)) *
                (1 + (_vcrc.Instance.Point2.Enthalpy - _vcrc.Instance.Point3.Enthalpy) /
                    (_vcrc.Instance.Point3.Enthalpy - _vcrc.Instance.Point7.Enthalpy)),
                _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();
        _vcrc.Instance.IntermediateSpecificMassFlow.Equals(
                _vcrc.Instance.HeatReleaserSpecificMassFlow - _vcrc.Instance.EvaporatorSpecificMassFlow *
                (_vcrc.Instance.Point11.Quality!.Value.DecimalFractions /
                 (1 - _vcrc.Instance.Point11.Quality!.Value.DecimalFractions)),
                _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();
    }

    [Fact]
    public void IsentropicSpecificWork_Always_ReturnsEnthalpyDifferenceForIsentropicCompression() =>
        _vcrc.Instance.IsentropicSpecificWork.Equals(
                (_vcrc.Instance.HeatReleaserSpecificMassFlow -
                 _vcrc.Instance.IntermediateSpecificMassFlow).DecimalFractions *
                (_vcrc.Instance.Point2s.Enthalpy - _vcrc.Instance.Point1.Enthalpy) +
                _vcrc.Instance.HeatReleaserSpecificMassFlow.DecimalFractions *
                (_vcrc.Instance.Point4s.Enthalpy - _vcrc.Instance.Point3.Enthalpy),
                _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();

    [Fact]
    public void SpecificWork_Always_ReturnsEnthalpyDifferenceForRealCompression() =>
        _vcrc.Instance.SpecificWork.Equals(
                _vcrc.Instance.IsentropicSpecificWork / _vcrc.Compressor.Efficiency.DecimalFractions,
                _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();

    [Fact]
    public void SpecificCoolingCapacity_Always_ReturnsEnthalpyDifferenceInEvaporator() =>
        _vcrc.Instance.SpecificCoolingCapacity.Equals(
                _vcrc.Instance.Point14.Enthalpy - _vcrc.Instance.Point13.Enthalpy,
                _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();

    [Fact]
    public void SpecificHeatingCapacity_Always_ReturnsEnthalpyDifferenceInCondenser() =>
        _vcrc.Instance.SpecificHeatingCapacity.Equals(
                _vcrc.Instance.HeatReleaserSpecificMassFlow.DecimalFractions *
                (_vcrc.Instance.Point4.Enthalpy - _vcrc.Instance.Point5.Enthalpy),
                _comparison.Tolerance, _comparison.Type)
            .Should().BeTrue();

    [Fact]
    public void EER_Always_ReturnsRatioBetweenSpecificCoolingCapacityAndSpecificWork()
    {
        _vcrc.Instance.EER.Should().Be(
            _vcrc.Instance.SpecificCoolingCapacity / _vcrc.Instance.SpecificWork);
        _vcrc.Instance.EER.Should().BeApproximately(3.1856966924103642, _comparison.Tolerance);
    }

    [Fact]
    public void COP_Always_ReturnsRatioBetweenSpecificHeatingCapacityAndSpecificWork()
    {
        _vcrc.Instance.COP.Should().Be(
            _vcrc.Instance.SpecificHeatingCapacity / _vcrc.Instance.SpecificWork);
        _vcrc.Instance.COP.Should().BeApproximately(4.1856342597396194, _comparison.Tolerance);
    }

    [Fact]
    public void ThermodynamicPerfection_ForThisCase_ReturnsAbout18() =>
        _vcrc.AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(18.60101108396916, _comparison.Tolerance);

    [Fact]
    public void MinSpecificWorkRatio_ForThisCase_ReturnsAbout18() =>
        _vcrc.AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(18.573237119700696, _comparison.Tolerance);

    [Fact]
    public void CompressorEnergyLossRatio_ForThisCase_Returns20() =>
        _vcrc.AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, _comparison.Tolerance);

    [Fact]
    public void CondenserEnergyLossRatio_ForThisCase_Returns0() =>
        _vcrc.AnalysisResult.CondenserEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void GasCoolerEnergyLossRatio_ForThisCase_ReturnsAbout13() =>
        _vcrc.AnalysisResult.GasCoolerEnergyLossRatio.Percent
            .Should().BeApproximately(13.752593268728353, _comparison.Tolerance);

    [Fact]
    public void ExpansionValvesEnergyLossRatio_ForThisCase_ReturnsAbout16() =>
        _vcrc.AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(16.69155493434279, _comparison.Tolerance);

    [Fact]
    public void EjectorEnergyLossRatio_ForThisCase_ReturnsAbout11() =>
        _vcrc.AnalysisResult.EjectorEnergyLossRatio.Percent
            .Should().BeApproximately(11.405846909838836, _comparison.Tolerance);

    [Fact]
    public void EvaporatorEnergyLossRatio_ForThisCase_ReturnsAbout15() =>
        _vcrc.AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(15.39929916463049, _comparison.Tolerance);

    [Fact]
    public void RecuperatorEnergyLossRatio_Always_Returns0() =>
        _vcrc.AnalysisResult.RecuperatorEnergyLossRatio.Percent.Should().Be(0);

    [Fact]
    public void EconomizerEnergyLossRatio_ForThisCase_ReturnsAbout2() =>
        _vcrc.AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(2.1452842259096747, _comparison.Tolerance);

    [Fact]
    public void TestMixingEnergyLossRatio_ForThisCase_ReturnsAbout2() =>
        _vcrc.AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(2.0321843768491616, _comparison.Tolerance);

    [Fact]
    public void AnalysisRelativeError_Always_ReturnsNegligibleValue()
    {
        _vcrc.AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(0.1495375528210737, _comparison.Tolerance);
        _vcrc.AnalysisResult.Sum().Percent.Should().BeApproximately(100, _comparison.Tolerance);
    }
}
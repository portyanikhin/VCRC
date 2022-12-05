using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests.Subcritical;

public static class TestVCRCMitsubishiZubadan
{
    private const double Tolerance = 1e-10;

    private static readonly Refrigerant Refrigerant = new(FluidsList.R32);

    private static readonly Evaporator Evaporator =
        new(Refrigerant.Name, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));

    private static readonly Compressor Compressor = new(80.Percent());

    private static readonly Condenser Condenser =
        new(Refrigerant.Name, 45.DegreesCelsius(), TemperatureDelta.FromKelvins(3));

    private static readonly EconomizerWithTPI Economizer = new(TemperatureDelta.FromKelvins(5));

    private static readonly VCRCMitsubishiZubadan Cycle =
        new(Evaporator, Compressor, Condenser, Economizer);

    private static readonly EntropyAnalysisResult AnalysisResult =
        Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());

    [Test]
    public static void TestSolutionNotFound()
    {
        Action action = () =>
            _ = new VCRCMitsubishiZubadan(
                Evaporator, Compressor,
                new Condenser(Refrigerant.Name, Condenser.Temperature,
                    TemperatureDelta.FromKelvins(50)),
                Economizer);
        action.Should().Throw<ArgumentException>().WithMessage("Solution not found!");
    }

    [Test]
    public static void TestComponents()
    {
        Cycle.Evaporator.Should().Be(Evaporator);
        Cycle.Recuperator.Should().Be(
            new Recuperator(Cycle.Point7.Temperature - Cycle.Point2.Temperature));
        Cycle.Compressor.Should().Be(Compressor);
        Cycle.Condenser.Should().Be(Condenser);
        Cycle.GasCooler.Should().BeNull();
        Cycle.Economizer.Should().Be(Economizer);
    }

    [Test]
    public static void TestIsTranscritical() =>
        Cycle.IsTranscritical.Should().BeFalse();

    [Test]
    public static void TestRecuperatorHighPressure()
    {
        Cycle.RecuperatorHighPressure.Should().Be(
            Math.Sqrt(Cycle.IntermediatePressure.Pascals * Condenser.Pressure.Pascals)
                .Pascals());
        var lowTemperatureCycle = new VCRCMitsubishiZubadan(
            new Evaporator(Refrigerant.Name,
                (-20).DegreesCelsius(), Evaporator.Superheat),
            Compressor, Condenser, Economizer);
        lowTemperatureCycle.RecuperatorHighPressure.Should().BeGreaterThan(
            Math.Sqrt(lowTemperatureCycle.IntermediatePressure.Pascals * Condenser.Pressure.Pascals)
                .Pascals());
    }

    [Test]
    public static void TestPoint1()
    {
        Cycle.Point1.Should().Be(Evaporator.Outlet);
        Cycle.Point1.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint2()
    {
        Cycle.Point2.Should().Be(
            Cycle.Point1.HeatingTo(
                Cycle.Point1.Enthalpy +
                Cycle.HeatReleaserSpecificMassFlow / Cycle.EvaporatorSpecificMassFlow *
                (Cycle.Point7.Enthalpy - Cycle.Point8.Enthalpy)));
        Cycle.Point2.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void TestPoint3s()
    {
        Cycle.Point3s.Should().Be(
            Cycle.Point2.IsentropicCompressionTo(Cycle.IntermediatePressure));
        Cycle.Point3s.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint3()
    {
        Cycle.Point3.Should().Be(
            Cycle.Point2.CompressionTo(Cycle.IntermediatePressure,
                Compressor.Efficiency));
        Cycle.Point3.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint4()
    {
        Cycle.Point4.Should().Be(
            Refrigerant.DewPointAt(Cycle.IntermediatePressure));
        Cycle.Point4.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void TestPoint5s()
    {
        Cycle.Point5s.Should().Be(
            Cycle.Point4.IsentropicCompressionTo(Condenser.Pressure));
        Cycle.Point5s.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint5()
    {
        Cycle.Point5.Should().Be(
            Cycle.Point4.CompressionTo(Condenser.Pressure,
                Compressor.Efficiency));
        Cycle.Point5.Phase.Should().Be(Phases.Gas);
    }

    [Test]
    public static void TestPoint6()
    {
        Cycle.Point6.Should().Be(Condenser.Outlet);
        Cycle.Point6.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public static void TestPoint7()
    {
        Cycle.Point7.Should().Be(
            Cycle.Point6.IsenthalpicExpansionTo(Cycle.RecuperatorHighPressure));
        Cycle.RecuperatorHighPressure.Should().BeGreaterThan(Cycle.IntermediatePressure);
        Cycle.RecuperatorHighPressure.Should().BeLessThan(Condenser.Pressure);
        Cycle.Point7.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint8()
    {
        Cycle.Point8.Should().Be(
            Refrigerant.BubblePointAt(Cycle.RecuperatorHighPressure));
        Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint9()
    {
        Cycle.Point9.Should().Be(
            Cycle.Point8.IsenthalpicExpansionTo(Cycle.IntermediatePressure));
        Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint10()
    {
        Cycle.Point10.Pressure.Should().Be(Cycle.IntermediatePressure);
        Cycle.Point10.Enthalpy.JoulesPerKilogram.Should().BeApproximately(
            (Cycle.Point4.Enthalpy - Cycle.EvaporatorSpecificMassFlow /
                (Cycle.HeatReleaserSpecificMassFlow - Cycle.EvaporatorSpecificMassFlow) *
                (Cycle.Point3.Enthalpy - Cycle.Point4.Enthalpy)).JoulesPerKilogram, 10);
        Cycle.Point10.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestPoint11()
    {
        Cycle.Point11.Should().Be(
            Cycle.Point8.CoolingTo(Cycle.Point9.Temperature + Economizer.TemperatureDifference));
        Cycle.Point11.Phase.Should().Be(Phases.Liquid);
    }

    [Test]
    public static void TestPoint12()
    {
        Cycle.Point12.Should().Be(
            Cycle.Point11.IsenthalpicExpansionTo(Evaporator.Pressure));
        Cycle.Point12.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestSpecificMassFlows()
    {
        Cycle.EvaporatorSpecificMassFlow.Should().Be(100.Percent());
        Cycle.HeatReleaserSpecificMassFlow.Percent.Should().BeApproximately(
            (Cycle.EvaporatorSpecificMassFlow *
             (1 + (Cycle.Point3.Enthalpy - Cycle.Point4.Enthalpy) /
                 (Cycle.Point4.Enthalpy - Cycle.Point10.Enthalpy))).Percent,
            1e-3);
        Cycle.IntermediateSpecificMassFlow.Should().Be(
            Cycle.HeatReleaserSpecificMassFlow - Cycle.EvaporatorSpecificMassFlow);
    }

    [Test]
    public static void TestIsentropicSpecificWork() =>
        Cycle.IsentropicSpecificWork.Should().Be(
            Cycle.Point3s.Enthalpy - Cycle.Point2.Enthalpy +
            Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Cycle.Point5s.Enthalpy - Cycle.Point4.Enthalpy));

    [Test]
    public static void TestSpecificWork() =>
        Cycle.SpecificWork.Should().Be(
            Cycle.IsentropicSpecificWork /
            Compressor.Efficiency.DecimalFractions);

    [Test]
    public static void TestSpecificCoolingCapacity() =>
        Cycle.SpecificCoolingCapacity.Should().Be(
            Cycle.Point1.Enthalpy - Cycle.Point12.Enthalpy);

    [Test]
    public static void TestSpecificHeatingCapacity() =>
        Cycle.SpecificHeatingCapacity.Should().Be(
            Cycle.HeatReleaserSpecificMassFlow.DecimalFractions *
            (Cycle.Point5.Enthalpy - Cycle.Point6.Enthalpy));

    [Test]
    public static void TestEER()
    {
        Cycle.EER.Should().Be(
            Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
        Cycle.EER.Should().BeApproximately(
            4.380342435725647, Tolerance);
    }

    [Test]
    public static void TestCOP()
    {
        Cycle.COP.Should().Be(
            Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
        Cycle.COP.Should().BeApproximately(
            5.380333104312685, Tolerance);
    }

    [Test]
    public static void TestThermodynamicPerfection() =>
        AnalysisResult.ThermodynamicPerfection.Percent
            .Should().BeApproximately(25.57644561474704, Tolerance);

    [Test]
    public static void TestMinSpecificWorkRatio() =>
        AnalysisResult.MinSpecificWorkRatio.Percent
            .Should().BeApproximately(25.27093000190046, Tolerance);

    [Test]
    public static void TestCompressorEnergyLossRatio() =>
        AnalysisResult.CompressorEnergyLossRatio.Percent
            .Should().BeApproximately(20, Tolerance);

    [Test]
    public static void TestCondenserEnergyLossRatio() =>
        AnalysisResult.CondenserEnergyLossRatio.Percent
            .Should().BeApproximately(17.402320942172274, Tolerance);

    [Test]
    public static void TestGasCoolerEnergyLossRatio() =>
        AnalysisResult.GasCoolerEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestExpansionValvesEnergyLossRatio() =>
        AnalysisResult.ExpansionValvesEnergyLossRatio.Percent
            .Should().BeApproximately(7.442291905240232, Tolerance);

    [Test]
    public static void TestEjectorEnergyLossRatio() =>
        AnalysisResult.EjectorEnergyLossRatio.Percent.Should().Be(0);

    [Test]
    public static void TestEvaporatorEnergyLossRatio() =>
        AnalysisResult.EvaporatorEnergyLossRatio.Percent
            .Should().BeApproximately(21.16669887701517, Tolerance);

    [Test]
    public static void TestRecuperatorEnergyLossRatio() =>
        AnalysisResult.RecuperatorEnergyLossRatio.Percent
            .Should().BeApproximately(1.3431046427670745, Tolerance);

    [Test]
    public static void TestEconomizerEnergyLossRatio() =>
        AnalysisResult.EconomizerEnergyLossRatio.Percent
            .Should().BeApproximately(0.44158251658286674, Tolerance);

    [Test]
    public static void TestMixingEnergyLossRatio() =>
        AnalysisResult.MixingEnergyLossRatio.Percent
            .Should().BeApproximately(6.9330711143219235, Tolerance);

    [Test]
    public static void TestAnalysisRelativeError()
    {
        AnalysisResult.AnalysisRelativeError.Percent
            .Should().BeApproximately(1.208960702370679, Tolerance);
        AnalysisResult.Sum().Percent
            .Should().BeApproximately(100, Tolerance);
    }
}
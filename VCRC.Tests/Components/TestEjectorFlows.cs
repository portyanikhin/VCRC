using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests.Components;

public static class TestEjectorFlows
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R32);

    private static readonly Ejector Ejector =
        new(90.Percent(), 90.Percent(), 80.Percent());

    private static readonly Refrigerant NozzleInlet =
        Refrigerant.BubblePointAt(45.DegreesCelsius());

    private static readonly Refrigerant SuctionInlet =
        Refrigerant.DewPointAt(5.DegreesCelsius());

    private static readonly EjectorFlows EjectorFlows =
        Ejector.CalculateFlows(NozzleInlet, SuctionInlet);

    private static readonly Speed MixingInletSpeed =
        EjectorFlows.FlowRatio.DecimalFractions *
        CalculateOutletSpeed(EjectorFlows.NozzleInlet, EjectorFlows.NozzleOutlet) +
        (1 - EjectorFlows.FlowRatio.DecimalFractions) *
        CalculateOutletSpeed(EjectorFlows.SuctionInlet, EjectorFlows.SuctionOutlet);

    private static readonly SpecificEnergy MixingInletKineticEnergy =
        (Math.Pow(MixingInletSpeed.MetersPerSecond, 2) / 2.0)
        .JoulesPerKilogram();

    [Test]
    public static void TestWrongRefrigerants()
    {
        Action action = () =>
            _ = Ejector.CalculateFlows(
                new Refrigerant(FluidsList.R22).BubblePointAt(45.DegreesCelsius()),
                Refrigerant.DewPointAt(5.DegreesCelsius()));
        action.Should().Throw<ValidationException>()
            .WithMessage("*Only one refrigerant should be selected!*");
    }

    [Test]
    public static void TestWrongPressures()
    {
        Action action = () =>
            _ = Ejector.CalculateFlows(
                Refrigerant.BubblePointAt(5.DegreesCelsius()),
                Refrigerant.DewPointAt(45.DegreesCelsius()));
        action.Should().Throw<ValidationException>().WithMessage(
            "*Ejector nozzle inlet pressure should be greater than suction inlet pressure!*");
    }

    [Test]
    public static void TestNozzleInlet() =>
        EjectorFlows.NozzleInlet.Should().Be(NozzleInlet);

    [Test]
    public static void TestSuctionInlet() =>
        EjectorFlows.SuctionInlet.Should().Be(SuctionInlet);

    [Test]
    public static void TestNozzleOutlet()
    {
        EjectorFlows.NozzleOutlet.Should().Be(
            EjectorFlows.NozzleInlet.ExpansionTo(
                EjectorFlows.MixingInlet.Pressure, Ejector.NozzleEfficiency));
        EjectorFlows.NozzleOutlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestSuctionOutlet()
    {
        EjectorFlows.SuctionOutlet.Should().Be(
            EjectorFlows.SuctionInlet.ExpansionTo(
                EjectorFlows.MixingInlet.Pressure, Ejector.SuctionEfficiency));
        EjectorFlows.SuctionOutlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestMixingInlet()
    {
        EjectorFlows.MixingInlet.Pressure.Should().Be(0.9 * SuctionInlet.Pressure);
        EjectorFlows.MixingInlet.Enthalpy.Should().Be(
            EjectorFlows.FlowRatio.DecimalFractions * NozzleInlet.Enthalpy +
            (1 - EjectorFlows.FlowRatio.DecimalFractions) * SuctionInlet.Enthalpy -
            MixingInletKineticEnergy);
        EjectorFlows.MixingInlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestDiffuserOutlet()
    {
        EjectorFlows.DiffuserOutlet.Should().Be(Refrigerant.WithState(
            Input.Pressure(
                Refrigerant.WithState(
                    Input.Entropy(EjectorFlows.MixingInlet.Entropy),
                    Input.Enthalpy(
                        EjectorFlows.MixingInlet.Enthalpy +
                        Ejector.DiffuserEfficiency.DecimalFractions *
                        MixingInletKineticEnergy)).Pressure),
            Input.Enthalpy(EjectorFlows.MixingInlet.Enthalpy + MixingInletKineticEnergy)));
        EjectorFlows.DiffuserOutlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Test]
    public static void TestFlowRatio() =>
        EjectorFlows.FlowRatio.Percent.Should().BeApproximately(
            EjectorFlows.DiffuserOutlet.Quality!.Value.Percent, 1e-3);

    private static Speed CalculateOutletSpeed(AbstractFluid inlet, AbstractFluid outlet) =>
        Math.Sqrt(2 * (inlet.Enthalpy - outlet.Enthalpy).JoulesPerKilogram).MetersPerSecond();
}
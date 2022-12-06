using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class EjectorFlowsTests
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R32);
    private static readonly Ejector Ejector = new(90.Percent(), 90.Percent(), 80.Percent());
    private static readonly Refrigerant NozzleInlet = Refrigerant.BubblePointAt(45.DegreesCelsius());
    private static readonly Refrigerant SuctionInlet = Refrigerant.DewPointAt(5.DegreesCelsius());

    public EjectorFlowsTests()
    {
        EjectorFlows = Ejector.CalculateFlows(NozzleInlet, SuctionInlet);
        MixingInletSpeed =
            EjectorFlows.FlowRatio.DecimalFractions *
            CalculateOutletSpeed(NozzleInlet, EjectorFlows.NozzleOutlet) +
            (1 - EjectorFlows.FlowRatio.DecimalFractions) *
            CalculateOutletSpeed(SuctionInlet, EjectorFlows.SuctionOutlet);
        MixingInletKineticEnergy = (Math.Pow(MixingInletSpeed.MetersPerSecond, 2) / 2.0)
            .JoulesPerKilogram();
    }

    private Speed MixingInletSpeed { get; }
    private SpecificEnergy MixingInletKineticEnergy { get; }
    private EjectorFlows EjectorFlows { get; }

    [Fact]
    public void EjectorFlows_WrongRefrigerants_ThrowsValidationException()
    {
        Action action = () => _ = Ejector.CalculateFlows(
            new Refrigerant(FluidsList.R22)
                .BubblePointAt(45.DegreesCelsius()), SuctionInlet);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Only one refrigerant should be selected!*");
    }

    [Fact]
    public void EjectorFlows_WrongPressures_ThrowsValidationException()
    {
        Action action = () => _ = Ejector.CalculateFlows(SuctionInlet, NozzleInlet);
        action.Should().Throw<ValidationException>().WithMessage(
            "*Ejector nozzle inlet pressure should be greater than suction inlet pressure!*");
    }

    [Fact]
    public void NozzleInlet_Always_ReturnsSpecifiedNozzleInletPoint() =>
        EjectorFlows.NozzleInlet.Should().Be(NozzleInlet);

    [Fact]
    public void SuctionInlet_Always_ReturnsSpecifiedSuctionInletPoint() =>
        EjectorFlows.SuctionInlet.Should().Be(SuctionInlet);

    [Fact]
    public void NozzleOutlet_Always_ReturnsNozzleInletAfterExpansionToMixingPressure()
    {
        EjectorFlows.NozzleOutlet.Should().Be(
            EjectorFlows.NozzleInlet.ExpansionTo(
                EjectorFlows.MixingInlet.Pressure, Ejector.NozzleEfficiency));
        EjectorFlows.NozzleOutlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void SuctionOutlet_Always_ReturnsSuctionInletAfterExpansionToMixingPressure()
    {
        EjectorFlows.SuctionOutlet.Should().Be(
            EjectorFlows.SuctionInlet.ExpansionTo(
                EjectorFlows.MixingInlet.Pressure, Ejector.SuctionEfficiency));
        EjectorFlows.SuctionOutlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void MixingInlet_Always_ReturnsPointMixPointAt90PercentsOfSuctionPressure()
    {
        EjectorFlows.MixingInlet.Pressure.Should().Be(0.9 * SuctionInlet.Pressure);
        EjectorFlows.MixingInlet.Enthalpy.Should().Be(
            EjectorFlows.FlowRatio.DecimalFractions * NozzleInlet.Enthalpy +
            (1 - EjectorFlows.FlowRatio.DecimalFractions) * SuctionInlet.Enthalpy -
            MixingInletKineticEnergy);
        EjectorFlows.MixingInlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void DiffuserOutlet_Always_ReturnsCompressedMixPoint()
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

    [Fact]
    public void FlowRatio_Always_ReturnsApproximatelyDiffuserOutletVaporQuality() =>
        EjectorFlows.FlowRatio.Percent.Should().BeApproximately(
            EjectorFlows.DiffuserOutlet.Quality!.Value.Percent, 1e-3);

    private static Speed CalculateOutletSpeed(AbstractFluid inlet, AbstractFluid outlet) =>
        Math.Sqrt(2 * (inlet.Enthalpy - outlet.Enthalpy).JoulesPerKilogram).MetersPerSecond();
}
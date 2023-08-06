using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class EjectorFlowsTests : IClassFixture<ComparisonFixture>
{
    private static readonly Refrigerant Refrigerant = new(FluidsList.R32);
    private static readonly Ejector Ejector =
        new(90.Percent(), 90.Percent(), 80.Percent());
    private static readonly Refrigerant NozzleInlet = Refrigerant.BubblePointAt(
        45.DegreesCelsius()
    );
    private static readonly Refrigerant SuctionInlet = Refrigerant.DewPointAt(
        5.DegreesCelsius()
    );
    private readonly ComparisonFixture _comparison;
    private readonly EjectorFlows _ejectorFlows;
    private readonly SpecificEnergy _mixingInletKineticEnergy;

    public EjectorFlowsTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _comparison.Tolerance = 1e-3;
        _ejectorFlows = Ejector.CalculateFlows(NozzleInlet, SuctionInlet);
        var mixingInletSpeed =
            _ejectorFlows.FlowRatio.DecimalFractions
                * CalculateOutletSpeed(NozzleInlet, _ejectorFlows.NozzleOutlet)
            + (1 - _ejectorFlows.FlowRatio.DecimalFractions)
                * CalculateOutletSpeed(
                    SuctionInlet,
                    _ejectorFlows.SuctionOutlet
                );
        _mixingInletKineticEnergy = (
            Math.Pow(mixingInletSpeed.MetersPerSecond, 2) / 2.0
        ).JoulesPerKilogram();
    }

    [Fact]
    public void EjectorFlows_WrongRefrigerants_ThrowsValidationException()
    {
        Action action = () =>
            _ = Ejector.CalculateFlows(
                new Refrigerant(FluidsList.R22).BubblePointAt(
                    45.DegreesCelsius()
                ),
                SuctionInlet
            );
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage("*Only one refrigerant should be selected!*");
    }

    [Fact]
    public void EjectorFlows_WrongPressures_ThrowsValidationException()
    {
        Action action = () =>
            _ = Ejector.CalculateFlows(SuctionInlet, NozzleInlet);
        action
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
                "*Ejector nozzle inlet pressure "
                    + "should be greater than suction inlet pressure!*"
            );
    }

    [Fact]
    public void NozzleInlet_Always_ReturnsSpecifiedNozzleInletPoint() =>
        _ejectorFlows.NozzleInlet.Should().Be(NozzleInlet);

    [Fact]
    public void SuctionInlet_Always_ReturnsSpecifiedSuctionInletPoint() =>
        _ejectorFlows.SuctionInlet.Should().Be(SuctionInlet);

    [Fact]
    public void NozzleOutlet_Always_ReturnsNozzleInletAfterExpansionToMixingPressure()
    {
        _ejectorFlows.NozzleOutlet
            .Should()
            .Be(
                _ejectorFlows.NozzleInlet.ExpansionTo(
                    _ejectorFlows.MixingInlet.Pressure,
                    Ejector.NozzleEfficiency
                )
            );
        _ejectorFlows.NozzleOutlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void SuctionOutlet_Always_ReturnsSuctionInletAfterExpansionToMixingPressure()
    {
        _ejectorFlows.SuctionOutlet
            .Should()
            .Be(
                _ejectorFlows.SuctionInlet.ExpansionTo(
                    _ejectorFlows.MixingInlet.Pressure,
                    Ejector.SuctionEfficiency
                )
            );
        _ejectorFlows.SuctionOutlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void MixingInlet_Always_ReturnsPointMixPointAt90PercentsOfSuctionPressure()
    {
        _ejectorFlows.MixingInlet.Pressure
            .Should()
            .Be(0.9 * SuctionInlet.Pressure);
        _ejectorFlows.MixingInlet.Enthalpy
            .Should()
            .Be(
                _ejectorFlows.FlowRatio.DecimalFractions * NozzleInlet.Enthalpy
                    + (1 - _ejectorFlows.FlowRatio.DecimalFractions)
                        * SuctionInlet.Enthalpy
                    - _mixingInletKineticEnergy
            );
        _ejectorFlows.MixingInlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void DiffuserOutlet_Always_ReturnsCompressedMixPoint()
    {
        _ejectorFlows.DiffuserOutlet
            .Should()
            .Be(
                Refrigerant.WithState(
                    Input.Pressure(
                        Refrigerant
                            .WithState(
                                Input.Entropy(
                                    _ejectorFlows.MixingInlet.Entropy
                                ),
                                Input.Enthalpy(
                                    _ejectorFlows.MixingInlet.Enthalpy
                                        + Ejector
                                            .DiffuserEfficiency
                                            .DecimalFractions
                                            * _mixingInletKineticEnergy
                                )
                            )
                            .Pressure
                    ),
                    Input.Enthalpy(
                        _ejectorFlows.MixingInlet.Enthalpy
                            + _mixingInletKineticEnergy
                    )
                )
            );
        _ejectorFlows.DiffuserOutlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void FlowRatio_Always_ReturnsApproximatelyDiffuserOutletVaporQuality() =>
        _ejectorFlows.FlowRatio
            .Equals(
                _ejectorFlows.DiffuserOutlet.Quality!.Value,
                _comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();

    private static Speed CalculateOutletSpeed(
        AbstractFluid inlet,
        AbstractFluid outlet
    ) =>
        Math.Sqrt(2 * (inlet.Enthalpy - outlet.Enthalpy).JoulesPerKilogram)
            .MetersPerSecond();
}

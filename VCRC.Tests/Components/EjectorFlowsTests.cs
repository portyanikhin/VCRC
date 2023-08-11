using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public class EjectorFlowsTests : IClassFixture<ComparisonFixture>
{
    private readonly ComparisonFixture _comparison;
    private readonly IEjector _ejector;
    private readonly SpecificEnergy _mixingInletKineticEnergy;
    private readonly IRefrigerant _nozzleInlet;
    private readonly IRefrigerant _refrigerant;
    private readonly IRefrigerant _suctionInlet;
    private readonly IEjectorFlows _sut;

    public EjectorFlowsTests(ComparisonFixture comparison)
    {
        _comparison = comparison;
        _refrigerant = new Refrigerant(FluidsList.R32);
        _ejector = new Ejector(90.Percent(), 90.Percent(), 80.Percent());
        _nozzleInlet = _refrigerant.BubblePointAt(45.DegreesCelsius());
        _suctionInlet = _refrigerant.DewPointAt(5.DegreesCelsius());
        _sut = _ejector.CalculateFlows(_nozzleInlet, _suctionInlet);
        var mixingInletSpeed =
            _sut.FlowRatio.DecimalFractions
                * CalculateOutletSpeed(_nozzleInlet, _sut.NozzleOutlet)
            + (1 - _sut.FlowRatio.DecimalFractions)
                * CalculateOutletSpeed(_suctionInlet, _sut.SuctionOutlet);
        _mixingInletKineticEnergy = (
            Math.Pow(mixingInletSpeed.MetersPerSecond, 2) / 2.0
        ).JoulesPerKilogram();
    }

    [Fact]
    public void EjectorFlows_WrongRefrigerants_ThrowsValidationException()
    {
        Action action = () =>
            _ = _ejector.CalculateFlows(
                new Refrigerant(FluidsList.R22).BubblePointAt(
                    45.DegreesCelsius()
                ),
                _suctionInlet
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
            _ = _ejector.CalculateFlows(_suctionInlet, _nozzleInlet);
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
        _sut.NozzleInlet.Should().Be(_nozzleInlet);

    [Fact]
    public void SuctionInlet_Always_ReturnsSpecifiedSuctionInletPoint() =>
        _sut.SuctionInlet.Should().Be(_suctionInlet);

    [Fact]
    public void NozzleOutlet_Always_ReturnsNozzleInletAfterExpansionToMixingPressure()
    {
        _sut.NozzleOutlet
            .Should()
            .Be(
                _sut.NozzleInlet.ExpansionTo(
                    _sut.MixingInlet.Pressure,
                    _ejector.NozzleEfficiency
                )
            );
        _sut.NozzleOutlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void SuctionOutlet_Always_ReturnsSuctionInletAfterExpansionToMixingPressure()
    {
        _sut.SuctionOutlet
            .Should()
            .Be(
                _sut.SuctionInlet.ExpansionTo(
                    _sut.MixingInlet.Pressure,
                    _ejector.SuctionEfficiency
                )
            );
        _sut.SuctionOutlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void MixingInlet_Always_ReturnsPointMixPointAt90PercentsOfSuctionPressure()
    {
        _sut.MixingInlet.Pressure.Should().Be(0.9 * _suctionInlet.Pressure);
        _sut.MixingInlet.Enthalpy
            .Should()
            .Be(
                _sut.FlowRatio.DecimalFractions * _nozzleInlet.Enthalpy
                    + (1 - _sut.FlowRatio.DecimalFractions)
                        * _suctionInlet.Enthalpy
                    - _mixingInletKineticEnergy
            );
        _sut.MixingInlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void DiffuserOutlet_Always_ReturnsCompressedMixPoint()
    {
        _sut.DiffuserOutlet
            .Should()
            .Be(
                _refrigerant.WithState(
                    Input.Pressure(
                        _refrigerant
                            .WithState(
                                Input.Entropy(_sut.MixingInlet.Entropy),
                                Input.Enthalpy(
                                    _sut.MixingInlet.Enthalpy
                                        + _ejector
                                            .DiffuserEfficiency
                                            .DecimalFractions
                                            * _mixingInletKineticEnergy
                                )
                            )
                            .Pressure
                    ),
                    Input.Enthalpy(
                        _sut.MixingInlet.Enthalpy + _mixingInletKineticEnergy
                    )
                )
            );
        _sut.DiffuserOutlet.Phase.Should().Be(Phases.TwoPhase);
    }

    [Fact]
    public void FlowRatio_Always_ReturnsApproximatelyDiffuserOutletVaporQuality() =>
        _sut.FlowRatio
            .Equals(
                _sut.DiffuserOutlet.Quality!.Value,
                _comparison.Tolerance.Percent()
            )
            .Should()
            .BeTrue();

    private static Speed CalculateOutletSpeed(
        IFluidState inlet,
        IFluidState outlet
    ) =>
        Math.Sqrt(2 * (inlet.Enthalpy - outlet.Enthalpy).JoulesPerKilogram)
            .MetersPerSecond();
}

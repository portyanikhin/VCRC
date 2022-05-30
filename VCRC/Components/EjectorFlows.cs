using System;
using FluentValidation;
using MathNet.Numerics;
using MathNet.Numerics.RootFinding;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToSpecificEnergy;
using UnitsNet.NumberExtensions.NumberToSpeed;

namespace VCRC;

/// <summary>
///     Ejector flows.
/// </summary>
public class EjectorFlows
{
    /// <summary>
    ///     Ejector flows.
    /// </summary>
    /// <param name="ejector">Ejector.</param>
    /// <param name="nozzleInlet">Nozzle inlet.</param>
    /// <param name="suctionInlet">Suction section inlet.</param>
    /// <exception cref="ValidationException">
    ///     Only one refrigerant should be selected!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Ejector nozzle inlet pressure should be greater than suction inlet pressure!
    /// </exception>
    public EjectorFlows(Ejector ejector, Refrigerant nozzleInlet, Refrigerant suctionInlet)
    {
        Refrigerant = new Refrigerant(nozzleInlet.Name);
        (Ejector, NozzleInlet, SuctionInlet) = (ejector, nozzleInlet, suctionInlet);
        new EjectorFlowsValidator().ValidateAndThrow(this);
        NozzleOutlet = NozzleInlet.ExpansionTo(MixingPressure, Ejector.NozzleEfficiency);
        SuctionOutlet = SuctionInlet.ExpansionTo(MixingPressure, Ejector.SuctionEfficiency);
        CalculateFlowRatio();
    }

    private Refrigerant Refrigerant { get; }

    private Ejector Ejector { get; }

    private Pressure MixingPressure => 0.9 * SuctionInlet.Pressure;

    private Speed MixingInletSpeed =>
        FlowRatio.DecimalFractions *
        CalculateOutletSpeed(NozzleInlet, NozzleOutlet) +
        (1 - FlowRatio.DecimalFractions) *
        CalculateOutletSpeed(SuctionInlet, SuctionOutlet);

    private SpecificEnergy MixingInletKineticEnergy =>
        (Math.Pow(MixingInletSpeed.MetersPerSecond, 2) / 2.0)
        .JoulesPerKilogram();

    /// <summary>
    ///     Nozzle inlet.
    /// </summary>
    public Refrigerant NozzleInlet { get; }

    /// <summary>
    ///     Suction section inlet.
    /// </summary>
    public Refrigerant SuctionInlet { get; }

    /// <summary>
    ///     Nozzle outlet.
    /// </summary>
    public Refrigerant NozzleOutlet { get; }

    /// <summary>
    ///     Suction section outlet.
    /// </summary>
    public Refrigerant SuctionOutlet { get; }

    /// <summary>
    ///     Mixing section inlet.
    /// </summary>
    public Refrigerant MixingInlet { get; private set; } = null!;

    /// <summary>
    ///     Diffuser outlet.
    /// </summary>
    public Refrigerant DiffuserOutlet { get; private set; } = null!;

    /// <summary>
    ///     Flow ratio.
    /// </summary>
    public Ratio FlowRatio { get; private set; }

    private void CalculateFlowRatio()
    {
        double ToSolve(double flowRatio)
        {
            FlowRatio = flowRatio.Percent();
            MixingInlet = Refrigerant.WithState(
                Input.Pressure(MixingPressure),
                Input.Enthalpy(
                    FlowRatio.DecimalFractions * NozzleInlet.Enthalpy +
                    (1 - FlowRatio.DecimalFractions) * SuctionInlet.Enthalpy -
                    MixingInletKineticEnergy));
            DiffuserOutlet = Refrigerant.WithState(
                Input.Pressure(
                    Refrigerant.WithState(
                        Input.Entropy(MixingInlet.Entropy),
                        Input.Enthalpy(
                            MixingInlet.Enthalpy +
                            Ejector.DiffuserEfficiency.DecimalFractions *
                            MixingInletKineticEnergy)).Pressure),
                Input.Enthalpy(MixingInlet.Enthalpy + MixingInletKineticEnergy));
            return (DiffuserOutlet.Quality!.Value - FlowRatio).Percent;
        }

        NewtonRaphson.FindRootNearGuess(
            ToSolve, Differentiate.FirstDerivativeFunc(ToSolve), 50, 1e-9, 100 - 1e-9, 1e-6);
    }

    private static Speed CalculateOutletSpeed(AbstractFluid inlet, AbstractFluid outlet) =>
        Math.Sqrt(2 * (inlet.Enthalpy - outlet.Enthalpy).JoulesPerKilogram).MetersPerSecond();
}
using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Tests
{
    public class TestVCRCWithCompleteIntercooling
    {
        public enum Bound
        {
            Lower,
            Higher
        }

        private VCRCWithCompleteIntercooling Cycle { get; set; } = null!;

        [SetUp]
        public void SetUp()
        {
            const FluidsList refrigerantName = FluidsList.R32;
            var evaporator = new Evaporator(refrigerantName, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
            var compressor = new Compressor(80.Percent());
            var condenser = new Condenser(refrigerantName, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));
            Cycle = new VCRCWithCompleteIntercooling(evaporator, compressor, condenser);
        }

        [TestCase(Bound.Lower, "Intermediate pressure should be > evaporating pressure!")]
        [TestCase(Bound.Higher, "Intermediate pressure should be < condensing pressure!")]
        public void TestWrongIntermediatePressure(Bound bound, string message)
        {
            Action action = () =>
                _ = new VCRCWithIncompleteIntercooling(Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser,
                    new IntermediateVessel(bound is Bound.Lower
                        ? Cycle.Evaporator.Pressure
                        : Cycle.Condenser.Pressure));
            action.Should().Throw<ValidationException>().WithMessage($"*{message}*");
        }

        [Test]
        public void TestWrongPhaseAtIntermediateVesselInlet()
        {
            Action action = () =>
                _ = new VCRCWithIncompleteIntercooling(Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser,
                    new IntermediateVessel(Cycle.Condenser.Pressure - 1.Pascals()));
            action.Should().Throw<ValidationException>()
                .WithMessage("*There should be a two-phase refrigerant at the intermediate vessel inlet!*");
        }

        [Test]
        public void TestSpecificMassFlows()
        {
            Cycle.FirstStageSpecificMassFlow.Should().Be(100.Percent());
            Cycle.SecondStageSpecificMassFlow.Should().Be(Cycle.FirstStageSpecificMassFlow *
                                                          (1 + (Cycle.Point2.Enthalpy - Cycle.Point3.Enthalpy) /
                                                              (Cycle.Point3.Enthalpy - Cycle.Point9.Enthalpy)) /
                                                          (1 - Cycle.Point8.Quality!.Value.DecimalFractions));
        }

        [Test]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void TestPoint2s()
        {
            Cycle.Point2s.Pressure.Should().Be(Cycle.IntermediateVessel.Pressure);
            Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
            Cycle.Point2s.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        public void TestPoint2()
        {
            Cycle.Point2.Pressure.Should().Be(Cycle.IntermediateVessel.Pressure);
            Cycle.Point2.Enthalpy.Should().Be(Cycle.Point1.Enthalpy + (Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy) /
                Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
            Cycle.Point2.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        public void TestPoint3()
        {
            Cycle.Point3.Pressure.Should().Be(Cycle.IntermediateVessel.Pressure);
            Cycle.Point3.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
            Cycle.Point3.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void TestPoint4s()
        {
            Cycle.Point4s.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point4s.Entropy.Should().Be(Cycle.Point3.Entropy);
            Cycle.Point4s.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        public void TestPoint4()
        {
            Cycle.Point4.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point4.Enthalpy.Should().Be(Cycle.Point3.Enthalpy + (Cycle.Point4s.Enthalpy - Cycle.Point3.Enthalpy) /
                Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
            Cycle.Point4.Enthalpy.Should().BeGreaterThan(Cycle.Point4s.Enthalpy);
            Cycle.Point4.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        public void TestPoint5()
        {
            Cycle.Point5.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point5.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
            Cycle.Point5.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        public void TestPoint6()
        {
            Cycle.Point6.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point6.Quality.Should().Be(TwoPhase.Bubble.VaporQuality());
            Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        public void TestPoint7()
        {
            Cycle.Point7.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point7.Temperature.Should().Be(Cycle.Point6.Temperature - Cycle.Condenser.Subcooling);
            Cycle.Point7.Phase.Should().Be(Phases.Liquid);
        }

        [Test]
        public void TestPoint8()
        {
            Cycle.Point8.Pressure.Should().Be(Cycle.IntermediateVessel.Pressure);
            Cycle.Point8.Enthalpy.Should().Be(Cycle.Point7.Enthalpy);
            Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        public void TestPoint9()
        {
            Cycle.Point9.Pressure.Should().Be(Cycle.IntermediateVessel.Pressure);
            Cycle.Point9.Quality.Should().Be(TwoPhase.Bubble.VaporQuality());
            Cycle.Point9.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        public void TestPoint10()
        {
            Cycle.Point10.Pressure.Should().Be(Cycle.Evaporator.Pressure);
            Cycle.Point10.Enthalpy.Should().Be(Cycle.Point9.Enthalpy);
            Cycle.Point10.Phase.Should().Be(Phases.TwoPhase);
        }
    }
}
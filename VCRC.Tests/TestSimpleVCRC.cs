using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Tests
{
    public class TestSimpleVCRC
    {
        private SimpleVCRC Cycle { get; set; } = null!;

        [SetUp]
        public void SetUp()
        {
            const FluidsList refrigerantName = FluidsList.R407C;
            var evaporator = new Evaporator(refrigerantName, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
            var compressor = new Compressor(80.Percent());
            var condenser = new Condenser(refrigerantName, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));
            Cycle = new SimpleVCRC(evaporator, compressor, condenser);
        }

        [TestCase(5, 4)]
        [TestCase(51, 50)]
        public void TestWrongTemperatures(double evaporatingTemperature, double condensingTemperature)
        {
            Action action = () =>
                _ = new SimpleVCRC(
                    new Evaporator(Cycle.RefrigerantName, evaporatingTemperature.DegreesCelsius(),
                        Cycle.Evaporator.Superheat), Cycle.Compressor,
                    new Condenser(Cycle.RefrigerantName, condensingTemperature.DegreesCelsius(),
                        Cycle.Condenser.Subcooling));
            action.Should().Throw<ValidationException>()
                .WithMessage("*Condensing temperature should be greater than evaporating temperature!*");
        }

        [Test]
        public void TestWrongRefrigerantNames()
        {
            Action action = () =>
                _ = new SimpleVCRC(
                    new Evaporator(FluidsList.R32, Cycle.Evaporator.Temperature, Cycle.Evaporator.Superheat),
                    Cycle.Compressor,
                    new Condenser(FluidsList.R410A, Cycle.Condenser.Temperature, Cycle.Condenser.Subcooling));
            action.Should().Throw<ValidationException>()
                .WithMessage("*Only one refrigerant should be selected!*");
        }

        [Test]
        public void TestZeroSuperheat()
        {
            var cycleWithZeroSuperheat =
                new SimpleVCRC(
                    new Evaporator(Cycle.RefrigerantName, Cycle.Evaporator.Temperature, TemperatureDelta.Zero),
                    Cycle.Compressor, Cycle.Condenser);
            cycleWithZeroSuperheat.Point0.Should().Be(cycleWithZeroSuperheat.Point1);
        }

        [Test]
        public void TestZeroSubcooling()
        {
            var cycleWithZeroSubcooling = new SimpleVCRC(Cycle.Evaporator, Cycle.Compressor,
                new Condenser(Cycle.RefrigerantName, Cycle.Condenser.Temperature, TemperatureDelta.Zero));
            cycleWithZeroSubcooling.Point4.Should().Be(cycleWithZeroSubcooling.Point5);
        }

        [Test]
        public void TestPoint0()
        {
            Cycle.Point0.Pressure.Should().Be(Cycle.Evaporator.Pressure);
            Cycle.Point0.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
            Cycle.Point0.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        public void TestPoint1()
        {
            Cycle.Point1.Pressure.Should().Be(Cycle.Evaporator.Pressure);
            Cycle.Point1.Temperature.Should().Be(Cycle.Point0.Temperature + Cycle.Evaporator.Superheat);
            Cycle.Point1.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void TestPoint2s()
        {
            Cycle.Point2s.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
            Cycle.Point2s.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        public void TestPoint2()
        {
            Cycle.Point2.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point2.Enthalpy.Should().Be(Cycle.Point1.Enthalpy + Cycle.SpecificWork);
            Cycle.Point2.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        public void TestPoint3()
        {
            Cycle.Point3.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point3.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
            Cycle.Point3.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        public void TestPoint4()
        {
            Cycle.Point4.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point4.Quality.Should().Be(TwoPhase.Bubble.VaporQuality());
            Cycle.Point4.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        public void TestPoint5()
        {
            Cycle.Point5.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point5.Temperature.Should().Be(Cycle.Point4.Temperature - Cycle.Condenser.Subcooling);
            Cycle.Point5.Phase.Should().Be(Phases.Liquid);
        }

        [Test]
        public void TestPoint6()
        {
            Cycle.Point6.Pressure.Should().Be(Cycle.Evaporator.Pressure);
            Cycle.Point6.Enthalpy.Should().Be(Cycle.Point5.Enthalpy);
            Cycle.Point6.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        public void TestIsentropicSpecificWork() =>
            Cycle.IsentropicSpecificWork.Should()
                .Be(Cycle.Compressor.IsentropicEfficiency.DecimalFractions * Cycle.SpecificWork);

        [Test]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void TestEER() => Cycle.EER.Should().Be(Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);

        [Test]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void TestCOP() => Cycle.COP.Should().Be(Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);

        [TestCase(20, 20, "Indoor and outdoor temperatures should not be equal!")]
        [TestCase(13, 35, "Wrong temperature difference in the evaporator! Increase 'cold' source temperature.")]
        [TestCase(18, 47, "Wrong temperature difference in the condenser! Decrease 'hot' source temperature.")]
        public void TestEntropyAnalysisWrongTemperatures(double indoor, double outdoor, string message)
        {
            IEntropyAnalysable vcrc = Cycle;
            Action action = () => vcrc.EntropyAnalysis(indoor.DegreesCelsius(), outdoor.DegreesCelsius());
            action.Should().Throw<ArgumentException>().WithMessage(message);
        }

        [Test]
        public void TestEntropyAnalysis()
        {
            var result = Cycle.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());
            result.ThermodynamicPerfection.Should().Be(19.37172248331848.Percent());
            result.CompressorEnergyLossRatio.Should().Be(20.Percent());
            result.CondenserEnergyLossRatio.Should().Be(23.87814506835601.Percent());
            result.ExpansionValvesEnergyLossRatio.Should().Be(16.623952424431838.Percent());
            result.EvaporatorEnergyLossRatio.Should().Be(20.12618002389366.Percent());
            result.RecuperatorEnergyLossRatio.Should().Be(Ratio.Zero);
            result.AnalysisRelativeError.Should().Be(7.984075402616695e-14.Percent());
        }
    }
}
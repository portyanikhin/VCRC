﻿using System;
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
    public class TestVCRCWithRecuperator
    {
        private VCRCWithRecuperator Cycle { get; set; } = null!;

        [SetUp]
        public void SetUp()
        {
            const FluidsList refrigerantName = FluidsList.R407C;
            var evaporator = new Evaporator(refrigerantName, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
            var recuperator = new Recuperator(TemperatureDelta.FromKelvins(5));
            var compressor = new Compressor(80.Percent());
            var condenser = new Condenser(refrigerantName, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));
            Cycle = new VCRCWithRecuperator(evaporator, recuperator, compressor, condenser);
        }

        [Test]
        public void TestWrongTemperatureDifferenceAtRecuperatorHotSide()
        {
            Action action = () =>
                _ = new VCRCWithRecuperator(
                    new Evaporator(Cycle.RefrigerantName, Cycle.Evaporator.Temperature,
                        TemperatureDelta.FromKelvins(50)), Cycle.Recuperator, Cycle.Compressor, Cycle.Condenser);
            action.Should().Throw<ValidationException>()
                .WithMessage("*Wrong temperature difference at recuperator 'hot' side!*");
        }

        [Test]
        public void TestWrongTemperatureDifferenceAtRecuperatorColdSide()
        {
            Action action = () =>
                _ = new VCRCWithRecuperator(Cycle.Evaporator, Cycle.Recuperator, Cycle.Compressor,
                    new Condenser(Cycle.RefrigerantName, Cycle.Condenser.Temperature,
                        TemperatureDelta.FromKelvins(50)));
            action.Should().Throw<ValidationException>()
                .WithMessage("*Wrong temperature difference at recuperator 'cold' side!*");
        }

        [Test]
        public void TestZeroEvaporatorSuperheat()
        {
            var cycleZeroEvaporatorSuperheat =
                new VCRCWithRecuperator(
                    new Evaporator(Cycle.RefrigerantName, Cycle.Evaporator.Temperature, TemperatureDelta.Zero),
                    Cycle.Recuperator, Cycle.Compressor, Cycle.Condenser);
            cycleZeroEvaporatorSuperheat.Point0.Should().Be(cycleZeroEvaporatorSuperheat.Point1);
        }

        [Test]
        public void TestZeroRecuperatorSuperheat()
        {
            var cycleZeroRecuperatorSuperheat = new VCRCWithRecuperator(Cycle.Evaporator,
                new Recuperator(TemperatureDelta.Zero), Cycle.Compressor, Cycle.Condenser);
            cycleZeroRecuperatorSuperheat.Point1.Should().Be(cycleZeroRecuperatorSuperheat.Point2);
        }

        [Test]
        public void TestZeroSubcooling()
        {
            var cycleWithZeroSubcooling = new VCRCWithRecuperator(Cycle.Evaporator, Cycle.Recuperator, Cycle.Compressor,
                new Condenser(Cycle.RefrigerantName, Cycle.Condenser.Temperature, TemperatureDelta.Zero));
            cycleWithZeroSubcooling.Point5.Should().Be(cycleWithZeroSubcooling.Point6);
        }

        [Test]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void TestCompareEERWithSimpleVCRC() => Cycle.EER.Should()
            .BeGreaterThan(new SimpleVCRC(Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser).EER);

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
        public void TestPoint2()
        {
            Cycle.Point2.Pressure.Should().Be(Cycle.Evaporator.Pressure);
            Cycle.Point2.Temperature.Should().Be(Cycle.Point1.Temperature + Cycle.Recuperator.Superheat);
            Cycle.Point2.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void TestPoint3s()
        {
            Cycle.Point3s.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point3s.Entropy.Should().Be(Cycle.Point2.Entropy);
            Cycle.Point3s.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        public void TestPoint3()
        {
            Cycle.Point3.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point3.Enthalpy.Should().Be(Cycle.Point2.Enthalpy + Cycle.SpecificWork);
            Cycle.Point3.Phase.Should().Be(Phases.SupercriticalGas);
            Cycle.Point3.Temperature.Should().BeGreaterThan(Cycle.Point3.CriticalTemperature);
        }

        [Test]
        public void TestPoint4()
        {
            Cycle.Point4.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point4.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
            Cycle.Point4.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        public void TestPoint5()
        {
            Cycle.Point5.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point5.Quality.Should().Be(TwoPhase.Bubble.VaporQuality());
            Cycle.Point5.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        public void TestPoint6()
        {
            Cycle.Point6.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point6.Temperature.Should().Be(Cycle.Point5.Temperature - Cycle.Condenser.Subcooling);
            Cycle.Point6.Phase.Should().Be(Phases.Liquid);
        }

        [Test]
        public void TestPoint7()
        {
            Cycle.Point7.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point7.Enthalpy.Should().Be(Cycle.Point6.Enthalpy - (Cycle.Point2.Enthalpy - Cycle.Point1.Enthalpy));
            Cycle.Point7.Phase.Should().Be(Phases.Liquid);
        }

        [Test]
        public void TestPoint8()
        {
            Cycle.Point8.Pressure.Should().Be(Cycle.Evaporator.Pressure);
            Cycle.Point8.Enthalpy.Should().Be(Cycle.Point7.Enthalpy);
            Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
        }
    }
}
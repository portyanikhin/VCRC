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
    public class TestVCRCWithEconomizer
    {
        private VCRCWithEconomizer Cycle { get; set; } = null!;

        [SetUp]
        public void SetUp()
        {
            const FluidsList refrigerantName = FluidsList.R32;
            var evaporator = new Evaporator(refrigerantName, 5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
            var compressor = new Compressor(80.Percent());
            var condenser = new Condenser(refrigerantName, 50.DegreesCelsius(), TemperatureDelta.FromKelvins(3));
            var economizer = new Economizer(evaporator, condenser, TemperatureDelta.FromKelvins(5),
                TemperatureDelta.FromKelvins(5));
            Cycle = new VCRCWithEconomizer(evaporator, compressor, condenser, economizer);
        }

        [TestCase(Bound.Lower, "Intermediate pressure should be greater than evaporating pressure!")]
        [TestCase(Bound.Higher, "Intermediate pressure should be less than condensing pressure!")]
        public void TestWrongIntermediatePressure(Bound bound, string message)
        {
            Action action = () =>
                _ = new VCRCWithEconomizer(Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser,
                    new Economizer(bound is Bound.Lower
                            ? Cycle.Evaporator.Pressure
                            : Cycle.Condenser.Pressure,
                        Cycle.Economizer.TemperatureDifference,
                        Cycle.Economizer.Superheat));
            action.Should().Throw<ValidationException>().WithMessage($"*{message}*");
        }

        [Test]
        public void TestWrongEconomizerSuperheat()
        {
            Action action = () =>
                _ = new VCRCWithEconomizer(Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser,
                    new Economizer(Cycle.Economizer.Pressure, Cycle.Economizer.TemperatureDifference,
                        TemperatureDelta.FromKelvins(50)));
            action.Should().Throw<ValidationException>()
                .WithMessage("*Wrong temperature difference at economizer 'hot' side!*");
        }

        [Test]
        public void TestWrongEconomizerTemperatureDifference()
        {
            Action action = () =>
                _ = new VCRCWithEconomizer(Cycle.Evaporator, Cycle.Compressor, Cycle.Condenser,
                    new Economizer(Cycle.Economizer.Pressure, TemperatureDelta.FromKelvins(50),
                        Cycle.Economizer.Superheat));
            action.Should().Throw<ValidationException>()
                .WithMessage("*Too high temperature difference at economizer 'cold' side!*");
        }

        [Test]
        public void TestSpecificMassFlows()
        {
            Cycle.FirstStageSpecificMassFlow.Should().Be(100.Percent());
            Cycle.SecondStageSpecificMassFlow.Should()
                .Be(Cycle.FirstStageSpecificMassFlow * (1 + (Cycle.Point7.Enthalpy - Cycle.Point10.Enthalpy) /
                    (Cycle.Point9.Enthalpy - Cycle.Point8.Enthalpy)));
        }

        [Test]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void TestPoint2s()
        {
            Cycle.Point2s.Pressure.Should().Be(Cycle.Economizer.Pressure);
            Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
            Cycle.Point2s.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        public void TestPoint2()
        {
            Cycle.Point2.Pressure.Should().Be(Cycle.Economizer.Pressure);
            Cycle.Point2.Enthalpy.Should().Be(Cycle.Point1.Enthalpy + (Cycle.Point2s.Enthalpy - Cycle.Point1.Enthalpy) /
                Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
            Cycle.Point2.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        public void TestPoint3()
        {
            Cycle.Point3.Pressure.Should().Be(Cycle.Economizer.Pressure);
            Cycle.Point3.Enthalpy.Should().Be(
                (Cycle.FirstStageSpecificMassFlow.DecimalFractions * Cycle.Point2.Enthalpy +
                 (Cycle.SecondStageSpecificMassFlow - Cycle.FirstStageSpecificMassFlow).DecimalFractions *
                 Cycle.Point9.Enthalpy) / Cycle.SecondStageSpecificMassFlow.DecimalFractions);
            Cycle.Point3.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void TestPoint4s()
        {
            Cycle.Point4s.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point4s.Entropy.Should().Be(Cycle.Point3.Entropy);
            Cycle.Point4s.Phase.Should().Be(Phases.SupercriticalGas);
        }

        [Test]
        public void TestPoint4()
        {
            Cycle.Point4.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point4.Enthalpy.Should().Be(Cycle.Point3.Enthalpy + (Cycle.Point4s.Enthalpy - Cycle.Point3.Enthalpy) /
                Cycle.Compressor.IsentropicEfficiency.DecimalFractions);
            Cycle.Point4.Enthalpy.Should().BeGreaterThan(Cycle.Point4s.Enthalpy);
            Cycle.Point4.Phase.Should().Be(Phases.SupercriticalGas);
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
            Cycle.Point8.Pressure.Should().Be(Cycle.Economizer.Pressure);
            Cycle.Point8.Enthalpy.Should().Be(Cycle.Point7.Enthalpy);
            Cycle.Point8.Phase.Should().Be(Phases.TwoPhase);
        }

        [Test]
        public void TestPoint9()
        {
            Cycle.Point9.Pressure.Should().Be(Cycle.Economizer.Pressure);
            Cycle.Point9.Temperature.Should()
                .Be(Cycle.Point9.WithState(Input.Pressure(Cycle.Economizer.Pressure),
                    Input.Quality(TwoPhase.Dew.VaporQuality())).Temperature + Cycle.Economizer.Superheat);
            Cycle.Point9.Phase.Should().Be(Phases.Gas);
        }

        [Test]
        public void TestPoint10()
        {
            Cycle.Point10.Pressure.Should().Be(Cycle.Condenser.Pressure);
            Cycle.Point10.Temperature.Should().Be(Cycle.Point8.Temperature + Cycle.Economizer.TemperatureDifference);
            Cycle.Point10.Phase.Should().Be(Phases.Liquid);
        }

        [Test]
        public void TestPoint11()
        {
            Cycle.Point11.Pressure.Should().Be(Cycle.Evaporator.Pressure);
            Cycle.Point11.Enthalpy.Should().Be(Cycle.Point10.Enthalpy);
            Cycle.Point11.Phase.Should().Be(Phases.TwoPhase);
        }
    }
}
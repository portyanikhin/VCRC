// using System;
// using System.Diagnostics.CodeAnalysis;
// using System.Globalization;
// using FluentAssertions;
// using FluentValidation;
// using NUnit.Framework;
// using SharpProp;
// using UnitsNet;
// using UnitsNet.NumberExtensions.NumberToRatio;
// using UnitsNet.NumberExtensions.NumberToTemperature;
// using VCRC.Components;
// using VCRC.Extensions;
//
// namespace VCRC.Tests
// {
//     public class TestSimpleVCRC
//     {
//         private SimpleVCRC Cycle { get; set; } = null!;
//
//         [SetUp]
//         public void SetUp()
//         {
//             Cycle = new SimpleVCRC(FluidsList.R407C, 5.DegreesCelsius(), 50.DegreesCelsius(),
//                 TemperatureDelta.FromKelvins(8), TemperatureDelta.FromKelvins(3), 80.Percent());
//         }
//
//         [TestCase(-74, 50, 8, 3, 80, "Evaporating temperature should be in (-73.15;86.2) °C!")]
//         [TestCase(87, 50, 8, 3, 80, "Evaporating temperature should be in (-73.15;86.2) °C!")]
//         [TestCase(5, -5, 8, 3, 80, "Condensing temperature should be greater than evaporating temperature!")]
//         [TestCase(5, 87, 8, 3, 80, "Condensing temperature should be in (-73.15;86.2) °C!")]
//         [TestCase(5, 50, -1, 3, 80, "Superheat in the evaporator should be in [0;50] K!")]
//         [TestCase(5, 50, 51, 3, 80, "Superheat in the evaporator should be in [0;50] K!")]
//         [TestCase(5, 50, 8, -1, 80, "Subcooling in the condenser should be in [0;50] K!")]
//         [TestCase(5, 50, 8, 51, 80, "Subcooling in the condenser should be in [0;50] K!")]
//         [TestCase(5, 50, 8, 3, 0, "Isentropic efficiency of the compressor should be in (0;100) %!")]
//         [TestCase(5, 50, 8, 3, 100, "Isentropic efficiency of the compressor should be in (0;100) %!")]
//         public void TestInitThrows(double evaporatingTemperature, double condensingTemperature, double superheat,
//             double subcooling, double isentropicEfficiency, string message)
//         {
//             CultureInfo.CurrentCulture = new CultureInfo("en-US");
//             Action action = () => _ = new SimpleVCRC(Cycle.RefrigerantName, evaporatingTemperature.DegreesCelsius(),
//                 condensingTemperature.DegreesCelsius(), TemperatureDelta.FromKelvins(superheat),
//                 TemperatureDelta.FromKelvins(subcooling), isentropicEfficiency.Percent());
//             action.Should().Throw<ValidationException>().WithMessage($"*{message}*");
//         }
//
//         [Test]
//         public void TestPoint0()
//         {
//             Cycle.Point0.Pressure.Should().Be(Cycle.EvaporatingPressure);
//             Cycle.Point0.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
//         }
//
//         [Test]
//         public void TestPoint1()
//         {
//             Cycle.Point1.Pressure.Should().Be(Cycle.EvaporatingPressure);
//             Cycle.Point1.Temperature.Should().Be(Cycle.Point0.Temperature + Cycle.Superheat);
//         }
//
//         [Test]
//         [SuppressMessage("ReSharper", "InconsistentNaming")]
//         public void TestPoint2s()
//         {
//             Cycle.Point2s.Pressure.Should().Be(Cycle.CondensingPressure);
//             Cycle.Point2s.Entropy.Should().Be(Cycle.Point1.Entropy);
//         }
//
//         [Test]
//         public void TestPoint2()
//         {
//             Cycle.Point2.Pressure.Should().Be(Cycle.CondensingPressure);
//             Cycle.Point2.Enthalpy.Should().Be(Cycle.Point1.Enthalpy + Cycle.SpecificWork);
//         }
//
//         [Test]
//         public void TestPoint3()
//         {
//             Cycle.Point3.Pressure.Should().Be(Cycle.CondensingPressure);
//             Cycle.Point3.Quality.Should().Be(TwoPhase.Dew.VaporQuality());
//         }
//
//         [Test]
//         public void TestPoint4()
//         {
//             Cycle.Point4.Pressure.Should().Be(Cycle.CondensingPressure);
//             Cycle.Point4.Quality.Should().Be(TwoPhase.Bubble.VaporQuality());
//         }
//
//         [Test]
//         public void TestPoint5()
//         {
//             Cycle.Point5.Pressure.Should().Be(Cycle.CondensingPressure);
//             Cycle.Point5.Temperature.Should().Be(Cycle.Point4.Temperature - Cycle.Subcooling);
//         }
//
//         [Test]
//         public void TestPoint6()
//         {
//             Cycle.Point6.Pressure.Should().Be(Cycle.EvaporatingPressure);
//             Cycle.Point6.Enthalpy.Should().Be(Cycle.Point5.Enthalpy);
//         }
//
//         [Test]
//         public void TestIsentropicSpecificWork() =>
//             Cycle.IsentropicSpecificWork.Should().Be(Cycle.IsentropicEfficiency.DecimalFractions * Cycle.SpecificWork);
//
//         [Test]
//         [SuppressMessage("ReSharper", "InconsistentNaming")]
//         public void TestEER() => Cycle.EER.Should().Be(Cycle.SpecificCoolingCapacity / Cycle.SpecificWork);
//
//         [Test]
//         [SuppressMessage("ReSharper", "InconsistentNaming")]
//         public void TestCOP() => Cycle.COP.Should().Be(Cycle.SpecificHeatingCapacity / Cycle.SpecificWork);
//
//         [Test]
//         public void TestZeroSuperheat()
//         {
//             var cycleWithZeroSuperheat = new SimpleVCRC(Cycle.RefrigerantName, Cycle.EvaporatingTemperature,
//                 Cycle.CondensingTemperature, TemperatureDelta.Zero, Cycle.Subcooling, Cycle.IsentropicEfficiency);
//             cycleWithZeroSuperheat.Point0.Should().Be(cycleWithZeroSuperheat.Point1);
//         }
//
//         [Test]
//         public void TestZeroSubcooling()
//         {
//             var cycleWithZeroSubcooling = new SimpleVCRC(Cycle.RefrigerantName, Cycle.EvaporatingTemperature,
//                 Cycle.CondensingTemperature, Cycle.Superheat, TemperatureDelta.Zero, Cycle.IsentropicEfficiency);
//             cycleWithZeroSubcooling.Point4.Should().Be(cycleWithZeroSubcooling.Point5);
//         }
//
//         [Test]
//         public void TestPressureDefinition()
//         {
//             var cycleWithOtherPressureDefinitions = new SimpleVCRC(Cycle.RefrigerantName, Cycle.EvaporatingTemperature,
//                 Cycle.CondensingTemperature, Cycle.Superheat, Cycle.Subcooling, Cycle.IsentropicEfficiency,
//                 TwoPhase.Bubble, TwoPhase.Dew);
//             var refrigerant = new Refrigerant(Cycle.RefrigerantName);
//             cycleWithOtherPressureDefinitions.EvaporatingPressureDefinition.Should().Be(TwoPhase.Bubble);
//             cycleWithOtherPressureDefinitions.EvaporatingPressure.Should().Be(refrigerant
//                 .WithState(Input.Temperature(Cycle.EvaporatingTemperature),
//                     Input.Quality(TwoPhase.Bubble.VaporQuality())).Pressure);
//             cycleWithOtherPressureDefinitions.CondensingPressureDefinition.Should().Be(TwoPhase.Dew);
//             cycleWithOtherPressureDefinitions.CondensingPressure.Should().Be(refrigerant
//                 .WithState(Input.Temperature(Cycle.CondensingTemperature),
//                     Input.Quality(TwoPhase.Dew.VaporQuality())).Pressure);
//         }
//     }
// }


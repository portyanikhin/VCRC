using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Abstract;
using VCRC.Components;
using VCRC.Extensions;
using VCRC.Subcritical;

namespace VCRC.Tests.Extensions;

public static class TestEntropyAnalysisExtensions
{
    private static readonly List<Temperature>
        Indoor = Enumerable.Range(18, 5).Select(i => i.DegreesCelsius()).ToList();

    private static readonly List<Temperature>
        Outdoor = Enumerable.Range(36, 5).Select(i => i.DegreesCelsius()).ToList();

    private static readonly List<IEntropyAnalysable>
        Cycles = Enumerable.Range(0, Indoor.Count).Select(i => CreateVCRC(Indoor[i], Outdoor[i])).ToList();

    private static readonly List<Temperature>
        IndoorExtended = Enumerable.Range(18, 6).Select(i => i.DegreesCelsius()).ToList();

    [Test]
    public static void TestEntropyAnalysisInRange() =>
        Cycles.EntropyAnalysis(Indoor, Outdoor).Should().Be(
            Cycles.Select((c, i) => c.EntropyAnalysis(Indoor[i], Outdoor[i])).ToList().Average());

    [Test]
    public static void TestInvalidInput()
    {
        Action action = () => Cycles.EntropyAnalysis(IndoorExtended, Outdoor);
        action.Should().Throw<ArgumentException>()
            .WithMessage("The lists should have the same length!");
    }

    private static IEntropyAnalysable CreateVCRC(Temperature indoor, Temperature outdoor)
    {
        const FluidsList refrigerantName = FluidsList.R32;
        var evaporator = new Evaporator(refrigerantName, indoor - TemperatureDelta.FromKelvins(7),
            TemperatureDelta.FromKelvins(5));
        var compressor = new Compressor(80.Percent());
        var condenser = new Condenser(refrigerantName, outdoor + TemperatureDelta.FromKelvins(10),
            TemperatureDelta.FromKelvins(3));
        return new SimpleVCRC(evaporator, compressor, condenser);
    }
}
# ![VCRC](https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/header.svg)

[![Build & Tests](https://github.com/portyanikhin/VCRC/actions/workflows/build-tests.yml/badge.svg)](https://github.com/portyanikhin/VCRC/actions/workflows/build-tests.yml)
[![CodeQL](https://github.com/portyanikhin/VCRC/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/portyanikhin/VCRC/actions/workflows/codeql-analysis.yml)
[![NuGet](https://img.shields.io/nuget/v/VCRC)](https://www.nuget.org/packages/VCRC)
![Platform](https://img.shields.io/badge/platform-win--64%20%7C%20linux--64-lightgrey)
[![License](https://img.shields.io/github/license/portyanikhin/VCRC)](https://github.com/portyanikhin/VCRC/blob/main/LICENSE)
[![codecov](https://codecov.io/gh/portyanikhin/VCRC/branch/main/graph/badge.svg?token=aJmrRHNQnS)](https://codecov.io/gh/portyanikhin/VCRC)

Cross-platform vapor-compression refrigeration cycles analysis tool
using [SharpProp](https://github.com/portyanikhin/SharpProp).

## Overview

- [Unit safety](#unit-safety)
- [VCRC components](#vcrc-components)
    - [Evaporator](#evaporator)
    - [Compressor](#compressor)
    - [Condenser](#condenser)
    - [Recuperator](#recuperator)
    - [Intermediate vessel](#intermediate-vessel)
    - [Economizer](#economizer)
    - [EconomizerTPI](#economizertpi)
- [Subcritical VCRCs](#subcritical-vcrcs)
    - [Simple single-stage VCRC](#simple-single-stage-vcrc)
    - [Single-stage VCRC with recuperator](#single-stage-vcrc-with-recuperator)
    - [Two-stage VCRC with incomplete intercooling](#two-stage-vcrc-with-incomplete-intercooling)
    - [Two-stage VCRC with complete intercooling](#two-stage-vcrc-with-complete-intercooling)
    - [Two-stage VCRC with economizer](#two-stage-vcrc-with-economizer)
    - [Two-stage VCRC with economizer and two-phase injection to the compressor](#two-stage-vcrc-with-economizer-and-two-phase-injection-to-the-compressor)
- [Entropy analysis](#entropy-analysis)

## Unit safety

All calculations are **_unit safe_** (thanks to [UnitsNet](https://github.com/angularsen/UnitsNet)).
This allows you to avoid errors associated with incorrect dimensions of quantities,
and will help you save a lot of time on their search and elimination.
In addition, you will be able to convert all values to many other dimensions without the slightest difficulty.

## VCRC components

To analyze the vapor-compression refrigeration cycle (VCRC), you first need to build it from individual components.

### Evaporator

For example:

- Refrigerant: _R407C_.
- Evaporating temperature: _5 °C_.
- Superheat: _8 K_.
- Definition of the evaporating pressure (bubble-point or dew-point): _bubble-point_ (by default _dew-point_).

```c#
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
using VCRC.Fluids;
```

```c#
var evaporator =
    new Evaporator(FluidsList.R407C, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(8), TwoPhase.Bubble);
```

### Compressor

Compressor with _80 %_ isentropic efficiency:

```c#
using UnitsNet.NumberExtensions.NumberToRatio;
using VCRC.Components;
```

```c#
var compressor = new Compressor((80).Percent());
```

### Condenser

For example:

- Refrigerant: _R407C_.
- Condensing temperature: _40 °C_.
- Subcooling: _3 K_.
- Definition of the condensing pressure (bubble-point or dew-point): _dew-point_ (by default _bubble-point_).

```c#
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
using VCRC.Fluids;
```

```c#
var condenser =
    new Condenser(FluidsList.R407C, (40).DegreesCelsius(), TemperatureDelta.FromKelvins(3), TwoPhase.Dew);
```

### Recuperator

Recuperator with _5 K_ superheat:

```c#
using UnitsNet;
using VCRC.Components;
```

```c#
var recuperator = new Recuperator(TemperatureDelta.FromKelvins(5));
```

### Intermediate vessel

You can define an intermediate vessel with a fixed intermediate pressure or with an evaporator and condenser.
In the latter case, the intermediate pressure is calculated as the square root of the product
of the evaporating pressure and the condensing pressure.

Intermediate vessel with fixed _1 bar_ absolute pressure:

```c#
using UnitsNet.NumberExtensions.NumberToPressure;
using VCRC.Components;
```

```c#
var intermediateVessel = new IntermediateVessel((1).Bars());
```

Intermediate vessel defined with evaporator and condenser:

```c#
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
```

```c#
var evaporator =
    new Evaporator(FluidsList.R407C, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(8));
var condenser =
    new Condenser(FluidsList.R407C, (40).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var intermediateVessel = new IntermediateVessel(evaporator, condenser);
```

### Economizer

For example:

- Absolute intermediate pressure: _1 bar_.
- Temperature difference at economizer "cold" side: _7 K_.
- Superheat: _5 K_.

```c#
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToPressure;
using VCRC.Components;
```

```c#
var economizer = 
    new Economizer((1).Bars(), TemperatureDelta.FromKelvins(7), TemperatureDelta.FromKelvins(5));
```

As with the intermediate vessel, you can determine the intermediate pressure using the evaporator and condenser:

```c#
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC.Components;
```

```c#
var evaporator =
    new Evaporator(FluidsList.R407C, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(8));
var condenser =
    new Condenser(FluidsList.R407C, (40).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var economizer = 
    new Economizer(evaporator, condenser, TemperatureDelta.FromKelvins(7), TemperatureDelta.FromKelvins(5));
```

### EconomizerTPI

This is a complete analog of the [Economizer](#economizer), but without superheat.

## Subcritical VCRCs

### Simple single-stage VCRC

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER) and
the coefficient of performance (aka heating coefficient, aka COP):

```c#
using System;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC;
using VCRC.Components;
```

```c#
var evaporator =
    new Evaporator(FluidsList.R32, (-25).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser =
    new Condenser(FluidsList.R32, (40).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var cycle = new SimpleVCRC(evaporator, compressor, condenser);
Console.WriteLine(cycle.EER); // 2.1851212290292206
Console.WriteLine(cycle.COP); // 3.1851212290292206
```

### Single-stage VCRC with recuperator

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER) and
the coefficient of performance (aka heating coefficient, aka COP):

```c#
using System;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC;
using VCRC.Components;
```

```c#
var evaporator =
    new Evaporator(FluidsList.R32, (-25).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser =
    new Condenser(FluidsList.R32, (40).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var recuperator = new Recuperator(TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithRecuperator(evaporator, recuperator, compressor, condenser);
Console.WriteLine(cycle.EER); // 2.170667134010685
Console.WriteLine(cycle.COP); // 3.1706671340106847
```

### Two-stage VCRC with incomplete intercooling

_Intermediate vessel with fixed pressure is optional.
By default, the intermediate pressure is calculated as the square root of the product
of the evaporating pressure and the condensing pressure._

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER) and
the coefficient of performance (aka heating coefficient, aka COP):

```c#
using System;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC;
using VCRC.Components;
```

```c#
var evaporator =
    new Evaporator(FluidsList.R32, (-25).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser =
    new Condenser(FluidsList.R32, (40).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var cycle = new VCRCWithIncompleteIntercooling(evaporator, compressor, condenser);
Console.WriteLine(cycle.EER); // 2.4122045253883138
Console.WriteLine(cycle.COP); // 3.412204525388314
```

### Two-stage VCRC with complete intercooling

_Intermediate vessel with fixed pressure is optional. 
By default, the intermediate pressure is calculated as the square root of the product 
of the evaporating pressure and the condensing pressure._

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER) and
the coefficient of performance (aka heating coefficient, aka COP):

```c#
using System;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC;
using VCRC.Components;
```

```c#
var evaporator =
    new Evaporator(FluidsList.R32, (-25).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser =
    new Condenser(FluidsList.R32, (40).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var cycle = new VCRCWithCompleteIntercooling(evaporator, compressor, condenser);
Console.WriteLine(cycle.EER); // 2.485885473340216
Console.WriteLine(cycle.COP); // 3.485885473340216
```

### Two-stage VCRC with economizer

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER) and
the coefficient of performance (aka heating coefficient, aka COP):

```c#
using System;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC;
using VCRC.Components;
```

```c#
var evaporator =
    new Evaporator(FluidsList.R32, (-25).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser =
    new Condenser(FluidsList.R32, (40).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var economizer =
    new Economizer(evaporator, condenser, TemperatureDelta.FromKelvins(7), TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEconomizer(evaporator, compressor, condenser, economizer);
Console.WriteLine(cycle.EER); // 2.359978191965046
Console.WriteLine(cycle.COP); // 3.359978191965046
```

### Two-stage VCRC with economizer and two-phase injection to the compressor

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER) and
the coefficient of performance (aka heating coefficient, aka COP):

```c#
using System;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC;
using VCRC.Components;
```

```c#
var evaporator =
    new Evaporator(FluidsList.R32, (-25).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser =
    new Condenser(FluidsList.R32, (40).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var economizer =
    new EconomizerTPI(evaporator, condenser, TemperatureDelta.FromKelvins(7));
var cycle = new VCRCWithEconomizerTPI(evaporator, compressor, condenser, economizer);
Console.WriteLine(cycle.EER); // 2.4347473905936
Console.WriteLine(cycle.COP); // 3.4347473905935995
```

## Entropy analysis

You can perform an entropy analysis of each VCRC mentioned earlier.
This analysis allows us to estimate with high accuracy the distribution of energy loss 
due to the nonequilibrium (irreversibility) of working processes in the refrigeration machine. 
Thanks to this, you can easily estimate the energy loss to compensate for the production of entropy 
in each part of the refrigeration cycle and make decisions that will help increase its efficiency.

For example, simple single-stage VCRC, _-18 °C_ indoor temperature, _30 °C_ outdoor temperature:

```c#
using System;
using SharpProp;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;
using UnitsNet.NumberExtensions.NumberToTemperature;
using VCRC;
using VCRC.Components;
```

```c#
var evaporator =
    new Evaporator(FluidsList.R32, (-25).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser =
    new Condenser(FluidsList.R32, (40).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var cycle = new SimpleVCRC(evaporator, compressor, condenser);
var result = cycle.EntropyAnalysis((-18).DegreesCelsius(), (30).DegreesCelsius());
Console.WriteLine(result.ThermodynamicPerfection);        // 41.11 %
Console.WriteLine(result.MinSpecificWorkRatio);           // 41.11 %
Console.WriteLine(result.CompressorEnergyLossRatio);      // 20 %
Console.WriteLine(result.CondenserEnergyLossRatio);       // 16.07 %
Console.WriteLine(result.ExpansionValvesEnergyLossRatio); // 15.55 %
Console.WriteLine(result.EvaporatorEnergyLossRatio);      // 7.27 %
Console.WriteLine(result.RecuperatorEnergyLossRatio);     // 0 %
Console.WriteLine(result.EconomizerEnergyLossRatio);      // 0 %
Console.WriteLine(result.MixingEnergyLossRatio);          // 0 %
Console.WriteLine(result.AnalysisRelativeError);          // 3.18e-14 %
```
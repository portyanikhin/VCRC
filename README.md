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
    - [Mitsubishi Zubadan VCRC](#mitsubishi-zubadan-vcrc)
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

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/SimpleVCRC.png" alt="Simple VCRC scheme" width="70%"/><br><br>
    <b><i>Temperature-entropy chart (T-s chart)</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/SimpleVCRC%20T-s%20chart.png" alt="Simple VCRC T-s chart" width="70%"/>
</p>

**_List of points:_**

- `Point0` - dew-point on the evaporating isobar.
- `Point1` - evaporator outlet / compression stage suction.
- `Point2s` - isentropic compression stage discharge.
- `Point2` - compression stage discharge / condenser inlet.
- `Point3` - dew-point on the condensing isobar.
- `Point4` - bubble-point on the condensing isobar.
- `Point5` - condenser outlet / EV inlet.
- `Point6` - EV outlet / evaporator inlet.

**_Example:_**

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperature:

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
Console.WriteLine(cycle.EER);                // 2.1851212290292206
Console.WriteLine(cycle.COP);                // 3.1851212290292206
Console.WriteLine(cycle.Point2.Temperature); // 123.71 °C
```

### Single-stage VCRC with recuperator

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCWithRecuperator.png" alt="VCRC with recuperator scheme" width="70%"/><br><br>
    <b><i>Temperature-entropy chart (T-s chart)</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCWithRecuperator%20T-s%20chart.png" alt="VCRC with recuperator T-s chart" width="70%"/>
</p>

**_List of points:_**

- `Point0` - dew-point on the evaporating isobar.
- `Point1` - evaporator outlet / recuperator "cold" inlet.
- `Point2` - recuperator "cold" outlet / compression stage suction.
- `Point3s` - isentropic compression stage discharge.
- `Point3` - compression stage discharge / condenser inlet.
- `Point4` - dew-point on the condensing isobar.
- `Point5` - bubble-point on the condensing isobar.
- `Point6` - condenser outlet / recuperator "hot" inlet.
- `Point7` - recuperator "hot" outlet / EV inlet.
- `Point8` - EV outlet / evaporator inlet.

**_Example:_**

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperature:

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
Console.WriteLine(cycle.EER);                // 2.170667134010685
Console.WriteLine(cycle.COP);                // 3.1706671340106847
Console.WriteLine(cycle.Point3.Temperature); // 130.49 °C
```

### Two-stage VCRC with incomplete intercooling

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCWithIncompleteIntercooling.png" alt="VCRC with incomplete intercooling scheme" width="70%"/><br><br>
    <b><i>Temperature-entropy chart (T-s chart)</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCWithIncompleteIntercooling%20T-s%20chart.png" alt="VCRC with incomplete intercooling T-s chart" width="70%"/>
</p>

**_List of points:_**

- `Point0` - dew-point on the evaporating isobar.
- `Point1` - evaporator outlet / first compression stage suction.
- `Point2s` - first isentropic compression stage discharge.
- `Point2` - first compression stage discharge.
- `Point3` - second compression stage suction.
- `Point4s` - second isentropic compression stage discharge.
- `Point4` - second compression stage discharge / condenser inlet.
- `Point5` - dew-point on the condensing isobar.
- `Point6` - bubble-point on the condensing isobar.
- `Point7` - condenser outlet / first EV inlet.
- `Point8` - first EV outlet / intermediate vessel inlet.
- `Point9` - intermediate vessel vapor outlet / injection of cooled vapor into the compressor.
- `Point10` - intermediate vessel liquid outlet / second EV inlet.
- `Point11` - second EV outlet / evaporator inlet.

**_NB:_**

_Intermediate vessel with fixed pressure is optional.
By default, the intermediate pressure is calculated as the square root of the product
of the evaporating pressure and the condensing pressure._

**_Example:_**

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperature:

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
Console.WriteLine(cycle.EER);                // 2.4122045253883138
Console.WriteLine(cycle.COP);                // 3.412204525388314
Console.WriteLine(cycle.Point4.Temperature); // 115.35 °C
```

### Two-stage VCRC with complete intercooling

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCWithCompleteIntercooling.png" alt="VCRC with complete intercooling scheme" width="70%"/><br><br>
    <b><i>Temperature-entropy chart (T-s chart)</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCWithCompleteIntercooling%20T-s%20chart.png" alt="VCRC with complete intercooling T-s chart" width="70%"/>
</p>

**_List of points:_**

- `Point0` - dew-point on the evaporating isobar.
- `Point1` - evaporator outlet / first compression stage suction.
- `Point2s` - first isentropic compression stage discharge.
- `Point2` - first compression stage discharge.
- `Point3` - intermediate vessel vapor outlet / second compression stage suction.
- `Point4s` - second isentropic compression stage discharge.
- `Point4` - second compression stage discharge / condenser inlet.
- `Point5` - dew-point on the condensing isobar.
- `Point6` - bubble-point on the condensing isobar.
- `Point7` - condenser outlet / first EV inlet.
- `Point8` - first EV outlet / intermediate vessel inlet.
- `Point9` - intermediate vessel liquid outlet / second EV inlet.
- `Point10` - second EV outlet / evaporator inlet.

**_NB:_**

_Intermediate vessel with fixed pressure is optional. 
By default, the intermediate pressure is calculated as the square root of the product 
of the evaporating pressure and the condensing pressure._

**_Example:_**

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperature:

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
Console.WriteLine(cycle.EER);                // 2.485885473340216
Console.WriteLine(cycle.COP);                // 3.485885473340216
Console.WriteLine(cycle.Point4.Temperature); // 74.77 °C
```

### Two-stage VCRC with economizer

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCWithEconomizer.png" alt="VCRC with economizer scheme" width="70%"/><br><br>
    <b><i>Temperature-entropy chart (T-s chart)</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCWithEconomizer%20T-s%20chart.png" alt="VCRC with economizer T-s chart" width="70%"/>
</p>

**_List of points:_**

- `Point0` - dew-point on the evaporating isobar.
- `Point1` - evaporator outlet / first compression stage suction.
- `Point2s` - first isentropic compression stage discharge.
- `Point2` - first compression stage discharge.
- `Point3` - second compression stage suction.
- `Point4s` - second isentropic compression stage discharge.
- `Point4` - second compression stage discharge / condenser inlet.
- `Point5` - dew-point on the condensing isobar.
- `Point6` - bubble-point on the condensing isobar.
- `Point7` - condenser outlet / first EV inlet / economizer "hot" inlet.
- `Point8` - first EV outlet / economizer "cold" inlet.
- `Point 9` - economizer "cold" outlet / injection of cooled vapor into the compressor.
- `Point10` - economizer "hot" outlet / second EV inlet.
- `Point11` - second EV outlet / evaporator inlet.

**_Example:_**

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperature:

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
    new Economizer(evaporator, condenser, TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEconomizer(evaporator, compressor, condenser, economizer);
Console.WriteLine(cycle.EER);                // 2.374585213330844
Console.WriteLine(cycle.COP);                // 3.3745852133308443
Console.WriteLine(cycle.Point4.Temperature); // 117.97 °C
```

### Two-stage VCRC with economizer and two-phase injection to the compressor

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCWithEconomizerTPI.png" alt="VCRC with economizer and two-phase injection to the compressor scheme" width="70%"/><br><br>
    <b><i>Temperature-entropy chart (T-s chart)</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCWithEconomizerTPI%20T-s%20chart.png" alt="VCRC with economizer and two-phase injection to the compressor T-s chart" width="70%"/>
</p>

**_List of points:_**

- `Point0` - dew-point on the evaporating isobar.
- `Point1` - evaporator outlet / first compression stage suction.
- `Point2s` - first isentropic compression stage discharge.
- `Point2` - first compression stage discharge.
- `Point3` - second compression stage suction.
- `Point4s` - second isentropic compression stage discharge.
- `Point4` - second compression stage discharge / condenser inlet.
- `Point5` - dew-point on the condensing isobar.
- `Point6` - bubble-point on the condensing isobar.
- `Point7` - condenser outlet / first EV inlet / economizer "hot" inlet.
- `Point8` - first EV outlet / economizer "cold" inlet.
- `Point 9` - economizer "cold" outlet / injection of two-phase refrigerant into the compressor.
- `Point10` - economizer "hot" outlet / second EV inlet.
- `Point11` - second EV outlet / evaporator inlet.

**_Example:_**

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperature:

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
    new EconomizerTPI(evaporator, condenser, TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEconomizerTPI(evaporator, compressor, condenser, economizer);
Console.WriteLine(cycle.EER);                // 2.449607553764133
Console.WriteLine(cycle.COP);                // 3.449607553764133
Console.WriteLine(cycle.Point4.Temperature); // 74.77 °C
```

### Mitsubishi Zubadan VCRC

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCMitsubishiZubadan.png" alt="Mitsubishi Zubadan VCRC scheme" width="70%"/><br><br>
    <b><i>Temperature-entropy chart (T-s chart)</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/VCRCMitsubishiZubadan%20T-s%20chart.png" alt="Mitsubishi Zubadan VCRC T-s chart" width="70%"/>
</p>

**_List of points:_**

- `Point0` - dew-point on the evaporating isobar.
- `Point1` - evaporator outlet / recuperator "cold" inlet.
- `Point2` - recuperator "cold" outlet / first compression stage suction.
- `Point 3s` - first isentropic compression stage discharge.
- `Point 3` - first compression stage discharge.
- `Point 4` - second compression stage suction.
- `Point 5s` - second isentropic compression stage discharge.
- `Point 5` - second compression stage discharge / condenser inlet.
- `Point 6` - dew-point on the condensing isobar.
- `Point 7` - bubble-point on the condensing isobar.
- `Point 8` - condenser outlet / first EV inlet.
- `Point 9` - first EV outlet / recuperator "hot" inlet.
- `Point 10` - recuperator "hot" outlet / second EV inlet / economizer "hot" inlet.
- `Point 11` - second EV outlet / economizer "cold" inlet.
- `Point 12` - economizer "cold" outlet / injection of two-phase refrigerant into the compressor.
- `Point 13` - economizer "hot" outlet / third EV inlet.
- `Point 14` - third EV outlet / evaporator inlet.

**_Example:_**

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperature:

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
var economizer =
    new EconomizerTPI(evaporator, condenser, TemperatureDelta.FromKelvins(5));
var cycle = new VCRCMitsubishiZubadan(evaporator, recuperator, compressor, condenser, economizer);
Console.WriteLine(cycle.EER);                // 2.416508981309997
Console.WriteLine(cycle.COP);                // 3.4165089804636946
Console.WriteLine(cycle.Point5.Temperature); // 74.77 °C
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

In addition, you can perform entropy analysis in the range of indoor and outdoor temperatures ([see example](https://github.com/portyanikhin/VCRC/blob/main/VCRC.Tests/Extensions/TestEntropyAnalysisExtensions.cs)).
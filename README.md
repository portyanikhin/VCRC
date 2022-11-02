# ![VCRC](https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/header.png)

[![Build & Tests](https://github.com/portyanikhin/VCRC/actions/workflows/build-tests.yml/badge.svg)](https://github.com/portyanikhin/VCRC/actions/workflows/build-tests.yml)
[![CodeQL](https://github.com/portyanikhin/VCRC/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/portyanikhin/VCRC/actions/workflows/codeql-analysis.yml)
[![NuGet](https://img.shields.io/nuget/v/VCRC)](https://www.nuget.org/packages/VCRC)
![Platform](https://img.shields.io/badge/platform-win--64%20%7C%20linux--64-lightgrey)
[![License](https://img.shields.io/github/license/portyanikhin/VCRC)](https://github.com/portyanikhin/VCRC/blob/main/LICENSE)
[![codecov](https://codecov.io/gh/portyanikhin/VCRC/branch/main/graph/badge.svg?token=aJmrRHNQnS)](https://codecov.io/gh/portyanikhin/VCRC)

Cross-platform vapor-compression refrigeration cycles analysis tool using 
[SharpProp](https://github.com/portyanikhin/SharpProp).

## Navigation

- [How to install](#how-to-install)
- [Unit safety](#unit-safety)
- [VCRC components](#vcrc-components)
    - [Evaporator](#evaporator)
    - [Compressor](#compressor)
    - [Condenser (for subcritical VCRCs)](#condenser-for-subcritical-vcrcs)
    - [Gas cooler (for transcritical VCRCs)](#gas-cooler-for-transcritical-vcrcs)
    - [Ejector](#ejector)
    - [Recuperator](#recuperator)
    - [Economizer](#economizer)
    - [Economizer with two-phase injection into the compressor](#economizer-with-two-phase-injection-into-the-compressor)
- [Simple single-stage VCRC](#simple-single-stage-vcrc)
- [Single-stage VCRC with recuperator](#single-stage-vcrc-with-recuperator)
- [Two-stage VCRC with incomplete intercooling](#two-stage-vcrc-with-incomplete-intercooling)
- [Two-stage VCRC with complete intercooling](#two-stage-vcrc-with-complete-intercooling)
- [Two-stage VCRC with parallel compression](#two-stage-vcrc-with-parallel-compression)
- [Two-stage VCRC with economizer](#two-stage-vcrc-with-economizer)
- [Two-stage VCRC with economizer and parallel compression](#two-stage-vcrc-with-economizer-and-parallel-compression)
- [Two-stage VCRC with economizer and two-phase injection into the compressor](#two-stage-vcrc-with-economizer-and-two-phase-injection-into-the-compressor)
- [Single-stage VCRC with an ejector as an expansion device](#single-stage-vcrc-with-an-ejector-as-an-expansion-device)
- [Two-stage VCRC with an ejector as an expansion device and economizer](#two-stage-vcrc-with-an-ejector-as-an-expansion-device-and-economizer)
- [Two-stage VCRC with an ejector as an expansion device, economizer and parallel compression](#two-stage-vcrc-with-an-ejector-as-an-expansion-device-economizer-and-parallel-compression)
- [Two-stage VCRC with an ejector as an expansion device, economizer and two-phase injection into the compressor](#two-stage-vcrc-with-an-ejector-as-an-expansion-device-economizer-and-two-phase-injection-into-the-compressor)
- [Mitsubishi Zubadan VCRC (subcritical only)](#mitsubishi-zubadan-vcrc-subcritical-only)
- [Entropy analysis](#entropy-analysis)

## How to install

Run the following commands in the
[Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

```shell
PM> NuGet\Install-Package SharpProp -Version 4.3.2
```

```shell
PM> NuGet\Install-Package VCRC -Version 2.2.1
```

Or add this to the `.csproj` file:

```xml
<ItemGroup>
    <PackageReference Include="SharpProp" Version="4.3.2"/>
    <PackageReference Include="VCRC" Version="2.2.1"/>
</ItemGroup>
```

## Unit safety

All calculations are **_unit safe_** (thanks to [UnitsNet](https://github.com/angularsen/UnitsNet)).
This allows you to avoid errors associated with incorrect dimensions of quantities,
and will help you save a lot of time on their search and elimination.
In addition, you will be able to convert all values to many other dimensions without the slightest difficulty.

## VCRC components

To analyze the vapor-compression refrigeration cycle (VCRC), you first need to build it from individual components.

### Evaporator

For example:

- Refrigerant: _R32_.
- Evaporating temperature (dew point): _5 °C_.
- Superheat: _5 K_.

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
```

### Compressor

Compressor with _80 %_ isentropic efficiency:

```c#
var compressor = new Compressor((80).Percent());
```

### Condenser (for subcritical VCRCs)

For example:

- Refrigerant: _R32_.
- Condensing temperature (bubble point): _45 °C_.
- Subcooling: _3 K_.

```c#
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
```

### Gas cooler (for transcritical VCRCs)

For R744, the absolute pressure in the gas cooler is optional.
If this is not specified, then the optimal pressure will be calculated automatically
in accordance with this article:

_Yang L. et al. Minimizing COP loss from optional high pressure correlation for transcritical CO2 cycle //
Applied Thermal Engineering. – 2015. – V. 89. – P. 656-662._

For example:

- Refrigerant: _R744_.
- Outlet temperature: _40 °C_.
- Pressure (optional for R744): _105 bar_.

```c#
var gasCooler = new GasCooler(
    FluidsList.R744, (40).DegreesCelsius());
var gasCoolerWithSpecifiedPressure = new GasCooler(
    FluidsList.R744, (40).DegreesCelsius(), (105).Bars());
Console.WriteLine(gasCooler.Pressure.Bars);                      // 100.448
Console.WriteLine(gasCoolerWithSpecifiedPressure.Pressure.Bars); // 105
```

### Ejector

Ejector with _90 %_ isentropic efficiency of the nozzle, suction section and diffuser:

```csharp
var ejector = new Ejector((90).Percent());
```

Ejector with _90 %_ isentropic efficiency of the nozzle and suction section and 
_80 %_ isentropic efficiency of the diffuser:

```csharp
var ejector = new Ejector((90).Percent(), (90).Percent(), (80).Percent());
```

### Recuperator

Recuperator with _5 K_ temperature difference at "hot" side:

```c#
var recuperator = new Recuperator(TemperatureDelta.FromKelvins(5));
```

### Economizer

Economizer with _5 K_ temperature difference at "cold" side and _5 K_ superheat:

```c#
var economizer = new Economizer(
    TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));
```

### Economizer with two-phase injection into the compressor

Economizer with two-phase injection to the compressor and _5 K_ temperature difference at "cold" side:

```c#
var economizer = new EconomizerWithTPI(TemperatureDelta.FromKelvins(5));
```

## Simple single-stage VCRC

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/1.1 - SimpleVCRC.png" alt="SimpleVCRC scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/1.2 - SimpleVCRC.png" alt="Subcritical SimpleVCRC log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/1.3 - SimpleVCRC.png" alt="Transcritical SimpleVCRC log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - evaporator outlet / compression stage suction.
- `Point 2s` - isentropic compression stage discharge.
- `Point 2` - compression stage discharge / condenser or gas cooler inlet.
- `Point 3` - condenser or gas cooler outlet / EV inlet.
- `Point 4` - EV outlet / evaporator inlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperature:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var cycle = new SimpleVCRC(evaporator, compressor, condenser);
Console.WriteLine(cycle.EER);                // 4.348012113427724
Console.WriteLine(cycle.COP);                // 5.348012113427722
Console.WriteLine(cycle.Point2.Temperature); // 88.76 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var cycle = new SimpleVCRC(evaporator, compressor, gasCooler);
Console.WriteLine(cycle.EER);                // 2.607514554616747
Console.WriteLine(cycle.COP);                // 3.6075145546167464
Console.WriteLine(cycle.Point2.Temperature); // 88.36 °C
```

## Single-stage VCRC with recuperator

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/2.1 - VCRCWithRecuperator.png" alt="VCRCWithRecuperator scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/2.2 - VCRCWithRecuperator.png" alt="Subcritical VCRCWithRecuperator log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/2.3 - VCRCWithRecuperator.png" alt="Transcritical VCRCWithRecuperator log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - evaporator outlet / recuperator "cold" inlet.
- `Point 2` - recuperator "cold" outlet / compression stage suction.
- `Point 3s` - isentropic compression stage discharge.
- `Point 3` - compression stage discharge / condenser or gas cooler inlet.
- `Point 4` - condenser or gas cooler outlet / recuperator "hot" inlet.
- `Point 5` - recuperator "hot" outlet / EV inlet.
- `Point 6` - EV outlet / evaporator inlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperature:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var recuperator = new Recuperator(TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var cycle = new VCRCWithRecuperator(
    evaporator, recuperator, compressor, condenser);
Console.WriteLine(cycle.EER);                // 4.201006672315493
Console.WriteLine(cycle.COP);                // 5.201006672315493
Console.WriteLine(cycle.Point3.Temperature); // 120.68 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var recuperator = new Recuperator(TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var cycle = new VCRCWithRecuperator(
    evaporator, recuperator, compressor, gasCooler);
Console.WriteLine(cycle.EER);                // 2.711892365925208
Console.WriteLine(cycle.COP);                // 3.7118923659252077
Console.WriteLine(cycle.Point3.Temperature); // 120.88 °C
```

## Two-stage VCRC with incomplete intercooling

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/3.1 - VCRCWithIIC.png" alt="VCRCWithIIC scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/3.2 - VCRCWithIIC.png" alt="Subcritical VCRCWithIIC log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/3.3 - VCRCWithIIC.png" alt="Transcritical VCRCWithIIC log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - evaporator outlet / first compression stage suction.
- `Point 2s` - first isentropic compression stage discharge.
- `Point 2` - first compression stage discharge.
- `Point 3` - second compression stage suction.
- `Point 4s` - second isentropic compression stage discharge.
- `Point 4` - second compression stage discharge / condenser or gas cooler inlet.
- `Point 5` - condenser or gas cooler outlet / first EV inlet.
- `Point 6` - first EV outlet / separator inlet.
- `Point 7` - separator vapor outlet / injection of cooled vapor into the compressor.
- `Point 8` - separator liquid outlet / second EV inlet.
- `Point 9` - second EV outlet / evaporator inlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperatures:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var cycle = new VCRCWithIIC(evaporator, compressor, condenser);
Console.WriteLine(cycle.EER);                // 4.617699450573777
Console.WriteLine(cycle.COP);                // 5.617699450573777
Console.WriteLine(cycle.Point2.Temperature); // 47.65 °C
Console.WriteLine(cycle.Point4.Temperature); // 85.53 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var cycle = new VCRCWithIIC(evaporator, compressor, gasCooler);
Console.WriteLine(cycle.EER);                // 2.931496855532257
Console.WriteLine(cycle.COP);                // 3.9314968555322563
Console.WriteLine(cycle.Point2.Temperature); // 47.43 °C
Console.WriteLine(cycle.Point4.Temperature); // 75.83 °C
```

## Two-stage VCRC with complete intercooling

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/4.1 - VCRCWithCIC.png" alt="VCRCWithCIC scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/4.2 - VCRCWithCIC.png" alt="Subcritical VCRCWithCIC log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/4.3 - VCRCWithCIC.png" alt="Transcritical VCRCWithCIC log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - evaporator outlet / first compression stage suction.
- `Point 2s` - first isentropic compression stage discharge.
- `Point 2` - first compression stage discharge.
- `Point 3` - separator vapor outlet / second compression stage suction.
- `Point 4s` - second isentropic compression stage discharge.
- `Point 4` - second compression stage discharge / condenser or gas cooler inlet.
- `Point 5` - condenser or gas cooler outlet / first EV inlet.
- `Point 6` - first EV outlet / separator inlet.
- `Point 7` - separator liquid outlet / second EV inlet.
- `Point 8` - second EV outlet / evaporator inlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperatures:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var cycle = new VCRCWithCIC(evaporator, compressor, condenser);
Console.WriteLine(cycle.EER);                // 4.726096383578692
Console.WriteLine(cycle.COP);                // 5.7260963835786916
Console.WriteLine(cycle.Point2.Temperature); // 47.65 °C
Console.WriteLine(cycle.Point4.Temperature); // 62.48 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var cycle = new VCRCWithCIC(evaporator, compressor, gasCooler);
Console.WriteLine(cycle.EER);                // 2.759856520794663
Console.WriteLine(cycle.COP);                // 3.759856520794664
Console.WriteLine(cycle.Point2.Temperature); // 47.43 °C
Console.WriteLine(cycle.Point4.Temperature); // 56.91 °C
```

## Two-stage VCRC with parallel compression

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/5.1 - VCRCWithPC.png" alt="VCRCWithPC scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/5.2 - VCRCWithPC.png" alt="Subcritical VCRCWithPC log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/5.3 - VCRCWithPC.png" alt="Transcritical VCRCWithPC log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - evaporator outlet / first compression stage suction.
- `Point 2s` - first isentropic compression stage discharge.
- `Point 2` - first compression stage discharge.
- `Point 3` - separator vapor outlet / second compression stage suction.
- `Point 4s` - second isentropic compression stage discharge.
- `Point 4` - second compression stage discharge.
- `Point 5` - condenser or gas cooler inlet.
- `Point 6` - condenser or gas cooler outlet / first EV inlet.
- `Point 7` - first EV outlet / separator inlet.
- `Point 8` - separator liquid outlet / second EV inlet.
- `Point 9` - second EV outlet / evaporator inlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperatures:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var cycle = new VCRCWithPC(evaporator, compressor, condenser);
Console.WriteLine(cycle.EER);                // 4.678905539800198
Console.WriteLine(cycle.COP);                // 5.678905539800197
Console.WriteLine(cycle.Point2.Temperature); // 88.76 °C
Console.WriteLine(cycle.Point4.Temperature); // 62.48 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var cycle = new VCRCWithPC(evaporator, compressor, gasCooler);
Console.WriteLine(cycle.EER);                // 2.960053557904453
Console.WriteLine(cycle.COP);                // 3.960053557904454
Console.WriteLine(cycle.Point2.Temperature); // 88.36 °C
Console.WriteLine(cycle.Point4.Temperature); // 56.91 °C
```

## Two-stage VCRC with economizer

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/6.1 - VCRCWithEconomizer.png" alt="VCRCWithEconomizer scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/6.2 - VCRCWithEconomizer.png" alt="Subcritical VCRCWithEconomizer log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/6.3 - VCRCWithEconomizer.png" alt="Transcritical VCRCWithEconomizer log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - evaporator outlet / first compression stage suction.
- `Point 2s` - first isentropic compression stage discharge.
- `Point 2` - first compression stage discharge.
- `Point 3` - second compression stage suction.
- `Point 4s` - second isentropic compression stage discharge.
- `Point 4` - second compression stage discharge / condenser or gas cooler inlet.
- `Point 5` - condenser or gas cooler outlet / first EV inlet / economizer "hot" inlet.
- `Point 6` - first EV outlet / economizer "cold" inlet.
- `Point 7` - economizer "cold" outlet / injection of cooled vapor into the compressor.
- `Point 8` - economizer "hot" outlet / second EV inlet.
- `Point 9` - second EV outlet / evaporator inlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperatures:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var economizer = new Economizer(
    TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEconomizer(
    evaporator, compressor, condenser, economizer);
Console.WriteLine(cycle.EER);                // 4.53620654385269
Console.WriteLine(cycle.COP);                // 5.53620654385269
Console.WriteLine(cycle.Point2.Temperature); // 47.65 °C
Console.WriteLine(cycle.Point4.Temperature); // 87.14 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var economizer = new Economizer(
    TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEconomizer(
    evaporator, compressor, gasCooler, economizer);
Console.WriteLine(cycle.EER);                // 2.972706326019418
Console.WriteLine(cycle.COP);                // 3.972706326019418
Console.WriteLine(cycle.Point2.Temperature); // 47.43 °C
Console.WriteLine(cycle.Point4.Temperature); // 81.12 °C
```

## Two-stage VCRC with economizer and parallel compression

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/7.1 - VCRCWithEconomizerAndPC.png" alt="VCRCWithEconomizerAndPC scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/7.2 - VCRCWithEconomizerAndPC.png" alt="Subcritical VCRCWithEconomizerAndPC log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/7.3 - VCRCWithEconomizerAndPC.png" alt="Transcritical VCRCWithEconomizerAndPC log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - evaporator outlet / first compression stage suction.
- `Point 2s` - first isentropic compression stage discharge.
- `Point 2` - first compression stage discharge.
- `Point 3` - economizer "cold" outlet / second compression stage suction.
- `Point 4s` - second isentropic compression stage discharge.
- `Point 4` - second compression stage discharge.
- `Point 5` - condenser or gas cooler inlet.
- `Point 6` - condenser or gas cooler outlet / first EV inlet / economizer "hot" inlet.
- `Point 7` -  first EV outlet / economizer "cold" inlet.
- `Point 8` - economizer "hot" outlet / second EV inlet.
- `Point 9` - second EV outlet / evaporator inlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperatures:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var economizer = new Economizer(
    TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEconomizerAndPC(
    evaporator, compressor, condenser, economizer);
Console.WriteLine(cycle.EER);                // 4.596749553808057
Console.WriteLine(cycle.COP);                // 5.596749553808058
Console.WriteLine(cycle.Point2.Temperature); // 88.76 °C
Console.WriteLine(cycle.Point4.Temperature); // 68.27 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var economizer = new Economizer(
    TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEconomizerAndPC(
    evaporator, compressor, gasCooler, economizer);
Console.WriteLine(cycle.EER);                // 3.0072874874974658
Console.WriteLine(cycle.COP);                // 4.007287487497467
Console.WriteLine(cycle.Point2.Temperature); // 88.36 °C
Console.WriteLine(cycle.Point4.Temperature); // 65.56 °C
```

## Two-stage VCRC with economizer and two-phase injection into the compressor

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/8.1 - VCRCWithEconomizerAndTPI.png" alt="VCRCWithEconomizerAndTPI scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/8.2 - VCRCWithEconomizerAndTPI.png" alt="Subcritical VCRCWithEconomizerAndTPI log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/8.3 - VCRCWithEconomizerAndTPI.png" alt="Transcritical VCRCWithEconomizerAndTPI log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - evaporator outlet / first compression stage suction.
- `Point 2s` - first isentropic compression stage discharge.
- `Point 2` - first compression stage discharge.
- `Point 3` - second compression stage suction.
- `Point 4s` - second isentropic compression stage discharge.
- `Point 4` - second compression stage discharge / condenser or gas cooler inlet.
- `Point 5` - condenser or gas cooler outlet / first EV inlet / economizer "hot" inlet.
- `Point 6` - first EV outlet / economizer "cold" inlet.
- `Point 7` - economizer "cold" outlet / injection of two-phase refrigerant into the compressor.
- `Point 8` - economizer "hot" outlet / second EV inlet.
- `Point 9` - second EV outlet / evaporator inlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperatures:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var economizer = new Economizer(
    TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEconomizerAndTPI(
    evaporator, compressor, condenser, economizer);
Console.WriteLine(cycle.EER);                // 4.646833169057064
Console.WriteLine(cycle.COP);                // 5.646833169057064
Console.WriteLine(cycle.Point2.Temperature); // 47.65 °C
Console.WriteLine(cycle.Point4.Temperature); // 62.48 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var economizer = new Economizer(
    TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEconomizerAndTPI(
    evaporator, compressor, gasCooler, economizer);
Console.WriteLine(cycle.EER);                // 2.778725656175372
Console.WriteLine(cycle.COP);                // 3.778725656175373
Console.WriteLine(cycle.Point2.Temperature); // 47.43 °C
Console.WriteLine(cycle.Point4.Temperature); // 56.91 °C
```

## Single-stage VCRC with an ejector as an expansion device

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/9.1 - VCRCWithEjector.png" alt="VCRCWithEjector scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/9.2 - VCRCWithEjector.png" alt="Subcritical VCRCWithEjector log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/9.3 - VCRCWithEjector.png" alt="Transcritical VCRCWithEjector log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - separator vapor outlet / compression stage suction.
- `Point 2s` - isentropic compression stage discharge.
- `Point 2` - compression stage discharge / condenser or gas cooler inlet.
- `Point 3` - condenser or gas cooler outlet / ejector nozzle inlet.
- `Point 4` - ejector nozzle outlet.
- `Point 5` - ejector mixing section inlet.
- `Point 6` - ejector diffuser outlet / separator inlet.
- `Point 7` - separator liquid outlet / EV inlet.
- `Point 8` - EV outlet / evaporator inlet.
- `Point 9` - evaporator outlet / ejector suction section inlet.
- `Point 10` - ejector suction section outlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperature:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var ejector = new Ejector((90).Percent(), (90).Percent(), (80).Percent());
var cycle = new VCRCWithEjector(
    evaporator, compressor, condenser, ejector);
Console.WriteLine(cycle.EER);                // 4.832330373984365
Console.WriteLine(cycle.COP);                // 5.832251779509525
Console.WriteLine(cycle.Point2.Temperature); // 79.05 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var ejector = new Ejector((90).Percent(), (90).Percent(), (80).Percent());
var cycle = new VCRCWithEjector(
    evaporator, compressor, gasCooler, ejector);
Console.WriteLine(cycle.EER);                // 3.404191144711264
Console.WriteLine(cycle.COP);                // 4.404140144895948
Console.WriteLine(cycle.Point2.Temperature); // 69.77 °C
```

## Two-stage VCRC with an ejector as an expansion device and economizer

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/10.1 - VCRCWithEjectorAndEconomizer.png" alt="VCRCWithEjectorAndEconomizer scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/10.2 - VCRCWithEjectorAndEconomizer.png" alt="Subcritical VCRCWithEjectorAndEconomizer log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/10.3 - VCRCWithEjectorAndEconomizer.png" alt="Transcritical VCRCWithEjectorAndEconomizer log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - separator vapor outlet / first compression stage suction.
- `Point 2s` - first isentropic compression stage discharge.
- `Point 2` - first compression stage discharge.
- `Point 3` - second compression stage suction.
- `Point 4s` - second isentropic compression stage discharge.
- `Point 4` - second compression stage discharge / condenser or gas cooler inlet.
- `Point 5` - condenser or gas cooler outlet / first EV inlet / economizer "hot" inlet.
- `Point 6` - first EV outlet / economizer "cold" inlet.
- `Point 7` - economizer "cold" outlet / injection of cooled vapor into the compressor.
- `Point 8` - economizer "hot" outlet / ejector nozzle inlet.
- `Point 9` - ejector nozzle outlet.
- `Point 10` - ejector mixing section inlet.
- `Point 11` - ejector diffuser outlet / separator inlet.
- `Point 12` - separator liquid outlet / second EV inlet.
- `Point 13` - second EV outlet / evaporator inlet.
- `Point 14` - evaporator outlet / ejector suction section inlet.
- `Point 15` - ejector suction section outlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperatures:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var ejector = new Ejector((90).Percent(), (90).Percent(), (80).Percent());
var economizer = new Economizer(TemperatureDelta.FromKelvins(5),
    TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEjectorAndEconomizer(
    evaporator, compressor, condenser, ejector, economizer);
Console.WriteLine(cycle.EER);                // 4.780603519591169
Console.WriteLine(cycle.COP);                // 5.780519699589865
Console.WriteLine(cycle.Point2.Temperature); // 41.82 °C
Console.WriteLine(cycle.Point4.Temperature); // 79.97 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var ejector = new Ejector((90).Percent(), (90).Percent(), (80).Percent());
var economizer = new Economizer(TemperatureDelta.FromKelvins(5),
    TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEjectorAndEconomizer(
    evaporator, compressor, gasCooler, ejector, economizer);
Console.WriteLine(cycle.EER);                // 3.4970089179388735
Console.WriteLine(cycle.COP);                // 4.496949450753824
Console.WriteLine(cycle.Point2.Temperature); // 40.93 °C
Console.WriteLine(cycle.Point4.Temperature); // 70.26 °C
```

## Two-stage VCRC with an ejector as an expansion device, economizer and parallel compression

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/11.1 - VCRCWithEjectorEconomizerAndPC.png" alt="VCRCWithEjectorEconomizerAndPC scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/11.2 - VCRCWithEjectorEconomizerAndPC.png" alt="Subcritical VCRCWithEjectorEconomizerAndPC log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/11.3 - VCRCWithEjectorEconomizerAndPC.png" alt="Transcritical VCRCWithEjectorEconomizerAndPC log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - separator vapor outlet / first compression stage suction.
- `Point 2s` - first isentropic compression stage discharge.
- `Point 2` - first compression stage discharge.
- `Point 3` - economizer "cold" outlet / second compression stage suction.
- `Point 4s` - second isentropic compression stage discharge.
- `Point 4` - second compression stage discharge.
- `Point 5` - condenser or gas cooler inlet.
- `Point 6` - condenser or gas cooler outlet / first EV inlet / economizer "hot" inlet.
- `Point 7` - first EV outlet / economizer "cold" inlet.
- `Point 8` - economizer "hot" outlet / ejector nozzle inlet.
- `Point 9` - ejector nozzle outlet.
- `Point 10` - ejector mixing section inlet.
- `Point 11` - ejector diffuser outlet / separator inlet.
- `Point 12` - separator liquid outlet / second EV inlet.
- `Point 13` - second EV outlet / evaporator inlet.
- `Point 14` - evaporator outlet / ejector suction section inlet.
- `Point 15` - ejector suction section outlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperatures:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var ejector = new Ejector((90).Percent(), (90).Percent(), (80).Percent());
var economizer = new Economizer(TemperatureDelta.FromKelvins(5),
    TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEjectorEconomizerAndPC(
    evaporator, compressor, condenser, ejector, economizer);
Console.WriteLine(cycle.EER);                // 4.84250403965006
Console.WriteLine(cycle.COP);                // 5.8424191343251834
Console.WriteLine(cycle.Point2.Temperature); // 80.82 °C
Console.WriteLine(cycle.Point4.Temperature); // 67.52 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var ejector = new Ejector((90).Percent(), (90).Percent(), (80).Percent());
var economizer = new Economizer(TemperatureDelta.FromKelvins(5),
    TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEjectorEconomizerAndPC(
    evaporator, compressor, gasCooler, ejector, economizer);
Console.WriteLine(cycle.EER);                // 3.5310640266642546
Console.WriteLine(cycle.COP);                // 4.53100398036674
Console.WriteLine(cycle.Point2.Temperature); // 73.07 °C
Console.WriteLine(cycle.Point4.Temperature); // 62.08 °C
```

## Two-stage VCRC with an ejector as an expansion device, economizer and two-phase injection into the compressor

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/12.1 - VCRCWithEjectorEconomizerAndTPI.png" alt="VCRCWithEjectorEconomizerAndTPI scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/12.2 - VCRCWithEjectorEconomizerAndTPI.png" alt="Subcritical VCRCWithEjectorEconomizerAndTPI log P-h chart" width="75%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the transcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/12.3 - VCRCWithEjectorEconomizerAndTPI.png" alt="Transcritical VCRCWithEjectorEconomizerAndTPI log P-h chart" width="75%"/>
</p>

### List of points

- `Point 1` - separator vapor outlet / first compression stage suction.
- `Point 2s` - first isentropic compression stage discharge.
- `Point 2` - first compression stage discharge.
- `Point 3` - second compression stage suction.
- `Point 4s` - second isentropic compression stage discharge.
- `Point 4` - second compression stage discharge / condenser or gas cooler inlet.
- `Point 5` - condenser or gas cooler outlet / first EV inlet / economizer "hot" inlet.
- `Point 6` - first EV outlet / economizer "cold" inlet.
- `Point 7` - economizer "cold" outlet / injection of two-phase refrigerant into the compressor.
- `Point 8` - economizer "hot" outlet / ejector nozzle inlet.
- `Point 9` - ejector nozzle outlet.
- `Point 10` - ejector mixing section inlet.
- `Point 11` - ejector diffuser outlet / separator inlet.
- `Point 12` - separator liquid outlet / second EV inlet.
- `Point 13` - second EV outlet / evaporator inlet.
- `Point 14` - evaporator outlet / ejector suction section inlet.
- `Point 15` - ejector suction section outlet.

### Examples

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperatures:

**_For the subcritical cycle_**

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var ejector = new Ejector((90).Percent(), (90).Percent(), (80).Percent());
var economizer = new EconomizerWithTPI(TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEjectorEconomizerAndTPI(
    evaporator, compressor, condenser, ejector, economizer);
Console.WriteLine(cycle.EER);                // 4.874001351966284
Console.WriteLine(cycle.COP);                // 5.873917103015547
Console.WriteLine(cycle.Point2.Temperature); // 41.82 °C
Console.WriteLine(cycle.Point4.Temperature); // 61.76 °C
```

**_For the transcritical cycle_**

```csharp
var evaporator = new Evaporator(
    FluidsList.R744, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var gasCooler = new GasCooler(FluidsList.R744, (40).DegreesCelsius());
var ejector = new Ejector((90).Percent(), (90).Percent(), (80).Percent());
var economizer = new EconomizerWithTPI(TemperatureDelta.FromKelvins(5));
var cycle = new VCRCWithEjectorEconomizerAndTPI(
    evaporator, compressor, gasCooler, ejector, economizer);
Console.WriteLine(cycle.EER);                // 3.1791454424406136
Console.WriteLine(cycle.COP);                // 4.179085189888726
Console.WriteLine(cycle.Point2.Temperature); // 40.93 °C
Console.WriteLine(cycle.Point4.Temperature); // 52.59 °C
```

## Mitsubishi Zubadan VCRC (subcritical only)

<!--suppress HtmlDeprecatedAttribute -->
<p align="center">
    <b><i>Schematic diagram</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/13.1 - VCRCMitsubishiZubadan.png" alt="VCRCMitsubishiZubadan scheme" width="70%"/><br><br>
    <b><i>Pressure-enthalpy chart (log P-h chart) for the subcritical cycle</i></b><br><br>
    <img src="https://raw.githubusercontent.com/portyanikhin/VCRC/main/VCRC/pictures/13.2 - VCRCMitsubishiZubadan.png" alt="VCRCMitsubishiZubadan log P-h chart" width="75%"/>
</p>

**_List of points:_**

- `Point 1` - evaporator outlet / recuperator "cold" inlet.
- `Point 2` - recuperator "cold" outlet / first compression stage suction.
- `Point 3s` - first isentropic compression stage discharge.
- `Point 3` - first compression stage discharge.
- `Point 4` - second compression stage suction.
- `Point 5s` - second isentropic compression stage discharge.
- `Point 5` - second compression stage discharge / condenser inlet.
- `Point 6` - condenser outlet / first EV inlet.
- `Point 7` - first EV outlet / recuperator "hot" inlet.
- `Point 8` - recuperator "hot" outlet / second EV inlet / economizer "hot" inlet.
- `Point 9` - second EV outlet / economizer "cold" inlet.
- `Point 10` - economizer "cold" outlet / injection of two-phase refrigerant into the compressor.
- `Point 11` - economizer "hot" outlet / third EV inlet.
- `Point 12` - third EV outlet / evaporator inlet.

### Example

To calculate the energy efficiency ratio (aka cooling coefficient, aka EER),
the coefficient of performance (aka heating coefficient, aka COP) and the compressor discharge temperatures:

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var economizer = new EconomizerWithTPI(TemperatureDelta.FromKelvins(5));
var cycle = new VCRCMitsubishiZubadan(
    evaporator, compressor, condenser, economizer);
Console.WriteLine(cycle.EER);                // 4.392768334506883
Console.WriteLine(cycle.COP);                // 5.392751672812034
Console.WriteLine(cycle.Point3.Temperature); // 67.82 °C
Console.WriteLine(cycle.Point5.Temperature); // 62.48 °C
```

## Entropy analysis

You can perform an entropy analysis of each VCRC mentioned earlier.
This analysis allows us to estimate with high accuracy the distribution of energy loss 
due to the nonequilibrium (irreversibility) of working processes in the refrigeration machine. 
Thanks to this, you can easily estimate the energy loss to compensate for the production of entropy 
in each part of the refrigeration cycle and make decisions that will help increase its efficiency.

For example, simple single-stage VCRC, _18 °C_ indoor temperature, _35 °C_ outdoor temperature:

```c#
var evaporator = new Evaporator(
    FluidsList.R32, (5).DegreesCelsius(), TemperatureDelta.FromKelvins(5));
var compressor = new Compressor((80).Percent());
var condenser = new Condenser(
    FluidsList.R32, (45).DegreesCelsius(), TemperatureDelta.FromKelvins(3));
var cycle = new SimpleVCRC(evaporator, compressor, condenser);
var result = cycle.EntropyAnalysis((18).DegreesCelsius(), (35).DegreesCelsius());
Console.WriteLine(result.ThermodynamicPerfection);        // 25.39 %
Console.WriteLine(result.MinSpecificWorkRatio);           // 25.39 %
Console.WriteLine(result.CompressorEnergyLossRatio);      // 20 %
Console.WriteLine(result.CondenserEnergyLossRatio);       // 20.83 %
Console.WriteLine(result.GasCoolerEnergyLossRatio);       // 0 %
Console.WriteLine(result.ExpansionValvesEnergyLossRatio); // 12.39 %
Console.WriteLine(result.EjectorEnergyLossRatio);         // 0 %
Console.WriteLine(result.EvaporatorEnergyLossRatio);      // 21.4 %
Console.WriteLine(result.RecuperatorEnergyLossRatio);     // 0 %
Console.WriteLine(result.EconomizerEnergyLossRatio);      // 0 %
Console.WriteLine(result.MixingEnergyLossRatio);          // 0 %
Console.WriteLine(result.AnalysisRelativeError);          // 1.27e-13 %
```

In addition, you can perform entropy analysis in the range of indoor and outdoor temperatures ([see example](https://github.com/portyanikhin/VCRC/blob/main/VCRC.Tests/Extensions/TestEntropyAnalysisExtensions.cs)).
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC;

/// <summary>
///     VCRC working fluid.
/// </summary>
public class Refrigerant : Fluid
{
    /// <summary>
    ///     VCRC working fluid.
    /// </summary>
    /// <param name="name">Selected refrigerant name.</param>
    /// <exception cref="ValidationException">
    ///     The selected fluid is not a refrigerant
    ///     (its name should start with 'R')!
    /// </exception>
    public Refrigerant(FluidsList name)
        : base(name) => new RefrigerantValidator().ValidateAndThrow(this);

    /// <summary>
    ///     Absolute pressure at the critical point (by default, kPa).
    /// </summary>
    /// <exception cref="NullReferenceException">
    ///     Invalid critical pressure!
    /// </exception>
    public new Pressure CriticalPressure =>
        base.CriticalPressure
        ?? throw new NullReferenceException("Invalid critical pressure!");

    /// <summary>
    ///     Temperature at the critical point (by default, °C).
    /// </summary>
    /// <exception cref="NullReferenceException">
    ///     Invalid critical temperature!
    /// </exception>
    public new Temperature CriticalTemperature =>
        base.CriticalTemperature
        ?? throw new NullReferenceException("Invalid critical temperature!");

    /// <summary>
    ///     Temperature glide at atmospheric pressure (by default, K).
    /// </summary>
    public TemperatureDelta Glide =>
        (
            DewPointAt(1.Atmospheres()).Temperature
            - BubblePointAt(1.Atmospheres()).Temperature
        )
            .Abs()
            .ToUnit(TemperatureDeltaUnit.Kelvin);

    /// <summary>
    ///     True if the refrigerant has a temperature glide.
    /// </summary>
    public bool HasGlide => Glide > 0.01.Kelvins();

    /// <summary>
    ///     True if the refrigerant is a single component.
    /// </summary>
    public bool IsSingleComponent => !IsAzeotropicBlend && !IsZeotropicBlend;

    /// <summary>
    ///     True if the refrigerant is an azeotropic blend.
    /// </summary>
    public bool IsAzeotropicBlend => BlendRegex(false).IsMatch(Name.ToString());

    /// <summary>
    ///     True if the refrigerant is a zeotropic blend.
    /// </summary>
    public bool IsZeotropicBlend => BlendRegex(true).IsMatch(Name.ToString());

    /// <summary>
    ///     Absolute pressure at the triple point (by default, kPa).
    /// </summary>
    /// <exception cref="NullReferenceException">
    ///     Invalid triple pressure!
    /// </exception>
    public new Pressure TriplePressure =>
        base.TriplePressure
        ?? throw new NullReferenceException("Invalid triple pressure!");

    /// <summary>
    ///     Temperature at the triple point (by default, °C).
    /// </summary>
    /// <exception cref="NullReferenceException">
    ///     Invalid triple temperature!
    /// </exception>
    public new Temperature TripleTemperature =>
        base.TripleTemperature
        ?? throw new NullReferenceException("Invalid triple temperature!");

    /// <summary>
    ///     Subcooled refrigerant.
    /// </summary>
    /// <param name="bubblePointTemperature">Bubble point temperature.</param>
    /// <param name="subcooling">Subcooling.</param>
    /// <returns>Subcooled refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid subcooling!</exception>
    public Refrigerant Subcooled(
        Temperature bubblePointTemperature,
        TemperatureDelta subcooling
    ) =>
        subcooling < TemperatureDelta.Zero
            ? throw new ArgumentException("Invalid subcooling!")
            : subcooling.Equals(TemperatureDelta.Zero, Tolerance.Kelvins())
                ? BubblePointAt(bubblePointTemperature)
                : BubblePointAt(bubblePointTemperature)
                    .CoolingTo(bubblePointTemperature - subcooling);

    /// <summary>
    ///     Subcooled refrigerant.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="subcooling">Subcooling.</param>
    /// <returns>Subcooled refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid subcooling!</exception>
    public Refrigerant Subcooled(
        Pressure pressure,
        TemperatureDelta subcooling
    ) =>
        subcooling < TemperatureDelta.Zero
            ? throw new ArgumentException("Invalid subcooling!")
            : subcooling.Equals(TemperatureDelta.Zero, Tolerance.Kelvins())
                ? BubblePointAt(pressure)
                : BubblePointAt(pressure)
                    .CoolingTo(
                        BubblePointAt(pressure).Temperature - subcooling
                    );

    /// <summary>
    ///     Superheated refrigerant.
    /// </summary>
    /// <param name="dewPointTemperature">Dew point temperature.</param>
    /// <param name="superheat">Superheat.</param>
    /// <returns>Superheated refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid superheat!</exception>
    public Refrigerant Superheated(
        Temperature dewPointTemperature,
        TemperatureDelta superheat
    ) =>
        superheat < TemperatureDelta.Zero
            ? throw new ArgumentException("Invalid superheat!")
            : superheat.Equals(TemperatureDelta.Zero, Tolerance.Kelvins())
                ? DewPointAt(dewPointTemperature)
                : DewPointAt(dewPointTemperature)
                    .HeatingTo(dewPointTemperature + superheat);

    /// <summary>
    ///     Superheated refrigerant.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="superheat">Superheat.</param>
    /// <returns>Superheated refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid superheat!</exception>
    public Refrigerant Superheated(
        Pressure pressure,
        TemperatureDelta superheat
    ) =>
        superheat < TemperatureDelta.Zero
            ? throw new ArgumentException("Invalid superheat!")
            : superheat.Equals(TemperatureDelta.Zero, Tolerance.Kelvins())
                ? DewPointAt(pressure)
                : DewPointAt(pressure)
                    .HeatingTo(DewPointAt(pressure).Temperature + superheat);

    protected override AbstractFluid CreateInstance() => new Refrigerant(Name);

    /// <summary>
    ///     Performs deep (full) copy of the refrigerant instance.
    /// </summary>
    /// <returns>Deep copy of the refrigerant instance.</returns>
    public new Refrigerant Clone() => (Refrigerant)base.Clone();

    /// <summary>
    ///     Creates a new refrigerant instance with no defined state.
    /// </summary>
    /// <returns>A new refrigerant instance with no defined state.</returns>
    public new Refrigerant Factory() => new(Name);

    /// <summary>
    ///     Returns a new refrigerant instance with a defined state.
    /// </summary>
    /// <param name="firstInput">First input property.</param>
    /// <param name="secondInput">Second input property.</param>
    /// <returns>A new refrigerant instance with a defined state.</returns>
    /// <exception cref="ArgumentException">
    ///     Need to define 2 unique inputs!
    /// </exception>
    public new Refrigerant WithState(
        IKeyedInput<Parameters> firstInput,
        IKeyedInput<Parameters> secondInput
    ) => (Refrigerant)base.WithState(firstInput, secondInput);

    /// <summary>
    ///     The process of isentropic compression to a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <returns>The state of the fluid at the end of the process.</returns>
    /// <exception cref="ArgumentException">
    ///     Compressor outlet pressure should be higher than inlet pressure!
    /// </exception>
    public new Refrigerant IsentropicCompressionTo(Pressure pressure) =>
        (Refrigerant)base.IsentropicCompressionTo(pressure);

    /// <summary>
    ///     The process of compression to a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="isentropicEfficiency">
    ///     Compressor isentropic efficiency.
    /// </param>
    /// <returns>The state of the fluid at the end of the process.</returns>
    /// <exception cref="ArgumentException">
    ///     Compressor outlet pressure should be higher than inlet pressure!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid compressor isentropic efficiency!
    /// </exception>
    public new Refrigerant CompressionTo(
        Pressure pressure,
        Ratio isentropicEfficiency
    ) => (Refrigerant)base.CompressionTo(pressure, isentropicEfficiency);

    /// <summary>
    ///     The process of isenthalpic expansion to a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <returns>The state of the fluid at the end of the process.</returns>
    /// <exception cref="ArgumentException">
    ///     Expansion valve outlet pressure should be lower than inlet pressure!
    /// </exception>
    public new Refrigerant IsenthalpicExpansionTo(Pressure pressure) =>
        (Refrigerant)base.IsenthalpicExpansionTo(pressure);

    /// <summary>
    ///     The process of isentropic expansion to a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <returns>The state of the fluid at the end of the process.</returns>
    /// <exception cref="ArgumentException">
    ///     Expander outlet pressure should be lower than inlet pressure!
    /// </exception>
    public new Refrigerant IsentropicExpansionTo(Pressure pressure) =>
        (Refrigerant)base.IsentropicExpansionTo(pressure);

    /// <summary>
    ///     The process of expansion to a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="isentropicEfficiency">Expander isentropic efficiency.</param>
    /// <returns>The state of the fluid at the end of the process.</returns>
    /// <exception cref="ArgumentException">
    ///     Expander outlet pressure should be lower than inlet pressure!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid expander isentropic efficiency!
    /// </exception>
    public new Refrigerant ExpansionTo(
        Pressure pressure,
        Ratio isentropicEfficiency
    ) => (Refrigerant)base.ExpansionTo(pressure, isentropicEfficiency);

    /// <summary>
    ///     The process of cooling to a given temperature.
    /// </summary>
    /// <param name="temperature">Temperature.</param>
    /// <param name="pressureDrop">
    ///     Pressure drop in the heat exchanger (optional).
    /// </param>
    /// <returns>The state of the fluid at the end of the process.</returns>
    /// <exception cref="ArgumentException">
    ///     During the cooling process, the temperature should decrease!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid pressure drop in the heat exchanger!
    /// </exception>
    public new Refrigerant CoolingTo(
        Temperature temperature,
        Pressure? pressureDrop = null
    ) => (Refrigerant)base.CoolingTo(temperature, pressureDrop);

    /// <summary>
    ///     The process of cooling to a given enthalpy.
    /// </summary>
    /// <param name="enthalpy">Enthalpy.</param>
    /// <param name="pressureDrop">
    ///     Pressure drop in the heat exchanger (optional).
    /// </param>
    /// <returns>The state of the fluid at the end of the process.</returns>
    /// <exception cref="ArgumentException">
    ///     During the cooling process, the enthalpy should decrease!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid pressure drop in the heat exchanger!
    /// </exception>
    public new Refrigerant CoolingTo(
        SpecificEnergy enthalpy,
        Pressure? pressureDrop = null
    ) => (Refrigerant)base.CoolingTo(enthalpy, pressureDrop);

    /// <summary>
    ///     The process of heating to a given temperature.
    /// </summary>
    /// <param name="temperature">Temperature.</param>
    /// <param name="pressureDrop">
    ///     Pressure drop in the heat exchanger (optional).
    /// </param>
    /// <returns>The state of the fluid at the end of the process.</returns>
    /// <exception cref="ArgumentException">
    ///     During the heating process, the temperature should increase!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid pressure drop in the heat exchanger!
    /// </exception>
    public new Refrigerant HeatingTo(
        Temperature temperature,
        Pressure? pressureDrop = null
    ) => (Refrigerant)base.HeatingTo(temperature, pressureDrop);

    /// <summary>
    ///     The process of heating to a given enthalpy.
    /// </summary>
    /// <param name="enthalpy">Enthalpy.</param>
    /// <param name="pressureDrop">
    ///     Pressure drop in the heat exchanger (optional).
    /// </param>
    /// <returns>The state of the fluid at the end of the process.</returns>
    /// <exception cref="ArgumentException">
    ///     During the heating process, the enthalpy should increase!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid pressure drop in the heat exchanger!
    /// </exception>
    public new Refrigerant HeatingTo(
        SpecificEnergy enthalpy,
        Pressure? pressureDrop = null
    ) => (Refrigerant)base.HeatingTo(enthalpy, pressureDrop);

    /// <summary>
    ///     Returns a bubble point at a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <returns>A bubble point at a given pressure.</returns>
    public new Refrigerant BubblePointAt(Pressure pressure) =>
        (Refrigerant)base.BubblePointAt(pressure);

    /// <summary>
    ///     Returns a bubble point at a given temperature.
    /// </summary>
    /// <param name="temperature">Temperature.</param>
    /// <returns>A bubble point at a given temperature.</returns>
    public new Refrigerant BubblePointAt(Temperature temperature) =>
        (Refrigerant)base.BubblePointAt(temperature);

    /// <summary>
    ///     Returns a dew point at a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <returns>A dew point at a given pressure.</returns>
    public new Refrigerant DewPointAt(Pressure pressure) =>
        (Refrigerant)base.DewPointAt(pressure);

    /// <summary>
    ///     Returns a dew point at a given temperature.
    /// </summary>
    /// <param name="temperature">Temperature.</param>
    /// <returns>A dew point at a given temperature.</returns>
    public new Refrigerant DewPointAt(Temperature temperature) =>
        (Refrigerant)base.DewPointAt(temperature);

    /// <summary>
    ///     Returns a two-phase point at a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="quality">Vapor quality.</param>
    /// <returns>Two-phase point at a given pressure.</returns>
    public new Refrigerant TwoPhasePointAt(Pressure pressure, Ratio quality) =>
        (Refrigerant)base.TwoPhasePointAt(pressure, quality);

    /// <summary>
    ///     The mixing process.
    /// </summary>
    /// <param name="firstSpecificMassFlow">
    ///     Specific mass flow rate of the fluid at the first state.
    /// </param>
    /// <param name="first">Fluid at the first state.</param>
    /// <param name="secondSpecificMassFlow">
    ///     Specific mass flow rate of the fluid at the second state.
    /// </param>
    /// <param name="second">Fluid at the second state.</param>
    /// <returns>The state of the fluid at the end of the process.</returns>
    /// <exception cref="ArgumentException">
    ///     The mixing process is possible only for the same fluids!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     The mixing process is possible only for flows with the same pressure!
    /// </exception>
    public Refrigerant Mixing(
        Ratio firstSpecificMassFlow,
        AbstractFluid first,
        Ratio secondSpecificMassFlow,
        AbstractFluid second
    ) =>
        (Refrigerant)
            base.Mixing(
                firstSpecificMassFlow,
                first,
                secondSpecificMassFlow,
                second
            );

    private static Regex BlendRegex(bool zeotropic) =>
        new(zeotropic ? @"^R4\d{2}" : @"^R5\d{2}", RegexOptions.Compiled);
}

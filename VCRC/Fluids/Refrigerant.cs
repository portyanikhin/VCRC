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
    ///     The selected fluid is not a refrigerant (its name should start with 'R')!
    /// </exception>
    public Refrigerant(FluidsList name) : base(name) =>
        new RefrigerantValidator().ValidateAndThrow(this);

    /// <summary>
    ///     Absolute pressure at the critical point (by default, kPa).
    /// </summary>
    /// <exception cref="NullReferenceException">Invalid critical pressure!</exception>
    public new Pressure CriticalPressure =>
        base.CriticalPressure ?? throw new NullReferenceException("Invalid critical pressure!");

    /// <summary>
    ///     Temperature at the critical point (by default, °C).
    /// </summary>
    /// <exception cref="NullReferenceException">Invalid critical temperature!</exception>
    public new Temperature CriticalTemperature =>
        base.CriticalTemperature ?? throw new NullReferenceException("Invalid critical temperature!");

    /// <summary>
    ///     Temperature glide at atmospheric pressure (by default, K).
    /// </summary>
    public TemperatureDelta Glide =>
        (DewPointAt(1.Atmospheres()).Temperature -
         BubblePointAt(1.Atmospheres()).Temperature)
        .Abs().ToUnit(TemperatureDeltaUnit.Kelvin);

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
    /// <exception cref="NullReferenceException">Invalid triple pressure!</exception>
    public new Pressure TriplePressure =>
        base.TriplePressure ?? throw new NullReferenceException("Invalid triple pressure!");

    /// <summary>
    ///     Temperature at the triple point (by default, °C).
    /// </summary>
    /// <exception cref="NullReferenceException">Invalid triple temperature!</exception>
    public new Temperature TripleTemperature =>
        base.TripleTemperature ?? throw new NullReferenceException("Invalid triple temperature!");

    /// <summary>
    ///     Subcooled refrigerant.
    /// </summary>
    /// <param name="bubblePointTemperature">Bubble point temperature.</param>
    /// <param name="subcooling">Subcooling.</param>
    /// <returns>Subcooled refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid subcooling!</exception>
    public Refrigerant Subcooled(Temperature bubblePointTemperature, TemperatureDelta subcooling) =>
        subcooling < TemperatureDelta.Zero
            ? throw new ArgumentException("Invalid subcooling!")
            : subcooling.Equals(TemperatureDelta.Zero, ComparisonTolerance, ComparisonType)
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
    public Refrigerant Subcooled(Pressure pressure, TemperatureDelta subcooling) =>
        subcooling < TemperatureDelta.Zero
            ? throw new ArgumentException("Invalid subcooling!")
            : subcooling.Equals(TemperatureDelta.Zero, ComparisonTolerance, ComparisonType)
                ? BubblePointAt(pressure)
                : BubblePointAt(pressure)
                    .CoolingTo(BubblePointAt(pressure).Temperature - subcooling);

    /// <summary>
    ///     Superheated refrigerant.
    /// </summary>
    /// <param name="dewPointTemperature">Dew point temperature.</param>
    /// <param name="superheat">Superheat.</param>
    /// <returns>Superheated refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid superheat!</exception>
    public Refrigerant Superheated(Temperature dewPointTemperature, TemperatureDelta superheat) =>
        superheat < TemperatureDelta.Zero
            ? throw new ArgumentException("Invalid superheat!")
            : superheat.Equals(TemperatureDelta.Zero, ComparisonTolerance, ComparisonType)
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
    public Refrigerant Superheated(Pressure pressure, TemperatureDelta superheat) =>
        superheat < TemperatureDelta.Zero
            ? throw new ArgumentException("Invalid superheat!")
            : superheat.Equals(TemperatureDelta.Zero, ComparisonTolerance, ComparisonType)
                ? DewPointAt(pressure)
                : DewPointAt(pressure)
                    .HeatingTo(DewPointAt(pressure).Temperature + superheat);

    protected override AbstractFluid CreateInstance() => new Refrigerant(Name);

    public new Refrigerant Clone() => (Refrigerant) base.Clone();

    public new Refrigerant Factory() => new(Name);

    public new Refrigerant
        WithState(IKeyedInput<Parameters> firstInput, IKeyedInput<Parameters> secondInput) =>
        (Refrigerant) base.WithState(firstInput, secondInput);

    public new Refrigerant IsentropicCompressionTo(Pressure pressure) =>
        (Refrigerant) base.IsentropicCompressionTo(pressure);

    public new Refrigerant CompressionTo(Pressure pressure, Ratio isentropicEfficiency) =>
        (Refrigerant) base.CompressionTo(pressure, isentropicEfficiency);

    public new Refrigerant IsenthalpicExpansionTo(Pressure pressure) =>
        (Refrigerant) base.IsenthalpicExpansionTo(pressure);

    public new Refrigerant IsentropicExpansionTo(Pressure pressure) =>
        (Refrigerant) base.IsentropicExpansionTo(pressure);

    public new Refrigerant ExpansionTo(Pressure pressure,
        Ratio isentropicEfficiency) =>
        (Refrigerant) base.ExpansionTo(pressure, isentropicEfficiency);

    public new Refrigerant CoolingTo(Temperature temperature,
        Pressure? pressureDrop = null) =>
        (Refrigerant) base.CoolingTo(temperature, pressureDrop);

    public new Refrigerant CoolingTo(SpecificEnergy enthalpy,
        Pressure? pressureDrop = null) =>
        (Refrigerant) base.CoolingTo(enthalpy, pressureDrop);

    public new Refrigerant HeatingTo(Temperature temperature,
        Pressure? pressureDrop = null) =>
        (Refrigerant) base.HeatingTo(temperature, pressureDrop);

    public new Refrigerant HeatingTo(SpecificEnergy enthalpy,
        Pressure? pressureDrop = null) =>
        (Refrigerant) base.HeatingTo(enthalpy, pressureDrop);

    public new Refrigerant BubblePointAt(Pressure pressure) =>
        (Refrigerant) base.BubblePointAt(pressure);

    public new Refrigerant BubblePointAt(Temperature temperature) =>
        (Refrigerant) base.BubblePointAt(temperature);

    public new Refrigerant DewPointAt(Pressure pressure) =>
        (Refrigerant) base.DewPointAt(pressure);

    public new Refrigerant DewPointAt(Temperature temperature) =>
        (Refrigerant) base.DewPointAt(temperature);

    public new Refrigerant TwoPhasePointAt(Pressure pressure, Ratio quality) =>
        (Refrigerant) base.TwoPhasePointAt(pressure, quality);

    public new Refrigerant Mixing(Ratio firstSpecificMassFlow, AbstractFluid first,
        Ratio secondSpecificMassFlow, AbstractFluid second) =>
        (Refrigerant) base.Mixing(firstSpecificMassFlow, first,
            secondSpecificMassFlow, second);

    private static Regex BlendRegex(bool zeotropic) =>
        new(zeotropic ? @"^R4\d{2}" : @"^R5\d{2}", RegexOptions.Compiled);
}
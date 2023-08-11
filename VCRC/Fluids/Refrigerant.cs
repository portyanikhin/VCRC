using UnitsNet.NumberExtensions.NumberToTemperatureDelta;

namespace VCRC;

/// <summary>
///     VCRC working fluid.
/// </summary>
public class Refrigerant : Fluid, IRefrigerant
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

    public new Pressure CriticalPressure =>
        base.CriticalPressure
        ?? throw new NullReferenceException("Invalid critical pressure!");

    public new Temperature CriticalTemperature =>
        base.CriticalTemperature
        ?? throw new NullReferenceException("Invalid critical temperature!");

    public new Pressure TriplePressure =>
        base.TriplePressure
        ?? throw new NullReferenceException("Invalid triple pressure!");

    public new Temperature TripleTemperature =>
        base.TripleTemperature
        ?? throw new NullReferenceException("Invalid triple temperature!");

    public TemperatureDelta Glide =>
        (
            DewPointAt(1.Atmospheres()).Temperature
            - BubblePointAt(1.Atmospheres()).Temperature
        )
            .Abs()
            .ToUnit(TemperatureDeltaUnit.Kelvin);

    public bool HasGlide => Glide > 0.01.Kelvins();

    public bool IsSingleComponent => !IsAzeotropicBlend && !IsZeotropicBlend;

    public bool IsAzeotropicBlend => BlendRegex(false).IsMatch(Name.ToString());

    public bool IsZeotropicBlend => BlendRegex(true).IsMatch(Name.ToString());

    public IRefrigerant Subcooled(
        Temperature bubblePointTemperature,
        TemperatureDelta subcooling
    ) =>
        subcooling < TemperatureDelta.Zero
            ? throw new ArgumentException("Invalid subcooling!")
            : subcooling.Equals(TemperatureDelta.Zero, Tolerance.Kelvins())
                ? BubblePointAt(bubblePointTemperature)
                : BubblePointAt(bubblePointTemperature)
                    .CoolingTo(bubblePointTemperature - subcooling);

    public IRefrigerant Subcooled(
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

    public IRefrigerant Superheated(
        Temperature dewPointTemperature,
        TemperatureDelta superheat
    ) =>
        superheat < TemperatureDelta.Zero
            ? throw new ArgumentException("Invalid superheat!")
            : superheat.Equals(TemperatureDelta.Zero, Tolerance.Kelvins())
                ? DewPointAt(dewPointTemperature)
                : DewPointAt(dewPointTemperature)
                    .HeatingTo(dewPointTemperature + superheat);

    public IRefrigerant Superheated(
        Pressure pressure,
        TemperatureDelta superheat
    ) =>
        superheat < TemperatureDelta.Zero
            ? throw new ArgumentException("Invalid superheat!")
            : superheat.Equals(TemperatureDelta.Zero, Tolerance.Kelvins())
                ? DewPointAt(pressure)
                : DewPointAt(pressure)
                    .HeatingTo(DewPointAt(pressure).Temperature + superheat);

    public new IRefrigerant WithState(
        IKeyedInput<Parameters> firstInput,
        IKeyedInput<Parameters> secondInput
    ) => (Refrigerant)base.WithState(firstInput, secondInput);

    public new IRefrigerant IsentropicCompressionTo(Pressure pressure) =>
        (Refrigerant)base.IsentropicCompressionTo(pressure);

    public new IRefrigerant CompressionTo(
        Pressure pressure,
        Ratio isentropicEfficiency
    ) => (Refrigerant)base.CompressionTo(pressure, isentropicEfficiency);

    public new IRefrigerant IsenthalpicExpansionTo(Pressure pressure) =>
        (Refrigerant)base.IsenthalpicExpansionTo(pressure);

    public new IRefrigerant IsentropicExpansionTo(Pressure pressure) =>
        (Refrigerant)base.IsentropicExpansionTo(pressure);

    public new IRefrigerant ExpansionTo(
        Pressure pressure,
        Ratio isentropicEfficiency
    ) => (Refrigerant)base.ExpansionTo(pressure, isentropicEfficiency);

    public new IRefrigerant CoolingTo(
        Temperature temperature,
        Pressure? pressureDrop = null
    ) => (Refrigerant)base.CoolingTo(temperature, pressureDrop);

    public new IRefrigerant CoolingTo(
        SpecificEnergy enthalpy,
        Pressure? pressureDrop = null
    ) => (Refrigerant)base.CoolingTo(enthalpy, pressureDrop);

    public new IRefrigerant HeatingTo(
        Temperature temperature,
        Pressure? pressureDrop = null
    ) => (Refrigerant)base.HeatingTo(temperature, pressureDrop);

    public new IRefrigerant HeatingTo(
        SpecificEnergy enthalpy,
        Pressure? pressureDrop = null
    ) => (Refrigerant)base.HeatingTo(enthalpy, pressureDrop);

    public new IRefrigerant BubblePointAt(Pressure pressure) =>
        (Refrigerant)base.BubblePointAt(pressure);

    public new IRefrigerant BubblePointAt(Temperature temperature) =>
        (Refrigerant)base.BubblePointAt(temperature);

    public new IRefrigerant DewPointAt(Pressure pressure) =>
        (Refrigerant)base.DewPointAt(pressure);

    public new IRefrigerant DewPointAt(Temperature temperature) =>
        (Refrigerant)base.DewPointAt(temperature);

    public new IRefrigerant TwoPhasePointAt(Pressure pressure, Ratio quality) =>
        (Refrigerant)base.TwoPhasePointAt(pressure, quality);

    public IRefrigerant Mixing(
        Ratio firstSpecificMassFlow,
        IRefrigerant first,
        Ratio secondSpecificMassFlow,
        IRefrigerant second
    ) =>
        (Refrigerant)
            base.Mixing(
                firstSpecificMassFlow,
                first,
                secondSpecificMassFlow,
                second
            );

    public new IRefrigerant Clone() => (Refrigerant)base.Clone();

    public new IRefrigerant Factory() => (Refrigerant)base.Factory();

    protected override AbstractFluid CreateInstance() => new Refrigerant(Name);

    private static Regex BlendRegex(bool zeotropic) =>
        new(zeotropic ? @"^R4\d{2}" : @"^R5\d{2}", RegexOptions.Compiled);
}

namespace VCRC;

/// <summary>
///     VCRC working fluid.
/// </summary>
public interface IRefrigerant : IFluid
{
    /// <summary>
    ///     Absolute pressure at the critical point (by default, kPa).
    /// </summary>
    /// <exception cref="NullReferenceException">
    ///     Invalid critical pressure!
    /// </exception>
    public new Pressure CriticalPressure { get; }

    /// <summary>
    ///     Temperature at the critical point (by default, °C).
    /// </summary>
    /// <exception cref="NullReferenceException">
    ///     Invalid critical temperature!
    /// </exception>
    public new Temperature CriticalTemperature { get; }

    /// <summary>
    ///     Absolute pressure at the triple point (by default, kPa).
    /// </summary>
    /// <exception cref="NullReferenceException">
    ///     Invalid triple pressure!
    /// </exception>
    public new Pressure TriplePressure { get; }

    /// <summary>
    ///     Temperature at the triple point (by default, °C).
    /// </summary>
    /// <exception cref="NullReferenceException">
    ///     Invalid triple temperature!
    /// </exception>
    public new Temperature TripleTemperature { get; }

    /// <summary>
    ///     Temperature glide at atmospheric pressure (by default, K).
    /// </summary>
    public TemperatureDelta Glide { get; }

    /// <summary>
    ///     <c>true</c> if the refrigerant has a temperature glide.
    /// </summary>
    public bool HasGlide { get; }

    /// <summary>
    ///     <c>true</c> if the refrigerant is a single component.
    /// </summary>
    public bool IsSingleComponent { get; }

    /// <summary>
    ///     <c>true</c> if the refrigerant is an azeotropic blend.
    /// </summary>
    public bool IsAzeotropicBlend { get; }

    /// <summary>
    ///     <c>true</c> if the refrigerant is a zeotropic blend.
    /// </summary>
    public bool IsZeotropicBlend { get; }

    /// <summary>
    ///     Subcooled refrigerant.
    /// </summary>
    /// <param name="bubblePointTemperature">Bubble point temperature.</param>
    /// <param name="subcooling">Subcooling.</param>
    /// <returns>Subcooled refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid subcooling!</exception>
    public IRefrigerant Subcooled(
        Temperature bubblePointTemperature,
        TemperatureDelta subcooling
    );

    /// <summary>
    ///     Subcooled refrigerant.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="subcooling">Subcooling.</param>
    /// <returns>Subcooled refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid subcooling!</exception>
    public IRefrigerant Subcooled(
        Pressure pressure,
        TemperatureDelta subcooling
    );

    /// <summary>
    ///     Superheated refrigerant.
    /// </summary>
    /// <param name="dewPointTemperature">Dew point temperature.</param>
    /// <param name="superheat">Superheat.</param>
    /// <returns>Superheated refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid superheat!</exception>
    public IRefrigerant Superheated(
        Temperature dewPointTemperature,
        TemperatureDelta superheat
    );

    /// <summary>
    ///     Superheated refrigerant.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="superheat">Superheat.</param>
    /// <returns>Superheated refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid superheat!</exception>
    public IRefrigerant Superheated(
        Pressure pressure,
        TemperatureDelta superheat
    );

    /// <inheritdoc cref="IFluid.WithState"/>
    public new IRefrigerant WithState(
        IKeyedInput<Parameters> firstInput,
        IKeyedInput<Parameters> secondInput
    );

    /// <inheritdoc cref="IFluid.IsentropicCompressionTo"/>
    public new IRefrigerant IsentropicCompressionTo(Pressure pressure);

    /// <inheritdoc cref="IFluid.CompressionTo"/>
    public new IRefrigerant CompressionTo(
        Pressure pressure,
        Ratio isentropicEfficiency
    );

    /// <inheritdoc cref="IFluid.IsenthalpicExpansionTo"/>
    public new IRefrigerant IsenthalpicExpansionTo(Pressure pressure);

    /// <inheritdoc cref="IFluid.IsentropicExpansionTo"/>
    public new IRefrigerant IsentropicExpansionTo(Pressure pressure);

    /// <inheritdoc cref="IFluid.ExpansionTo"/>
    public new IRefrigerant ExpansionTo(
        Pressure pressure,
        Ratio isentropicEfficiency
    );

    /// <inheritdoc cref="IFluid.CoolingTo(Temperature, Pressure?)"/>
    public new IRefrigerant CoolingTo(
        Temperature temperature,
        Pressure? pressureDrop = null
    );

    /// <inheritdoc cref="IFluid.CoolingTo(SpecificEnergy, Pressure?)"/>
    public new IRefrigerant CoolingTo(
        SpecificEnergy enthalpy,
        Pressure? pressureDrop = null
    );

    /// <inheritdoc cref="IFluid.HeatingTo(Temperature, Pressure?)"/>
    public new IRefrigerant HeatingTo(
        Temperature temperature,
        Pressure? pressureDrop = null
    );

    /// <inheritdoc cref="IFluid.HeatingTo(SpecificEnergy, Pressure?)"/>
    public new IRefrigerant HeatingTo(
        SpecificEnergy enthalpy,
        Pressure? pressureDrop = null
    );

    /// <inheritdoc cref="IFluid.BubblePointAt(Pressure)"/>
    public new IRefrigerant BubblePointAt(Pressure pressure);

    /// <inheritdoc cref="IFluid.BubblePointAt(Temperature)"/>
    public new IRefrigerant BubblePointAt(Temperature temperature);

    /// <inheritdoc cref="IFluid.DewPointAt(Pressure)"/>
    public new IRefrigerant DewPointAt(Pressure pressure);

    /// <inheritdoc cref="IFluid.DewPointAt(Temperature)"/>
    public new IRefrigerant DewPointAt(Temperature temperature);

    /// <inheritdoc cref="IFluid.TwoPhasePointAt"/>
    public new IRefrigerant TwoPhasePointAt(Pressure pressure, Ratio quality);

    /// <inheritdoc cref="IFluid.Mixing"/>
    public IRefrigerant Mixing(
        Ratio firstSpecificMassFlow,
        IRefrigerant first,
        Ratio secondSpecificMassFlow,
        IRefrigerant second
    );

    /// <inheritdoc cref="IClonable{T}.Clone"/>
    public new IRefrigerant Clone();

    /// <inheritdoc cref="IFactory{T}.Factory"/>
    public new IRefrigerant Factory();
}

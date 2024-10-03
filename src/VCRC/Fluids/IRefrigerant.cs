namespace VCRC;

/// <summary>
///     VCRC working fluid.
/// </summary>
public interface IRefrigerant : IFluid
{
    /// <summary>
    ///     Absolute pressure at the critical point (by default, kPa).
    /// </summary>
    /// <exception cref="NullReferenceException">Invalid critical pressure!</exception>
    new Pressure CriticalPressure { get; }

    /// <summary>
    ///     Temperature at the critical point (by default, °C).
    /// </summary>
    /// <exception cref="NullReferenceException">Invalid critical temperature!</exception>
    new Temperature CriticalTemperature { get; }

    /// <summary>
    ///     Absolute pressure at the triple point (by default, kPa).
    /// </summary>
    /// <exception cref="NullReferenceException">Invalid triple pressure!</exception>
    new Pressure TriplePressure { get; }

    /// <summary>
    ///     Temperature at the triple point (by default, °C).
    /// </summary>
    /// <exception cref="NullReferenceException">Invalid triple temperature!</exception>
    new Temperature TripleTemperature { get; }

    /// <summary>
    ///     Temperature glide at atmospheric pressure (by default, K).
    /// </summary>
    TemperatureDelta Glide { get; }

    /// <summary>
    ///     <c>true</c> if the refrigerant has a temperature glide.
    /// </summary>
    bool HasGlide { get; }

    /// <summary>
    ///     <c>true</c> if the refrigerant is a single component.
    /// </summary>
    bool IsSingleComponent { get; }

    /// <summary>
    ///     <c>true</c> if the refrigerant is an azeotropic blend.
    /// </summary>
    bool IsAzeotropicBlend { get; }

    /// <summary>
    ///     <c>true</c> if the refrigerant is a zeotropic blend.
    /// </summary>
    bool IsZeotropicBlend { get; }

    /// <summary>
    ///     Subcooled refrigerant.
    /// </summary>
    /// <param name="bubblePointTemperature">Bubble point temperature.</param>
    /// <param name="subcooling">Subcooling.</param>
    /// <returns>Subcooled refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid subcooling!</exception>
    IRefrigerant Subcooled(Temperature bubblePointTemperature, TemperatureDelta subcooling);

    /// <summary>
    ///     Subcooled refrigerant.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="subcooling">Subcooling.</param>
    /// <returns>Subcooled refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid subcooling!</exception>
    IRefrigerant Subcooled(Pressure pressure, TemperatureDelta subcooling);

    /// <summary>
    ///     Superheated refrigerant.
    /// </summary>
    /// <param name="dewPointTemperature">Dew point temperature.</param>
    /// <param name="superheat">Superheat.</param>
    /// <returns>Superheated refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid superheat!</exception>
    IRefrigerant Superheated(Temperature dewPointTemperature, TemperatureDelta superheat);

    /// <summary>
    ///     Superheated refrigerant.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="superheat">Superheat.</param>
    /// <returns>Superheated refrigerant.</returns>
    /// <exception cref="ArgumentException">Invalid superheat!</exception>
    IRefrigerant Superheated(Pressure pressure, TemperatureDelta superheat);

    /// <inheritdoc cref="IFluid.SpecifyPhase"/>
    new IRefrigerant SpecifyPhase(Phases phase);

    /// <inheritdoc cref="IFluid.UnspecifyPhase"/>
    new IRefrigerant UnspecifyPhase();

    /// <inheritdoc cref="IFluid.WithState"/>
    new IRefrigerant WithState(
        IKeyedInput<Parameters> firstInput,
        IKeyedInput<Parameters> secondInput
    );

    /// <inheritdoc cref="IFluid.IsentropicCompressionTo"/>
    new IRefrigerant IsentropicCompressionTo(Pressure pressure);

    /// <inheritdoc cref="IFluid.CompressionTo"/>
    new IRefrigerant CompressionTo(Pressure pressure, Ratio isentropicEfficiency);

    /// <inheritdoc cref="IFluid.IsenthalpicExpansionTo"/>
    new IRefrigerant IsenthalpicExpansionTo(Pressure pressure);

    /// <inheritdoc cref="IFluid.IsentropicExpansionTo"/>
    new IRefrigerant IsentropicExpansionTo(Pressure pressure);

    /// <inheritdoc cref="IFluid.ExpansionTo"/>
    new IRefrigerant ExpansionTo(Pressure pressure, Ratio isentropicEfficiency);

    /// <inheritdoc cref="IFluid.CoolingTo(Temperature, Pressure?)"/>
    new IRefrigerant CoolingTo(Temperature temperature, Pressure? pressureDrop = null);

    /// <inheritdoc cref="IFluid.CoolingTo(SpecificEnergy, Pressure?)"/>
    new IRefrigerant CoolingTo(SpecificEnergy enthalpy, Pressure? pressureDrop = null);

    /// <inheritdoc cref="IFluid.HeatingTo(Temperature, Pressure?)"/>
    new IRefrigerant HeatingTo(Temperature temperature, Pressure? pressureDrop = null);

    /// <inheritdoc cref="IFluid.HeatingTo(SpecificEnergy, Pressure?)"/>
    new IRefrigerant HeatingTo(SpecificEnergy enthalpy, Pressure? pressureDrop = null);

    /// <inheritdoc cref="IFluid.BubblePointAt(Pressure)"/>
    new IRefrigerant BubblePointAt(Pressure pressure);

    /// <inheritdoc cref="IFluid.BubblePointAt(Temperature)"/>
    new IRefrigerant BubblePointAt(Temperature temperature);

    /// <inheritdoc cref="IFluid.DewPointAt(Pressure)"/>
    new IRefrigerant DewPointAt(Pressure pressure);

    /// <inheritdoc cref="IFluid.DewPointAt(Temperature)"/>
    new IRefrigerant DewPointAt(Temperature temperature);

    /// <inheritdoc cref="IFluid.TwoPhasePointAt"/>
    new IRefrigerant TwoPhasePointAt(Pressure pressure, Ratio quality);

    /// <inheritdoc cref="IFluid.Mixing"/>
    IRefrigerant Mixing(
        Ratio firstSpecificMassFlow,
        IRefrigerant first,
        Ratio secondSpecificMassFlow,
        IRefrigerant second
    );

    /// <inheritdoc cref="IClonable{T}.Clone"/>
    new IRefrigerant Clone();

    /// <inheritdoc cref="IFactory{T}.Factory"/>
    new IRefrigerant Factory();
}

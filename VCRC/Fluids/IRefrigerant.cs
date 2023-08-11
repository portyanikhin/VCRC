﻿namespace VCRC;

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

    /// <summary>
    ///     Returns a new fluid instance with a defined state.
    /// </summary>
    /// <param name="firstInput">First input property.</param>
    /// <param name="secondInput">Second input property.</param>
    /// <returns>
    ///     A new fluid instance with a defined state.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Need to define 2 unique inputs!
    /// </exception>
    public new IRefrigerant WithState(
        IKeyedInput<Parameters> firstInput,
        IKeyedInput<Parameters> secondInput
    );

    /// <summary>
    ///     The process of isentropic compression to a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <returns>
    ///     The state of the fluid at the end of the process.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Compressor outlet pressure should be higher than inlet pressure!
    /// </exception>
    public new IRefrigerant IsentropicCompressionTo(Pressure pressure);

    /// <summary>
    ///     The process of compression to a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="isentropicEfficiency">
    ///     Compressor isentropic efficiency.
    /// </param>
    /// <returns>
    ///     The state of the fluid at the end of the process.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Compressor outlet pressure should be higher than inlet pressure!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid compressor isentropic efficiency!
    /// </exception>
    public new IRefrigerant CompressionTo(
        Pressure pressure,
        Ratio isentropicEfficiency
    );

    /// <summary>
    ///     The process of isenthalpic expansion to a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <returns>
    ///     The state of the fluid at the end of the process.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Expansion valve outlet pressure should be lower than inlet pressure!
    /// </exception>
    public new IRefrigerant IsenthalpicExpansionTo(Pressure pressure);

    /// <summary>
    ///     The process of isentropic expansion to a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <returns>
    ///     The state of the fluid at the end of the process.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Expander outlet pressure should be lower than inlet pressure!
    /// </exception>
    public new IRefrigerant IsentropicExpansionTo(Pressure pressure);

    /// <summary>
    ///     The process of expansion to a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="isentropicEfficiency">
    ///     Expander isentropic efficiency.
    /// </param>
    /// <returns>
    ///     The state of the fluid at the end of the process.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Expander outlet pressure should be lower than inlet pressure!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid expander isentropic efficiency!
    /// </exception>
    public new IRefrigerant ExpansionTo(
        Pressure pressure,
        Ratio isentropicEfficiency
    );

    /// <summary>
    ///     The process of cooling to a given temperature.
    /// </summary>
    /// <param name="temperature">Temperature.</param>
    /// <param name="pressureDrop">
    ///     Pressure drop in the heat exchanger (optional).
    /// </param>
    /// <returns>
    ///     The state of the fluid at the end of the process.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     During the cooling process, the temperature should decrease!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid pressure drop in the heat exchanger!
    /// </exception>
    public new IRefrigerant CoolingTo(
        Temperature temperature,
        Pressure? pressureDrop = null
    );

    /// <summary>
    ///     The process of cooling to a given enthalpy.
    /// </summary>
    /// <param name="enthalpy">Enthalpy.</param>
    /// <param name="pressureDrop">
    ///     Pressure drop in the heat exchanger (optional).
    /// </param>
    /// <returns>
    ///     The state of the fluid at the end of the process.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     During the cooling process, the enthalpy should decrease!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid pressure drop in the heat exchanger!
    /// </exception>
    public new IRefrigerant CoolingTo(
        SpecificEnergy enthalpy,
        Pressure? pressureDrop = null
    );

    /// <summary>
    ///     The process of heating to a given temperature.
    /// </summary>
    /// <param name="temperature">Temperature.</param>
    /// <param name="pressureDrop">
    ///     Pressure drop in the heat exchanger (optional).
    /// </param>
    /// <returns>
    ///     The state of the fluid at the end of the process.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     During the heating process, the temperature should increase!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid pressure drop in the heat exchanger!
    /// </exception>
    public new IRefrigerant HeatingTo(
        Temperature temperature,
        Pressure? pressureDrop = null
    );

    /// <summary>
    ///     The process of heating to a given enthalpy.
    /// </summary>
    /// <param name="enthalpy">Enthalpy.</param>
    /// <param name="pressureDrop">
    ///     Pressure drop in the heat exchanger (optional).
    /// </param>
    /// <returns>
    ///     The state of the fluid at the end of the process.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     During the heating process, the enthalpy should increase!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Invalid pressure drop in the heat exchanger!
    /// </exception>
    public new IRefrigerant HeatingTo(
        SpecificEnergy enthalpy,
        Pressure? pressureDrop = null
    );

    /// <summary>
    ///     Returns a bubble point at a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <returns>
    ///     A bubble point at a given pressure.
    /// </returns>
    public new IRefrigerant BubblePointAt(Pressure pressure);

    /// <summary>
    ///     Returns a bubble point at a given temperature.
    /// </summary>
    /// <param name="temperature">Temperature.</param>
    /// <returns>
    ///     A bubble point at a given temperature.
    /// </returns>
    public new IRefrigerant BubblePointAt(Temperature temperature);

    /// <summary>
    ///     Returns a dew point at a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <returns>
    ///     A dew point at a given pressure.
    /// </returns>
    public new IRefrigerant DewPointAt(Pressure pressure);

    /// <summary>
    ///     Returns a dew point at a given temperature.
    /// </summary>
    /// <param name="temperature">Temperature.</param>
    /// <returns>
    ///     A dew point at a given temperature.
    /// </returns>
    public new IRefrigerant DewPointAt(Temperature temperature);

    /// <summary>
    ///     Returns a two-phase point at a given pressure.
    /// </summary>
    /// <param name="pressure">Pressure.</param>
    /// <param name="quality">Vapor quality.</param>
    /// <returns>
    ///     Two-phase point at a given pressure.
    /// </returns>
    public new IRefrigerant TwoPhasePointAt(Pressure pressure, Ratio quality);

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
    /// <returns>
    ///     The state of the fluid at the end of the process.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     The mixing process is possible only for the same fluids!
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     The mixing process is possible only for flows with the same pressure!
    /// </exception>
    public IRefrigerant Mixing(
        Ratio firstSpecificMassFlow,
        IRefrigerant first,
        Ratio secondSpecificMassFlow,
        IRefrigerant second
    );

    /// <summary>
    ///     Performs deep (full) copy of the instance.
    /// </summary>
    /// <returns>Deep copy of the instance.</returns>
    public new IRefrigerant Clone();

    /// <summary>
    ///     Creates a new instance with no defined state.
    /// </summary>
    /// <returns>
    ///     A new instance with no defined state.
    /// </returns>
    public new IRefrigerant Factory();
}

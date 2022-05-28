using FluentValidation;
using UnitsNet;
using UnitsNet.Units;

namespace VCRC;

/// <summary>
///     Ejector as a VCRC component.
/// </summary>
public record Ejector
{
    /// <summary>
    ///     Ejector as a VCRC component.
    /// </summary>
    /// <param name="isentropicEfficiency">
    ///     Isentropic efficiency of the nozzle, suction section and diffuser.
    /// </param>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the nozzle should be in (0;100) %!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the suction section should be in (0;100) %!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the diffuser should be in (0;100) %!
    /// </exception>
    public Ejector(Ratio isentropicEfficiency) :
        this(isentropicEfficiency, isentropicEfficiency, isentropicEfficiency)
    {
    }

    /// <summary>
    ///     Ejector as a VCRC component.
    /// </summary>
    /// <param name="nozzleIsentropicEfficiency">Isentropic efficiency of the nozzle.</param>
    /// <param name="suctionIsentropicEfficiency">Isentropic efficiency of the suction section.</param>
    /// <param name="diffuserIsentropicEfficiency">Isentropic efficiency of the diffuser.</param>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the nozzle should be in (0;100) %!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the suction section should be in (0;100) %!
    /// </exception>
    /// <exception cref="ValidationException">
    ///     Isentropic efficiency of the diffuser should be in (0;100) %!
    /// </exception>
    public Ejector(Ratio nozzleIsentropicEfficiency, Ratio suctionIsentropicEfficiency,
        Ratio diffuserIsentropicEfficiency)
    {
        (NozzleIsentropicEfficiency,
                SuctionIsentropicEfficiency,
                DiffuserIsentropicEfficiency) =
            (nozzleIsentropicEfficiency.ToUnit(RatioUnit.Percent),
                suctionIsentropicEfficiency.ToUnit(RatioUnit.Percent),
                diffuserIsentropicEfficiency.ToUnit(RatioUnit.Percent));
        new EjectorValidator().ValidateAndThrow(this);
    }

    /// <summary>
    ///     Isentropic efficiency of the nozzle.
    /// </summary>
    public Ratio NozzleIsentropicEfficiency { get; }

    /// <summary>
    ///     Isentropic efficiency of the suction section.
    /// </summary>
    public Ratio SuctionIsentropicEfficiency { get; }

    /// <summary>
    ///     Isentropic efficiency of the diffuser.
    /// </summary>
    public Ratio DiffuserIsentropicEfficiency { get; }
}
using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;

namespace VCRC;

internal class EjectorValidator : AbstractValidator<Ejector>
{
    internal EjectorValidator()
    {
        RuleFor(ejector => ejector.NozzleIsentropicEfficiency)
            .ExclusiveBetween(Ratio.Zero, 100.Percent())
            .WithMessage("Isentropic efficiency of the nozzle should be in (0;100) %!");
        RuleFor(ejector => ejector.SuctionIsentropicEfficiency)
            .ExclusiveBetween(Ratio.Zero, 100.Percent())
            .WithMessage("Isentropic efficiency of the suction section should be in (0;100) %!");
        RuleFor(ejector => ejector.DiffuserIsentropicEfficiency)
            .ExclusiveBetween(Ratio.Zero, 100.Percent())
            .WithMessage("Isentropic efficiency of the diffuser should be in (0;100) %!");
    }
}
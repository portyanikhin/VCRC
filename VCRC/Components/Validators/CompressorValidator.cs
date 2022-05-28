using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;

namespace VCRC;

internal class CompressorValidator : AbstractValidator<Compressor>
{
    internal CompressorValidator()
    {
        RuleFor(compressor => compressor.Efficiency)
            .ExclusiveBetween(Ratio.Zero, 100.Percent())
            .WithMessage("Isentropic efficiency of the compressor should be in (0;100) %!");
    }
}
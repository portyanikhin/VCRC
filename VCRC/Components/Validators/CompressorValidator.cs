namespace VCRC;

internal class CompressorValidator : AbstractValidator<ICompressor>
{
    public CompressorValidator() =>
        RuleFor(compressor => compressor.Efficiency)
            .ExclusiveBetween(Ratio.Zero, 100.Percent())
            .WithMessage(
                "Isentropic efficiency of the compressor "
                    + "should be in (0;100) %!"
            );
}

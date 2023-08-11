namespace VCRC;

internal class EjectorValidator : AbstractValidator<IEjector>
{
    public EjectorValidator()
    {
        RuleFor(ejector => ejector.NozzleEfficiency)
            .ExclusiveBetween(Ratio.Zero, 100.Percent())
            .WithMessage(
                "Isentropic efficiency of the nozzle should be in (0;100) %!"
            );
        RuleFor(ejector => ejector.SuctionEfficiency)
            .ExclusiveBetween(Ratio.Zero, 100.Percent())
            .WithMessage(
                "Isentropic efficiency of the suction section "
                    + "should be in (0;100) %!"
            );
        RuleFor(ejector => ejector.DiffuserEfficiency)
            .ExclusiveBetween(Ratio.Zero, 100.Percent())
            .WithMessage(
                "Isentropic efficiency of the diffuser should be in (0;100) %!"
            );
    }
}

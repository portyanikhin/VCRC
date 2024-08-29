namespace VCRC;

internal sealed class EjectorFlowsValidator : AbstractValidator<IEjectorFlows>
{
    public EjectorFlowsValidator()
    {
        RuleFor(flows => flows.NozzleInlet.Name)
            .Equal(flows => flows.SuctionInlet.Name)
            .WithMessage("Only one refrigerant should be selected!");
        RuleFor(flows => flows.NozzleInlet.Pressure)
            .GreaterThan(flows => flows.SuctionInlet.Pressure)
            .WithMessage(
                "Ejector nozzle inlet pressure should be greater than suction inlet pressure!"
            );
    }
}

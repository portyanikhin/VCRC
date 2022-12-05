namespace VCRC;

internal class EntropyAnalyzerValidator : AbstractValidator<EntropyAnalyzer>
{
    internal EntropyAnalyzerValidator()
    {
        RuleFor(analyzer => analyzer.ColdSource)
            .NotEqual(analyzer => analyzer.HotSource)
            .WithMessage("Indoor and outdoor temperatures should not be equal!");
        RuleFor(analyzer => analyzer.ColdSource)
            .GreaterThan(analyzer => analyzer.EvaporatorInfo.Outlet.Temperature)
            .WithMessage(
                "Wrong temperature difference in the evaporator! " +
                "Increase 'cold' source temperature.");
        RuleFor(analyzer => analyzer.HotSource)
            .LessThan(analyzer => analyzer.HeatReleaserInfo.Outlet.Temperature)
            .WithMessage(
                "Wrong temperature difference in the condenser or gas cooler! " +
                "Decrease 'hot' source temperature.");
    }
}
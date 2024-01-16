namespace VCRC;

internal class VCRCWithRecuperatorValidator
    : AbstractValidator<IVCRCWithRecuperator>
{
    public VCRCWithRecuperatorValidator()
    {
        RuleFor(vcrc =>
                vcrc.Point4.Temperature - vcrc.Recuperator.TemperatureDifference
            )
            .GreaterThan(vcrc => vcrc.Point1.Temperature)
            .WithMessage(
                "Too high temperature difference at the recuperator 'hot' side!"
            );
    }
}

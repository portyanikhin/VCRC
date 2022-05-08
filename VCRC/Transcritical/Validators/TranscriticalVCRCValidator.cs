using FluentValidation;

namespace VCRC.Transcritical.Validators;

public class TranscriticalVCRCValidator : AbstractValidator<TranscriticalVCRC>
{
    public TranscriticalVCRCValidator()
    {
        RuleFor(vcrc => vcrc.GasCooler.RefrigerantName)
            .Equal(vcrc => vcrc.Evaporator.RefrigerantName)
            .WithMessage("Only one refrigerant should be selected!");
    }
}
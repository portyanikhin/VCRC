using FluentValidation;

namespace VCRC.Validators
{
    public class SubcriticalVCRCValidator : AbstractValidator<SubcriticalVCRC>
    {
        public SubcriticalVCRCValidator()
        {
            RuleFor(vcrc => vcrc.Condenser.RefrigerantName).Equal(vcrc => vcrc.Evaporator.RefrigerantName)
                .WithMessage("Only one refrigerant should be selected!");
            RuleFor(vcrc => vcrc.Condenser.Temperature).GreaterThan(vcrc => vcrc.Evaporator.Temperature)
                .WithMessage("Condensing temperature should be greater than evaporating temperature!");
        }
    }
}
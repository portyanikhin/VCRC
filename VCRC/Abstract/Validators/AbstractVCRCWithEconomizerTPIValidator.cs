﻿using FluentValidation;

namespace VCRC.Abstract.Validators;

internal class AbstractVCRCWithEconomizerTPIValidator : AbstractValidator<AbstractVCRCWithEconomizerTPI>
{
    internal AbstractVCRCWithEconomizerTPIValidator()
    {
        RuleFor(vcrc => vcrc.Point7.Quality)
            .Must(quality => quality?.DecimalFractions is > 0 and < 1)
            .WithMessage("There should be a two-phase refrigerant at the compressor injection circuit!");
        RuleFor(vcrc => vcrc.Point7.Temperature)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Wrong temperature difference at economizer 'hot' side!");
        RuleFor(vcrc => vcrc.Point8.Temperature)
            .LessThan(vcrc => vcrc.Point5.Temperature)
            .WithMessage("Too high temperature difference at economizer 'cold' side!");
    }
}